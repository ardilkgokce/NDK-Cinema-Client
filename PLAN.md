# Quest 3 Migration Plan - NDK Cinema Client

Bu plan, PICO tabanlı VR Cinema uygulamasının Meta Quest 3'e taşınması için gerekli adımları içerir.

## Mevcut Durum Analizi

### PICO'ya Bağımlılıklar
- **PICO XR SDK 2.3.0** (`com.unity.xr.picoxr`) - manifest.json'da tanımlı
- **XR Loader:** `Assets/XR/Loaders/PXR_Loader.asset`
- **XR Settings:** `Assets/XR/Settings/PXR_Settings.asset`
- **Input:** `KeyCode.Joystick1Button0` (PICO controller)
- **Dosya Yolu:** `file:///storage/emulated/0/Movies/` (Android)

### Platform-Agnostik Kodlar (Değişiklik Gerektirmeyen)
- UDP ağ iletişimi (PicoCommunicator.cs)
- Video oynatma mantığı (VideoManager.cs)
- İstatistik yönetimi (TicketsManager.cs)
- DOTween animasyonları
- Skybox/RenderTexture sistemi

---

## Geçiş Planı

### Faz 1: Unity ve SDK Hazırlığı

#### 1.1 Unity Sürümünü Güncelle
- [ ] Unity 2020.3.48f1 → Unity 2021.3+ LTS veya 2022.3+ LTS
- Quest 3 için en az Unity 2021.3 gerekli
- Önerilen: Unity 2022.3 LTS (Quest 3 tam destek)

#### 1.2 Meta XR SDK Kurulumu
- [ ] PICO SDK'yı kaldır: `Packages/manifest.json` içinden `com.unity.xr.picoxr` satırını sil
- [ ] Meta XR All-in-One SDK ekle (Unity Asset Store veya Meta Developer Hub)
- [ ] Alternatif: Oculus Integration Package

#### 1.3 XR Management Güncelle
- [ ] `com.unity.xr.management` 4.4.0 → 4.5+ güncelle
- [ ] Oculus XR Plugin ekle: `com.unity.xr.oculus`

---

### Faz 2: Proje Ayarları Değişiklikleri

#### 2.1 Build Settings
- [ ] Platform: Android (değişmez)
- [ ] Texture Compression: ASTC (Quest için optimize)
- [ ] Minimum API Level: Android 10 (API 29) → Android 12 (API 32) Quest 3 için

#### 2.2 Player Settings Güncellemeleri
```
Company Name: NDK
Product Name: NDKCinemaClient (değişmez)
Package Name: com.NDK.NDKCinemaClient (değişmez veya com.NDK.NDKCinemaQuest)
Minimum SDK: 32 (Quest 3)
Target SDK: 32+
Scripting Backend: IL2CPP (değişmez)
Target Architectures: ARM64 (sadece)
```

#### 2.3 XR Plugin Management
- [ ] `Assets/XR/Loaders/` altındaki PICO loader'ları sil
- [ ] `Assets/XR/Settings/` altındaki PICO ayarlarını sil
- [ ] Edit → Project Settings → XR Plug-in Management
- [ ] Android sekmesinde "Oculus" seç
- [ ] Oculus settings: Quest 3 hedef cihaz olarak işaretle

#### 2.4 Quality Settings
- [ ] VR için optimize render ayarları
- [ ] Fixed Foveated Rendering etkinleştir (Quest 3)
- [ ] Eye Tracked Foveated Rendering (opsiyonel)

---

### Faz 3: Kod Değişiklikleri

#### 3.1 PicoCommunicator.cs → QuestCommunicator.cs (İsim Değişikliği Opsiyonel)
```csharp
// Mevcut (Satır 87-89):
if (Input.GetKeyDown(KeyCode.Joystick1Button0))
{
    DataDispatched("Audio_Request");
}

// Yeni (OVRInput kullanarak):
if (OVRInput.GetDown(OVRInput.Button.One) ||
    OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
{
    DataDispatched("Audio_Request");
}
```

#### 3.2 TicketsManager.cs Input Değişikliği
```csharp
// Mevcut (Triple-click için):
if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Joystick1Button0))

// Yeni:
if (Input.GetKeyDown(KeyCode.T) || OVRInput.GetDown(OVRInput.Button.Two))
```

#### 3.3 VideoManager.cs - Dosya Yolu Kontrolü
```csharp
// Mevcut yol Quest'te de çalışır:
file:///storage/emulated/0/Movies/Movie_*.mp4

// Alternatif (Quest için önerilen):
Application.persistentDataPath + "/Movies/Movie_*.mp4
// veya
/sdcard/Movies/Movie_*.mp4
```

