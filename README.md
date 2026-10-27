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
| Employee | Dashboard, kendi oturum bilgileri, kendi ticket'larını listeleme ve oluşturma |

Kullanıcı, rol, departman, ticket sözlükleri ve audit yönetimi Admin'e özeldir. Frontend menü gizleme dışında API tarafında da yetki kontrolü uygulanır.

### Ticket listeleme ve oluşturma

`/dashboard/tickets` sayfası mevcut talepleri listeler ve yeni ticket oluşturur. Kategori, öncelik ve isteğe bağlı demirbaş seçenekleri `/api/Ticket/form-options` üzerinden gelir; Employee yalnızca aktif olarak kendisine zimmetli demirbaşları seçebilir. Demirbaş seçmeden de talep açılabilir.

Ticket sahibi giriş yapan kullanıcıdan, başlangıç durumu `Open` tanımından ve oluşturulma zamanı sunucudan belirlenir. Employee liste ve ID ile sorgulamada yalnızca kendi ticket'larına erişir; başkasının ticket'ı için 404 döner.

### Ticket çalışma listeleri

Admin ve Technician, Ticket ekranında **Tümü**, **Bana atananlar** ve **Atanmamışlar** listeleri arasında geçiş yapar. Employee yalnızca **Taleplerim** listesini görür. API `GET /api/Ticket?view=all|assigned|unassigned|created` parametresini destekler; kullanıcı kimliği JWT'den alınır ve filtreler PostgreSQL sorgusuna uygulanır. Employee'nin diğer çalışma listelerine erişimi 403 ile reddedilir. Parametresiz istek Employee için kendi taleplerini, diğer roller için tüm ticket'ları döndürür. Listeler durumdan bağımsızdır; durum filtresi eklenmemiştir.

### Ticket detayı ve atama işlemleri

Listedeki ticket başlığı `/dashboard/tickets/:id` detay ekranını açar. Admin aktif bir Technician hesabına atama/yeniden atama yapabilir; Technician atanmamış, açık bir ticket'ı **Üzerime al** ile kendisine atayabilir. Employee kendi ticket detayını salt okunur görür. Kapalı ticket atanamaz.

API, atanan kullanıcı ve `UpdatedAt` değerini koşul olarak kullanıp tek SQL güncellemesi yapar; eşzamanlı değişiklikte 409 döner ve ekran güncel kaydı yükler. Claim işlemi kullanıcı ID'sini JWT'den alır. Genel güncelleme endpoint'i atama değiştiremez ve Technician yalnızca kendisine atanmış ticket'ı güncelleyebilir; bu güncelleme de eşzamanlı atamayı ezmez.

Detay ekranında durum akışı **Open → In Progress → Closed** şeklindedir. İşe başlamak için aktif bir teknisyene atama gerekir. Admin tüm ticket'larda, Technician yalnızca kendisine atanmış ticket'larda işlem yapabilir. Kapanış tarihi sunucuda kaydedilir; kapalı ticket yeniden açılamaz. Durum değişikliği de sürüm ve atama kontrolüyle atomik uygulanır; genel güncelleme endpoint'i bu akışı aşamaz.

### Yerel bağlantı bilgileri ve gizli anahtarlar

Veritabanı parolası ve JWT/API anahtarları Git'e yazılmamalıdır. Development ortamında API, proje dosyasındaki UserSecretsId üzerinden .NET User Secrets deposunu otomatik okur. Yerel bağlantıyı `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<yerel bağlantı diziniz>" --project EnterpriseIT.API` komutuyla tanımlayın. User Secrets dosyaları kullanıcı profilinde tutulur, depoya dahil değildir. Diğer bilgisayarlarda bu ayarın ayrıca yapılması gerekir.

Canlı ortamda bağlantıyı `ConnectionStrings__DefaultConnection`, JWT anahtarını `Jwt__SigningKey` ortam değişkenleriyle veya deployment secret deposuyla verin. Ortam dosyaları, özel sertifikalar ve yüklenen ekler Git dışında tutulur. Önceki commitlerde paylaşılmış parolalar yapılandırmadan çıkarılınca Git geçmişinden silinmiş olmaz; ilgili parola/anahtar ayrıca değiştirilmelidir.

