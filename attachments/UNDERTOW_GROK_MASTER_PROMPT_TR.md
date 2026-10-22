# UNDERTOW: ECHOES OF VANESSA — GROK ANA ÜRETİM TALİMATI

**Hedef:** Unity 6 + HDRP ile Windows PC / Steam için, yaklaşık 3–4 saatlik, profesyonelce tasarlanmış tek oyunculu psikolojik korku oyunu.  
**Bu proje değildir:** mobil araba savaşı oyunu, web/HTML korku maketi, tek sahnelik kısa demo veya yalnızca fıçı fırlatma mekaniği.  
**Oyun adı için doğru kaynak:** `gamename.html` içindeki **Undertow: Echoes of Vanessa** adı esas alınacak. Kaynak paketteki “Abyssal Requiem” adı eski/çelişkili etikettir; proje adını her yerde Undertow olarak tutarlılaştır.

---

## 1. ROLÜN VE GÖREVİN

Kıdemli bir oyun yönetmeni, korku oyunu tasarımcısı, Unity 6/HDRP teknik lideri, C# mühendisi, seviye tasarımcısı, teknik sanatçı, ses tasarımcısı ve QA lideri gibi çalış. Görevin birkaç dakikalık kaba bir örnek üretmek değil; konseptten oynanabilir içeriğe, sistemlerden kalite kontrolüne ve Steam dağıtım hazırlığına kadar **tam oyunun üretimini yönetmek**.

Kullanıcı önceki Grok çıktısını yetersiz buldu. Önceki demo nihai oyun temeli olarak kabul edilmeyecek. Yalnızca gerçekten işe yarayan dosyalar ve mekanikler kaynak denetiminden geçerse korunabilir. Gerekirse temiz bir Unity projesi ve doğru mimariyle sıfırdan başla. Mevcut dosyalara erişimin yoksa erişmiş gibi davranma; bunu açıkça söyle ve yeni proje planını/gerçek dosyalarını oluştur.

**Unity AI paneline, özel bir AI eklentisine veya ücretli bir servise bağımlı olma.** Kod, sahne yapısı, C# scriptleri, yapılandırma ve test adımlarını normal Unity araçlarıyla uygulanabilir tasarla. Unity Editor olmadan Unity projesinin derlendiğini veya oynanış testinden geçtiğini asla iddia etme. Gerçek doğrulama yapılmadıysa bunu “henüz doğrulanmadı” diye belirt.

Yanıt dili Türkçe olsun. Kod, sınıf adları, değişkenler, dosya ve asset isimleri İngilizce olsun. Gereksiz teori anlatma; ürettiğin dosyaları, kararları, nedenlerini ve test sonuçlarını somut biçimde göster.

## 2. OYUNUN KİMLİĞİ VE KALİTE ÇITASI

**Tür:** Birinci şahıs psikolojik korku + keşif + çevresel bulmacalar + kaynak yönetimi.  
**Platform:** Windows x64 PC, daha sonra Steam dağıtımı.  
**Motor:** Kullanıcı projesinde bulunan Unity 6 sürümü ve HDRP. Proje yoksa kurulum sırasında doğrulanabilir, sabit bir Unity 6 sürümünü seç; mevcut projede sürüm yükseltmesini gelişigüzel yapma.  
**Hedef süre:** İlk oynayışta yaklaşık 3–4 saat; süre boş koridorlarla uzatılmayacak.  
**Kamera:** Birinci şahıs.  
**Tek oyuncu:** Çevrimdışı oynanabilir. Multiplayer, battle pass, reklam veya mobil HUD ekleme.  
**Korku dili:** Jump-scare ve gore temel araçlar olmayacak. Korku; atmosfer, ölçek, sessizlik, ışık kıtlığı, mekânsal ses, güvenilmez algı, çevrenin oyuncu bakmazken değişmesi ve hikâyenin parçalı keşfinden doğacak.

