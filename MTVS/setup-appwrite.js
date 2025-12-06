/**
 * Script tự động tạo Collections và Storage Buckets trong Appwrite
 * Chạy: node setup-appwrite.js
 */

const { Client, Databases, Storage, ID, Permission, Role } = require('node-appwrite');
const fs = require('fs');
const path = require('path');

// Đọc cấu hình từ AppConfig.json.example
const configPath = path.join(__dirname, 'AppConfig.json.example');
let config;

try {
    const configContent = fs.readFileSync(configPath, 'utf8');
    config = JSON.parse(configContent);
} catch (error) {
    console.error('❌ Không thể đọc file AppConfig.json.example:', error.message);
    process.exit(1);
}

const { Endpoint, ProjectId, ApiKey } = config.Appwrite;

if (!Endpoint || !ProjectId || !ApiKey) {
    console.error('❌ Thiếu thông tin cấu hình Appwrite trong AppConfig.json.example');
    process.exit(1);
}

// Khởi tạo Appwrite Client
const client = new Client()
    .setEndpoint(Endpoint)
    .setProject(ProjectId)
    .setKey(ApiKey);

const databases = new Databases(client);
const storage = new Storage(client);

const DATABASE_ID = 'main'; // Database ID mặc định

// Màu sắc cho console
const colors = {
    reset: '\x1b[0m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    red: '\x1b[31m',
    blue: '\x1b[34m',
    cyan: '\x1b[36m'
};

function log(message, color = 'reset') {
    console.log(`${colors[color]}${message}${colors.reset}`);
}

// Tạo Database nếu chưa có
async function createDatabase() {
    try {
        log('📦 Đang kiểm tra Database...', 'cyan');
        await databases.get(DATABASE_ID);
        log('✅ Database đã tồn tại', 'green');
    } catch (error) {
        if (error.code === 404) {
            log('📦 Đang tạo Database...', 'cyan');
            await databases.create(DATABASE_ID, 'Version Manager Database');
            log('✅ Đã tạo Database thành công', 'green');
        } else {
            throw error;
        }
    }
}

// Tạo Collection
async function createCollection(collectionId, name, attributes, indexes) {
    try {
        log(`📋 Đang kiểm tra Collection "${collectionId}"...`, 'cyan');
        await databases.getCollection(DATABASE_ID, collectionId);
        log(`✅ Collection "${collectionId}" đã tồn tại`, 'green');
    } catch (error) {
        if (error.code === 404) {
            log(`📋 Đang tạo Collection "${collectionId}"...`, 'cyan');
            await databases.createCollection(DATABASE_ID, collectionId, name);
            log(`✅ Đã tạo Collection "${collectionId}"`, 'green');
        } else {
            throw error;
        }
    }

    // Tạo Attributes
    for (const attr of attributes) {
        try {
            log(`  📝 Đang tạo attribute "${attr.key}"...`, 'yellow');
            
            const isRequired = attr.required || false;
            const isArray = attr.array || false;
            
            // Nếu required = true, không được truyền default
            // Nếu required = false, có thể truyền default = '' (empty string)
            if (isArray) {
                if (isRequired) {
                    // Required + Array: không có default
                    await databases.createStringAttribute(
                        DATABASE_ID,
                        collectionId,
                        attr.key,
                        attr.size || 255,
                        true,
                        true // array
                    );
                } else {
                    // Optional + Array: có default empty string
                    await databases.createStringAttribute(
                        DATABASE_ID,
                        collectionId,
                        attr.key,
                        attr.size || 255,
                        false,
                        true, // array
                        '' // default empty string
                    );
                }
            } else {
                if (isRequired) {
                    // Required + Not Array: không có default
                    await databases.createStringAttribute(
                        DATABASE_ID,
                        collectionId,
                        attr.key,
                        attr.size || 255,
                        true,
                        false // not array
                    );
                } else {
                    // Optional + Not Array: có default empty string
                    await databases.createStringAttribute(
                        DATABASE_ID,
                        collectionId,
                        attr.key,
                        attr.size || 255,
                        false,
                        false, // not array
                        '' // default empty string
                    );
                }
            }
            log(`  ✅ Đã tạo attribute "${attr.key}"`, 'green');
            // Đợi một chút để attribute được tạo xong
            await new Promise(resolve => setTimeout(resolve, 500));
        } catch (error) {
            if (error.code === 409 || error.message?.includes('already exists')) {
                log(`  ⚠️  Attribute "${attr.key}" đã tồn tại`, 'yellow');
            } else {
                log(`  ❌ Lỗi tạo attribute "${attr.key}": ${error.message}`, 'red');
            }
        }
    }

    // Đợi attributes được tạo xong trước khi tạo indexes
    log(`  ⏳ Đợi attributes được index hoàn toàn...`, 'yellow');
    await new Promise(resolve => setTimeout(resolve, 2000));

    // Tạo Indexes
    for (const index of indexes) {
        try {
            log(`  🔍 Đang tạo index "${index.key}"...`, 'yellow');
            const indexType = index.type === 'unique' ? 'unique' : 'key';
            await databases.createIndex(
                DATABASE_ID,
                collectionId,
                index.key,
                indexType,
                index.attributes || [index.key],
                index.orders || ['ASC']
            );
            log(`  ✅ Đã tạo index "${index.key}"`, 'green');
            await new Promise(resolve => setTimeout(resolve, 500));
        } catch (error) {
            if (error.code === 409 || error.message?.includes('already exists')) {
                log(`  ⚠️  Index "${index.key}" đã tồn tại`, 'yellow');
            } else {
                log(`  ❌ Lỗi tạo index "${index.key}": ${error.message}`, 'red');
            }
        }
    }
}

// Tạo Storage Bucket
async function createBucket(bucketId, name, config) {
    try {
        log(`🗂️  Đang kiểm tra Bucket "${bucketId}"...`, 'cyan');
        await storage.getBucket(bucketId);
        log(`✅ Bucket "${bucketId}" đã tồn tại`, 'green');
    } catch (error) {
        if (error.code === 404) {
            log(`🗂️  Đang tạo Bucket "${bucketId}"...`, 'cyan');
            try {
                await storage.createBucket(
                    bucketId,
                    name,
                    config.fileSecurity !== undefined ? config.fileSecurity : false,
                    config.allowedFileExtensions || [],
                    config.maxFileSize || 0,
                    config.compression || 'none',
                    config.encryption !== undefined ? config.encryption : false,
                    config.antivirus !== undefined ? config.antivirus : false
                );
                log(`✅ Đã tạo Bucket "${bucketId}"`, 'green');
            } catch (createError) {
                // Thử với API đơn giản hơn nếu có lỗi
                log(`  ⚠️  Thử tạo bucket với cấu hình đơn giản...`, 'yellow');
                await storage.createBucket(
                    bucketId,
                    name,
                    config.fileSecurity !== undefined ? config.fileSecurity : false,
                    config.allowedFileExtensions || [],
                    config.maxFileSize || 0
                );
                log(`✅ Đã tạo Bucket "${bucketId}" (cấu hình cơ bản)`, 'green');
            }
        } else {
            throw error;
        }
    }
}

// Main function
async function setup() {
    try {
        log('🚀 Bắt đầu setup Appwrite...\n', 'blue');

        // Tạo Database
        await createDatabase();
        log('');

        // Tạo Collections
        log('📚 Đang tạo Collections...\n', 'blue');

        // Collection: releases
        await createCollection('releases', 'Releases', [
            { key: 'product', required: true, size: 100 },
            { key: 'version', required: true, size: 50 },
            { key: 'channel', required: true, size: 20 },
            { key: 'manifestRef', required: true, size: 255 },
            { key: 'files', required: true, array: true, size: 255 },
            { key: 'os', required: true, size: 20 },
            { key: 'arch', required: true, size: 20 },
            { key: 'minVersion', required: false, size: 50 },
            { key: 'publishedAt', required: true, size: 255 }, // datetime as string
            { key: 'signedHash', required: true, size: 64 },
            { key: 'changelog', required: false, size: 5000 },
            { key: 'riskLevel', required: false, size: 20 },
            { key: 'constraints', required: false, size: 1000 }
        ], [
            { key: 'product_version', type: 'unique', attributes: ['product', 'version'] },
            { key: 'channel', attributes: ['channel'] },
            { key: 'publishedAt', attributes: ['publishedAt'], orders: ['DESC'] },
            { key: 'os_arch', attributes: ['os', 'arch'] }
        ]);
        log('');

        // Collection: clients
        await createCollection('clients', 'Clients', [
            { key: 'clientId', required: true, size: 255 },
            { key: 'product', required: true, size: 100 },
            { key: 'site', required: false, size: 100 },
            { key: 'tenant', required: false, size: 100 },
            { key: 'currentVersion', required: true, size: 50 },
            { key: 'os', required: true, size: 20 },
            { key: 'arch', required: true, size: 20 },
            { key: 'lastSeen', required: true, size: 255 }, // datetime as string
            { key: 'status', required: true, size: 20 },
            { key: 'updateChannel', required: true, size: 20 }
        ], [
            { key: 'clientId', type: 'unique', attributes: ['clientId'] },
            { key: 'product', attributes: ['product'] },
            { key: 'site', attributes: ['site'] },
            { key: 'lastSeen', attributes: ['lastSeen'] }
        ]);
        log('');

        // Collection: rolloutPolicies
        await createCollection('rolloutPolicies', 'Rollout Policies', [
            { key: 'releaseId', required: true, size: 255 },
            { key: 'product', required: true, size: 100 },
            { key: 'channel', required: true, size: 20 },
            { key: 'targetGroups', required: false, array: true, size: 100 },
            { key: 'timeWindow', required: false, size: 500 },
            { key: 'mode', required: true, size: 50 },
            { key: 'rolloutPercentage', required: false, size: 10 }
        ], [
            { key: 'releaseId', attributes: ['releaseId'] },
            { key: 'product_channel', attributes: ['product', 'channel'] }
        ]);
        log('');

        // Collection: backups
        await createCollection('backups', 'Backups', [
            { key: 'clientId', required: true, size: 255 },
            { key: 'backupId', required: true, size: 255 },
            { key: 'storageFileId', required: true, size: 255 },
            { key: 'product', required: true, size: 100 },
            { key: 'version', required: true, size: 50 },
            { key: 'size', required: true, size: 20 }, // integer as string
            { key: 'checksum', required: true, size: 64 },
            { key: 'createdAt', required: true, size: 255 }, // datetime as string
            { key: 'encrypted', required: true, size: 10 }, // boolean as string
            { key: 'metadata', required: false, size: 2000 }
        ], [
            { key: 'clientId', attributes: ['clientId'] },
            { key: 'createdAt', attributes: ['createdAt'], orders: ['DESC'] },
            { key: 'product_version', attributes: ['product', 'version'] }
        ]);
        log('');

        // Collection: events
        await createCollection('events', 'Events', [
            { key: 'clientId', required: true, size: 255 },
            { key: 'type', required: true, size: 50 },
            { key: 'product', required: true, size: 100 },
            { key: 'version', required: false, size: 50 },
            { key: 'payload', required: false, size: 5000 },
            { key: 'timestamp', required: true, size: 255 } // datetime as string
        ], [
            { key: 'clientId', attributes: ['clientId'] },
            { key: 'type', attributes: ['type'] },
            { key: 'timestamp', attributes: ['timestamp'], orders: ['DESC'] },
            { key: 'product', attributes: ['product'] }
        ]);
        log('');

        // Tạo Storage Buckets
        log('💾 Đang tạo Storage Buckets...\n', 'blue');

        // Bucket: artifacts
        await createBucket('artifacts', 'Artifacts', {
            fileSecurity: true,
            allowedFileExtensions: ['zip', '7z', 'tar.gz', 'exe', 'msi'],
            maxFileSize: 524288000, // 500MB
            compression: 'gzip'
        });
        log('');

        // Bucket: backups
        await createBucket('backups', 'Backups', {
            fileSecurity: true,
            allowedFileExtensions: ['zip', '7z', 'tar.gz'],
            maxFileSize: 1073741824, // 1GB
            compression: 'gzip',
            encryption: false
        });
        log('');

        // Bucket: manifests
        await createBucket('manifests', 'Manifests', {
            fileSecurity: true,
            allowedFileExtensions: ['json'],
            maxFileSize: 1048576, // 1MB
            compression: 'gzip'
        });
        log('');

        log('✅ Hoàn thành setup Appwrite!', 'green');
        log('\n📝 Lưu ý:', 'yellow');
        log('   - Các attributes datetime được lưu dưới dạng string (ISO format)', 'yellow');
        log('   - Các attributes boolean được lưu dưới dạng string ("true"/"false")', 'yellow');
        log('   - Các attributes integer được lưu dưới dạng string', 'yellow');
        log('   - Cần đợi vài giây để attributes được index hoàn toàn', 'yellow');

    } catch (error) {
        log(`\n❌ Lỗi: ${error.message}`, 'red');
        if (error.response) {
            log(`   Code: ${error.code}`, 'red');
            log(`   Response: ${JSON.stringify(error.response, null, 2)}`, 'red');
        }
        process.exit(1);
    }
}

// Chạy setup
setup();

