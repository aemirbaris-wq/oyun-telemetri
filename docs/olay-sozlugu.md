# Olay Sözlüğü

Her olay kaydında ortak olarak `oturum_id`, `oyuncu_id`, `surum` (`v1`/`v2`), `sira` ve `t_ms` alanları tutulur. Kişisel veri toplanmaz.

| Olay (`tur`) | Tetiklenme Zamanı | Doldurulan Özel Alanlar |
| :--- | :--- | :--- |
| `oturum_basla` | KVKK onay ekranından sonra | `surum`, `oyuncu_id` (`bolum = 0`) |
| `bolum_basla` | Bölüm sahnesi yüklendiğinde | `bolum` |
| `olum` | Karakter öldüğünde | `bolum`, `x`, `y`, `neden` (`cukur` / `diken` / `dusman`) |
| `checkpoint` | Kontrol bayrağına değdiğinde | `bolum`, `x`, `y` |
| `jeton` | Jeton toplandığında | `bolum`, `x`, `y` |
| `duraklat` | Duraklatma menüsü açıldığında | `bolum` |
| `bolum_bitir` | Bölüm çıkışına ulaşıldığında | `bolum` |
| `oyun_bitir` | 6. bölüm tamamlandığında | `bolum = 6` |

*Not: Oyuncu tarayıcı sekmesini her an kapatabileceği için ayrı "bıraktım" olayı gönderilmez; son olayı `oyun_bitir` olmayan oturumlar oyunu bırakmış kabul edilir.*