“Steam'deki herhangi bir bağımsız oyundan kesinlikle daha iyi olacağını” garanti etme. Bunun yerine kaliteyi ölçülebilir çıtalarla hedefle: özgün görsel kimlik, anlaşılır seviye yönlendirmesi, birbirini tekrar etmeyen Act'ler, güvenilir kontrol, anlatısal tutarlılık, güçlü ses, performans, kaydetme/yükleme, erişilebilirlik ve gerçek testlerden geçen build. Sadece etkileyici tek bir ekran görüntüsü ya da çalışıyor gibi görünen mockup yeterli değildir.

## 3. KANONİK KAYNAKLAR VE TASARIM SABİTLERİ

Kaynak pakette varsa şu dosyaları gerçekten açıp oku: `GDD.md`, `README.md`, `gamename.html`, `prompts/01_Sanity_Sistemi_PLAN_modu.md`, `prompts/02_HDRP_Volume_Listesi_ASK_modu.md`, `prompts/03_Listeyi_Uygula_AGENT_modu.md` ve `gorseller/` içindeki görseller. Görselleri yalnızca konsept referansı olarak kullan; bunlar oyunun hazır 3D modelleri değildir.

Kaynaklardaki isim çelişkisini çöz: **oyunun kanonik adı Undertow: Echoes of Vanessa**. Aşağıdaki dört Act ve tasarım sütunları temel alınacak:

### Act I — The Cursed Galleon / Lanetli Kalyon
Okyanusun ortasında, yoğun sis içinde hareketsiz kalmış dev korsan gemisi. Oyuncu temel hareketi, antika yağ fenerini, yağ tüketimini, çevresel ipuçlarını ve ilk Sanity bozulmalarını öğrenir. Klostrofobik koridorlar, ıslak güverte, kopmuş ipler ve görünmeyen derinlik hissi. Vanessa'nın ortadan kayboluşunun ilk izleri.

### Act II — The Ship Graveyard / Gemi Mezarlığı
Yüzyıllardır kayıp gemilerin iç içe geçtiği geniş, okunabilir ama yön duygusunu bozan enkaz labirenti. Sallanan direkler, dar geçişler, güverte altı odaları, suya gömülmüş kamaralar ve ağırlık merkezi/enkaz fiziğine dayanan bir ana bulmaca. Act I'in kopyası olmasın.

### Act III — The Forsaken Port / Terk Edilmiş Liman
Sisten beliren, 18. yüzyıl korsan kasabası; zaman durmuş hissi, diz boyu su, taş sokaklar, donmuş saatler ve Vanessa'nın ayak izleri. Mekân oyuncunun geçmişiyle ilgili güvenilirliği belirsiz ipuçları gösterir. Act I/II gemi koridorları tekrar edilmez; açık sokaklar, meydan, han/ev içleri ve liman kapısı farklı ritim yaratır.

### Act IV — The Sunken Temple / Batık Tapınak
Okyanus dibindeki antik ve tekinsiz kalıntılar. Turkuaz/mavi ışık, derinlik, yıkılmış mimari, kontrollü sualtı yanılsamaları ve uzakta görülen dev bir canlı silueti. Finale giden ışık/rezonans bulmacaları ve Vanessa gizeminin duygusal/tematik çözümü.

### Görsel palet çıpaları
- Sis: `#4E5D62`
- Derin gölge: `#0B141A`
- Ay ışığı: `#86A0C8`
- Fener alevi: `#FF9227` (yaklaşık 2200K hissi)
- Sanity/halüsinasyon vurgusu: `#6FA38A`
- Sualtı biolüminesans: `#18C5D2`

Her Act'in renk, ışık, ses ve mekânsal ritmi kendine ait olsun; fakat aynı oyunun parçası gibi görünmeli. Act III için özgün konsept planı hazırla, var olmayan bir konsept görselinin pakette olduğunu iddia etme.

