const express = require('express');
const router = express.Router();
const { prisma } = require('../config/database');

/**
 * POST /api/events
 * Ghi event mới
 */
router.post('/', async (req, res) => {
    try {
        const {
            clientId,
            type,
            product,
            version,
            payload
        } = req.body;

        if (!clientId || !type || !product) {
            return res.status(400).json({
                success: false,
                error: 'Missing required fields: clientId, type, product'
            });
        }

        // Validate event type
        const validTypes = ['check', 'download', 'backup_ok', 'install_ok', 'health_fail', 'rollback', 'error'];
        if (!validTypes.includes(type)) {
            return res.status(400).json({
                success: false,
                error: `Invalid event type. Must be one of: ${validTypes.join(', ')}`
            });
        }

        // Parse payload nếu có
        let payloadJson = null;
        if (payload) {
            try {
                payloadJson = typeof payload === 'string' ? JSON.parse(payload) : payload;
            } catch (e) {
                console.warn('Invalid payload JSON:', e);
            }
        }

        // Lưu event với Prisma
        const event = await prisma.event.create({
            data: {
                clientId,
                type,
                product,
                version: version || null,
                payload: payloadJson
            }
        });

        res.json({
            success: true,
            event: {
                id: event.id,
                clientId: event.clientId,
                type: event.type,
                product: event.product,
                timestamp: event.timestamp
            }
        });
    } catch (error) {
        console.error('Error creating event:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/events/:clientId
 * Lấy danh sách events của client
 */
router.get('/:clientId', async (req, res) => {
    try {
        const { clientId } = req.params;
        const { type, product, limit = 100, offset = 0 } = req.query;

        const where = { clientId };
        if (type) where.type = type;
        if (product) where.product = product;

        const events = await prisma.event.findMany({
            where,
            orderBy: {
                timestamp: 'desc'
            },
            take: parseInt(limit),
            skip: parseInt(offset)
        });

        res.json({
            success: true,
            events
        });
    } catch (error) {
        console.error('Error fetching events:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

/**
 * GET /api/events
 * Lấy danh sách events (tất cả hoặc filter)
 */
router.get('/', async (req, res) => {
    try {
        const { clientId, type, product, limit = 100, offset = 0 } = req.query;

        const where = {};
        if (clientId) where.clientId = clientId;
        if (type) where.type = type;
        if (product) where.product = product;

        const events = await prisma.event.findMany({
            where,
            orderBy: {
                timestamp: 'desc'
            },
            take: parseInt(limit),
            skip: parseInt(offset)
        });

        res.json({
            success: true,
            events
        });
    } catch (error) {
        console.error('Error fetching events:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

module.exports = router;

