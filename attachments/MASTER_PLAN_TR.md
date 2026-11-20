# ANA PROMPT — Abyssal Requiem: Echoes of Vanessa (Android-first)

Aşağıdaki talimatı eksiksiz uygula. Bu ilk aşamada YALNIZCA analiz, mimari ve uygulanabilir geliştirme planı üret; henüz kod yazma, büyük dosyalar üretme veya mevcut tasarımı onay almadan değiştirme.

## ROLÜN

Kıdemli bir 3D mobil oyun teknik direktörü, korku oyunu tasarımcısı, Godot/GDScript mimarı, teknik sanatçı ve QA lideri gibi çalış. Oyun adı: **Abyssal Requiem: Echoes of Vanessa**. Açıklamalar Türkçe, kod sembolleri ve dosya adları İngilizce olacak.

## GİRDİLERİ İNCELE

Bu paketteki tüm dosyaları ve referansları satır satır incele:
- `original_unity_ai_pack.zip` içindeki `GDD.md`, `README.md`, üç Unity AI promptu, `gamename.html` ve görseller.
- `references/act1_cursed_galleon.jpg`
- `references/act2_ship_graveyard.jpg`
- `references/act4_sunken_temple.jpg`
- `SOURCE_ANALYSIS_TR.md`
- `TECH_DECISION_TR.md`

Yalnızca dosya isimlerini okumakla yetinme. GDD ve promptlar arasındaki çelişkileri, görsel referansların kompozisyonunu, teknik olarak mobilde pahalı olacak özellikleri, eksik tanımları ve test edilemeyen gereksinimleri raporla. Paket içinde gerçekte olmayan modellerin, ses dosyalarının veya sahnelerin var olduğunu iddia etme.

## TEKNOLOJİ KARARI VE KISITLAR

Başlangıç varsayımı: **Godot 4.7.2 stable + GDScript + Mobile renderer + Android-first**. Beta/dev motor sürümü kullanma. C#/.NET kullanma; Android Editor kısıtlarına takılmamak için GDScript kullan. 3D korku oyunu Kotlin uygulaması olarak sıfırdan yazmaya çalışma. Unity/HDRP'ye özgü `Volume`, `ScriptableObject`, HDRP fog ve C# API'lerini doğrudan kopyalama; bunların tasarım amaçlarını Godot'ta doğru karşılıklarla yeniden tasarla.

Önemli sınır: Godot projesi daha sonra Unity'ye otomatik taşınamaz. Bu yüzden hikâye, bulmaca tanımları, seviye akışları, görsel/işitsel varlıklar ve test senaryolarını motor bağımsız tut. Gelecekte Unity'ye geçiş istenirse oyun mantığı ve render entegrasyonunun yeniden yazılacağını açıkça belirt. Godot Android Editor'ın deneysel olduğunu ve telefonda APK export'un otomatik garanti olmadığını plana dahil et. İlk kilometre taşında export/paketleme yolunu doğrulamayı unutma; gerekirse GitHub Actions/uzak build kullan.

## OYUNUN VİZYONU

Birinci şahıs psikolojik korku / hayatta kalma / çevresel bulmaca oyunu. Hedeflenen tam oyun 3–4 saat olabilir; fakat başlangıçta tüm 4 Act'i üretme. İlk gerçek hedef **10–15 dakikalık, baştan sona tamamlanabilen Act 1 dikey kesiti**. Jump-scare ve gore kullanma. Korku; yoğun sis, ışık kıtlığı, mekânsal ses, oyuncunun bakmadığı zaman çevrenin değişmesi, sessizlik ve güvenilmez algıdan gelsin.

Vanessa'nın kaybolması merkezî gizemdir. Hikâyeyi uzun açıklama duvarlarıyla anlatma; ses, iz, müzik kutusu, gemi nesneleri ve çevresel ipuçlarıyla anlat. Ucuz jump-scare, rastgele ekran görüntüsü, sebepsiz düşman saldırısı ve anlamsız rastgele eşya teleportu kullanma.

