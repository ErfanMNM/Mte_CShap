# Mte Node OPC UA Server

OPC UA Server chạy bằng Node.js dùng [`node-opcua`](https://www.npmjs.com/package/node-opcua) (v2.175.1). Server publish các tag mô phỏng (simulated) theo file `taglist.json`, cho phép client kết nối `Anonymous` với `SecurityMode=None` / `SecurityPolicy=None`. Tương thích trực tiếp với client C# đã có ở `../TTManager/Communication/OPCUA/OpcUa_Client_Helper.cs`.

## Yêu cầu
- Node.js >= 20 (đã verify với v22.22.0)
- npm >= 10

## Cài đặt
```bash
cd DProject-Node-UPC-Server
npm install
```

## Chạy server
```bash
npm start
```
Mặc định lắng nghe tại `opc.tcp://127.0.0.1:4840/UA/MteServer`. Có thể override port bằng:
```bash
node src/server.js --port 4841
node src/server.js --taglist ./my-tags.json
```

## Test nhanh bằng Node client
Terminal 1:
```bash
npm start
```
Terminal 2:
```bash
npm test
```
Script `examples/quick-test-client.js` sẽ:
1. Kết nối tới server
2. Browse `Objects/MteServer`
3. Đọc giá trị hiện tại của tất cả tag
4. Thử ghi/đọc lại với tag `StartCommand` (ReadWrite)

## Cấu trúc thư mục
```
DProject-Node-UPC-Server/
├── package.json
├── .gitignore
├── taglist.json              # khai báo tag + simulator
├── src/
│   ├── server.js             # entry point
│   ├── addressSpace.js       # build OPC UA address space từ taglist
│   ├── simulators.js         # hàm sinh giá trị (static/counter/sine/random/ramp)
│   └── logger.js
└── examples/
    └── quick-test-client.js
```

## Định dạng `taglist.json`
```json
{
  "namespaceUri": "urn:mte:opcua:server",
  "objectsFolder": "MteServer",
  "tags": [
    {
      "name": "Temperature",
      "nodeId": "ns=2;s=Temperature",
      "dataType": "Double",
      "simulator": "sine",
      "min": 20.0,
      "max": 35.0,
      "periodMs": 1000,
      "accessLevel": "Read"
    }
  ]
}
```
Các kiểu `simulator` hỗ trợ:
- `static` — giá trị cố định, chấp nhận write từ client
- `counter` — đếm tăng theo `step` mỗi `periodMs`
- `sine` — dao động sine giữa `min` và `max` với chu kỳ `periodMs`
- `random` — uniform random trong `[min, max]`
- `ramp` — tăng tuyến tính từ `min` đến `max`, reset về `min`

`accessLevel`: `Read` (mặc định) hoặc `ReadWrite`.

`dataType`: `Boolean`, `SByte`, `Byte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Float`, `Double`, `String`, `DateTime`, `Guid`, `ByteString`.

## Sử dụng với C# client hiện có
```csharp
using TTManager.Communication.OPCUA;

var opcClient = new OpcUaClientHelper("opc.tcp://127.0.0.1:4840/UA/MteServer");
await opcClient.ConnectAsync();
var temp = await opcClient.ReadValueAsync("ns=2;s=Temperature");
await opcClient.WriteAsync("ns=2;s=StartCommand", true);
await opcClient.DisconnectAsync();
```

## Bảo mật
- `MessageSecurityMode.None`, `SecurityPolicy.None`
- Chỉ `Anonymous`
- Không có certificate / xác thực

Phù hợp cho môi trường phát triển, test, demo. Không dùng cho môi trường production có kết nối tới internet.

## Giới hạn
- Không hỗ trợ subscribe/monitor realtime (cần `ClientSubscription` riêng).
- Không lưu lịch sử (History Server).
- Không hỗ trợ Alarms & Conditions.
- Không giao tiếp PLC/device thật — chỉ publish giá trị mô phỏng.

## License
ISC
