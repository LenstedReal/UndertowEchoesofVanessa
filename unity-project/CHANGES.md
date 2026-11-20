# Değişiklikler

## 0.3.0 — 20 Kasım 2026

Önceki sürüm (`db30c12`, Unity 6000.0.23f1) üzerine yeniden yapım. Onaylanan plana göre.

### Motor ve proje
- Unity **6000.3.27f1**'e geçildi; HDRP / core / shadergraph / vfx **17.3.0**, Input System 1.11.2, uGUI 2.0.0, Test Framework 1.4.5.
- `activeInputHandler` yalnızca yeni Input System; renk alanı Linear; grafik API'leri DX12 + DX11.
- HDRP varlığı doğru betik GUID'iyle yeniden yazıldı; ilk açılışta otomatik kurulum (HDRP ataması, build sahneleri, oyuncu ayarları, malzeme üretimi).
- Unity Build Automation tanımı yeni sürüme güncellendi; giriş noktaları aynı kaldı.

### Mimari
- Oyun kodu dört derlemeye ayrıldı: `Undertow.Logic` (Unity'siz, test edilebilir), `Undertow.Runtime`, `Undertow.Editor`, `Undertow.Tests.EditMode`.
- Sahneler boş kabuk; dört bölümün dünyası oyun açılırken veriden kurulur.
- Eski 40 betik, eski prefab'lar, eski JSON yerelleştirme ve eski test dosyası kaldırıldı (git geçmişinde duruyor).

### Oyun
- Dört bölüm hikâye kanonuna göre baştan kuruldu: kalyon (gövde, direkler, yelkenler, top güvertesi, 8 kamara), gemi mezarlığı (enkaz labirenti, çadır, çan teknesi, sis kapısı, koy), Port Saint-Clair (sular altındaki sokaklar, 5 bina, saat kulesi, Vali'nin konağı, aynalı salon, deniz merdivenleri), batık tapınak (aynalar, balina galerisi, sunak).
- Fener/yağ, gizli akıl sağlığı, dinleme, Echo/Mimic sesleri, ipucu sistemi, kontrol noktaları, 3 kayıt yuvası, bölüm seçimi, üç son ve gizli ninni.
- Arayüz: açılış (dil, uyarı, parlaklık, logo), ana menü, ayarlar (5 sekme), duraklatma, günlük, not okuyucu, fotoğraf modu, cevap anı, ekstralar, jenerik.
- 136 özgün ses, 55 doku, 5 OFL yazı tipi eklendi.

### Bu sürümün son geçişinde
- **Yerelleştirme:** 10 dilin tamamı, her biri 685 anahtar (arayüz, notlar, sesler, ipuçları, başarımlar, sonlar, jenerik, günlük satırları). İngilizce ve Türkçe elden; diğer 8 dil yapay zekâ taslağı.
- **Yerelleştirme testleri:** her tablonun okunması, eksiksizliği, bilinmeyen anahtar olmaması, yer tutucuların İngilizceyle eşleşmesi, boş değer olmaması, notların 80 kelimeyi aşmaması, iki sesin her dilde ayırt edilebilmesi, kadran saatlerinin ipuçlarında bulunması.
- **Yayıncı modu** artık gerçekten bir şey yapıyor: fotoğraf kaydedildiğinde tam yol (Windows kullanıcı adı) yerine yalnızca dosya adı gösterilir ve ses sınırlayıcı zorla açılır.
- Fotoğraf modunda kumandayla pozlama (L3/R3) ve odak (X basılı + LT/RT) eklendi.
- Sonların anlatıcı satırları konuşmacı adı olmadan gösterilir.
- Türkçede oyma yazı başlıkları büyük harfe doğru çevrilir (i → İ).
- Rusça için 1724 belgeleri Kiril harfli bir yazı tipine düşer.
- Doğrulayıcı `hud.` ve `toast.` anahtarlarını da denetler; `msg.bells_solved_` gibi önekleri yanlışlıkla eksik anahtar saymaz.
- Soy ağacı dokusu düzeltildi: Ruth Crane 1966–2021 (eskisi 1977'ydi; Adrian doğduğunda 15 yaşında oluyordu) ve bağlantı çizgileri gerçek bir ağaç gibi çizildi.
- `Resources/Undertow` altındaki ses, doku, yazı tipi ve dil dosyaları Git LFS dışında tutulur.
- README, bilinen durumlar ve üçüncü taraf bildirimleri yeniden yazıldı.