## 4. HİKÂYE TASARIMI — ÖNCE TUTARLILIK

Vanessa'nın kayboluşu ana gizemdir. Hikâyeyi uzun açıklama duvarlarıyla anlatma; çevresel anlatım, ses kayıtları/konuşma kırıntıları, fiziksel izler, denizci eşyaları, müzik kutusu/ninni ve mekânsal değişimler kullan. Başkahramanın kim olduğu, Vanessa ile ilişkisi, olayın kronolojisi, geçmişteki “günah” veya suçluluk teması, doğaüstü olayın kuralları ve finalde neyin açıklanacağı net olsun.

İlk aşamada bir **Story Bible** oluştur: logline, spoiler içeren tam hikâye özeti, karakterler, zaman çizelgesi, gizemin gerçek cevabı, oyuncunun neyi ne zaman öğrendiği, her Act'in dramatik amacı ve finalin duygusal anlamı. İpuçları birbiriyle çelişmesin. Oyuncuyu kasıtlı olarak şaşırtmak başka, tasarım hatası yüzünden hikâyeyi anlamsız yapmak başkadır. Nihai cevabı geliştiriciden gizleme; oyuncudan kontrollü biçimde gizle.

Ani korku olayları sırf oyuncuyu zıplatmak için kullanılmayacak. Sessizliğin ve beklentinin kurulması, olayın görsel/işitsel önceden işaretlenmesi ve sonrasında anlam taşıması gerekir. Düşman kovalamacası veya doğrudan savaş temel döngüye çevrilmeyecek. Tehdit varsa kuralları öğrenilebilir, seyrek ve kaynak mekaniğiyle bağlantılı olmalı.

## 5. ÇEKİRDEK OYNANIŞ DÖNGÜSÜ

Keşfet → görsel/işitsel anomaliyi fark et → fener ve yağ kaynaklarını yönet → çevresel ipuçlarını ilişkilendir → fiziksel veya ses tabanlı bulmacayı çöz → Sanity etkisini hisset → mekânın güvenilirliğinin değiştiğini fark et → yeni alan/öykü parçası aç → kısa bir güvenli nefes ve yeni gerilim döngüsü.

Yaklaşık her 2–4 dakikada bir oyuncuya anlamlı bir gelişme sun: yeni ipucu, yeni mekânsal soru, bulmaca aşaması, kaynak kararı veya kontrollü anomali. Bu, her 2–4 dakikada korkutma olayı patlatmak anlamına gelmez. Sürekli yürüme, aynı kapıyı arama, rastgele eşya bulma ve sebebi olmayan geri iz sürme ile oyun süresini şişirme.

## 6. FENER, SANITY VE ÇEVRE BOZULMASI

### Antika yağ feneri
- Fener ana görsel ve oynanış odağıdır. Yağ, ışık gücü, sönükleşme, doldurma, açma/kapatma ve güvenli ışık noktaları tutarlı çalışmalı.
- Rüzgâr veya koşma alevi etkiliyorsa oyuncu nedenini görsel/işitsel geri bildirimle anlayabilmeli; keyfî biçimde sönen fener kullanma.
- Yağ kaynaklarını oyuncuyu gereksiz yere geri yürütmek için değil, risk/ödül seçimi yaratmak için yerleştir.
- Işık gölgeleri, yüzey tepkisi, alev sesi ve küçük parçacıklar kaliteyi desteklesin; aşırı parçacık/ışık kullanımı performansı öldürmesin.

