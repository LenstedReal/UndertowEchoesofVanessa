# Değişiklikler

## 0.3.2 — 22 Kasım 2026

0.3.1 bulut bilgisayarda açıldı: görüntü doğru (pembe ekran gitti), sürüm v0.3.1. Ama oyun telefondan Moonlight ile oynanırken ilerlenemedi: Moonlight'ın ekran kolu bu makinede Windows'a ulaşmıyor, dokunmalar oyuna yalnızca fare tıklaması olarak geliyor.

- **Dokunmatik test kontrolleri (geçici, yalnızca test için):** Oynanış sırasında ekranda tıklanabilir tuşlar var. Sol tarafta İLERİ / GERİ / SOL / SAĞ (aç-kapa), DUR, KOŞ, EĞİL, DÜZ BAK, < DÖN / ARKA / DÖN >. Sağ tarafta AL / KULLAN, FENER, DİNLE, GÜNLÜK, MENÜ. Dünyada bir yere dokununca bakış oraya döner, nişangâha dokununca bakılan eşya kullanılır. Kol ya da klavye kullanılınca tuşlar gizlenir. **Steam sürümünden önce kapatılacak:** `Scripts/Runtime/UI/TouchControls.cs` içinde `Enabled = false`.
- **Fare ve dokunmayla kullanım (kalıcı):** Parlaklık ekranına tıklanabilir `<`, `>` ve Onayla düğmeleri eklendi; önceden yalnızca klavye ve kolla geçiliyordu. Menülerin, günlüğün, ayarların ve galerinin sol altına tıklanabilir "Geri" eklendi. Jenerik, fare basılı tutulunca hızlanır.
- Sürüm 0.3.2.

## 0.3.1 — 22 Kasım 2026

İlk gerçek test: 0.3.0'ın Unity Build Automation build'i (#4) bulut bilgisayarda (NVIDIA L4, Windows Server 2025) açıldı. Oyun çalıştı, sesler geldi, ama ekran baştan sona pembe kaldı.

- **Sebep:** Build Automation, Unity'yi batch modda çalıştırır; projeyi açınca kendiliğinden çalışan kurulum bu modda atlanıyordu. Malzeme kütüphanesi `Resources` altına üretilmediği için HDRP Lit/Unlit shader'ları build'e girmedi. Ayrıca arayüzün tamamı koddan kurulduğu için `UI/Default` shader'ına build'de hiçbir şey bağlanmıyordu. Shader'ı bulamayan her şey Unity'nin hata rengiyle (pembe) çiziliyor.
- **Düzeltme:** Yeni `UndertowBuildPreprocess`, Build Automation dahil her build'den önce çalışır: malzemeleri üretir ve `UI/Default` ile `Sprites/Default`'u Always Included Shaders listesine ekler. Malzeme üretilemezse build pembe bir oyun çıkarmak yerine açık bir hata mesajıyla durur.
- `GraphicsSettings.asset` dosyasına Always Included Shaders listesi eklendi.
- HDRP shader'ları adla bulunamazsa paket yolundan yüklenir.
- Build Automation panelinde ek ayar gerekmez.
- Sürüm 0.3.1. Sağ alttaki sürüm yazısından yeni build'i ayırt edebilirsin.

## Depo temizliği — 20 Kasım 2026

Oyun koduna, sürüm bilgilerine ve oyunun `.md` belgelerine dokunulmadı. Silinenlerin hepsi git geçmişinde duruyor.

- Oyunun hiçbir yerde kullanmadığı eski sanat dosyaları kaldırıldı: 18 adet 4K PNG doku (359 MB, Git LFS), 10 OBJ model, 5 eski malzeme. Kodda, sahnelerde ve ayarlarda bunlara tek bir bağlantı yoktu. Taze bir klon artık 359 MB indirmez, Unity'nin ilk açılışı kısalır.
- Birebir aynı kopyalar kaldırıldı: ana üretim talimatının iki `.txt` kopyası (aslı `.md` duruyor), talimat paketinin `-2.zip` kopyası, `AbyssalRequiem_UnityAI_Pack.zip` (aynısı öteki Abyssal Requiem paketinin içindeydi), `artifacts/` içindeki iki görsel (aynıları `public/` içinde).
- Önceki teslimin artık var olmayan dosyaları gösteren iki SHA-256 listesi kaldırıldı.
- Repoda yalnızca oyun ve belgeleri kaldı. İlk tarayıcı prototipi (`src`, `public`, `server`, `scripts`, `migrations`, `.vercel`, çalışma alanı ayarları, web projesi ayar dosyaları ve `AGENTS.md`), `artifacts/` görselleri, ekteki referans görseller ve eski paket zip'leri kaldırıldı. Dört bölümün yerleşimi ve bulmacaları zaten Unity'ye taşınmıştı.
- Kalanlar: `unity-project/` (oyun), `README.md`, `docs/` (hikâye, tasarım, denetim), `attachments/` içindeki iki `.md` ve `.gitignore`.

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
