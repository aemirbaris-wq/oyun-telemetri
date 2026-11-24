# Kapsam Notu

## Tez Kapsamında Yapılacaklar
- Unity 6 LTS şablonu üzerine kurulu, tarayıcıda oynanan 6 bölümlü tek oyunculu 2D platform oyunu.
- 8 olay türünden oluşan C# telemetri modülü ve Row Level Security (RLS) ile korunan PostgreSQL (Supabase) veritabanı.
- İki aşamalı veri toplama: v1 için 40–60 oturum, v2 bölüm revizyonları sonrası 30+ yeni oyuncu oturumu.
- Bölüm bazlı zorluk analizi (huni grafiği, ölüm ısı haritaları), k-means ile oyuncu profilleri, ilk iki bölümden bırakma tahmini (LR, RF) ve önceden commit'lenmiş hipotezlerle v1–v2 istatistiksel karşılaştırması.

## Kapsam Dışı Bırakılanlar
- Çok oyunculu modlar, mobil derleme, yeni oyun mekanikleri veya hikâye kurgusu.
- Derin öğrenme modelleri, gerçek zamanlı dinamik zorluk ayarı ve hazır üçüncü taraf analitik servisleri.
