const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const path = require('path');
require('dotenv').config();

const { testConnection, disconnect } = require('./config/database');
const googleDriveService = require('./services/googleDriveService');

// Import routes
const releasesRouter = require('./routes/releases');
const clientsRouter = require('./routes/clients');
const backupsRouter = require('./routes/backups');
const eventsRouter = require('./routes/events');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(helmet({
    contentSecurityPolicy: false // Allow inline scripts for admin panel
}));
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Serve static files (admin panel)
app.use(express.static(path.join(__dirname, 'public')));

// Rate limiting
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 100 // limit each IP to 100 requests per windowMs
});
app.use('/api/', limiter);

// Health check
app.get('/health', async (req, res) => {
    const dbStatus = await testConnection();
    res.json({
        status: 'ok',
        database: dbStatus ? 'connected' : 'disconnected',
        timestamp: new Date().toISOString()
    });
});

// API Routes
app.use('/api/releases', releasesRouter);
app.use('/api/clients', clientsRouter);
app.use('/api/backups', backupsRouter);
app.use('/api/events', eventsRouter);

// Error handling middleware
app.use((err, req, res, next) => {
    console.error('Error:', err);
    res.status(err.status || 500).json({
        success: false,
        error: err.message || 'Internal server error'
    });
});

// Serve admin panel for root and other non-API routes
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// 404 handler for API routes
app.use('/api/*', (req, res) => {
    res.status(404).json({
        success: false,
        error: 'Route not found'
    });
});

// Serve admin panel for other routes
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Initialize services and start server
async function startServer() {
    try {
        // Test database connection
        console.log('🔌 Testing Prisma connection...');
        const dbConnected = await testConnection();
        if (!dbConnected) {
            console.error('❌ Database connection failed. Please check your configuration.');
            process.exit(1);
        }

        // Initialize Google Drive service
        console.log('🔌 Initializing Google Drive service...');
        await googleDriveService.initialize();

        // Start server
        app.listen(PORT, () => {
            console.log(`\n✅ MTVS Backend Server is running on port ${PORT}`);
            console.log(`📡 API Base URL: http://localhost:${PORT}/api`);
            console.log(`🏥 Health Check: http://localhost:${PORT}/health`);
            console.log(`🎨 Admin Panel: http://localhost:${PORT}/`);
            console.log(`🗄️  Using Prisma ORM for database\n`);
        });
    } catch (error) {
        console.error('❌ Failed to start server:', error);
        process.exit(1);
    }
}

// Graceful shutdown
process.on('SIGINT', async () => {
    console.log('\n🛑 Shutting down gracefully...');
    await disconnect();
    process.exit(0);
});

process.on('SIGTERM', async () => {
    console.log('\n🛑 Shutting down gracefully...');
    await disconnect();
    process.exit(0);
});

startServer();

