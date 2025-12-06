const express = require('express');
const router = express.Router();
const { prisma } = require('../config/database');
const googleDriveService = require('../services/googleDriveService');
const multer = require('multer');
const crypto = require('crypto');
const fs = require('fs').promises;

const upload = multer({
    dest: 'uploads/backups/',
    limits: {
        fileSize: parseInt(process.env.MAX_FILE_SIZE) || 1073741824 // 1GB
    }
});

/**
 * POST /api/backups
 * Upload backup file
 */
router.post('/', upload.single('file'), async (req, res) => {
    try {
        const {
            clientId,
            product,
            version,
            encrypted,
            filePassword,
            metadata
        } = req.body;

        if (!clientId || !product || !version) {
            return res.status(400).json({
                success: false,
                error: 'Missing required fields: clientId, product, version'
            });
        }

        if (!req.file) {
            return res.status(400).json({
                success: false,
                error: 'No file uploaded'
            });
        }

        // Tính checksum
        const fileBuffer = await fs.readFile(req.file.path);
        const checksum = crypto.createHash('sha256').update(fileBuffer).digest('hex');

        // Tạo backup ID
        const backupId = crypto.randomUUID();

        // Upload lên Google Drive
        const fileName = `backup-${clientId}-${backupId}.${req.file.originalname.split('.').pop()}`;
        const mimeType = req.file.mimetype || 'application/zip';
        
        const driveFile = await googleDriveService.uploadFile(
            req.file.path,
            fileName,
            mimeType,
            filePassword || process.env.BACKUP_PASSWORD || null
        );

        // Parse metadata nếu có
        let metadataJson = null;
        if (metadata) {
            try {
                metadataJson = typeof metadata === 'string' ? JSON.parse(metadata) : metadata;
            } catch (e) {
                console.warn('Invalid metadata JSON:', e);
            }
        }

        // Lưu vào database với Prisma
        const backup = await prisma.backup.create({
            data: {
                clientId,
                backupId,
                googleDriveFileId: driveFile.fileId,
                googleDriveFileName: driveFile.fileName,
                product,
                version,
                size: BigInt(req.file.size),
                checksum,
                encrypted: encrypted === 'true' || encrypted === true,
                filePassword: filePassword || process.env.BACKUP_PASSWORD || null,
                metadata: metadataJson
            }
        });

        // Xóa file tạm
        await fs.unlink(req.file.path);

        res.json({
            success: true,
            backup: {
                id: backup.id,
                backupId: backup.backupId,
                clientId: backup.clientId,
                product: backup.product,
                version: backup.version,
                size: backup.size.toString(),
                checksum: backup.checksum,
                downloadUrl: googleDriveService.getDownloadUrl(backup.googleDriveFileId),
                filePassword: backup.filePassword,
                createdAt: backup.createdAt
            }
        });
    } catch (error) {
        console.error('Error uploading backup:', error);
        
        // Xóa file tạm nếu có lỗi
        if (req.file && req.file.path) {
            try {
                await fs.unlink(req.file.path);
            } catch (unlinkError) {
                console.error('Error deleting temp file:', unlinkError);
            }
        }

        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/backups/:clientId
 * Lấy danh sách backups của client
 */
router.get('/:clientId', async (req, res) => {
    try {
        const { clientId } = req.params;
        const { product, limit = 50, offset = 0 } = req.query;

        const where = { clientId };
        if (product) where.product = product;

        const backups = await prisma.backup.findMany({
            where,
            orderBy: {
                createdAt: 'desc'
            },
            take: parseInt(limit),
            skip: parseInt(offset)
        });

        // Thêm download URL
        const backupsWithUrls = backups.map(backup => ({
            ...backup,
            size: backup.size.toString(),
            downloadUrl: googleDriveService.getDownloadUrl(backup.googleDriveFileId)
        }));

        res.json({
            success: true,
            backups: backupsWithUrls
        });
    } catch (error) {
        console.error('Error fetching backups:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/backups/:clientId/:backupId
 * Lấy thông tin backup cụ thể
 */
router.get('/:clientId/:backupId', async (req, res) => {
    try {
        const { clientId, backupId } = req.params;

        const backup = await prisma.backup.findFirst({
            where: {
                clientId,
                backupId
            }
        });

        if (!backup) {
            return res.status(404).json({
                success: false,
                error: 'Backup not found'
            });
        }

        res.json({
            success: true,
            backup: {
                ...backup,
                size: backup.size.toString(),
                downloadUrl: googleDriveService.getDownloadUrl(backup.googleDriveFileId)
            }
        });
    } catch (error) {
        console.error('Error fetching backup:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * DELETE /api/backups/:backupId
 * Xóa backup
 */
router.delete('/:backupId', async (req, res) => {
    try {
        const { backupId } = req.params;

        // Lấy thông tin backup
        const backup = await prisma.backup.findUnique({
            where: { backupId }
        });

        if (!backup) {
            return res.status(404).json({
                success: false,
                error: 'Backup not found'
            });
        }

        // Xóa từ Google Drive
        try {
            await googleDriveService.deleteFile(backup.googleDriveFileId);
        } catch (driveError) {
            console.error('Error deleting from Google Drive:', driveError);
            // Tiếp tục xóa trong DB dù có lỗi
        }

        // Xóa từ database
        await prisma.backup.delete({
            where: { backupId }
        });

        res.json({
            success: true,
            message: 'Backup deleted'
        });
    } catch (error) {
        if (error.code === 'P2025') {
            return res.status(404).json({
                success: false,
                error: 'Backup not found'
            });
        }
        console.error('Error deleting backup:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

module.exports = router;

