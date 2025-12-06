# Prisma Guide cho MTVS Backend

Hướng dẫn sử dụng Prisma ORM trong MTVS Backend.

## Tổng quan

Prisma là một ORM (Object-Relational Mapping) hiện đại cho Node.js, cung cấp:
- ✅ Type-safe database client
- ✅ Auto-completion trong IDE
- ✅ Migration management
- ✅ Database introspection
- ✅ Prisma Studio (GUI để xem/edit data)

## Cài đặt

Prisma đã được thêm vào `package.json`. Cài đặt dependencies:

```bash
npm install
```

## Cấu hình

### 1. DATABASE_URL trong .env

Thêm hoặc cập nhật `DATABASE_URL` trong file `.env`:

```env
DATABASE_URL="mysql://user:password@localhost:3306/mtvs_db"
```

Hoặc nếu đã có các biến riêng lẻ:

```env
DATABASE_URL="mysql://${DB_USER}:${DB_PASSWORD}@${DB_HOST}:${DB_PORT}/${DB_NAME}"
```

### 2. Generate Prisma Client

Sau khi cài đặt dependencies, generate Prisma Client:

```bash
npm run prisma:generate
# hoặc
npx prisma generate
```

## Migration

### Tạo migration mới

Khi thay đổi schema (file `prisma/schema.prisma`):

```bash
npm run prisma:migrate
# hoặc
npx prisma migrate dev --name migration_name
```

Lệnh này sẽ:
1. Tạo migration file mới
2. Apply migration vào database
3. Generate Prisma Client mới

### Deploy migrations (Production)

Trong production, chỉ deploy migrations đã có:

```bash
npm run prisma:deploy
# hoặc
npx prisma migrate deploy
```

### Reset database (Development)

⚠️ **CẢNH BÁO**: Xóa toàn bộ data!

```bash
npx prisma migrate reset
```

## Sử dụng Prisma Client

### Import Prisma Client

```javascript
const { prisma } = require('./config/database');
```

### Ví dụ queries

**Create:**
```javascript
const release = await prisma.release.create({
    data: {
        product: 'MTVS',
        version: '1.0.0',
        channel: 'stable',
        os: 'windows',
        arch: 'x64',
        signedHash: 'abc123...'
    }
});
```

**Find:**
```javascript
// Find unique
const client = await prisma.client.findUnique({
    where: { clientId: 'client-001' }
});

// Find first
const latestRelease = await prisma.release.findFirst({
    where: { product: 'MTVS', channel: 'stable' },
    orderBy: { publishedAt: 'desc' }
});

// Find many
const releases = await prisma.release.findMany({
    where: { product: 'MTVS' },
    take: 10,
    skip: 0
});
```

**Update:**
```javascript
const client = await prisma.client.update({
    where: { clientId: 'client-001' },
    data: { status: 'active', lastSeen: new Date() }
});
```

**Upsert (create or update):**
```javascript
const client = await prisma.client.upsert({
    where: { clientId: 'client-001' },
    update: { lastSeen: new Date() },
    create: {
        clientId: 'client-001',
        product: 'MTVS',
        currentVersion: '1.0.0',
        os: 'windows',
        arch: 'x64'
    }
});
```

**Delete:**
```javascript
await prisma.backup.delete({
    where: { backupId: 'backup-123' }
});
```

**Relations:**
```javascript
// Include related data
const release = await prisma.release.findUnique({
    where: { id: 1 },
    include: {
        rolloutPolicies: true,
        backups: true
    }
});
```

## Prisma Studio

GUI để xem và edit database:

```bash
npm run prisma:studio
# hoặc
npx prisma studio
```

Mở browser tại `http://localhost:5555`

## Schema Changes

### Thay đổi schema

1. Edit file `prisma/schema.prisma`
2. Tạo migration:
   ```bash
   npm run prisma:migrate
   ```
3. Prisma sẽ tự động generate client mới

### Ví dụ thêm field mới

```prisma
model Release {
  // ... existing fields
  newField String? @db.VarChar(100)  // Thêm field mới
}
```

Sau đó chạy migration.

## Best Practices

1. **Luôn tạo migration khi thay đổi schema**
   - Không edit database trực tiếp
   - Dùng migrations để version control

2. **Sử dụng transactions cho operations phức tạp**
   ```javascript
   await prisma.$transaction(async (tx) => {
       await tx.release.create({ ... });
       await tx.client.update({ ... });
   });
   ```

3. **Handle errors đúng cách**
   ```javascript
   try {
       const client = await prisma.client.findUnique({ ... });
   } catch (error) {
       if (error.code === 'P2025') {
           // Record not found
       }
   }
   ```

4. **Sử dụng select để chỉ lấy fields cần thiết**
   ```javascript
   const release = await prisma.release.findUnique({
       where: { id: 1 },
       select: { id: true, version: true, product: true }
   });
   ```

## Troubleshooting

### Lỗi: "Prisma Client has not been generated yet"

Chạy:
```bash
npm run prisma:generate
```

### Lỗi: "Migration failed"

1. Kiểm tra DATABASE_URL trong .env
2. Kiểm tra database đã tồn tại chưa
3. Kiểm tra quyền user MySQL

### Reset và tạo lại từ đầu

```bash
npx prisma migrate reset
npm run prisma:generate
```

## So sánh với Raw SQL

### Trước (Raw SQL):
```javascript
const [rows] = await pool.execute(
    'SELECT * FROM releases WHERE product = ? AND channel = ?',
    [product, channel]
);
```

### Sau (Prisma):
```javascript
const releases = await prisma.release.findMany({
    where: { product, channel }
});
```

**Lợi ích:**
- ✅ Type-safe
- ✅ Auto-completion
- ✅ Dễ đọc hơn
- ✅ Ít lỗi hơn
- ✅ Dễ maintain

## Tài liệu tham khảo

- [Prisma Docs](https://www.prisma.io/docs)
- [Prisma Client API](https://www.prisma.io/docs/reference/api-reference/prisma-client-reference)
- [Prisma Migrate](https://www.prisma.io/docs/concepts/components/prisma-migrate)

