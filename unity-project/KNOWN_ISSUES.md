# Bilinen durumlar ve sınırlar — v0.3.3 (22 Kasım 2026)

Bu liste, yapılmamış ya da burada doğrulanamamış her şeyi açıkça yazar.

## Gerçek testler (şimdiye kadar)

- **0.3.0:** Unity Build Automation gerçek Unity'de hatasız derledi (#4). Build bulut bilgisayarda (AirGPU, NVIDIA L4, Windows Server 2025) açıldı: oyun çalıştı, sesler geldi, ekran tamamen pembeydi → 0.3.1'de düzeltildi.
- **0.3.1:** aynı makinede açıldı, görüntü doğru. Parlaklık ekranı yalnızca klavye/kolla geçildiği için telefondan ilerlenemedi → 0.3.2.
- **0.3.2:** ana menüye kadar gelindi; menü görseli üst üste binmişti, açılış ekranlarının sırası karışık göründü → 0.3.3'te düzeltildi.
- **0.3.3 henüz hiçbir gerçek makinede açılmadı.** Aşağıdaki "Doğrulanmadı" bölümü bu sürüm için geçerlidir.

## 0.3.3'te nasıl test edildi

Unity ve ekran kartı olmayan bir ortamda, oyunun kendi kodu Unity'nin yerine geçen **başsız bir benzetimde** çalıştırıldı (MonoBehaviour döngüsü, eşyordamlar, sahne yükleme, Input System tuş yolları, uGUI seçim/tıklama, kutu çarpıştırıcılarla fizik, karakter denetleyicisi). 201 kontrolün hepsi geçti: açılış, menüler, dört bölümün kurulması, üç tam oynanış (üç son), ekran oranları, 10 dil, klavye-fare kontrolleri, kayıt/yükleme.

Bu benzetimin **göremediği** şeyler: gerçek görüntü (ışık, sis, gölge, shader), ses karışımı, kare hızı ve bellek, HDRP'nin gerçek davranışı, Windows'a özgü pencere/çözünürlük geçişleri, gerçek kumanda. Benzetim "kod takılmıyor, kilitlenmiyor, hata fırlatmıyor, oyun baştan sona bitirilebiliyor" der; "güzel görünüyor ve akıcı" diyemez.

## Doğrulanmadı

1. **Unity Editor'de hiç açılmadı.** Kod, Unity 6000.3.27f1 / HDRP 17.3.0 / Input System 1.11.2 / uGUI kaynak kodundan üretilmiş API taslaklarına karşı derlendi (0 hata, 0 uyarı). Önceki sürümleri Build Automation gerçek Unity'de derledi; 0.3.3 için yeni build alınmalı.
2. **Gerçek ekranda oynanmadı.** Dört bölüm hem mantık katmanında hem başsız benzetimde baştan sona oynatıldı; 3B dünyada gerçek ışık ve sesle bir insan tarafından denenmedi. Görsel kusurlar (yanlış yöne bakan eşya, birbirine giren dekor, fazla karanlık köşe) ilk gerçek testte çıkabilir.
3. **Işık artık çok daha karanlık.** 0.3.3 ışık birimini düzeltti (lümen → kandela) ve kapalı mekânların ortam ışığını devreye aldı; sahneler 0.3.2'dekinden belirgin biçimde karanlık. Değerler hesapla ayarlandı, gözle değil. Fazla karanlıksa Ayarlar → Görüntü → Parlaklık; yine yetmezse bildir, pozlama değerini değiştiririm.
4. **Testler Unity'de koşmadı.** 53 test .NET 8 üzerinde kendi küçük NUnit uyumlu koşucumla geçti. Unity Test Runner'da (Mono) aynı sonucu vermeleri beklenir ama denenmedi.
5. **Performans ölçülmedi.** Kare hızı, bellek, yükleme süreleri bilinmiyor. Eski ya da zayıf ekran kartlarında oyun ilk açılışta Düşük kaliteyle başlar; yine de akıcı olacağı garanti değil. **F3** paneli kare hızını ve ekran kartını gösterir.
6. **Steam'e yüklenmedi.** Steamworks entegrasyonu yok.

