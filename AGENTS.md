# Proje Çalışma Kuralları

## Kapsam ve verimlilik
- Yalnızca istenen görevi yap; ilgisiz değişiklik, refactoring, optimizasyon, formatlama veya özellik ekleme.
- Yalnızca gerekli dosyaları ve bağımlılıkları incele. node_modules, bin, obj, dist, build ve diğer üretilmiş klasörleri ihtiyaç olmadıkça tarama.
- Gereksiz okumaları, komutları ve test tekrarlarını önle; basit görevleri karmaşıklaştırma.
- Yeterli bilgi varsa gereksiz açıklama veya onay soruları sormadan ilerle.

## Kodlama
- Mevcut mimariyi, isimlendirmeyi ve çalışan özellikleri koru; mevcut kodu yeniden kullan.
- Yeni kütüphane, framework veya mimari yaklaşım için gerekçeyi açıklayıp kullanıcı onayı al.
- Gereksiz soyutlama, katman veya yardımcı sınıf ekleme; güvenlik, veri bütünlüğü ve hata yönetimini koru.

## Değişiklik kontrolü
- Kullanıcının mevcut değişikliklerini koru ve yapılan dosya değişikliklerini açıkça incelemeye sun.
- Açık onay olmadan commit, push, branch değiştirme veya merge yapma.
- Açık onay olmadan veritabanı migrationı ya da veriyi değiştiren işlem çalıştırma.
- Gereksiz silme ve toplu yeniden yazma yapma.

## Doğrulama
- Kapsama uygun testleri çalıştır; doğruluğu riske atacak şekilde analiz veya testleri atlama.
- Küçük değişikliklerde tüm projeyi gereksiz yere tekrar tekrar derleme.
- Hataların nedenini araştır; rastgele değişiklik yapma. Test edilemeyen noktaları belirt.

## İletişim
- Türkçe, kısa ve doğrudan iletişim kur; önemli teknik kararların nedenini kısaca ve öğretici biçimde açıkla.
- Sonuçta ne değiştiğini, nedenini, yapılan testleri ve kullanıcının kontrol etmesi gerekenleri belirt.
- İstenmedikçe uzun kod blokları veya teorik açıklamalar paylaşma.

## Öncelik
Doğru tamamlama > Kod kalitesi ve güvenlik > Mevcut mimariyle uyum > Token ve zaman tasarrufu.