### Dashboard

`/dashboard`, `GET /api/Dashboard` üzerinden gerçek ticket özetini gösterir. Admin tüm ticket'ları, Technician kendisine atananları, Employee yalnızca kendi oluşturduklarını görür. Kullanıcı ve rol JWT'den alınır; istemci farklı kullanıcı/kapsam seçemez. Toplam, Open, In Progress, Closed sayıları; durum dağılımı, aktif ticket'ların öncelikleri ve en yeni beş kayıt gösterilir. Eski/özel durumlar Diğer altında korunur; ClosedAt bulunan kayıtlar kapalı sayılır.

Admin ve Technician ayrıca tüm destek kuyruğundaki atanmamış aktif ticket sayısını görür. Bu sayı kişisel özetten ayrı etiketlenir; Employee'ye gönderilmez. Sayımlar SQL'de yapılır; liste beş kayıtla sınırlandırılır ve açıklama/yorum içeriği taşınmaz. Bölümler aynı RepeatableRead transaction'ında okunur; veri değiştirilmez ve audit üretilmez. Yenileme elle yapılır, son başarılı yükleme zamanı gösterilir; yenileme hatasında eski veriler uyarıyla korunur.

### Ticket dosya ekleri

Ticket detayındaki **Dosya ekleri** bölümünden PDF, PNG, JPG/JPEG ve UTF-8 TXT dosyaları yüklenir ve indirilir. Dosya başına sınır 10 MB'dır; boş dosyalar ve uzantıyla uyuşmayan imza/UTF-8 içerik reddedilir. Bu kontrol antivirüs taraması değildir. Her kullanıcı dakikada en fazla 10 yükleme denemesi yapabilir. Kapalı ticket'ların ekleri okunabilir; yeni yükleme engellenir. Employee yalnızca kendi taleplerinin eklerine, Admin/Technician tüm ticket eklerine erişebilir.

Yükleme multipart `ticketId` ve `file` alanlarını kullanır; kullanıcı, tarih ve saklama anahtarı sunucuda belirlenir. Önceki istemciden dosya yolu alan oluşturma/düzenleme endpoint'leri kaldırılmıştır; silme bu sürümde sunulmaz. DTO'lar fiziksel yolu veya saklama anahtarını içermez. İndirme JWT kontrolüyle HttpClient üzerinden yapılır, `application/octet-stream`, indirme adı, `nosniff` ve `no-store` kullanılır; dosya önizlemesi açılmaz.

Dosyalar varsayılan olarak `EnterpriseIT.API/App_Data/TicketAttachments` altında rastgele GUID adlarıyla saklanır; bu klasör Git'e dahil değildir ve statik olarak sunulmaz. `Attachments__StoragePath` ile özel bir konum verilebilir; web kökü altındaki konumlar reddedilir. Deployment'ta kalıcı disk kullanılmalı ve dosyalar veritabanıyla birlikte yedeklenmelidir; bu sürüm tek sunucuda yerel depolama içindir. Migration gerekmez.

Dosya tamamlandıktan sonra metadata ve audit aynı veritabanı transaction'ında yazılır. Ticket satırı kilitlenerek kapanış ve sahiplik yeniden kontrol edilir; bu kontrol reddedilirse dosya temizlenir. Dosya sistemi ile veritabanı ortak transaction desteklemediğinden sunucunun aniden durması veya belirsiz commit hatası sahipsiz bir dosya bırakabilir. Belirsiz hata halinde olası kaydın dosyasını kaybetmemek için dosya korunur ve saklama anahtarı sunucu günlüğüne yazılır; otomatik toplu temizleme yapılmaz.

### Kullanıcı bildirimleri

