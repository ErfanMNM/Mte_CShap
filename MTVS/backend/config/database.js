const { PrismaClient } = require('@prisma/client');
require('dotenv').config();

const prisma = new PrismaClient({
    log: process.env.NODE_ENV === 'development' 
        ? ['query', 'error', 'warn'] 
        : ['error'],
});

// Test connection
async function testConnection() {
    try {
        await prisma.$connect();
        console.log('✅ Prisma connection successful');
        return true;
    } catch (error) {
        console.error('❌ Prisma connection failed:', error.message);
        return false;
    }
}

// Graceful shutdown
async function disconnect() {
    await prisma.$disconnect();
}

// Handle process termination
process.on('beforeExit', async () => {
    await disconnect();
});

process.on('SIGINT', async () => {
    await disconnect();
    process.exit(0);
});

process.on('SIGTERM', async () => {
    await disconnect();
    process.exit(0);
});

module.exports = {
    prisma,
    testConnection,
    disconnect
};

