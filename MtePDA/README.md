# MtePDA - Android App cho PDA Mobydata

App Android (Kotlin) tich hop Datalogic ScanSDK 5.0 de quet barcode/QR, tu dong gui ma ve TTManager.

## Cau Truc Project

```
MtePDA/
├── app/
│   ├── build.gradle              # App-level build config (AGP 4.2.2)
│   ├── proguard-rules.pro
│   └── src/main/
│       ├── AndroidManifest.xml   # Permissions + ScanSDK library
│       ├── java/com/ttmanager/pda/
│       │   ├── MainActivity.kt   # Main UI + scan handling
│       │   ├── ScanManager.kt    # Datalogic ScanSDK integration
│       │   ├── ApiClient.kt     # Retrofit API client
│       │   └── ScanAdapter.kt   # RecyclerView adapter
│       ├── res/
│       │   ├── layout/          # activity_main.xml, item_scan_history.xml
│       │   ├── drawable/        # Icons + backgrounds
│       │   ├── values/          # colors, strings, dimens, themes
│       │   └── xml/             # network_security_config.xml
│       └── libs/
│           └── ScanSDK5.0.jar   # Datalogic scanning SDK
├── build.gradle                  # Root build (AGP 4.2.2, Kotlin 1.5.32)
├── settings.gradle
├── gradle.properties            # AndroidX enabled
├── gradle/wrapper/
│   ├── gradle-wrapper.jar
│   └── gradle-wrapper.properties (Gradle 7.2)
└── gradlew / gradlew.bat
```

## Yeu Cau He Thong

- **Android SDK** (minSdk 24, targetSdk 34)
- **JDK 11+** (JDK 8 chi du cho AGP 4.x)
- **Android Studio** (de import va build)

## Setup & Build

### 1. Cai Android SDK

Neu chua co Android SDK, tai Android Studio:
- https://developer.android.com/studio
- Sau khi cai Android Studio, SDK se tu dong duoc cai dat

Hoac cai SDK truc tiep:
- Tai Android Command Line Tools: https://developer.android.com/studio#command-line-tools-only
- Giai nen vao `C:\Users\THUC\AppData\Local\Android\Sdk`
- Chay: `sdkmanager "platforms;android-34" "build-tools;34.0.0" "platform-tools"`

### 2. Import vao Android Studio

1. File > Open > Chon thu muc `MtePDA`
2. Android Studio tu dong nhan dien gradle project
3. Cho phep sync Gradle (neu duoc yeu cau)

### 3. Build APK

```bash
# Bang Android Studio:
# Build > Build Bundle(s) / APK(s) > Build APK

# Hoac bang command line:
cd MtePDA
.\gradlew.bat assembleDebug
```

APK dau ra: `app/build/outputs/apk/debug/app-debug.apk`

## Tinh nang

| Tinh nang | Mo ta |
|-----------|-------|
| Quet barcode/QR | Datalogic ScanSDK 5.0 (hardware scanner tren PDA Mobydata) |
| Gui ma ve TTManager | POST /api/scan len port 6969 |
| Nhap tay ma Code | Text input + nut GUI |
| Phim tat nhanh | QC-OK, QC-NG, START, STOP |
| Lich su quet | Hien thi trong RecyclerView, max 50 items |
| Trang thai server | Kiem tra health endpoint moi 10s |
| Luu cau hinh | Server IP + PDA Name luu vao SharedPreferences |

## Su dung

1. Cai APK len PDA Mobydata
2. Nhap IP cua may chay TTManager (VD: `192.168.1.100`)
3. Nhap ten PDA (VD: `PDA-KHO-01`)
4. Bam nut **QUET** de kich hoat scanner
5. Quet barcode/QR - ma tu dong gui ve TTManager
6. Hoac nhap tay va bam **GUI**

## Tich hop ScanSDK

App su dung Datalogic ScanSDK 5.0 voi `com.android.device` system library:

```kotlin
// ScanManager.kt
val barcodeManager = BarcodeManager()
barcodeManager.addReadListener(object : ReadListener {
    override fun onRead(result: DecodeResult?) {
        val code = result?.text ?: return
        // Gui ve server...
    }
})
barcodeManager.startDecode(30000)  // 30s timeout
barcodeManager.stopDecode()
barcodeManager.release()
```

## API Endpoint

```
POST http://<IP_TTManager>:6969/api/scan
Content-Type: application/json

{
  "code": "ABC123456",
  "pdaName": "PDA-KHO-01"
}

Response:
{ "success": true, "message": "Scan received." }
```

## loi thuong gap

### SDK location not found
Set ANDROID_HOME:
```powershell
$env:ANDROID_HOME = "C:\Users\THUC\AppData\Local\Android\Sdk"
```

### gradle-wrapper.jar loi
Copy tu project ScanSdkDemo:
```
ScanSdkDemo/ScanSdkSample/ScanSdkSample/gradle/wrapper/gradle-wrapper.jar
```

### Java version conflict
AGP 4.2.2 ho tro JDK 8-16. Neu gap loi, dam bao JAVA_HOME tro toi JDK 11+.

## Note

- App chi chay duoc tren **PDA Mobydata** (co hardware scanner Datalogic)
- Khong chay duoc tren emulator thong thuong (thieu `com.android.device` library)
- TTManager phai dang chay va API server (port 6969) phai co san