## GÖRSEL REFERANS ANALİZİ

1. Act 1: soğuk gri-mavi yoğun sis; oyuncunun elindeki sağ ön plandaki eski yağ feneri tek sıcak amber renk noktası; ıslak/çatlak güverte, çürümüş korkuluk, direk ve ipler; arka plan yaklaşık uzaklık içinde sisle kaybolur. Sis derinlik hissini kapatmasın; yol kenarı ve yakın etkileşim nesneleri okunmalı.
2. Act 2: devasa, birbirine yığılmış gemi enkazları iki tarafta duvar etkisi oluşturur; ortada dar su rotası; soğuk ay ışığı, karanlık bulutlar ve az miktarda turkuaz yansıma. Ana his: ölçek, kaybolma ve yanlış yöne gidiyor olma.
3. Act 3: görsel eksik. Yeni referans üretmeden önce konsept planı öner: 18. yüzyıl korsan limanı, sığ/diz boyu su, dar taş sokaklar, durmuş saatler, Vanessa'nın ayak izleri ve geçmişe ait rahatsız edici ipuçları. Görsel olarak Act 1/2'yle aynı hava ailesinde kalsın.
4. Act 4: turkuaz/mavi derin deniz, antik/gotik sualtı kalıntıları, üstten ışık huzmeleri, solda/ön planda ışıklı sandık, uzakta büyük deniz canlısı silueti. Büyük canlıyı pahalı detaylı AI düşmanı olarak yapmak yerine başlangıçta uzak siluet + 3D ses + ışığın kesilmesiyle kullan.

Palet çıpaları: sis `#4E5D62`, derin gölge `#0B141A`, ay ışığı `#86A0C8`, fener `#FF9227`, sanity halüsinasyon vurgusu `#6FA38A`, sualtı biolüminesans `#18C5D2`. Turuncu yalnızca fener/ateş gibi sıcak kaynaklarda, turkuaz çoğunlukla Act 2/4 ve halüsinasyon ayrıntılarında kullanılsın. Kontrast oyuncunun yakın çevreyi okumasına yetsin.

## ANA OYNANIŞ DÖNGÜSÜ

Keşfet → bir görsel/işitsel ipucunu fark et → fener yağı ve güvenli ışık yönet → çevresel bulmacayı çöz → Sanity üzerindeki sonucu hisset → mekânın algısal değişimini fark et → yeni alanı aç. Oyuncu her 2–4 dakikada bir küçük yeni soru/ipuçla ödüllendirilsin; sürekli aynı koridorda dolaşma olmasın.

## SANITY SİSTEMİ

Gizli, 0–100 arası bir değer; başlangıç 100. HUD'da Sanity barı yok. Geliştirme/debug durumunda ayrı test paneli olabilir ama normal oynanışta görünmez.

- Stable: 100–70
- Uneasy: 70–45
- Disturbed: 45–20
- Breaking: 20–0
- Kademe geçişlerinde 3 puan histerezis uygula, eşik titreşimi oluşmasın.
- İlk taslak tetikler: karanlıkta saniyede −0.8; oyuncu 6 saniyeden uzun denize bakınca saniyede −1.2; fener açık ve tanımlı güvenli bölgede saniyede +1.0; rahatsız edici objeyi inceleme −4; müzik kutusu bulmacasını çözme +15. Bu değerleri yapılandırılabilir yap ve mobil testle ayarlanacağını belirt.
- Sanity bileşeni tek bir dev script olmasın. GDScript sinyalleriyle değer/kademe değişikliklerini yayınlasın. Lantern, environment distortion, audio mood ve post-process sistemleri gevşek bağlı olsun.
- FOV efektleri baş dönmesi yaratmayacak kadar küçük ve kapatılabilir olmalı. Kamera etkileri kademeli ve yumuşak geçsin; motion-sickness seçeneği olan oyuncular için FOV dalgalanması/post-process azaltılabilsin.
- Distortion yalnızca oyuncunun bakmadığı/engel arkasında kalan nesnelerde ve seyrek çalışsın. Kamera önünde teleport yok. Global değişim aralığı ilk taslakta 4–8 saniye, nesne başına 30 saniye cooldown; tek karede sınırlı aday kontrolü. Sanity yükselip 10 saniye sabit kaldığında nesneler güvenli biçimde ilk durumuna dönsün.
- Distortion tier davranışı: Uneasy mikro kayma (en fazla 5 cm/5°); Disturbed heykel/tabloların kademeli yön değiştirmesi (en fazla 20°) ve hafif mobilya kayması (en fazla 35 cm); Breaking ise yalnızca önceden belirlenmiş güvenli teleport/nokta değiştirmesi. Fizik, çarpışma, görev eşyası ve path-blocking bozulmamalı.