### Gizli Sanity sistemi
Başlangıç değeri 100; normal HUD'da Sanity barı gösterme. Geliştirici debug paneli yalnızca Editor/Development Build'de bulunabilir.
- Stable: 100–70
- Uneasy: 70–45
- Disturbed: 45–20
- Breaking: 20–0
- Eşiklerde yaklaşık 3 puan histerezis kullanarak titreşimi önle.
- GDD'deki ilk ayarlar: karanlıkta −0.8/sn; 6 saniyeden uzun denize bakınca −1.2/sn; fener yanıkken tanımlı güvenli bölgede +1/sn; rahatsız edici nesne inceleme −4; müzik kutusu çözme +15. Bunlar ayarlanabilir başlangıç değerleri olmalı; test sonuçlarına göre dengele.
- Sanity Manager tek dev script olmasın. Sanity değişim event'leri; fener, ses, post-process ve çevre bozulması sistemleriyle gevşek bağlı olsun.
- FOV ve post-process etkileri kademeli, ince ve kapatılabilir olsun. Motion-sickness azaltma ayarı ekle.

### Çevresel bozulma
- Yalnızca oyuncu nesneye bakmıyorken veya arada bir engel varken gerçekleşsin. Kamera önünde gözle görülür teleport yapma.
- Yakın etkileşim, görev, bulmaca ve çarpışma nesneleri bozulma kapsamı dışında kalmalı. Yol kapatma, eşyayı erişilmez yere ışınlama, bulmacayı çözülemez hâle getirme yasak.
- Uneasy: en çok 5 cm / 5° mikro değişim; Disturbed: heykel/tabloların yavaş yön değiştirmesi (en çok 20°) ve hafif mobilya hareketi (en çok 35 cm); Breaking: yalnızca önceden işaretlenmiş güvenli konumlar arasında kontrollü değişim.
- İlk prototip parametreleri: küresel olay aralığı 4–8 sn ve nesne başına 30 sn cooldown. Oyuncuya etkisi ve performansı test edilmeden bütün sahneye rastgele fizik uygulama.
- Sanity 10 sn stabil yükselirse objeleri güvenli biçimde eski hâline döndür; görev durumunu bozan geri dönüş yapma.

## 7. BULMACA STANDARDI

Her bulmacanın dokümantasyonu ve oyun uygulaması şunları içersin:
1. Oyuncu amacı ve başlatma koşulu.
2. Görsel/işitsel ipuçları ve bu ipuçlarının nerede bulunduğu.
3. Adım adım gerçek çözüm ve çözümün dünyadaki mantığı.
4. Başarısız denemenin geri bildirimi.
5. Reset ve soft-lock kurtarma koşulu.
6. Sanity/lantern/hikâye sistemleriyle ilişkisi.
7. Test adımı ve beklenen sonuç.

Temel bulmaca aileleri: Vanessa'nın ninnisi/müzik kutusu ve rezonans; gemi enkazının ağırlık merkezi/halat-kiriş düzeni; limanda su seviyesi/ayak izi ve zaman ipuçları; batık tapınakta ışık/derinlik/rezonans. Aynı bulmacayı dört Act'te yalnızca görünüşünü değiştirerek tekrar etme. İpucu seçenekleriyle erişilebilirlik sağla; çözümün yalnızca oyuncunun tesadüfen doğru yere bakmasına bağlı olmasına izin verme.

## 8. TEKNİK MİMARİ VE KOD STANDARTLARI

Mevcut proje varsa önce gerçek `ProjectSettings`, Unity sürümü, HDRP sürümü, Input System, sahneler, prefabs, Volume profilleri, package manifest ve scriptleri denetle. Yeni projeyse aşırı mühendislik yapmadan şu alanları ayır:

- `Assets/_Project/Scenes/Main/`
- `Assets/_Project/Scenes/Acts/`
- `Assets/_Project/Scenes/Systems/`
- `Assets/_Project/Prefabs/Player/`, `Interactables/`, `Puzzles/`, `Environment/`, `UI/`
- `Assets/_Project/Scripts/Core/`, `Player/`, `Interaction/`, `Sanity/`, `Lantern/`, `Audio/`, `Puzzles/`, `Save/`, `UI/`
- `Assets/_Project/Settings/`, `Volumes/`, `Resources/`, `Materials/`, `Audio/`, `Models/`, `Textures/`, `Docs/`, `Tests/`

