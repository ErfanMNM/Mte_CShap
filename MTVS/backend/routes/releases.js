const express = require('express');
const router = express.Router();
const { prisma } = require('../config/database');
const googleDriveService = require('../services/googleDriveService');
const multer = require('multer');
const path = require('path');
const crypto = require('crypto');
const fs = require('fs').promises;

// Cấu hình multer để lưu file tạm
const upload = multer({
    dest: 'uploads/',
    limits: {
        fileSize: parseInt(process.env.MAX_FILE_SIZE) || 524288000 // 500MB
    }
});

/**
 * GET /api/releases/check
 * Kiểm tra có update mới không
 */
router.get('/check', async (req, res) => {
    try {
        const { product, channel, currentVersion, os, arch } = req.query;

        if (!product || !channel || !currentVersion || !os || !arch) {
            return res.status(400).json({
                success: false,
                error: 'Missing required parameters: product, channel, currentVersion, os, arch'
            });
        }

        // Tìm release mới nhất phù hợp
        const latestRelease = await prisma.release.findFirst({
            where: {
                product,
                channel,
                os,
                arch
            },
            orderBy: {
                publishedAt: 'desc'
            }
        });

        if (!latestRelease) {
            return res.json({
                success: true,
                hasUpdate: false,
                message: 'No releases found'
            });
        }

        const hasUpdate = compareVersions(latestRelease.version, currentVersion) > 0;

        // Kiểm tra minVersion nếu có
        if (latestRelease.minVersion && compareVersions(currentVersion, latestRelease.minVersion) < 0) {
            return res.json({
                success: true,
                hasUpdate: false,
                message: 'Current version is too old. Manual update required.',
                requiresManualUpdate: true,
                minVersion: latestRelease.minVersion
            });
        }

        // Tạo download URL
        const downloadUrl = latestRelease.googleDriveFileId 
            ? googleDriveService.getDownloadUrl(latestRelease.googleDriveFileId)
            : null;

        res.json({
            success: true,
            hasUpdate: hasUpdate,
            latestRelease: {
                version: latestRelease.version,
                channel: latestRelease.channel,
                changelog: latestRelease.changelog,
                publishedAt: latestRelease.publishedAt,
                signedHash: latestRelease.signedHash,
                riskLevel: latestRelease.riskLevel
            },
            downloadUrl: downloadUrl,
            filePassword: latestRelease.filePassword,
            manifestRef: latestRelease.manifestRef
        });
    } catch (error) {
        console.error('Error checking for updates:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * POST /api/releases
 * Tạo release mới
 */
router.post('/', upload.single('file'), async (req, res) => {
    try {
        const {
            product,
            version,
            channel,
            os,
            arch,
            minVersion,
            changelog,
            riskLevel,
            constraints,
            manifestRef,
            filePassword
        } = req.body;

        if (!product || !version || !channel || !os || !arch) {
            return res.status(400).json({
                success: false,
                error: 'Missing required fields'
            });
        }

        if (!req.file) {
            return res.status(400).json({
                success: false,
                error: 'No file uploaded'
            });
        }

        // Tính hash của file
        const fileBuffer = await fs.readFile(req.file.path);
        const signedHash = crypto.createHash('sha256').update(fileBuffer).digest('hex');

        // Upload lên Google Drive
        const fileName = `${product}-${version}-${os}-${arch}.${path.extname(req.file.originalname).slice(1)}`;
        const mimeType = req.file.mimetype || 'application/zip';
        
        const driveFile = await googleDriveService.uploadFile(
            req.file.path,
            fileName,
            mimeType,
            filePassword || null
        );

        // Parse constraints nếu có
        let constraintsJson = null;
        if (constraints) {
            try {
                constraintsJson = typeof constraints === 'string' ? JSON.parse(constraints) : constraints;
            } catch (e) {
                console.warn('Invalid constraints JSON:', e);
            }
        }

        // Lưu vào database với Prisma
        const release = await prisma.release.create({
            data: {
                product,
                version,
                channel,
                manifestRef: manifestRef || null,
                files: [driveFile.fileId],
                os,
                arch,
                minVersion: minVersion || null,
                signedHash,
                changelog: changelog || null,
                riskLevel: riskLevel || null,
                constraints: constraintsJson,
                googleDriveFileId: driveFile.fileId,
                googleDriveFileName: driveFile.fileName,
                filePassword: filePassword || null
            }
        });

        // Xóa file tạm
        await fs.unlink(req.file.path);

        res.json({
            success: true,
            release: {
                id: release.id,
                product: release.product,
                version: release.version,
                channel: release.channel,
                googleDriveFileId: release.googleDriveFileId,
                downloadUrl: googleDriveService.getDownloadUrl(release.googleDriveFileId),
                signedHash: release.signedHash
            }
        });
    } catch (error) {
        console.error('Error creating release:', error);
        
        // Xóa file tạm nếu có lỗi
        if (req.file && req.file.path) {
            try {
                await fs.unlink(req.file.path);
            } catch (unlinkError) {
                console.error('Error deleting temp file:', unlinkError);
            }
        }

        // Handle Prisma unique constraint error
        if (error.code === 'P2002') {
            return res.status(409).json({
                success: false,
                error: 'Release with this product and version already exists'
            });
        }

        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/releases
 * Lấy danh sách releases
 */
router.get('/', async (req, res) => {
    try {
        const { product, channel, os, arch, limit = 50, offset = 0 } = req.query;
        
        const where = {};
        if (product) where.product = product;
        if (channel) where.channel = channel;
        if (os) where.os = os;
        if (arch) where.arch = arch;

        const releases = await prisma.release.findMany({
            where,
            orderBy: {
                publishedAt: 'desc'
            },
            take: parseInt(limit),
            skip: parseInt(offset)
        });

        // Thêm download URL cho mỗi release
        const releasesWithUrls = releases.map(release => ({
            ...release,
            downloadUrl: release.googleDriveFileId 
                ? googleDriveService.getDownloadUrl(release.googleDriveFileId)
                : null
        }));

        res.json({
            success: true,
            releases: releasesWithUrls
        });
    } catch (error) {
        console.error('Error fetching releases:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/releases/:id
 * Lấy thông tin release cụ thể
 */
router.get('/:id', async (req, res) => {
    try {
        const { id } = req.params;
        const release = await prisma.release.findUnique({
            where: { id: parseInt(id) }
        });

        if (!release) {
            return res.status(404).json({
                success: false,
                error: 'Release not found'
            });
        }

        res.json({
            success: true,
            release: {
                ...release,
                downloadUrl: release.googleDriveFileId 
                    ? googleDriveService.getDownloadUrl(release.googleDriveFileId)
                    : null
            }
        });
    } catch (error) {
        console.error('Error fetching release:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * DELETE /api/releases/:id
 * Xóa release
 */
router.delete('/:id', async (req, res) => {
    try {
        const { id } = req.params;

        // Lấy thông tin release
        const release = await prisma.release.findUnique({
            where: { id: parseInt(id) }
        });

        if (!release) {
            return res.status(404).json({
                success: false,
                error: 'Release not found'
            });
        }

        // Xóa file từ Google Drive nếu có
        if (release.googleDriveFileId) {
            try {
                await googleDriveService.deleteFile(release.googleDriveFileId);
            } catch (driveError) {
                console.error('Error deleting from Google Drive:', driveError);
                // Tiếp tục xóa trong DB dù có lỗi
            }
        }

        // Xóa từ database
        await prisma.release.delete({
            where: { id: parseInt(id) }
        });

        res.json({
            success: true,
            message: 'Release deleted successfully'
        });
    } catch (error) {
        if (error.code === 'P2025') {
            return res.status(404).json({
                success: false,
                error: 'Release not found'
            });
        }
        console.error('Error deleting release:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * So sánh version (đơn giản)
 */
function compareVersions(v1, v2) {
    const parts1 = v1.split('.').map(Number);
    const parts2 = v2.split('.').map(Number);
    
    for (let i = 0; i < Math.max(parts1.length, parts2.length); i++) {
        const part1 = parts1[i] || 0;
        const part2 = parts2[i] || 0;
        
        if (part1 > part2) return 1;
        if (part1 < part2) return -1;
    }
    
    return 0;
}

module.exports = router;