## FENER VE KAYNAK YÖNETİMİ

Antika yağ feneri oyunun en önemli görsel ve mekanik odağıdır. Yağ miktarı, yanma, sönükleşme, yeniden doldurma ve güvenli bölge etkileşimi tanımlansın. Rüzgâr/koşma alevi etkileyebilir, ancak oyuncu bunun neden olduğunu anlayabilsin. Fenerin ışık rengi sıcak amber; çevrede soğuk ay ışığı. Fener açıkken yüzeylerde ışık hareketi, sesli çıtırtı ve az miktarda parçacık olabilir. Mobilde eşzamanlı gerçek zamanlı ışık sayısını sınırlı tut; mümkünse çevrenin çoğunu baked/static lighting ile çöz.

## BULMACALAR

- Müzik kutusu/ninni: ses dizisini dinleme, basit tekrarlama veya ortamda farklı noktalardaki rezonansı bulma. Dokunmatik ekranda hassas zamanlama gerektirmesin; erişilebilir ses/altyazı ipucu seçeneği olsun.
- Enkaz/ağırlık merkezi bulmacası: fizik simülasyonunu mobilde kararlı ve basit tut; nesneler kilitlenirse oyuncuyu soft-lock'tan çıkaran reset olmalı.
- Her bulmaca: giriş, ipuçları, başarılı/başarısız durum, reset koşulu ve otomatik test senaryosuyla tarif edilsin. Bulmaca çözümü yalnızca yapay zekânın belleğine bağlı olmasın; oyun içi tutarlı ipuçları verilsin.

## DÖRT ACT'İN PLANLANMASI

- Act 1 — The Cursed Galleon: 10–15 dakikalık ilk dikey kesit. Güverte başlangıcı, fener kontrolü, yağ kaynağı, ilk kapalı koridor, en az bir sanity olayı, ilk müzik kutusu ve bir checkpoint ile tamamlanan mini final. Bu dilim bütün temel sistemleri test etsin.
- Act 2 — The Ship Graveyard: birbirine geçmiş gemiler arasında rota bulma, enkaz hareketleri, platform/keşif. Mobil kontrol zor olacağı için hassas platforming minimumda tutulmalı.
- Act 3 — The Forsaken Port: ayak izleri, zamanın durmuş olduğu liman, su seviyesi ve geçmiş günahları çağrıştıran hallucination set-piece'leri.
- Act 4 — The Sunken Temple: turkuaz ışıklı sualtı kalıntıları, ışık/derinlik bulmacası ve uzaktaki dev canlı ile final yüzleşmesi. İlk prototipte gerçek su altı simülasyonu değil, kontrollü shader/fog ve sesle illüzyon yarat.

## MOBİL KONTROLLER

- Oyun yatay (landscape) çalışsın.
- Sol başparmak: hareket joystick'i.
- Sağ yarıda sürükleme: kamera bakışı; hassasiyet ayarlanabilir.
- Sağ alt: bağlama göre değişen tek “Etkileşim” düğmesi.
- Ayrı bir fener düğmesi; gerektiğinde sprint ve duraklatma. Gereksiz kalabalık HUD kurma.
- Büyük ve güvenli dokunma hedefleri, ekran kenarlarında safe-area payı, ayarlanabilir UI ölçeği ve hassasiyet. Android geri tuşu/pause, ekran kilidi, uygulamadan çıkıp geri gelme testleri olmalı.
- Jiroskop opsiyonel, varsayılan kapalı. Titreşim (haptic) opsiyonel. Altyazılar, renk körlüğü için yalnız renge dayalı olmayan işaretler ve motion-sickness seçenekleri düşünülmeli.