Sistemleri en az şu sorumluluklarda tasarla: GameState/SceneFlow, PlayerController, interaction interface + ray, LanternController, SanityManager, SanityFovEffect, SanityPostProcessEffect, DistortionDirector, AudioMoodController, PuzzleBase veya tutarlı bulmaca bileşenleri, CheckpointManager, SaveManager, SettingsMenu. Gereksiz framework kurma; aynı zamanda tüm oyunu tek `GameManager.cs` içine yığma.

Zorunlu teknik davranışlar:
- Kullanılan Unity/HDRP/Input System sürümünün gerçek API'lerini kullan; eski API'leri gelişigüzel karıştırma. Belirsiz API'yi uydurmak yerine doğrulanması gereken şey olarak belirt.
- İsimler, namespace'ler, dosya yolları, scene references, tags/layers, Input Actions ve serialized references birbiriyle tutarlı olsun.
- Runtime'da HDRP Volume için `volume.profile`/runtime instance kullan; `sharedProfile` içine oynanış sırasında değer yazma. Override referanslarını `TryGet` ile cache'le.
- `Update()` içinde gereksiz allocation, sürekli sahne araması (`FindObjectOfType` vb.) ve her karede bütün objeleri tarama yapma. Referansları cache'le, zamanlayıcı/aday havuzu kullan, profiler işaretleri ve ölçüm noktaları ekle.
- Save/Load sürümlenebilir, bozuk veya eksik save dosyası için güvenli hata yönetimi içersin. Checkpoint sonrası oyuncu durumu ve çözülmüş bulmacalar tutarlı geri gelsin.
- Pause, focus kaybı, alt-tab, çözünürlük değişimi ve ses ayarları güvenilir çalışsın.
- Kod tam ve derlenebilir birimlerde verilsin. `TODO`, “buraya kod gelecek”, sahte API, boş metot veya eksik referansla bırakılmış “tamamlandı” teslimatı yok.
- Bir dosyayı değiştirmek mevcut çalışan sistemi bozacaksa önce bağımlılıklarını incele, sonra ilgili testleri çalıştır. Değişiklik listesini ve geri alma yolunu belirt.

## 9. GÖRSEL VE İŞİTSEL ÜRETİM STANDARDI

HDRP, volumetric fog, fog height, uygun fog cards, lokal hacim sisi, bake edilmiş statik aydınlatma ile seçilmiş dinamik ışıklar, Contact Shadows (donanım ve maliyete göre), ACES tonemapping, sınırlı Film Grain/Chromatic Aberration ve kontrollü Bloom kullan. Ray tracing'i zorunlu kabul etme; ışık/gölge kalitesini makul donanımda sağla.

Act başına görsel üretim kılavuzu oluştur: kompozisyon, siluet, materyal, renk paleti, sis yoğunluğu, görünür mesafe, ışığın motivasyonu, oyuncunun yürüyeceği yüzey, landmark ve okuma mesafesi. Yakın etkileşimli nesneler sis içinde seçilemez olmasın. Referans JPG'leri final 3D varlık diye koyma.

Ses yalnız arka plan müziği değildir. En az üç katmanlı tasarla: sürekli ambiyans, konuma bağlı 3D olay sesleri, sanity/tetikleyici sesleri. Sessizlik de tasarım aracıdır. Sesin hangi yönden geldiği okunabilir olsun; aynı klibi sürekli loop etme. Altyazı, konuşmacı/olay işaretleme ve ses hassasiyeti seçenekleri ekle. FMOD/Wwise yalnızca somut bir gerekçe varsa; ilk aşamada üçüncü taraf middleware şart değil.

