/**
 * Setup Database với Prisma
 * Chạy: npm run setup-db
 * 
 * Script này sẽ:
 * 1. Generate Prisma Client
 * 2. Deploy migrations (tạo database và tables)
 */

const { execSync } = require('child_process');
const path = require('path');

console.log('🚀 Starting database setup with Prisma...\n');

try {
    // 1. Generate Prisma Client
    console.log('📦 Generating Prisma Client...');
    execSync('npx prisma generate', { stdio: 'inherit', cwd: __dirname + '/..' });
    console.log('✅ Prisma Client generated\n');

    // 2. Deploy migrations
    console.log('🗄️  Deploying database migrations...');
    execSync('npx prisma migrate deploy', { stdio: 'inherit', cwd: __dirname + '/..' });
    console.log('✅ Migrations deployed\n');

    console.log('✅ Database setup completed successfully!');
    console.log('\n💡 Next steps:');
    console.log('   - Run "npm run prisma:studio" to open Prisma Studio');
    console.log('   - Run "npm start" to start the server');
} catch (error) {
    console.error('❌ Error setting up database:', error.message);
    process.exit(1);
}