Üst menüdeki **Bildirimler** paneli tüm roller için yalnızca giriş yapan kullanıcının bildirimlerini gösterir. JWT kimliği API sorgularına uygulanır; başka kullanıcıların bildirimleri ID ile de okunamaz veya değiştirilemez. Kayıtlar en yeniden eskiye 20'şer sayfalanır; okunmamış sayısı ve okundu işaretleme desteklenir. Panel açılınca ve Yenile ile güncellenir; kapalıyken sayı 60 saniyede bir kontrol edilir.

Yeni atamada ilgili aktif teknisyene, ticket başlatma/kapatmada talebi açan aktif kullanıcıya, yorumda ise talep sahibi ve atanmış aktif teknisyene bildirim üretilir. İşlemi yapan kişiye bildirim gönderilmez; aynı alıcıya tekrar bildirim oluşturulmaz. Yorum metni gibi içerikler bildirime kopyalanmaz; ticket numarası ve olay bilgisi bulunur. Bildirim, ticket işlemi ve audit aynı transaction içinde kaydedilir; herhangi biri yazılamazsa işlem commit edilmez. API dışarıdan bildirim oluşturma veya silme sunmaz. Mevcut tablo kullanılır, migration gerekmez; geçmiş işlemler için geriye dönük bildirim üretilmez.

### Otomatik audit kayıtları

Admin `/dashboard/audit-logs` ekranında en yeni kayıtları 50'şer kayıtlık sayfalarda görür. API yalnızca GET sunar; manuel audit oluşturma, düzenleme ve silme endpoint'i yoktur. Kayıtta işlem türü, entity türü/ID'si, JWT'den alınan yapan kullanıcı ID'si ve UTC zaman bulunur. Ekrandaki kullanıcı adı mevcut kullanıcı kaydından gelir; kalıcı kimlik UserId'dir. Parola/hash, telefon, yorum metni ve istek gövdesi audit'e yazılmaz.

EF Core ile izlenen başarılı ekleme/güncelleme/silmeler ortak DbContext kayıt akışında otomatik izlenir. Doğrudan SQL kullanan ticket atama, üzerine alma, başlatma/kapatma, profil ve departman işlemleri açıkça audit yazar. İşlem ve audit aynı transaction'da kaydedilir; audit yazılamazsa işlem de kaydedilmez. Çakışma/başarısız işlem ve salt okuma audit üretmez. Kimliği doğrulanmış HTTP kullanıcısı bulunmayan development başlangıç komutu kapsam dışıdır; eski işlemler geriye dönük üretilmez.

Geçmişi korumak için uygulamadan kalıcı kullanıcı silme kapatılmıştır; kullanıcı pasifleştirilir. EF üzerinden audit entity'si oluşturma, değiştirme veya silme de reddedilir. Veritabanı şeması değiştirilmez; doğrudan SQL erişimi olan veritabanı yöneticisine karşı ayrı bir kurcalama koruması bu sürümün kapsamında değildir.

### Demirbaş bilgilerini düzenleme

Admin ve Technician `/dashboard/assets` listesindeki **Düzenle** düğmesiyle mevcut ekleme formunu düzenleme modunda açar. Mevcut bilgiler ve tarihler doldurulur; kayıt `PUT /api/Asset/{id}` üzerinden güncellenip liste yenilenir. Standart seçenekler dışında bir eski durum değeri varsa korunur. Zorunlu alanlar, alan uzunlukları ve garanti/satın alma tarih sırası API'de de doğrulanır. Başka kaydın kullandığı demirbaş kodu reddedilir; güncelleme zamanı sunucuda atanır.

### Departman ekranı

Admin `/dashboard/departments` ekranından departmanları listeler, ekler, düzenler ve onay vererek siler. Ad zorunlu ve en fazla 100, açıklama isteğe bağlı ve en fazla 250 karakterdir. Adların baş/son boşlukları temizlenir; büyük/küçük harf farkıyla tekrar kayıt yapılamaz.

İsim kontrolü ile yazma transaction içinde PostgreSQL advisory lock kullanılarak sıralanır. Silme ayrıca departman satırını kilitleyip aktif/pasif tüm kullanıcı bağlantılarını kontrol eder; kullanıcısı bulunan departman 409 ile reddedilir. Kullanıcı atama ile eşzamanlı silme de FK satır kilidi üzerinden sıralanır. Migration gerekmez.

