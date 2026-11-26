using System;
using System.Collections.Generic;
using UnityEngine;

public class DilYoneticisi : MonoBehaviour
{
    public static DilYoneticisi Ornek { get; private set; }
    public static event Action DilDegisti;

    [Header("Gecici Oyun Adi (Sonradan Degistirilebilir)")]
    public string oyunAdiTr = "YEDİ PERDE";
    public string oyunAdiEn = "SEVEN VEILS";

    [Header("Gelistirici Imzasi")]
    public string gelistiriciImzaTr = "Tasarım & Geliştirme: Emir Barış Al  •  Lisans Bitirme Tezi Araştırma Sürümü";
    public string gelistiriciImzaEn = "Design & Development: Emir Barış Al  •  Undergraduate Thesis Research Build";

    public bool TurkceMi { get; private set; } = true;

    private readonly Dictionary<string, (string tr, string en)> sozluk = new Dictionary<string, (string, string)>
    {
        // Ana Menu
        { "menu_alt_baslik", ("Cennetin Işığından Kendi Gölgesine İnen Bir Yolculuk", "A Descent from Heaven's Light into One's Own Shadow") },
        { "menu_oyna", ("YOLCULUĞA BAŞLA", "BEGIN THE DESCENT") },
        { "menu_kontroller", ("KONTROLLER", "CONTROLS") },
        { "menu_cikis", ("ÇIKIŞ", "QUIT") },
        { "menu_kontroller_detay", ("← → : Hareket   |   ↑ : Zıpla   |   SPACE : Ruh Atılımı (Boost)   |   E : Tablet Oku   |   ESC : Duraklat", "← → : Move   |   ↑ : Jump   |   SPACE : Soul Dash (Boost)   |   E : Read Tablet   |   ESC : Pause") },

        // Arastirma ve KVKK / GDPR Aydinlatma
        { "kvkk_baslik", ("ARAŞTIRMA PROTOKOLÜ VE VERİ AYDINLATMASI", "RESEARCH PROTOCOL & DATA DISCLOSURE") },
        { "kvkk_metin", (
            "Bu yapım, 'Oyuncu Telemetrisiyle Veri Güdümlü Oyun Analitiği' akademik araştırması kapsamında geliştirilmiştir.\n\n" +
            "• Oynanış boyunca yalnızca anonim telemetri olayları (bölüm geçişleri, düşme/engel noktaları, toplanan kalıntılar ve süreler) kaydedilir.\n" +
            "• Ad, e-posta, IP adresi veya kimliğinizi tanımlayabilecek hiçbir kişisel veri toplanmaz; yalnızca cihazınıza özel rastgele bir anonim oturum kimliği üretilir.\n" +
            "• Veriler yalnızca bölüm zorluk dengesini ve oyuncu deneyimini iyileştirmek amacıyla bilimsel olarak analiz edilir.",
            "This title was developed as part of an academic study on 'Data-Driven Game Analytics via Player Telemetry'.\n\n" +
            "• During gameplay, only anonymous telemetry events (level transitions, failure points, collected relics, and timestamps) are recorded.\n" +
            "• No personally identifiable information (name, email, IP address) is ever collected; only a randomized anonymous session token is generated.\n" +
            "• Collected metrics are analyzed strictly for scientific level-balancing and player experience research."
        ) },
        { "kvkk_onay", ("ANLADIM, KABUL EDİYORUM", "I UNDERSTAND & AGREE") },
        { "kvkk_geri", ("GERİ DÖN", "BACK") },

        // Duraklatma Menusu
        { "pause_baslik", ("ZAMAN DURDURULDU", "TIME SUSPENDED") },
        { "pause_devam", ("DEVAM ET", "RESUME") },
        { "pause_yeniden", ("BÖLÜMÜ YENİDEN BAŞLAT", "RESTART CHAPTER") },
        { "pause_menu", ("ANA MENÜYE DÖN", "RETURN TO MAIN MENU") },

        // Etkilesim ve Tablet
        { "etkilesim_oku", ("[ E ]  Yazıtı İncele", "[ E ]  Examine Inscription") },
        { "etkilesim_kapat", ("[ E ] veya [ ESC ]  Kapat", "[ E ] or [ ESC ]  Close") },

        // 7 Bolum Basliklari (Gunahlar)
        { "bolum_1_baslik", ("BÖLÜM I — TEMBELLİK", "CHAPTER I — SLOTH") },
        { "bolum_1_alt", ("Aydınlık Cennet  •  Sessiz Başlangıç", "Radiant Heaven  •  The Quiet Beginning") },
        { "bolum_2_baslik", ("BÖLÜM II — OBURLUK", "CHAPTER II — GLUTTONY") },
        { "bolum_2_alt", ("Altın İkindi  •  Doymayan İstek", "Golden Dusk  •  Insatiable Craving") },
        { "bolum_3_baslik", ("BÖLÜM III — ŞEHVET", "CHAPTER III — LUST") },
        { "bolum_3_alt", ("Kızıl Ufuk  •  Yanıltıcı Parıltı", "Crimson Horizon  •  Deceptive Glow") },
        { "bolum_4_baslik", ("BÖLÜM IV — AÇGÖZLÜLÜK", "CHAPTER IV — GREED") },
        { "bolum_4_alt", ("Alacakaranlık Araf  •  Gölge Bedeli", "Twilight Purgatory  •  The Price of Shadow") },
        { "bolum_5_baslik", ("BÖLÜM V — KISKANÇLIK", "CHAPTER V — ENVY") },
        { "bolum_5_alt", ("Sisli Koruluk  •  Zehirli Bakış", "Misty Grove  •  Venomous Gaze") },
        { "bolum_6_baslik", ("BÖLÜM VI — ÖFKE", "CHAPTER VI — WRATH") },
        { "bolum_6_alt", ("Volkanik Geçit  •  Körleşen Öfke", "Volcanic Pass  •  Blinding Fury") },
        { "bolum_7_baslik", ("BÖLÜM VII — KİBİR", "CHAPTER VII — PRIDE") },
        { "bolum_7_alt", ("Cehennem Zirvesi  •  Gölgenin Yüzleşmesi", "Infernal Summit  •  Confronting the Shadow") }
    };

    private void Awake()
    {
        if (Ornek != null && Ornek != this)
        {
            Destroy(gameObject);
            return;
        }
        Ornek = this;
        DontDestroyOnLoad(gameObject);

        TurkceMi = PlayerPrefs.GetInt("dil_tr", 1) == 1;
    }

    public void DilDegistir(bool turkce)
    {
        if (TurkceMi == turkce) return;
        TurkceMi = turkce;
        PlayerPrefs.SetInt("dil_tr", turkce ? 1 : 0);
        PlayerPrefs.Save();
        DilDegisti?.Invoke();
    }

    public string Metin(string anahtar)
    {
        if (anahtar == "oyun_adi")
        {
            return TurkceMi ? oyunAdiTr : oyunAdiEn;
        }
        if (anahtar == "imza")
        {
            return TurkceMi ? gelistiriciImzaTr : gelistiriciImzaEn;
        }
        if (sozluk.TryGetValue(anahtar, out var cift))
        {
            return TurkceMi ? cift.tr : cift.en;
        }
        return anahtar;
    }
}