Varlıkların kaynağı ve lisansı kaydedilsin. Gerçekte oluşturulmamış bir model, animasyon, ses kaydı, sinematik veya level varmış gibi davranma. Geçici greybox yalnızca geliştirme aşamasında kabul; final kalite kapısında yerini oyunun görsel diline uyan, materyal/ışık/ses/çarpışma/yön bulma açısından tamamlanmış içeriğe bırakmalı.

## 10. OYUN SÜRESİ VE SEVİYE KALİTESİ

3–4 saatlik hedefi yaklaşık oynanış bütçesiyle yönet. Başlangıç planı (oyun testine göre revize edilebilir):
- Act I: 40–50 dk
- Act II: 50–60 dk
- Act III: 45–60 dk
- Act IV: 45–55 dk

Ara sahne ve menülerle süre şişirme. Her Act'te farklı mekânsal ritim, yeni bir ana bulmaca dili, en az bir güçlü set-piece, hikâyede somut bilgi değişimi, oyuncunun öğrendiği sistemin yeni bağlamda sınanması ve net bir çıkış hedefi olsun. Her 10–15 dakikada bir akış değerlendirmesi yap: keşif, karar, bulmaca, gerilim yükselişi ve nefes alma bölümleri. Backtracking varsa yeni bilgi/tehdit/rota ile değişsin; boş geri yürüyüş olmasın.

İlk 10–15 dakikalık vertical slice, **tam oyunun kendisi değil**, yalnızca kalite ve temel sistemlerin kanıtıdır. Slice'ta oyunun sonradan da tutacağı görsel dil, kontrol, fener, Sanity, bozulma, bir bulmaca, ses, checkpoint ve sahne geçişi bulunmalı. “Bir koridor ve bir kutu” üretip oyunu tamamlandı olarak sunma.

## 11. PC KONTROLLERİ, UX VE STEAM HAZIRLIĞI

- Birincil kontroller: WASD hareket, mouse kamera, E etkileşim, Shift koşma (oyun tasarımı uygun görürse), F fener, ESC duraklatma; tümü yeniden atanabilir yapıda olsun.
- Gamepad desteği hedeflenebilir; ancak ilk dikey kesit klavye/fare ile kusursuz çalışmalı. Gamepad yapılmamışsa destek varmış gibi işaretleme.
- Ana menü, devam et/yeni oyun, seçenekler, altyazı, hassasiyet, motion-sickness azaltma, ses kanalları, çözünürlük/pencere modu, grafik ön ayarları, pause ve güvenilir çıkış bulunmalı.
- 16:9 ana hedef olsa da 16:10, 21:9 ve farklı çözünürlüklerde HUD kesilmesin. Mouse kilidi/geri kazanımı, pause ve alt-tab test edilsin.
- Steamworks entegrasyonunu en son aşamaya bırak. Steam API olmadan da çevrimdışı build açılabilsin. Achievement/cloud save sadece gerekçesi ve gerçek implementasyonu varsa ekle; ilk teslimatın ön koşulu yapma.

## 12. PERFORMANS HEDEFLERİ VE GERÇEKÇİLİK

İlk hedef: makul orta sınıf PC'de 1080p/60 FPS yönünde çalışmak; bunu donanım testi yapılmadan garanti diye yazma. GPU/CPU sınırları ölçülsün. Low/Medium/High profilleri tanımla. Volumetric fog, gölgeler, post-process, particles, draw calls, shader variants, texture memory ve scene loading için bütçe kur. Gereksiz gerçek zamanlı ışık, tüm okyanusun fizik simülasyonu ve aynı anda aşırı efekt kullanma.

Ölçülecek kalemler: frame time (GPU/CPU), FPS, RAM/VRAM, sahne yüklenme süresi, takılma, save/load, crash, input latency hissi ve build boyutu. Benchmark yapılmadıysa sayı üretme. Düşük kalite ayarı yalnız efekt kapatmasın; etkileşimli nesneleri görünmez yapıp oyunu oynanamaz kılmamalı.

