# EnterpriseITServiceDesk

# Enterprise IT Service Desk & Asset Management System

A modern enterprise IT service desk and asset management platform built with ASP.NET Core, Angular and PostgreSQL.

## Technologies

- ASP.NET Core 9
- Angular
- PostgreSQL
- Entity Framework Core
- Docker

## Giriş ve yetkilendirme

- API, JWT Bearer kullanır. `POST /api/Auth/login` e-posta ve parola ile 30 dakika geçerli token döndürür.
- Angular token'ı yalnızca mevcut sekmenin `sessionStorage` alanında tutar; parolayı saklamaz. Sayfa yenilenince `/api/Auth/me` ile kullanıcı ve rol doğrulanır.
- `POST /api/Auth/logout` mevcut token'ı sunucuda iptal eder. Hesap pasifleştirme, rol veya parola değişikliği de sonraki istekte oturumu geçersiz kılar.
- Oturumlar tek API sürecine aittir; API yeniden başlatıldığında tekrar giriş gerekir. Birden çok API örneği için ortak oturum/iptal deposu gerekir.
- Geliştirmede JWT anahtarı bellekte rastgele üretilir. Üretimde en az 32 baytlık rastgele `Jwt__SigningKey` ortam değişkeni ve HTTPS zorunludur. Anahtarı kaynak koduna yazmayın. İsteğe bağlı `Jwt__Issuer` ve `Jwt__Audience` ayarlanabilir.
- Swagger'da login yanıtındaki `accessToken`, **Authorize** alanına girilebilir.

### Mevcut erişim sınırları

| Rol | Erişim |
| --- | --- |
| Admin | Dashboard ve tüm mevcut API modülleri |
| Technician | Dashboard, demirbaşlar, tür listesi, zimmet, bakım, ticket, yorum, ek ve bildirim işlemleri |
| Employee | Dashboard ve kendi oturum bilgileri |

Kullanıcı, rol, departman, ticket sözlükleri ve audit yönetimi Admin'e özeldir. Employee için kendi ticket'larına erişim, sahiplik kontrolüyle ayrıca geliştirilecektir. Frontend menü gizleme dışında API tarafında da yetki kontrolü uygulanır.

### İlk Admin parolası

Yalnızca açık onayla ve Development ortamında: `BOOTSTRAP_ADMIN_PASSWORD` ortam değişkenine 12–128 karakterlik güçlü parola verilip `dotnet run --project EnterpriseIT.API -- --initialize-admin-password` çalıştırılabilir. Komut yalnızca tek aktif Admin bulunduğunda ve parola alanı boşken işlem yapar; mevcut parolayı sıfırlamaz. Ortam değişkenini işlemden sonra temizleyin. Otomatik seed veya migration çalıştırılmaz.

Yeni kullanıcılar Admin yetkisiyle oluşturulur. Parolalar ASP.NET Core `PasswordHasher` ile hashlenir; kullanıcı güncellemesinde parola gönderilmezse mevcut hash korunur. İlk parolayı değiştirmek için Admin, kullanıcı güncelleme endpoint'ini kullanabilir.

JWT doğrulaması [Microsoft belgesindeki](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication) imza, issuer, audience ve süre kontrollerini uygular.
