# Undertow: Echoes of Vanessa — Oyun Tasarımı ve Yapılacaklar

**Tür:** Birinci şahıs psikolojik korku, keşif, çevresel bulmaca · **Platform:** Windows (Steam), sonra Steam Deck · **Motor:** Unity 6 + HDRP · **Süre:** 3–4 saat, 4 Act · **Dil:** İngilizce seslendirme, 10 dilde altyazı

## 1. Çekirdek döngü
Keşfet → bir ses/ışık anomalisini fark et → fener yağını yönet → ipuçlarını birleştir → bulmacayı çöz → akıl sağlığı (gizli) değişir, mekân değişir → yeni alan açılır. Her 2–4 dakikada yeni bir ipucu ya da soru, ama her 2–4 dakikada bir korkutma değil.

## 2. Sistemler
| Sistem | Kısa tanım |
|---|---|
| Fener ve yağ | Tek sıcak ışık kaynağı (#FF9227). Yağ biter, doldurulur. Rüzgârda alev titrer, sebebi görülür. |
| Gizli akıl sağlığı | 0–100, ekranda gösterilmez. Karanlık, denize uzun bakmak ve rahatsız edici nesneler düşürür; güvenli ışık yükseltir. Kademeleri: Stable / Uneasy / Disturbed / Breaking. |
| Çevre bozulması | Sadece oyuncu bakmıyorken nesneler yer değiştirir. Görev nesneleri ve yollar asla bozulmaz. |
| Echo / Mimic | İki Vanessa sesi. Echo ninniyi yanlış söyler ve doğru yola götürür, Mimic doğru söyler ve tehlikeye götürür. Oyunun temel ipucu. |
| Dinleme tuşu | Basılı tutunca dünya sessizleşir, sesler belirginleşir. Yön bulma ve bulmaca aracı. |
| Bulmacalar | Web prototipinde denenmiş olanlar korunur (bkz. `03_PROTOTYPE_AUDIT_TR.md`): Act I beş çan / müzik kutusu (La-Mi-Do-Mi-La), kanca ve iki kasa · Act II fener sayısı = nota (tek La, iki Do, üç Mi) ve çan teknesi · Act III kule saatinin üç kadranı (11:40, 03:17, 06:05) → su çekilir · Act IV dört ayna ve prizma ile ışın. Her biri için ipucu, sıfırlama levyesi ve takılma kurtarması var. |
| Kayıt | Checkpoint ile otomatik kayıt, 3 kayıt yuvası, bölüm seçimi (bitirince açılır), Steam Cloud. |
| Sonlar | Answer / Surface / Undertow (bkz. Story Bible). |

## 3. Ses motoru
- **FMOD Studio + Steam Audio eklentisi.** HRTF ile kulaklıkta 3D ses, duvar arkası boğuklaşma (occlusion), mekâna göre yankı. FMOD'un bağımsız geliştirici lisans şartlarını başlamadan resmi sitesinden kontrol et.
- **Katmanlar:** ortam (deniz, rüzgâr, gemi gıcırtısı) · ruh hâli müziği (akıl sağlığı kademesine göre) · Echo/Mimic sesleri · fener çıtırtısı · nefes ve kalp atışı (Breaking kademesinde).
- **"Sessizlik" bir ses olayıdır:** Önemli anlardan önce ortam sesi 2 saniye kesilir.
- **Yayıncı modu:** Tüm müzik özgün ve telifsiz. Ayarlarda "Yayıncı modu" açılınca lisanslı bir parça varsa susturulur (hedef: hiç olmaması).

## 4. Altyazı ve yerelleştirme
- Unity Localization paketi. Kaynak dil İngilizce.
- **Altyazı ayarları:** boyut (4 kademe), arka plan opaklığı, konuşmacı adı ve rengi, **[ses açıklamaları]** (ör. *[uzaktan ninni]*), yön oku (sesin geldiği yön).
- **Echo ve Mimic farklı yazı stiliyle** gösterilir (Echo italik, Mimic düz). Sağır ve az işiten oyuncular da ipucunu alabilsin diye.
- **Çıkış dilleri:** İngilizce, Türkçe, Basitleştirilmiş Çince, Rusça, İspanyolca (Latin Amerika), Brezilya Portekizcesi, Almanca, Fransızca, Japonca, Korece.

## 5. Yayıncılar için tasarım
- Her Act tek yayında bitecek uzunlukta (45–60 dk), sonunda bir **"klip anı"**.
- Gizli akıl sağlığı ve üç son → yorumlarda tartışma, ikinci oynanış.
- **Fotoğraf modu**, HUD gizleme, alan derinliği.
- Kontrollü ses seviyeleri: çığlık ve ani ses yayıncının kulağını patlatmasın (dinamik sınırlayıcı).
- Ölüm yok veya çok nadir: tekrar oynatma cezası yayını öldürür. Başarısızlık mekânın değişmesi ya da geri sürüklenmeyle olur.
- Twitch entegrasyonu (izleyici oylamasıyla fener titrer vb.) **çıkıştan sonra**.

## 6. Yapılacaklar (sırasıyla)

### Faz 0 — Kurulum (1 hafta)
- [ ] Unity 6 LTS + HDRP projesi, Git LFS, `.gitignore`
- [ ] FMOD + Steam Audio entegrasyonu, Unity Localization paketi
- [ ] Klasör yapısı: `Assets/_Project/{Scenes,Scripts,Prefabs,Audio,Art,Localization}`
- [ ] Gri kutu test sahnesi, Steamworks.NET (Steam Cloud ve başarımlar için)

### Faz 1 — Act I dikey kesit (4–6 hafta) · **en önemli faz**
- [ ] Oyuncu hareketi, etkileşim, fener ve yağ
- [ ] Gizli akıl sağlığı ve çevre bozulması
- [ ] Echo/Mimic ses sistemi, dinleme tuşu
- [ ] Müzik kutusu bulmacası, checkpoint ve kayıt
- [ ] Altyazı sistemi (EN + TR)
- [ ] Act I tam görsel ve ses kalitesinde, 45 dk
- **Çıkış ölçütü:** 5 kişi oynadı, kimse takılmadı, kayıt ve yükleme sağlam, orta sistemde 60 FPS.

### Faz 2 — Steam sayfası ve demo (2 hafta)
- [ ] Steam Direct ücreti ($100), mağaza sayfası, kapsül görselleri, fragman
- [ ] Act I'den 20 dakikalık demo
- [ ] Steam Next Fest'e başvuru, yayıncılara demo anahtarı

### Faz 3 — Act II–IV (3–5 ay)
- [ ] Her Act: gri kutu → oynanış testi → sanat → ses → altyazı
- [ ] Üç son, bölüm seçimi, başarımlar (en az 20)

### Faz 4 — Cila ve çıkış (1–2 ay)
- [ ] 10 dile çeviri (makine çevirisi yok)
- [ ] Erişilebilirlik ayarları, Steam Deck testi
- [ ] Performans: orta sistemde 60 FPS, düşük sistemde 30 FPS
- [ ] Mağaza sayfası çıkıştan en az 2 hafta önce "Yakında" olarak yayında olmalı (Steam kuralı)

## 7. Riskler
| Risk | Önlem |
|---|---|
| Kapsam büyür | Önce sadece Act I. Diğer Act'ler onaydan sonra. |
| Sanat bütçesi | Modüler gemi kitleri, Unity Asset Store'dan lisanslı paketler, fotogrametri dokular |
| Seslendirme maliyeti | Az diyalog: Echo, Mimic, Thorne, Ruth. Toplam ~15 dakika ses. |
| Korku sıkıcı olur | Her 2–4 dakikada yeni soru; boş koridor yok |
| HDRP performansı | Erken profil çıkarma, düşük kalite ön ayarı |
