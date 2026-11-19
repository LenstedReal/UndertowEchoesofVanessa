# Undertow — Teknik İnceleme ve Sonraki Adımlar

## Bu revizyonda yapılanlar

- Unity Player Settings içindeki `activeInputHandler` değeri `0` (Both) yapıldı. Mevcut hareket, kamera, etkileşim ve dinleme kodu `UnityEngine.Input` kullandığı için yalnızca yeni Input System seçimiyle oluşabilecek input uyumsuzluğu giderildi.
- Beş çan bulmacasına beklenen dizi uzunluğunu aşan girişler için koruma eklendi.
- Campaign singleton'ının sahneye ait bütün sistemleri taşıyan bir root objesini yok etmesi engellendi; yalnızca kendi GameObject'i kalıcı hale geliyor.
- Act I için çan, cerrah kancası, kargo kapağı ve sandık-halka etkileşim bileşenleri eklendi.
- Fener yağı pickup'ı ve temel oynanış HUD'u eklendi.
- Ana menü için üç kayıt yuvası kullanan prototip IMGUI ekranı eklendi.
- Editor sahne üreticisi genişletildi: gemi güvertesi, plank dizilimi, bordalar, korkuluklar, direk/yelken, kabin blokları, kargo alanı, fener ve Act I puzzle props oluşturuluyor. Act II–IV için keşfedilebilir blockout kabukları ve bölüm puzzle controller'ları oluşturuluyor.
- HDRP global post-processing Volume profili (Color Adjustments, Vignette, Chromatic Aberration) üretiliyor ve PBR doku setleri blockout materyallerine bağlanıyor.
- Dört PBR seti 512 yerine 2048 çözünürlükte yeniden üretildi.
- Save sistemi geçici dosyayı flush edip masaüstü dosya sistemlerinde `File.Replace` ile atomik değiştirmeyi deneyecek şekilde iyileştirildi; uyumsuz dosya sistemleri için fallback mevcut.
- Validator, OBJ dosyasının varlığının yanında Unity tarafından Mesh olarak import edilip edilmediğini ve texture importlarını kontrol edecek şekilde genişletildi.
- README ve varlık lisans/provenance açıklamaları gerçek duruma göre düzeltildi.

## Önemli sınırlar

- Bu çalışma alanında Unity 6 Editor, Windows build modülü veya C# derleyicisi çalıştırılmadı. Dolayısıyla proje derlendi, sahneler Unity'de açıldı, testler geçti veya EXE üretildi denemez.
- ZIP içindeki mevcut `.unity` dosyaları küçük sahne kabuklarıdır. Genişletilmiş çevreler Unity Editor açıldıktan sonra **Undertow > Build All Canonical Scenes & Prefabs** komutuyla oluşturulmalıdır.
- Oluşturulan çevreler greybox/vertical-slice seviyesindedir. Primitive geometriler ve düşük/orta poligonlu OBJ'ler final fotogerçekçi sanat varlıkları değildir.
- Act I etkileşimleri prototip seviyesindedir. Sandıklar E ile hedef halkaya kaydırılır; fizik tabanlı taşıma yoktur. Hikâye collectible'ları, gerçek diyalog tetikleme, animasyonlar, final cutscene'ler ve tam bölüm uzunlukları tamamlanmış değildir.
- Act II–IV sahneleri oynanabilir 3–4 saatlik bölümler olarak kabul edilmemelidir.
- Kaydedilmiş seslendirme, FMOD bankaları, müzik ve lisanslı ambiyans sesleri yoktur.

## Kabul kapısı

1. Unity 6000.0.23f1 ile projeyi aç ve package resolve/import işlemini bitir.
2. Console'da sıfır compile error doğrula.
3. `Undertow > Build All Canonical Scenes & Prefabs` çalıştır.
4. EditMode testlerini Test Runner'da çalıştır.
5. Act I'ı baştan sona Play Mode'da test et: hareket, çan sırası, kanca pickup, kapak, iki sandık, fener yağı ve kayıt/yükleme.
6. Ancak bu adımlardan sonra Windows x64 Development Build al ve gerçek makinede performans/çökme testi yap.