### Ticket yorum geçmişi

Ticket detayında yorum geçmişi ve 1–1000 karakterlik yeni yorum formu bulunur. Employee yalnızca kendi taleplerinin yorumlarına, Admin ve Technician tüm ticket yorumlarına erişebilir. `GET /api/TicketComment` için `ticketId` zorunludur; yorum ID sorgusu da Employee sahipliğiyle sınırlandırılır. Yazar ve tarih sunucuda belirlenir.

Kapalı ticket geçmişi okunabilir; yeni yorum eklenemez. Yorum ekleme transaction içinde ticket satırını `FOR UPDATE` ile kilitler, kapanış ve sahipliği tekrar kontrol eder; eşzamanlı kapanış işlemiyle sıralanır. Yorum düzenleme/silme arayüzü eklenmemiştir; mevcut yönetim endpoint'leri Admin ile sınırlıdır.

### Rol ekranı

Yalnızca Admin'in eriştiği `/dashboard/roles` ekranı, API'deki rol kayıtlarını ve mevcut uygulama yetkilerinin karşılaştırmasını gösterir. Ekran salt okunurdur; kullanıcı rolü Users üzerinden değiştirilir. Yeni rol ekleme/silme veya özel izin düzenleme arayüzü yoktur. Yetki tablosu açıklama amaçlıdır; asıl erişim kontrolü API'de uygulanır. Desteklenmeyen rol kayıtları yetki verilmiş gibi gösterilmez.

### Profil ekranı

Tüm giriş yapmış kullanıcılar `/dashboard/profile` ekranından kendi hesap, rol ve departman bilgilerini görebilir. Yalnızca telefon numarası düzenlenebilir; ad, e-posta, rol, departman ve aktiflik Admin yönetimindedir. Profil endpoint'leri kullanıcı ID'sini JWT'den alır.

Parola değişikliği mevcut parolanın doğrulanmasını ve 12–128 karakterlik yeni parolanın tekrarını gerektirir. Değişiklik hash karşılaştırmasıyla koşullu kaydedilir; eşzamanlı parola değişikliği ezilmez. Başarıda tüm mevcut token'lar geçersiz olur ve kullanıcı yeniden giriş yapar. Giriş denemesi sınırı parola değiştirme endpoint'ine de uygulanır.

### Kullanıcı yönetimi ekranı

Admin, `/dashboard/users` ekranından kullanıcıları listeler, oluşturur ve düzenler. Rol, departman ve aktiflik durumu bu formda yönetilir. Oluşturmada 12–128 karakterlik ilk parola zorunludur; düzenlemede parola boş bırakılırsa mevcut hash korunur. Kendi parolasını değiştiren Admin yeniden giriş yapar. Kendi hesabının Admin rolünü kaldırması veya hesabını pasifleştirmesi formda ve API'de engellenir. Kullanıcı silme arayüzü eklenmemiştir.

### İlk Admin parolasını tanımlama

Yalnızca açık onayla ve Development ortamında: `BOOTSTRAP_ADMIN_PASSWORD` ortam değişkenine 12–128 karakterlik güçlü parola verilip `dotnet run --project EnterpriseIT.API -- --initialize-admin-password` çalıştırılabilir. Komut yalnızca tek aktif Admin bulunduğunda ve parola alanı boşken işlem yapar; mevcut parolayı sıfırlamaz. Ortam değişkenini işlemden sonra temizleyin. Otomatik seed veya migration çalıştırılmaz.

Yeni kullanıcılar Admin yetkisiyle oluşturulur. Parolalar ASP.NET Core `PasswordHasher` ile hashlenir; kullanıcı güncellemesinde parola gönderilmezse mevcut hash korunur. İlk parolayı değiştirmek için Admin, kullanıcı güncelleme endpoint'ini kullanabilir.

JWT doğrulaması [Microsoft belgesindeki](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication) imza, issuer, audience ve süre kontrollerini uygular.