## TEKNİK MİMARİ İÇİN BEKLENTİLER

İlk planında önerilecek klasör yapısı en az şu alanları ayırsın:
- `scenes/main/`, `scenes/acts/`, `scenes/player/`, `scenes/ui/`, `scenes/interactables/`, `scenes/puzzles/`
- `scripts/core/`, `scripts/player/`, `scripts/sanity/`, `scripts/interaction/`, `scripts/lantern/`, `scripts/audio/`, `scripts/save/`, `scripts/puzzles/`
- `resources/sanity/`, `resources/audio/`, `resources/items/`, `assets/concept/`, `assets/materials/`, `assets/models/`, `assets/audio/`
- `docs/` ve `tests/` (manuel test senaryoları için).

Mimari bileşenleri en azından isim ve sorumluluk düzeyinde öner: `GameState`, `PlayerController`, `MobileInputController`, `InteractionRay`, `SanitySystem`, `LanternController`, `DistortionDirector`, `AudioMoodController`, `CheckpointManager`, `SaveManager`, `PuzzleBase`, `SettingsMenu`. İsimleri Godot yaşam döngüsüyle uyumlu hâle getir. Çok karmaşık framework kurma; ama bütün mantığı `Game.gd` içine de yığma.

Kod yazma aşamasına geçildiğinde:
- GDScript 4.x sözdizimini ve Godot 4.7.2 API'lerini kullan; Godot 3 örneğini kopyalama.
- Tam dosya yolları ve eksiksiz dosya içerikleri ver. `TODO`, “buraya kod gelecek”, boş metot veya uydurma API bırakma.
- Dosya adları, node path'leri, InputMap action isimleri ve sinyal bağlantıları birbirleriyle tam eşleşsin.
- Scene tree ve node tiplerini açık yaz; dışarıdan indirilecek varlıkları zorunlu bağımlılık yapma.
- Önce düz gri kutu/placeholder ile derlenebilir ve oynanabilir bir iskelet kur; görsel assetleri ayrı aşamada değiştir.
- Hatalı asset, eksik texture, ses dosyası veya export template olduğunda uygulama çökmek yerine okunaklı uyarı/placeholder versin.
- Rastgelelik gerekiyorsa seed kullan; test tekrar üretilebilir olsun.
- Oyun akışı için explicit state machine kullan: `BOOT`, `MAIN_MENU`, `PLAYING`, `PAUSED`, `PUZZLE`, `CUTSCENE`, `DEAD/RESTART` (ölüm varsa sadece tasarımda tanımlandığı şekilde), `ENDING`.

## MOBİL PERFORMANS PLANI

- İlk hedef: desteklenen orta sınıf Android cihazda sabit 30 FPS; güçlü cihazda 60 FPS kalite seçeneği. Bunlar vaat değil, gerçek cihaz testiyle doğrulanacak hedefler.
- Volumetric fog ve gerçek zamanlı birden çok ışık yerine uygun mesafede sis, fog shader/WorldEnvironment, billboarding parçacıklar ve sınırlı dynamic light kullan.
- Su fiziğini ve bütün okyanusu simüle etme; basit vertex/shader dalgası, normal/roughness hareketi ve sesle derinlik algısı oluştur.
- Statik gemi geometrisini mümkün olduğu kadar birleştir, görünürlük/mesafe limitleri ve LOD kullan; aynı anda görünen benzersiz materyal ve draw call sayısını takip et.
- Yüksek çözünürlüklü görselleri, 3D dokuları ve sesleri bütçeli tut; mobil sıkıştırma ve yüklenme süresi tasarla. Referans JPG'leri final 3D varlık gibi kullanma.
- Nesne bozulmasını her karede bütün objelere tarama; zamanlayıcı ve küçük aday havuzu kullan.
- Grafik seçenekleri: Low/Medium/High; sis kalitesi, gölge, parçacık yoğunluğu, render scale ve post-process kontrol edilsin.
- FPS, yüklenme süresi, RAM, APK/AAB boyutu, pil ısısı ve düşük bellek durumunu test planında ölçülebilir olarak tanımla. Ölçülmemiş sonuçları raporlama.

