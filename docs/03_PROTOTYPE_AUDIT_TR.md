# Web Prototipi Denetimi (14 Kasım 2026)

Bu depodaki çalışan oyun, Grok'ta yapılmış bir **tarayıcı prototipi** (React + three.js, `src/game/`). Unity'deki asıl oyun değil. Ama 4 Act'in yerleşimi ve bulmacaları burada oynanabilir hâlde, o yüzden **atılmıyor**. Unity'de gri kutu planı olarak kullanılacak.

Kanon sırası: `01_STORY_BIBLE.md` > `02_GAME_DESIGN_AND_TODO.md` > prototip metinleri. Bir çelişki varsa Story Bible geçerli.

## 1. Prototipte ne var

| Dosya | İçerik |
|---|---|
| `src/game/content.ts` (846 satır) | 4 Act, oda yerleşimleri (x, z, genişlik, kat), notlar, görev hedefleri, bulmaca verileri |
| `src/game/engine.ts` (1280 satır) | Hareket, fener ve yağ, gizli akıl sağlığı, bulmaca mantığı, Act geçişleri, son |
| `src/game/audio.ts` | Prosedürel ses (sadece referans; Unity'de FMOD) |
| `src/game/save.ts` | Bayrak (flag) tabanlı kayıt |
| `src/components/HorrorGame.tsx` | Menü, HUD, ayarlar, günlük, kartlar |
| `attachments/` | Grok ana talimatı (`UNDERTOW_GROK_MASTER_PROMPT_TR.md`), ana plan, referans görseller, eski "Abyssal Requiem" paketleri |

## 2. Unity'ye taşınacaklar (koru)

- **Act I kalyon yerleşimi:** kıç güverte → ana güverte → kıç merdiveni → top güvertesi → tayfa kamarası → ninni odası (güvenli) → Vanessa'nın kamarası → cerrah kamarası → pruva ambarı → yük ambarı → pruva çıkışı → pruva ucu. Ölçüler `content.ts` içinde (metre gibi okunabilir).
- **Act I bulmacası:** beş çan, sıra La-Mi-Do-Mi-La (duvar noktaları 1-3-2-3-5), müzik kutusu aynı beş notayı çalar; kanca cerrahın dolabında; iki kasa sancak iç halkaya → pruva sürgüsü kalkar; sıfırlama levyesi kapının yanında.
- **Act II bulmacası:** enkazlardaki fener sayısı notayı verir (tek = La, iki = Do, üç = Mi); çan teknesinde küçükten büyüğe çalınır; kasa sancak halkaya gidince geçit açılır.
- **Act III bulmacası:** beş bina (gümrük, taverna, antrepo, şapel, ev) sonra kule; kulenin üç kadranı. Saatler Story Bible'a göre: 11:40, 03:17, 06:05.
- **Act IV bulmacası:** dört ayna, her birinde çizik yönünde 1-2-3-1 kez çevirme; cam prizma olmadan ışın sunağa ulaşmaz; galerideki mercek en son alınır.
- **Sis renkleri ve yoğunlukları** (`ACTS`): Act I #4E5D62 / 0.052 · Act II #3A4A54 / 0.04 · Act III #3E4B52 / 0.036 · Act IV #0E3C44 / 0.046.
- **Act IV'teki varlık kuralı:** "O bir düşman değil. Işık kesilirse yaklaşır, vurmaz. Feneri tut." Story Bible'daki Undertow ile uyumlu, aynen kalır.
- Akıl sağlığı başlangıç değerleri: Grok talimatındaki tablo (karanlık −0.8/sn, 6 sn'den uzun denize bakma −1.2/sn, güvenli ışık +1/sn, rahatsız edici nesne −4, müzik kutusu +15).

## 3. Çelişkiler ve kararlar

| # | Prototipte | Story Bible | Unity'de yapılacak |
|---|---|---|---|
| 1 | Defter: "sis **02:17**'de indi" (`content.ts` 587, 599) | Her şey **03:17** (2009 polis raporu ~03:15) | Defter: "sis 03:17'de indi" |
| 2 | "**Vanessa dümeni bıraktı.**" (203) Vanessa'yı gemiyi yöneten bir yetişkin gibi gösteriyor | Vanessa 9 yaşında, 2009'da kayboldu | Defter Kaptan Thorne'un: "Dümeni bıraktım. Ninni hâlâ ambarda çalıyor. Denize uzun bakma. Feneri söndürme." |
| 3 | Vanessa'nın mektubu yetişkin diliyle (270) | Çocuk, çizim yapar, ninniyi yanlış söyler | Çocuk el yazısı, çizimli not: "Abi, kolyem pruvada kaldı. Beni denizde arama. Beni denizde bırakma." (EN: "Adrian, my necklace is at the front. Don't look for me in the sea. Don't leave me in the sea.") İpucu cümleleri (kanca, halkalar) ayrı bir nota taşınır |
| 4 | "Yağlıboya bir kadın, imza silinmiş" (215) | Isabel Crane, ninninin ilk bestecisi | Portre Isabel Crane'in; Act III'te imzanın tamamı bulunur |
| 5 | Act II'de enkaz denge/halat bulmacası yok | 02 belgesi denge/halat diyordu | Prototipteki fener-nota bulmacası esas alındı (02 belgesi güncellendi) |
| 6 | Platform: tarayıcı / mobil | Windows, Steam, Unity 6 HDRP | Prototip tasarım aracı olarak kalır; oyun HTML'e indirgenmez |

## 4. Depo düzeni (hiçbir şey silinmedi, öneri)

- `attachments/UNDERTOW_GROK_MASTER_PROMPT_TR.md`, `.txt` ve `-2.txt` **aynı dosya** (3 kopya). `..._PACK_TR.zip` ve `..._PACK_TR-2.zip` de aynı. Birer kopya yeterli; istersen sonra temizleriz. **20 Kasım 2026: kopyalar silindi.**
- `AbyssalRequiem_*.zip` eski çalışma adı. Arşiv olarak kalabilir.
- `.vercel/output/` (37 dosya) derleme çıktısı. Vercel'e bu depodan yayın yapılmıyorsa `.gitignore`'a eklenebilir.
- `.grok/`, `AGENTS.md`, `startup.sh`: Grok çalışma alanı dosyaları; prototipi Grok'ta açmak için gerekli, dokunulmuyor.
- **Unity projesi:** Bu depoya karıştırılmaz. Önerilen: ayrı bir **private** depo (`undertow-unity`), Git LFS ile. Bu depo tasarım + prototip olarak kalır.
- Depo herkese açıksa: `docs/01_STORY_BIBLE.md` bütün sonları anlatıyor. Steam çıkışından önce bu depo private yapılmalı ya da Story Bible private depoya taşınmalı.

## 5. Prototipi çalıştırma (bilgisayarda)

```
npm install
npm run dev      # http://localhost:8080
```
Not: Bu komutlar bu denetimde çalıştırılmadı; Grok çalışma alanı ayarlarına (`scripts/with-app-env.mjs`) bağlı.
