## Monorepo: Auth API, Web, Mobile

Backend: ASP.NET Core Web API (JWT + Refresh Token, rol bazlı yetki)
Web: Angular 17 (Material, HTTP interceptors, alan bazlı doğrulama/hata)
Mobil: Flutter (Dio, Shared Preferences, token yenileme ve doğrulama)

### Özellikler
- Kullanıcı: register, login, logout, token yenileme
- JWT + Refresh Token, Roller: User / Admin
- Tutarlı hata şeması: `{ message, errors: [{ field, messages[] }] }`
- Web ve mobilde alan-bazlı validasyon ve sunucu hatası gösterimi

## Teknolojiler
- Backend: .NET 9, EF Core (SQLite), JWT
- Web: Angular 17, Angular Material, RxJS
- Mobil: Flutter, Dio, Shared Preferences

## Hızlı Başlangıç

### Backend (API)
- Varsayılan URL: https://localhost:7128

```powershell
cd "authApi/src/WebApi"
dotnet dev-certs https --trust
dotnet restore
dotnet run
```

### Web (Angular)
- Varsayılan URL: http://localhost:4200
- API adresi: `webapp/src/environments/environment.ts` → `apiUrl: 'https://localhost:7128'`

```powershell
cd "webapp"
npm install
npm start
```

### Mobil (Flutter)
- API adresi platforma göre `mobile/lib/src/core/env.dart` üzerinden gelir
- Android emülatörü: `https://10.0.2.2:7128`

```powershell
cd "mobile"
flutter pub get
flutter run -d windows
```

## Doğrulama Kuralları
- Email: geçerli format ve domain noktası zorunlu (ör. `user@example.com`)
- Kullanıcı adı: 3–32 karakter, `A–Z a–z 0–9 . _ -`; email olamaz
- Şifre: 8–128; en az bir büyük, bir küçük, bir rakam, bir sembol; boşluk yok

## Hata Yönetimi
- Backend: Global hata yakalama ile 400/401/404/409 gibi uygun kodlarla JSON döner; API durmaz
- Web/Mobil:
  - Alan bazlı hatalar input altında gösterilir
  - 401 için (non-auth isteklerde) otomatik refresh; başarısızsa yerel oturumu temizler ve girişe yönlendirir

## Depo Yapısı
```text
authApi/     # ASP.NET Core Web API
webapp/      # Angular 17 frontend
mobile/      # Flutter uygulaması
```

## Notlar
- CORS, `http://localhost:4200` için açıktır.
- Port/host değiştirirseniz Angular `environment.ts` ve Flutter `env.dart` değerlerini güncelleyin.
- Geliştirmede self-signed HTTPS sertifikası için OS/tarayıcı onayı gerekebilir.