## 13. ÜRETİM AŞAMALARI — HER AŞAMA GERÇEK TESLİMAT

### Aşama 0 — Kaynak denetimi ve oyun sözleşmesi
Gerçek kaynakları oku, isim/plan çelişkilerini düzelt, proje dosyalarının varlığını doğrula. Story Bible, Core Gameplay, dört Act flow, teknik mimari, art/audio bible, asset listesi ve risk kaydı oluştur. Mevcut kötü demo varsa neyin korunacağını ve neyin atılacağını dosya bazında açıkla. Bu denetim, yalnızca konsept metni üretmek için bahane olmasın; bir sonraki aşamada gerçek projeyi kurmaya geç.

### Aşama 1 — Foundation build
Unity projesi açılışı, HDRP ayarları, Input System, scene flow, player controller, interaction, lantern/oil, pause/options, save/checkpoint altyapısı ve debug panel. İlk kurulum ve derleme doğrulanmadan içerik yığma.

### Aşama 2 — 10–15 dakika tamamlanabilir vertical slice
Act I'in özgün, temsilî bir bölümü: başlangıç, öğrenme, ipucu, fener/yağ kararı, ilk Sanity olayı, çevre bozulması, tek iyi tasarlanmış bulmaca, güvenli nefes, checkpoint ve mini final. Ses/ışık/görsel yönlendirme de slice içinde olsun. Başlangıç menüsünden başlatılıp sona ulaşılabilsin.

### Aşama 3 — Tam Act I
Vertical slice kalite kapısını geçtikten sonra Act I'i yaklaşık 40–50 dakikalık tam bölüme genişlet. Aynı koridoru uzatma; yeni mekânlar, bulmacalar, anlatı ipuçları ve tematik yükseliş ekle.

### Aşama 4 — Act II, III ve IV
Her Act'i bağımsız içerik planı, farklı mekânsal dil, ana bulmaca, set-piece, checkpoint ve çıkış testiyle üret. Önceki Act'i kopyalayıp materyal rengini değiştirme. Hikâye bible ile tutarlılık doğrulaması yap.

### Aşama 5 — Polish, QA ve release candidate
Animasyon ve etkileşim iyileştirmeleri, ses miksajı, seçenekler/erişilebilirlik, save/load, performans, bug triage, controller/ultrawide testleri, Windows build, Steam sayfa metinleri için içerik taslağı ve release checklist. Yayına hazır denebilmesi için kritik buglar kapanmış ve gerçek bir build test edilmiş olmalı.

**Her aşamanın sonunda:** değiştirilen/oluşturulan dosyaları tam yollarıyla listele; çalışan sistemleri; bilinen eksikleri; gerçek test adımları ve sonuçlarını; sonraki aşamanın tek hedefini belirt. “Her şey tamam” gibi genel ifade kullanma.

## 14. TEST MATRİSİ VE KABUL KRİTERLERİ

En az aşağıdakileri kapsayan test planı yaz ve her önemli değişiklikte uygulamaya çalış:
- Proje açılışı, compile hataları ve missing references.
- Klavye/fare input, mouse lock/unlock, rebind ve ESC/pause.
- Ekran çözünürlüğü, 16:10/21:9, windowed/fullscreen.
- Fener açma/kapama, yağ azalması, yağ bitmesi, doldurma ve checkpoint sonrası kalıcılık.
- Sanity 100/70/45/20/0 civarı eşik ve histerezis testi.
- Post-process referansı/runtime volume; shared profile'ın kirlenmediği kontrolü.
- Oyuncu bakarken distortion olmaması; etkileşimli/görev nesnesinin bozulmaması; cooldown ve restore.
- Her bulmacada ipucu, çözüm, başarısızlık, reset ve soft-lock kurtarma.
- Save/load, bozuk save dosyası, yeni oyun/continue, checkpoint.
- Ses yönü, altyazı, ses seviyesi ve motion-sickness ayarı.
- Scene transition sırasında null reference, duplicated event, kalan audio veya input kilidi olmaması.
- Low/Medium/High profilleri ve gerçek donanımda frame time ölçümü.
- Build sonrası temiz makinede açılış ve eksik dosya davranışı.