## İLK TESLİMDE İSTEDİĞİM ÇIKTI — HENÜZ KOD YOK

Yanıtını şu başlıklarla sırala:
1. **Kaynak denetimi:** Her kaynak dosyasının hangi bilgiyi verdiği, bütün çelişkiler ve eksikler.
2. **Teknoloji kararı:** Godot 4.7.2 stable + GDScript + Mobile renderer neden seçildi; Android export riskleri ve Unity'ye geçiş maliyeti. Bilinen ve bilinmeyenleri ayır.
3. **Oyun vizyonu ve çekirdek döngü:** 1 paragraf vizyon, 6–10 adımlı oynanış döngüsü.
4. **10–15 dakikalık Act 1 dikey kesit:** dakika aralıklarıyla başlangıçtan checkpoint/final'e kadar akış, oyuncu amacı, ipucu, etkileşim, sanity olayı, ses ve test noktaları.
5. **Dört Act storyboard:** her Act için amaç, 3 ana mekân, 1 bulmaca, 1 sanity olayı, 1 görsel/işitsel set-piece, çıkış koşulu.
6. **Mekanik tasarım:** player control, touch UI, lantern/oil, sanity tiers, environmental distortion, interaction, puzzle, save/checkpoint, audio, pause/settings.
7. **Dosya ve sahne mimarisi:** dosya ağacı; her script/scene/resource sorumluluğu; sinyal/veri akış diyagramı; sahne geçişi.
8. **Görsel/işitsel teknik plan:** sahne başına palet, sis, ışık, material, water illusion, sound layers; HDRP talimatlarının Godot karşılıkları.
9. **Mobil performans bütçesi:** Low/Medium/High ayarları ve nelerin ölçüleceği; bilinmeyen performans değerlerini uydurma.
10. **Fazlar ve kabul kriterleri:** A) proje boot ve giriş kontrolü, B) Act 1 vertical slice, C) art/audio polish, D) Act 2–4; her faz için çıkış kriterleri.
11. **Test matrisi:** 15–20 manuel senaryo; bozuk giriş, ekran boyutu, pause/resume, Android geri, save/load, yağ bitişi, sanity sınırları, puzzle reset, performans, ses, asset eksikliği ve export denemesi.
12. **Risk listesi:** en az 10 risk, şiddet, erken uyarı ve azaltma yolu.
13. **En fazla 3 kritik soru:** sorular cevapsız kalırsa makul varsayımla devam et.
14. **Onay bekleme:** sonuçta yalnızca planı sun ve onay iste; henüz kod üretme.

## PLAN KALİTESİ KURALLARI

- “Hatasız” olduğunu iddia etmek yerine hata riskini ve doğrulama yöntemini açıkla.
- Sürüm ve API konusunda emin değilsen ad uydurma; resmî dokümanda doğrulanacak şekilde not et.
- Tek seferde dört bölümün tamamını üretmeye çalışma. İlk dikey kesiti tamamlamadan büyük içerik üretimine geçme.
- İlk aşamada ücretli asset, API, online servis veya büyük üçüncü taraf plugin zorunlu olmasın.
- Kullanıcı bir Android telefon ve Termux merkezli iş akışıyla çalışıyor olabilir; talimatlar mümkün olduğunda küçük, kopyalanabilir ve tek amaca yönelik olsun. Telefon üzerinde gerçek editör/build sınırlamalarını saklama.
- Her teknik önerinin niçin gerekli olduğunu yaz; sanat hedefi, oynanış hedefi ve performans maliyetini ayrı değerlendir.
