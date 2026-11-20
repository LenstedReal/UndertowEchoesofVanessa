# Undertow: Echoes of Vanessa — Unity 6 HDRP (v0.3.0)

Birinci şahıs, hikâye odaklı psikolojik korku oyunu. Dört bölüm (Lanetli Kalyon, Gemi Mezarlığı, Terk Edilmiş Liman, Batık Tapınak), üç son, on dil.

| | |
|---|---|
| Unity Editor | **6000.3.27f1** |
| Render pipeline | HDRP **17.3.0** (core, shadergraph, vfx 17.3.0) |
| Input | Input System 1.11.2 (yalnızca yeni sistem) |
| Hedef | Windows x86-64, DirectX 12 / DirectX 11 |
| Sürüm | `bundleVersion` 0.3.0 |

## Durum — dürüst özet

**Yapıldı ve burada doğrulandı**

- Bütün oyun kodu yeniden yazıldı: `Undertow.Logic` (Unity'den bağımsız saf C#), `Undertow.Runtime`, `Undertow.Editor`, `Undertow.Tests.EditMode`.
- Derleme kapısı: kod, Unity 6000.3.27f1 / HDRP 17.3.0 / Input System 1.11.2 / uGUI kaynaklarından üretilmiş API taslaklarına karşı derlendi. **Oyun (player) ve editör yapılandırmasında 0 hata, 0 uyarı.**
- Mantık testleri: **52/52 geçti.** Bunlar .NET 8 üzerinde, kendi küçük test koşucumla çalıştı; Unity'nin Test Runner'ında değil. Aynı testler Unity'de `Window > General > Test Runner > EditMode` altında da var.
- Hikâye yürüyüşü: dört bölümün bütün dalları (Echo/Mimic yolu, cevap/sessizlik, iki ninni seçimi) mantık katmanında baştan sona oynatıldı.
- Yerelleştirme: 10 dilin her biri 685 anahtarın tamamını içeriyor; yer tutucular, not uzunlukları ve kritik ipuçları test ediliyor.

**Yapılmadı / burada yapılamaz**

- Proje **Unity Editor'de açılmadı, Play'e basılmadı, Windows build'i alınmadı.** Bu ortamda Unity ve GPU yok. İlk açılışta hata çıkarsa bana tam metniyle gönder.
- Performans, görüntü kalitesi ve ses karışımı gerçek donanımda ölçülmedi.
- Seslendirme yok: konuşmalar şimdilik geçici "mırıltı" sesleri + altyazı. Müzik ve efektler oyun için üretildi (telifsiz, özgün).
- Türkçe dışındaki 8 dil yapay zekâ taslağıdır; yayından önce anadili konuşan biri okumalı.

Ayrıntılı liste: [`KNOWN_ISSUES.md`](KNOWN_ISSUES.md). Bu sürümde neyin değiştiği: [`CHANGES.md`](CHANGES.md).

## Sanal makinede test hakkında (önemli)

HDRP, compute shader destekleyen **gerçek bir DirectX 11/12 ekran kartı** ister. VirtualBox ve Hyper-V'nin varsayılan sanal ekran kartlarıyla oyun açılmaz, siyah ekranda kalır ya da hemen kapanır. Sanal makinede denemek için GPU paylaşımı/geçişi gerekir (Hyper-V GPU-P, Proxmox/VFIO gibi). En güvenilir test: gerçek bir Windows bilgisayar. Tahmini en az donanım (ölçülmedi): GTX 1060 / RX 580 sınıfı ekran kartı, 8 GB RAM.

## Projeyi açma

1. Unity Hub'dan **Unity 6000.3.27f1** kur; "Windows Build Support (IL2CPP)" modülünü ekle.
2. Unity Hub > **Add > Add project from disk** > `unity-project` klasörünü seç.
3. İlk açılış birkaç dakika sürer (paketler indirilir, dokular içe aktarılır).
4. Derleme bitince **kurulum kendiliğinden çalışır**: HDRP varlığını atar, sahneleri build listesine ekler, oyuncu ayarlarını (Linear renk, DX12 + DX11) yapar ve malzemeleri `Resources/Undertow/Materials` altına üretir. Çalışmazsa: **Undertow > Run Project Setup**.
5. **Undertow > Open Scene > Main Menu** ile ana menüyü aç ve **Play**'e bas. Bölümleri tek tek denemek için `Open Scene > Act I…IV`.
6. Sorun ararken: **Undertow > Validate Project** sahneleri, HDRP ayarını, yazı tiplerini, dokuları, malzemeleri, sesleri, 10 dilin anahtarlarını ve dört bölümün yürüyüşünü kontrol eder; sonucu Console'a yazar.
7. İlk açılışta Unity yeni dosyalar için `.meta` dosyaları üretir; bunları da commit'le.

## Windows build

- Editörden: **Undertow > Build Windows 64-bit Release Player** → `Builds/Windows64/Release/UndertowEchoesOfVanessa.exe`
- Geliştirme sürümü: **Undertow > Build Windows 64-bit Development Player**
- Unity Build Automation: `DevOps/unity_build_automation.yaml` (giriş noktaları: `UndertowAssetImportQuality.ConfigureAllProjectAssets`, `UndertowHDRPSceneBuilder.BuildAllScenes`, `UndertowBatchValidator.ValidateAll`, `UndertowBatchValidator.BuildWindows64Release`).

## Oyunda neler var

- **Dünya:** Sahneler boş kabuktur; dört bölümün 3B dünyası oyun açılırken koddan kurulur (`WorldBuilder` → geometri, dekor, eşyalar, engeller, ışık). Kalyonun gövdesi, direkleri, top güvertesi, kamaraları; enkaz labirenti, kıyı, çadır, çan teknesi; 1724 limanı, saat kulesi, Vali'nin konağı, aynalı salon; batık tapınak, balina galerisi, sunak.
- **Sistemler:** fener ve yağ, gizli akıl sağlığı (bulanıklık, fısıltı, kalp atışı, görmediğinde yer değiştiren eşyalar), dinleme ile yön bulma, Echo/Mimic sesleri, takılınca çocuğun sesiyle gelen ipuçları, kontrol noktalı kayıt (3 yuva), bölüm seçimi.
- **Bulmacalar:** beş çan (La-Mi-Do-Mi-La), kasa/halka, fener sayısı–nota, üç kadran (11:40 · 03:17 · 06:05), dört ayna ışın yolu, sunak ve son seçim.
- **Sonlar:** Cevap, Yüzey, Akıntı; ana menüde gizli dördüncü ninni.
- **Ayarlar:** dil (anında değişir), altyazı boyutu/arka planı, konuşmacı adları, ses açıklamaları, yön okları, FOV (70–120°), kafa sallanması, ekran sarsıntısı, hareket bulanıklığı, akıl sağlığı efekt şiddeti, koş/eğil/dinle aç-kapa, mono ses, ses sınırlayıcı, tuş atama (klavye + kumanda), parlaklık ayar ekranı, ekran modu, çözünürlük, V-Sync, FPS sınırı, kalite, kenar yumuşatma, film greni, renk sapması, alan derinliği, sürüm numarası, **yayıncı modu** (dosya yolundaki kullanıcı adını gizler, ses tepelerini sınırlar).
- **Fotoğraf modu:** zaman durur; FOV, yatış, pozlama, odak, 4 filtre; klavye ve kumanda; fotoğraflar `Resimler/Undertow` klasörüne.
- **Ekstralar:** 28 başarım, çizim galerisi, görülen sonlar, müzik kutusu.

## Diller

`Assets/_Project/Resources/Undertow/Loc/<kod>.txt` — biçim: `anahtar = değer`, `\n` satır sonu, `#` yorum. İngilizce kaynak metin; diğer diller anahtar bazında İngilizceye düşer (ama hepsi şu an eksiksiz).

| Dil | Durum |
|---|---|
| English, Türkçe | Tam, elden yazıldı |
| Deutsch, Français, Español (Latinoamérica), Português (Brasil), Русский, 简体中文, 日本語, 한국어 | Tam ama **yapay zekâ taslağı** — anadili konuşan biri okumalı |

Çince, Japonca ve Korece için Windows'un kendi yazı tipleri kullanılır (Microsoft YaHei, Yu Gothic, Malgun Gothic).

## Klasörler

```
Assets/_Project/
  Scripts/Logic/     saf C#: içerik (4 bölüm), bulmacalar, akıl sağlığı, fener, sonlar, kayıt, yerelleştirme tablosu
  Scripts/Runtime/   Unity: dünya kurucu, oyuncu, ses, arayüz, sahne akışı
  Scripts/Editor/    kurulum, doğrulama, build, içe aktarma ayarları
  Tests/EditMode/    mantık + yerelleştirme testleri
  Resources/Undertow/  Audio (OGG), Textures (JPG), Fonts (OFL), Loc (10 dil), UI
  Scenes/            6 boş sahne kabuğu (dünya koddan kurulur)
```

`Resources/Undertow` içindeki dosyalar Git LFS'e girmez (iç içe `.gitattributes`); taze bir klondan oyun LFS indirmeden derlenir.

## Eski teslim belgeleri

`README_DELIVERY.md`, `DELIVERY_AUDIT.md` ve `Docs/` altındakiler 18 Kasım 2026 tarihli önceki teslimi (Unity 6000.0.23f1) anlatır; tarih kaydı olarak duruyor, güncel talimatlar bu dosyadadır.

## Lisanslar

Yazı tipleri SIL Open Font License 1.1 ile: [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md). Müzik, ses efektleri, dokular ve metinler bu oyun için üretildi.
