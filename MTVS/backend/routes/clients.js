const express = require('express');
const router = express.Router();
const { prisma } = require('../config/database');

/**
 * POST /api/clients/register
 * Đăng ký client mới hoặc cập nhật thông tin
 */
router.post('/register', async (req, res) => {
    try {
        const {
            clientId,
            product,
            site,
            tenant,
            currentVersion,
            os,
            arch,
            updateChannel
        } = req.body;

        if (!clientId || !product || !currentVersion || !os || !arch) {
            return res.status(400).json({
                success: false,
                error: 'Missing required fields: clientId, product, currentVersion, os, arch'
            });
        }

        // Upsert client (tạo mới hoặc cập nhật)
        const client = await prisma.client.upsert({
            where: {
                clientId: clientId
            },
            update: {
                product,
                site: site || null,
                tenant: tenant || null,
                currentVersion,
                os,
                arch,
                lastSeen: new Date(),
                status: 'active',
                updateChannel: updateChannel || 'stable'
            },
            create: {
                clientId,
                product,
                site: site || null,
                tenant: tenant || null,
                currentVersion,
                os,
                arch,
                status: 'active',
                updateChannel: updateChannel || 'stable'
            }
        });

        res.json({
            success: true,
            message: 'Client registered/updated',
            client: {
                id: client.id,
                clientId: client.clientId,
                product: client.product,
                currentVersion: client.currentVersion,
                lastSeen: client.lastSeen
            }
        });
    } catch (error) {
        console.error('Error registering client:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/clients/:clientId
 * Lấy thông tin client
 */
router.get('/:clientId', async (req, res) => {
    try {
        const { clientId } = req.params;

        const client = await prisma.client.findUnique({
            where: { clientId }
        });

        if (!client) {
            return res.status(404).json({
                success: false,
                error: 'Client not found'
            });
        }

        res.json({
            success: true,
            client
        });
    } catch (error) {
        console.error('Error fetching client:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/clients
 * Lấy danh sách clients
 */
router.get('/', async (req, res) => {
    try {
        const { product, site, status, limit = 100, offset = 0 } = req.query;

        const where = {};
        if (product) where.product = product;
        if (site) where.site = site;
        if (status) where.status = status;

        const clients = await prisma.client.findMany({
            where,
            orderBy: {
                lastSeen: 'desc'
            },
            take: parseInt(limit),
            skip: parseInt(offset)
        });

        res.json({
            success: true,
            clients
        });
    } catch (error) {
        console.error('Error fetching clients:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * PUT /api/clients/:clientId/status
 * Cập nhật trạng thái client
 */
router.put('/:clientId/status', async (req, res) => {
    try {
        const { clientId } = req.params;
        const { status } = req.body;

        if (!status || !['active', 'inactive', 'error'].includes(status)) {
            return res.status(400).json({
                success: false,
                error: 'Invalid status. Must be: active, inactive, or error'
            });
        }

        const client = await prisma.client.update({
            where: { clientId },
            data: {
                status,
                lastSeen: new Date()
            }
        });

        res.json({
            success: true,
            message: 'Client status updated',
            client
        });
    } catch (error) {
        if (error.code === 'P2025') {
            return res.status(404).json({
                success: false,
                error: 'Client not found'
            });
        }
        console.error('Error updating client status:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

module.exports = router;