#### 3.4 Yeni Script: OVRInputBridge.cs (Opsiyonel - Soyutlama için)
```csharp
public static class VRInputBridge
{
    public static bool GetPrimaryButtonDown()
    {
        #if OCULUS_XR
        return OVRInput.GetDown(OVRInput.Button.One);
        #elif PICO_XR
        return Input.GetKeyDown(KeyCode.Joystick1Button0);
        #else
        return Input.GetKeyDown(KeyCode.Space);
        #endif
    }
}
```

---

### Faz 4: Manifest ve İzinler

#### 4.1 AndroidManifest.xml Güncellemesi
- [ ] `Assets/Plugins/Android/AndroidManifest.xml` güncelle
- [ ] PICO'ya özgü izinleri kaldır
- [ ] Meta/Oculus izinlerini ekle:

```xml
<!-- Quest için gerekli -->
<uses-feature android:name="android.hardware.vr.headtracking" android:required="true" />
<uses-feature android:name="com.oculus.feature.PASSTHROUGH" android:required="false" />

<!-- Depolama izni (video dosyaları için) -->
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />

<!-- Ağ izni (UDP için) -->
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
```

#### 4.2 Meta Quest Store Gereksinimleri (Opsiyonel)
- [ ] App ID oluştur (Meta Developer Dashboard)
- [ ] Oculus Platform SDK entegrasyonu

---

### Faz 5: Sahne ve Prefab Güncellemeleri

#### 5.1 XR Rig Değişikliği
- [ ] PICO XR rig'i kaldır (varsa)
- [ ] OVRCameraRig veya XR Origin ekle
- [ ] Kamera ayarlarını Quest 3 için optimize et

#### 5.2 Controller Model (Opsiyonel)
- [ ] Quest 3 Touch Plus controller modelleri ekle
- [ ] Hand tracking desteği (opsiyonel)

---

### Faz 6: Test ve Optimizasyon

#### 6.1 Editor Testi
- [ ] Oculus Link ile Quest 3 bağlantısı
- [ ] Play Mode'da test

#### 6.2 Build ve Deploy
- [ ] Development Build oluştur
- [ ] ADB ile Quest 3'e yükle: `adb install -r app.apk`
- [ ] Video dosyalarını cihaza kopyala: `adb push Movies/ /sdcard/Movies/`

#### 6.3 Performans Optimizasyonu
- [ ] 72Hz/90Hz/120Hz test
- [ ] GPU Profiler ile analiz
- [ ] Fixed Foveated Rendering seviyesi ayarla

---

## Dosya Değişiklik Özeti

| Dosya | İşlem | Açıklama |
|-------|-------|----------|
| `Packages/manifest.json` | Düzenle | PICO SDK çıkar, Oculus XR ekle |
| `ProjectSettings/ProjectSettings.asset` | Düzenle | XR ve SDK ayarları |
| `Assets/XR/*` | Sil/Yeniden Oluştur | PICO loader → Oculus loader |
| `Assets/Scripts/PicoCommunicator.cs` | Düzenle | Input kodlarını güncelle |
| `Assets/Scripts/TicketsManager.cs` | Düzenle | Input kodlarını güncelle |
| `Assets/Plugins/Android/AndroidManifest.xml` | Düzenle | İzinler ve özellikler |
| `Assets/Scenes/SampleScene.unity` | Düzenle | XR Rig değişikliği |

---

## Risk ve Dikkat Edilecekler

1. **Video Codec Uyumluluğu:** Quest 3 H.264/H.265 destekler, mevcut .mp4 dosyaları çalışmalı
2. **Dosya Yolu İzinleri:** Android 12+ için Scoped Storage kısıtlamaları
3. **UDP Port:** 4242 portu Quest'te de açık olmalı (firewall)
4. **IP Adresi:** 192.168.70.150 sabit IP, Quest aynı ağda olmalı

---

## Tahmini Efor

| Faz | Açıklama | Karmaşıklık |
|-----|----------|-------------|
| Faz 1 | Unity/SDK Kurulumu | Düşük |
| Faz 2 | Proje Ayarları | Düşük |
| Faz 3 | Kod Değişiklikleri | Orta |
| Faz 4 | Manifest | Düşük |
| Faz 5 | Sahne Güncellemeleri | Orta |
| Faz 6 | Test | Orta |

---

## Sonraki Adımlar

1. Bu planı onaylayın
2. Unity sürümünü güncelleyin
3. Meta XR SDK'yı indirin ve kurun
4. Kod değişikliklerini uygulayın
5. Test edin