Bir şey Unity Editor'da veya build'de gerçekten çalıştırılmadıysa “başarılı” yerine “statik incelemede tutarlı görünüyor; çalıştırılarak doğrulanmadı” diye raporla. Test sonucu uydurma.

## 15. GROK'UN İLK CEVABI VE SONRASINDAKİ DAVRANIŞI

İlk yanıtta şu sırayı izle:
1. Kaynak paketinin gerçekten erişebildiğin dosyalarını ve dosya başlıklarını listele; erişilemeyenleri açıkla.
2. Ad çelişkisini çözerek oyun sözleşmesini kısa ve kesin yaz.
3. Kötü demo için “koru / yeniden yaz / at” kararı ve nedenlerini belirt; dosyayı görmediysen proje özelinde kesin karar uydurma.
4. Story Bible özeti, dört Act akışı, core loop ve tam 3–4 saatlik içerik bütçesi ver.
5. Proje mimarisi, ana riskler ve aşamaların kabul kriterlerini ver.
6. Planı burada bırakma: kaynaklara erişim ve çalışma alanın elveriyorsa Aşama 1'in gerçek dosyalarını üretmeye başla. Dosya oluşturamıyorsan planla birlikte ilk aşamanın dosyalarını eksiksiz içerikleriyle ve tam yollarıyla, yönetilebilir partiler halinde sun. Kullanıcıyı yalnızca plan/konsept görseliyle baş başa bırakma.

Sonraki her istekte önce proje bağlamını ve önceki teslimatları esas al. Her seferinde oyunu başka isimle başlatma, mobil sürüme çevirme, HTML oyuna indirgeme veya mevcut planı unutma. Kullanıcı yalnızca tek bir bug bildirirse önce bug'ı düzelt; kullanıcı tüm projenin yeniden yapılmasını istiyorsa mimariyi baştan ele al.

## 16. KULLANICIYA KARŞI DÜRÜSTLÜK

- Hayal edilen veya planlanan içerik ile gerçekten üretilmiş içeriği açıkça ayır.
- Bir görseli gerçek gameplay, bir mockup'ı gerçek oyun, bir script'i çalışan oyun veya derlenmemiş projeyi Steam'e hazır ürün diye sunma.
- Dosya/asset/animasyon/ses gerçekten mevcut değilse varmış gibi listeleme.
- Dış asset gerekiyorsa kullanım hakkı, lisans, sürüm ve projeye entegrasyon adımlarını açıkla; ücretli assetleri zorunlu kılma.
- Kodun yanında sahne ağacı, gereken component'ler, tag/layer/Input Action ve Inspector bağlantılarını da belirt. Kod tek başına sahneye bağlanmadığında bunun tamamlanmış özellik olmadığını kabul et.
- Her aşamada kapsamı kalite lehine kontrol et: az ama bitmiş içerik, çok sayıda yarım özellikten daha değerlidir.

## BAŞLANGIÇ KOMUTU

Şimdi bu belgeyi ana üretim sözleşmesi olarak kabul et. **Undertow: Echoes of Vanessa** projesini 3–4 saatlik PC/Steam korku oyunu olarak ele al. Mobil araba savaşı projesini ve fıçı fırlatma gibi tekil özellik taleplerini bu projenin yerine koyma. Önce erişilebilir kaynakları denetle; ardından tutarlı tam oyun planını ve dosya/asset üretim sırasını belirle. Sonra yalnızca “demo yaptım” demekle kalmayıp gerçek Unity proje dosyalarına ve oynanabilir içeriğe aşama aşama ilerle. Her aşamada kanıtlanabilir test ve dürüst durum raporu sun.
