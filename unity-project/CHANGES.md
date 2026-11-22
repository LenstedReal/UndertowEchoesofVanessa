# Değişiklikler

## 0.3.3 — 22 Kasım 2026

Klavye + fareye dönüş ve kapsamlı hata taraması. Oyunun kendi kodu, Unity'nin yerine geçen başsız (ekransız) bir benzetimde baştan sona çalıştırıldı: açılış, bütün menüler, dört bölüm, üç son, kayıt/yükleme, klavye-fare kontrolleri. Bulunan hatalar aşağıda. **Gerçek Unity'de ve gerçek ekran kartında bu sürüm henüz denenmedi** (bkz. `KNOWN_ISSUES.md`).

### Kontroller
- **Dokunmatik test tuşları kaldırıldı.** Oyun yine klavye + fare (WASD / ok tuşları, fare ile bakış) ve kumanda ile oynanır. Dokunmatik tuşlar gerekirse git geçmişinde: `495d24a`.
- 0.3.2'nin tıklanabilir menüleri kaldı (parlaklık ekranındaki `<` `>` Onayla, menülerde "Geri").
- Tuş atarken Esc ile vazgeçince Ayarlar ekranı da kapanıyordu; artık yalnızca atama iptal olur.
- Uzun listelerde (Ayarlar, Başarımlar) fare tekerleği çalışmıyordu: liste her karede seçili satıra geri kayıyordu. Artık liste yalnızca klavye/kumanda ile gezinince seçimi izler.
- Act I'in başında yürüme / kullanma / günlük tuşlarını gösteren kısa ipucu eklendi (10 dilde).

### Oynanışı kilitleyen ya da bozan hatalar
- **Top güvertesine inen merdiven:** zemin çarpıştırıcıları olması gerekenden kalındı, oyuncunun başı güverte kenarına takılıyordu. Kat döşemeleri seviye derleyicisinin varsaydığı kalınlığa (0,3 m) indirildi.
- **Vanessa'nın kamarasının kapısı** yanlış katta kuruluyordu: kamara baştan açıktı, ön güvertede ise boşta duran bir kapı ve görünmez bir duvar vardı. Engeller artık kendi kapı aralığının zeminine kurulur.
- **Bölüm sonu:** bölümü bitiren olay iki kez tetiklenebiliyor, geçiş kararmasında oyun takılabiliyordu. Bölüm bitişi tek seferlik yapıldı; kararma artık durdurulan bir işleme bağlı değil, takılamaz.
- **Bakışı çeviren sahneler** (ör. "şuraya bak" anları) bir işlem kesilirse oyuncunun kontrolünü geri vermeyebiliyordu; kilit her durumda açılır.
- **Fener doluyken yağ şişesi:** şişe alınamıyor, ona bağlı hedef de ilerlemiyordu. Şişe yerinde kalır, hedef tamamlanır.
- **Act II sandık bulmacası** kayıttan yüklenince yanlış anahtarla eşitleniyordu (`a2c` → `a2x`).
- **Son sahneler:** kamera kayması oyunun toplam çalışma süresine bağlıydı; uzun bir oturumdan sonra Cevap ve Akıntı sonlarında kamera yüzlerce metre öteye gidebiliyordu.
- **Menü müziği** bölümlere taşıyordu; sahne değişirken kararmayla birlikte söner.

### Görüntü
- **Işık şiddeti:** nokta ve spot ışıklar lümen olarak yazılıyordu, HDRP 17 ise bu değeri kandela olarak okur; ışıklar tasarlanandan ~12,5 kat parlaktı. Dönüşüm eklendi. Sahneler 0.3.2'ye göre **belirgin biçimde daha karanlık** (tasarlandığı gibi: fener yakını aydınlatır, 5 m ötesi neredeyse siyah). Fazla karanlık gelirse Ayarlar → Görüntü → Parlaklık.
- **Ana menüde üst üste binen görüntü:** arka plan sahnesi ile menü görseli aynı anda çiziliyordu; menü artık yalnızca 3B sahneyi kullanır.
- **Açılış sırası:** dil / uyarı / parlaklık ekranları arasında menünün arka planı bir an görünüyordu. İlk açılış ekranlarının arkasında siyah perde var; menü yalnızca stüdyo kartından sonra gelir.
- **Boru biçimli modeller** (halatlar, lomboz halkaları, teyp sapı) yön değiştirince çöküyor ya da siyah yüzey veriyordu; kesit artık yol boyunca taşınıyor.
- **Kol (lever)** çekilince hiçbir şey görünmüyordu (kolun modeli yoktu); taban ve kol eklendi. Çan, ayna, sandık, kapak ve kadran hareketleri üst üste tetiklenince kaymıyor.
- **Farklı ekran oranları:** arayüz 4:3, 5:4, 16:10, 21:9 ekranlarda kesilmiyor (tuval artık her yönde en az 1920×1080). Menü gölgeleri ekran yüksekliğine uzar.
- **Kapalı mekânlar:** kamaraların ve ambarın kendi ortam ışığı ve sis ayarı kameraya hiç uygulanmıyordu (kamera o katmanı okumuyordu); içeriler açık güverte kadar aydınlık kalıyordu. Artık uygulanır.
- Hikâyede fenerin söndüğü anda alev gerçekten söner (önceden bir sonraki karede geri yanıyordu).
- Fotoğraf modunda sürüm yazısı fotoğrafa girmez.
- Kalite ayarı bölüm ortasında değişince var olan ışıkların gölgeleri de yeni ayara geçer (önceden bölüm yeniden yüklenene kadar eski kalıyordu).

### Eski bilgisayarlar için
- **İlk açılışta kalite, ekran kartına göre seçilir:** 1,8 GB'tan az görüntü belleği ya da 7 GB'tan az RAM → Düşük; 3,5 GB'tan az → Orta; diğerleri → Yüksek. Ayarlardan değiştirilebilir.
- **F3:** gizli tanı paneli (kare hızı, ekran kartı ve belleği, çözünürlük, kalite, sahne/oda/konum, son hata). Sorun bildirirken bu panelin fotoğrafı yeterli.
- **Hata ekranı:** bir bölüm kurulurken hata olursa siyah ekranda kalmak yerine sebebi yazan bir ekran çıkar; ana menüye dönülür ya da oyundan çıkılır. Kayıtlar etkilenmez.

### Diğer
- Ses motorunda `Start` adlı parametreli bir yöntem Unity'de her açılışta "Script error" uyarısı verirdi; adı değişti.
- Sürüm 0.3.3. Yerelleştirme: 10 dilin her birinde 689 anahtar.

### Bu sürümde ne çalıştırıldı
- Derleme kapısı (oyun ve editör yapılandırması): 0 hata, 0 uyarı.
- Mantık testleri: 53/53.
- Başsız benzetim: 6 senaryo, **201 kontrol, hepsi geçti** — açılış sırası; bütün menüler ve ayar sekmeleri; dört bölümün dünyasının kurulması (ağ ve çarpıştırıcı denetimi); Yeni Oyun'dan jeneriğe üç tam oynanış (üç son); 4:3 / 5:4 / 16:10 / 21:9 ve 10 dilde taşma denetimi; WASD, ok tuşları, fare, koşma, eğilme, fener, dinleme, günlük, duraklatma, fotoğraf modu, kayıt ve Devam Et; bir bölümün kurulurken bilerek bozulması (hata ekranı ve ana menüye dönüş).
- Benzetim Unity değildir: görüntü, ışık, ses karışımı ve kare hızı **ölçülmedi**.

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
