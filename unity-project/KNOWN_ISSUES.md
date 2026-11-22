# Bilinen durumlar ve sınırlar — v0.3.2 (22 Kasım 2026)

Bu liste, yapılmamış ya da burada doğrulanamamış her şeyi açıkça yazar.

## İlk gerçek test (0.3.0, 22 Kasım 2026)

- Unity Build Automation 0.3.0'ı gerçek Unity'de hatasız derledi ve Windows build'i çıkardı (#4).
- Build bulut bilgisayarda (AirGPU, NVIDIA L4, Windows Server 2025) açıldı: oyun çalıştı, sesler geldi, **ama ekran tamamen pembeydi.** Sebep ve düzeltme `CHANGES.md` → 0.3.1'de.
- 0.3.1 build'i aynı makinede açıldı: görüntü doğru, sağ altta v0.3.1. Parlaklık ekranında takıldı: o ekran yalnızca klavye ve kolla geçiliyordu, Moonlight'ın ekran kolu ise bu makinede oyuna ulaşmadı.
- **0.3.2'deki dokunmatik test kontrolleri ve tıklanabilir menüler henüz gerçek bir build'de denenmedi.**

## Steam'den önce kaldırılacak

- **Dokunmatik test kontrolleri** yalnızca telefondan bulut bilgisayarla test için var. Steam build'inden önce `Assets/_Project/Scripts/Runtime/UI/TouchControls.cs` içinde `Enabled = false` yapılacak (ya da dosya ve `TouchControls` çağrıları silinecek).

## Doğrulanmadı

1. **Unity Editor'de hiç açılmadı.** Kod, Unity 6000.3.27f1 / HDRP 17.3.0 / Input System 1.11.2 / uGUI kaynak kodundan üretilmiş API taslaklarına karşı derlendi (0 hata, 0 uyarı). 0.3.0'ı Build Automation da gerçek Unity'de derledi. Bu, kodun derlendiğini gösterir; çalışma anındaki davranışı ve shader derlemesini tek başına kanıtlamaz.
2. **Oynanmadı.** Dört bölüm mantık katmanında baştan sona yürütüldü (`runlogic walk`); 3B dünyada yürünerek, gerçek ışık ve sesle denenmedi. Bir odada duvar içinde kalma, karanlık köşe, yanlış yöne bakan eşya gibi sorunlar ilk testte çıkabilir.
3. **Testler Unity'de koşmadı.** 52 test .NET 8 üzerinde kendi küçük NUnit uyumlu koşucumla geçti. Unity Test Runner'da (Mono) da aynı sonucu vermeleri beklenir ama denenmedi.
4. **Oynanabilir bir Windows build'i henüz görülmedi.** 0.3.0 build'i açıldı ama pembe ekranda kaldı; 0.3.1 build'i alınmadı. Steam'e yüklenmedi.
5. **Performans ölçülmedi.** Kare hızı, bellek, yükleme süreleri bilinmiyor. Işık değerleri (sabit pozlama EV100 ≈ 2, fener 560 lümen, ay ışığı) hesapla ayarlandı, gözle değil; ilk testte fazla karanlık ya da parlak gelebilir. Oyundaki parlaklık ayar ekranı bunun için var.

## Donanım

6. HDRP gerçek bir DirectX 11/12 ekran kartı ister. **VirtualBox / Hyper-V'nin varsayılan sanal ekran kartıyla çalışmaz** (siyah ekran, açılmama ya da çökme). Sanal makinede GPU geçişi gerekir; en güvenilir test gerçek bilgisayardır.
7. Çince, Japonca, Korece metinler Windows'un sistem yazı tiplerini kullanır (Microsoft YaHei, Yu Gothic, Malgun Gothic). Bunlar yoksa Unity'nin genel yedek yazı tipine düşer.

## İçerik

8. **Seslendirme yok.** Konuşmaların yerinde kısa geçici "mırıltı" sesleri ve altyazı var. Kod, `vo_<anahtar>` adlı bir ses dosyası bulursa onu çalar; gerçek kayıtlar `Resources/Undertow/Audio` altına bu adlarla eklenebilir.
9. **Müzik ve efektler** bu oyun için kodla üretildi (özgün, telifsiz). Profesyonel bir ses tasarımcısının elinden geçmedi.
10. **Görseller** prosedürel: modeller koddan kurulur, dokular üretilmiş JPG'lerdir. Fotogerçekçi 4K sanat değil; tutarlı ve okunaklı bir "sisli gri kutu+" seviyesindedir.
11. **8 dil yapay zekâ taslağı** (Deutsch, Français, Español LatAm, Português BR, Русский, 简体中文, 日本語, 한국어). Hepsi eksiksiz ve testlerden geçiyor (yer tutucular, ipucu saatleri, iki sesin ayrımı) ama anadili konuşan birinin okuması gerekir. İngilizce ve Türkçe elden yazıldı.
12. Rusçada 1724 belgeleri IM FELL English yerine EB Garamond ile görünür (IM FELL'de Kiril harfi yok).
13. Başarımlar yerel olarak kaydedilir; **Steam başarımlarına bağlı değil** (Steamworks entegrasyonu yok).

## Depo

14. Önceki sürümden kalan, oyunun hiçbir yerde kullanmadığı 18 adet 4K PNG doku (359 MB, Git LFS), 10 OBJ model ve 5 eski malzeme depodan kaldırıldı. Git geçmişinde (`08d09c2` ve öncesi) duruyorlar; gerekirse geri getirilebilir.
15. `README_DELIVERY.md`, `DELIVERY_AUDIT.md` ve `Docs/` altındaki belgeler önceki teslimi anlatır (tarih kaydı). O teslimin SHA-256 listeleri artık var olmayan dosyaları gösterdiği için kaldırıldı.
16. Yeni klasör ve dosyaların `.meta` dosyaları yok; Unity ilk açılışta üretir. Onları da commit'lemek gerekir.
17. Eski kod (40 betik), eski prefab'lar, eski JSON yerelleştirme ve eski test dosyası yeni mimariyle değiştirildi; git geçmişinde (`db30c12`) duruyorlar.