## Eski bilgisayarlar (2013 dolayı)

- Windows 10 21H1 (derleme 19043) ya da daha yenisi ve **DirectX 11 destekli, compute shader çalıştırabilen** bir ekran kartı şart. Windows 7 / 8.1'de açılmaz.
- İşlemciye gömülü eski grafiklerde (Intel HD 4000 / 4600 gibi) açılsa bile çok düşük kare hızı beklenir. O dönemin orta-üst ayrı kartlarında (GTX 660 / 760, R9 270 sınıfı) Düşük kalitede denenebilir. **Hiçbiri ölçülmedi.**
- Oyun zayıf kartlarda ilk açılışta kendiliğinden Düşük kaliteyle başlar. Takılıyorsa: Ayarlar → Görüntü'den çözünürlüğü düşür, kenar yumuşatmayı ve alan derinliğini kapat.
- Açılmazsa ya da siyah ekran gelirse: ekran kartı sürücüsünü güncelle. Sebep `Player.log` dosyasına yazılır (`%USERPROFILE%\AppData\LocalLow\LenstedReal\` altındaki oyun klasöründe); o dosyayı ya da F3 panelinin fotoğrafını gönder.

## Donanım

7. HDRP gerçek bir DirectX 11/12 ekran kartı ister. **VirtualBox / Hyper-V'nin varsayılan sanal ekran kartıyla çalışmaz** (siyah ekran, açılmama ya da çökme). Sanal makinede GPU geçişi gerekir; en güvenilir test gerçek bilgisayardır.
8. Çince, Japonca, Korece metinler Windows'un sistem yazı tiplerini kullanır (Microsoft YaHei, Yu Gothic, Malgun Gothic). Bunlar yoksa Unity'nin genel yedek yazı tipine düşer.

## İçerik

9. **Seslendirme yok.** Konuşmaların yerinde kısa geçici "mırıltı" sesleri ve altyazı var. Kod, `vo_<anahtar>` adlı bir ses dosyası bulursa onu çalar; gerçek kayıtlar `Resources/Undertow/Audio` altına bu adlarla eklenebilir.
10. **Müzik ve efektler** bu oyun için kodla üretildi (özgün, telifsiz). Profesyonel bir ses tasarımcısının elinden geçmedi.
11. **Görseller** prosedürel: modeller koddan kurulur, dokular üretilmiş JPG'lerdir. Fotogerçekçi 4K sanat değil; tutarlı ve okunaklı bir "sisli gri kutu+" seviyesindedir.
12. **8 dil yapay zekâ taslağı** (Deutsch, Français, Español LatAm, Português BR, Русский, 简体中文, 日本語, 한국어). Hepsi eksiksiz ve testlerden geçiyor (yer tutucular, ipucu saatleri, iki sesin ayrımı) ama anadili konuşan birinin okuması gerekir. İngilizce ve Türkçe elden yazıldı.
13. Rusçada 1724 belgeleri IM FELL English yerine EB Garamond ile görünür (IM FELL'de Kiril harfi yok).
14. Başarımlar yerel olarak kaydedilir; **Steam başarımlarına bağlı değil** (Steamworks entegrasyonu yok).

## Depo

15. Önceki sürümden kalan, oyunun hiçbir yerde kullanmadığı 18 adet 4K PNG doku (359 MB, Git LFS), 10 OBJ model ve 5 eski malzeme depodan kaldırıldı. Git geçmişinde (`08d09c2` ve öncesi) duruyorlar; gerekirse geri getirilebilir.
16. `README_DELIVERY.md`, `DELIVERY_AUDIT.md` ve `Docs/` altındaki belgeler önceki teslimi anlatır (tarih kaydı). O teslimin SHA-256 listeleri artık var olmayan dosyaları gösterdiği için kaldırıldı.
17. Yeni klasör ve dosyaların `.meta` dosyaları yok; Unity ilk açılışta üretir. Onları da commit'lemek gerekir.
18. Eski kod (40 betik), eski prefab'lar, eski JSON yerelleştirme ve eski test dosyası yeni mimariyle değiştirildi; git geçmişinde (`db30c12`) duruyorlar.
