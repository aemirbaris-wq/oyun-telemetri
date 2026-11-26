using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SevenVeilsTelemetriVeStats : MonoBehaviour
{
    public static SevenVeilsTelemetriVeStats Instance { get; private set; }

    [Header("Supabase Akademik Telemetri (Opsiyonel)")]
    [SerializeField] private string supabaseUrl = "";
    [SerializeField] private string supabaseAnonKey = "";
    [SerializeField] private string tabloAdi = "telemetri_olaylari";

    [Serializable]
    public class BolumStatVerisi
    {
        public int bolumNo;
        public string gunahAdi;
        public bool tamamlandi;
        public int toplamOlum;
        public float enIyiSure = -1f;
        public float toplamGecirilenSure;
        public int tamamlanmaSayisi;
        public bool sifirOlumRozeti;
        public int toplananParca;
        public int toplamParca;
        public bool gizliOdaBulundu;
        public float gizliOdaBulmaSuresi = -1f;
        public bool gunahYazitiOkundu;
        public bool erdemSunagiOkundu;
    }

    [Serializable]
    public class OlumNedeniKaydi
    {
        public string neden;
        public int sayi;
    }

    [Serializable]
    public class RagebaitNoktaKaydi
    {
        public int bolumNo;
        public float x;
        public float y;
        public string neden;
        public int ardisikOlum;
        public int toplamOlum;
    }

    [Serializable]
    public class BasarimVerisi
    {
        public string id;
        public string baslikTR;
        public string baslikEN;
        public string aciklamaTR;
        public string aciklamaEN;
        public bool acildi;
        public string acilmaTarihi;
    }

    [Serializable]
    public class TumOyunVerisi
    {
        public string oyuncuOturumId;
        public float toplamOynamaSuresi;
        public int toplamOlum;
        public int toplamZiplama;
        public int toplamDash;
        public int toplamWallJump;
        public int toplamEzilenDusman;
        public int toplamBoostTetikleme;
        public bool normalSonAcildi;
        public bool secretEndingAcildi;
        public List<BolumStatVerisi> bolumler = new List<BolumStatVerisi>();
        public List<OlumNedeniKaydi> olumNedenleri = new List<OlumNedeniKaydi>();
        public List<RagebaitNoktaKaydi> ragebaitNoktalari = new List<RagebaitNoktaKaydi>();
        public List<BasarimVerisi> basarimlar = new List<BasarimVerisi>();
    }

    private TumOyunVerisi veri;
    private const string KAYIT_ANAHTARI = "SevenVeils_Dashboard_Stats_v1";

    private int aktifBolumNo = 1;
    private bool bolumAktif;
    private float aktifBolumToplamSure;
    private int aktifBolumOlumSayisi;
    private bool aktifCheckpointGecildi;
    private float checkpointOncesiSure;
    private int checkpointOncesiOlum;
    private int checkpointSonrasiOlum;

    private Vector2 sonOlumKonumu;
    private int ayniNoktaArdisikOlum;

    public float AktifBolumSuresi => aktifBolumToplamSure;
    public int AktifBolumOlumu => aktifBolumOlumSayisi;
    public TumOyunVerisi Veri => veri;

    public event Action<BasarimVerisi> OnBasarimAcildi;

    private static readonly string[] GunahAdlari =
    {
        "Oburluk", "Tembellik", "Sehvet", "Acgozluluk", "Kibir", "Kiskanclik", "Ofke (Cehennem)"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        VeriyiYukleVeyaOlustur();
    }

    private void Update()
    {
        if (Time.timeScale <= 0.001f) return;

        veri.toplamOynamaSuresi += Time.unscaledDeltaTime;
        if (bolumAktif)
        {
            aktifBolumToplamSure += Time.deltaTime;
            if (!aktifCheckpointGecildi)
                checkpointOncesiSure += Time.deltaTime;
        }
    }

    public static string SureFormatla(float saniye)
    {
        if (saniye < 0f) return "--:--.--";
        int dakika = Mathf.FloorToInt(saniye / 60f);
        float kalanSaniye = saniye % 60f;
        return $"{dakika:00}:{kalanSaniye:00.00}";
    }

    public void BolumBaslat(int bolumNo, int bolumdekiToplamParca = 10)
    {
        aktifBolumNo = Mathf.Clamp(bolumNo, 1, 7);
        bolumAktif = true;
        aktifBolumToplamSure = 0f;
        aktifBolumOlumSayisi = 0;
        aktifCheckpointGecildi = false;
        checkpointOncesiSure = 0f;
        checkpointOncesiOlum = 0;
        checkpointSonrasiOlum = 0;
        ayniNoktaArdisikOlum = 0;

        BolumStatVerisi b = BolumStatGetir(aktifBolumNo);
        if (bolumdekiToplamParca > 0) b.toplamParca = bolumdekiToplamParca;

        if (aktifBolumNo == 7) BasarimAc("cehenneme_inis");

        TelemetriOlayiGonder("bolum_basla", aktifBolumNo, Vector2.zero, "baslangic", 0);
        VeriyiKaydet();
    }

    public void CheckpointGecildi()
    {
        if (!aktifCheckpointGecildi)
        {
            aktifCheckpointGecildi = true;
            TelemetriOlayiGonder("checkpoint_aktif", aktifBolumNo, Vector2.zero, $"cp_sure:{checkpointOncesiSure:F1}s", aktifBolumOlumSayisi);
        }
    }

    public void OlumKaydet(int bolumNo, Vector2 konum, string olumNedeni)
    {
        int bNo = Mathf.Clamp(bolumNo, 1, 7);
        if (string.IsNullOrWhiteSpace(olumNedeni)) olumNedeni = "Bilinmeyen Engel";

        veri.toplamOlum++;
        aktifBolumOlumSayisi++;

        if (aktifCheckpointGecildi) checkpointSonrasiOlum++;
        else checkpointOncesiOlum++;

        BolumStatVerisi b = BolumStatGetir(bNo);
        b.toplamOlum++;

        OlumNedeniKaydi nedenKayit = veri.olumNedenleri.Find(x => x.neden == olumNedeni);
        if (nedenKayit == null)
        {
            nedenKayit = new OlumNedeniKaydi { neden = olumNedeni, sayi = 0 };
            veri.olumNedenleri.Add(nedenKayit);
        }
        nedenKayit.sayi++;

        if (Vector2.Distance(konum, sonOlumKonumu) <= 2.5f)
            ayniNoktaArdisikOlum++;
        else
            ayniNoktaArdisikOlum = 1;

        sonOlumKonumu = konum;

        RagebaitNoktaKaydi mevcutHotspot = veri.ragebaitNoktalari.Find(r =>
            r.bolumNo == bNo && Vector2.Distance(new Vector2(r.x, r.y), konum) <= 2.5f);

        if (mevcutHotspot == null)
        {
            mevcutHotspot = new RagebaitNoktaKaydi
            {
                bolumNo = bNo,
                x = Mathf.Round(konum.x * 10f) / 10f,
                y = Mathf.Round(konum.y * 10f) / 10f,
                neden = olumNedeni,
                ardisikOlum = ayniNoktaArdisikOlum,
                toplamOlum = 1
            };
            veri.ragebaitNoktalari.Add(mevcutHotspot);
        }
        else
        {
            mevcutHotspot.toplamOlum++;
            mevcutHotspot.neden = olumNedeni;
            if (ayniNoktaArdisikOlum > mevcutHotspot.ardisikOlum)
                mevcutHotspot.ardisikOlum = ayniNoktaArdisikOlum;
        }

        if (veri.toplamOlum >= 50) BasarimAc("inatci_ruh");

        TelemetriOlayiGonder("oyuncu_olum", bNo, konum, olumNedeni, ayniNoktaArdisikOlum);
        VeriyiKaydet();
    }

    public void MekanikKullanildi(string mekanikTipi)
    {
        switch (mekanikTipi)
        {
            case "ziplama": veri.toplamZiplama++; break;
            case "dash": veri.toplamDash++; break;
            case "wall_jump": veri.toplamWallJump++; break;
            case "stomp": veri.toplamEzilenDusman++; break;
            case "boost": veri.toplamBoostTetikleme++; break;
        }
    }

    public void ToplanabilirGuncelle(int bolumNo, int toplanan, int toplam)
    {
        BolumStatVerisi b = BolumStatGetir(bolumNo);
        if (toplanan > b.toplananParca) b.toplananParca = toplanan;
        if (toplam > 0) b.toplamParca = toplam;

        if (b.toplamParca > 0 && b.toplananParca >= b.toplamParca)
            BasarimAc("tam_koleksiyoncu");

        VeriyiKaydet();
    }

    public void GizliOdaKesfedildi(int bolumNo, Vector2 konum)
    {
        BolumStatVerisi b = BolumStatGetir(bolumNo);
        if (!b.gizliOdaBulundu)
        {
            b.gizliOdaBulundu = true;
            b.gizliOdaBulmaSuresi = aktifBolumToplamSure;
            BasarimAc("olculu_goz");
            TelemetriOlayiGonder("gizli_oda_kesif", bolumNo, konum, $"sure:{aktifBolumToplamSure:F1}s", 0);
            VeriyiKaydet();
        }
    }

    public void YazitVeyaSunakOkundu(int bolumNo, bool erdemSunagiMi)
    {
        BolumStatVerisi b = BolumStatGetir(bolumNo);
        if (erdemSunagiMi) b.erdemSunagiOkundu = true;
        else b.gunahYazitiOkundu = true;

        int okunanGunah = 0;
        int okunanErdem = 0;
        foreach (var item in veri.bolumler)
        {
            if (item.gunahYazitiOkundu) okunanGunah++;
            if (item.erdemSunagiOkundu) okunanErdem++;
        }

        if (okunanGunah >= 7) BasarimAc("gunahlarin_itirafi");
        if (okunanErdem >= 7) BasarimAc("hakikat_arayicisi");

        TelemetriOlayiGonder(erdemSunagiMi ? "erdem_sunagi_okundu" : "gunah_yaziti_okundu", bolumNo, Vector2.zero, "", 0);
        VeriyiKaydet();
    }

    public void BolumTamamlandi(int bolumNo, int toplananParca, int toplamParca, float hedefSpeedrunSuresi = 90f)
    {
        bolumAktif = false;
        int bNo = Mathf.Clamp(bolumNo, 1, 7);
        BolumStatVerisi b = BolumStatGetir(bNo);

        b.tamamlandi = true;
        b.tamamlanmaSayisi++;
        b.toplamGecirilenSure += aktifBolumToplamSure;

        if (b.enIyiSure < 0f || aktifBolumToplamSure < b.enIyiSure)
            b.enIyiSure = aktifBolumToplamSure;

        if (aktifBolumOlumSayisi == 0)
        {
            b.sifirOlumRozeti = true;
            BasarimAc("dokunulmaz");
        }

        if (aktifBolumToplamSure <= hedefSpeedrunSuresi)
            BasarimAc("golge_ustasi");

        if (toplananParca > b.toplananParca) b.toplananParca = toplananParca;
        if (toplamParca > 0) b.toplamParca = toplamParca;
        if (b.toplamParca > 0 && b.toplananParca >= b.toplamParca)
            BasarimAc("tam_koleksiyoncu");

        if (bNo == 1) BasarimAc("ilk_esik");

        if (bNo == 7)
        {
            if (YuzdeKacTamamlandi() >= 99.9f)
            {
                veri.secretEndingAcildi = true;
                BasarimAc("yedi_perde_aralandi");
            }
            else
            {
                veri.normalSonAcildi = true;
                BasarimAc("golgelerin_ardindan");
            }
        }

        string ozet = $"sure:{aktifBolumToplamSure:F2}|olum:{aktifBolumOlumSayisi}|cp_oncesi:{checkpointOncesiOlum}|cp_sonrasi:{checkpointSonrasiOlum}";
        TelemetriOlayiGonder("bolum_tamamla", bNo, Vector2.zero, ozet, aktifBolumOlumSayisi);
        VeriyiKaydet();
    }

    public void BolumTerkEdildi(float ilerlemeYuzdesi)
    {
        if (!bolumAktif) return;
        bolumAktif = false;
        string detay = $"ilerleme:%{ilerlemeYuzdesi:F0}|sure:{aktifBolumToplamSure:F1}s";
        TelemetriOlayiGonder("bolum_terk_ragequit", aktifBolumNo, Vector2.zero, detay, aktifBolumOlumSayisi);
        VeriyiKaydet();
    }

    public float YuzdeKacTamamlandi()
    {
        int toplanan = 0;
        int toplam = 0;
        foreach (var b in veri.bolumler)
        {
            toplanan += b.toplananParca;
            toplam += Mathf.Max(1, b.toplamParca);
        }
        return toplam > 0 ? (toplanan * 100f) / toplam : 0f;
    }

    public RagebaitNoktaKaydi EnCokRagebaitYenenNokta()
    {
        RagebaitNoktaKaydi enKotusu = null;
        foreach (var r in veri.ragebaitNoktalari)
        {
            if (enKotusu == null || r.toplamOlum > enKotusu.toplamOlum)
                enKotusu = r;
        }
        return enKotusu;
    }

    public OlumNedeniKaydi EnCokOldurenNeden()
    {
        OlumNedeniKaydi enCok = null;
        foreach (var o in veri.olumNedenleri)
        {
            if (enCok == null || o.sayi > enCok.sayi)
                enCok = o;
        }
        return enCok;
    }

    public BolumStatVerisi BolumStatGetir(int bolumNo)
    {
        int bNo = Mathf.Clamp(bolumNo, 1, 7);
        BolumStatVerisi b = veri.bolumler.Find(x => x.bolumNo == bNo);
        if (b == null)
        {
            b = new BolumStatVerisi
            {
                bolumNo = bNo,
                gunahAdi = GunahAdlari[bNo - 1],
                toplamParca = 10
            };
            veri.bolumler.Add(b);
        }
        return b;
    }

    public void BasarimAc(string id)
    {
        BasarimVerisi basarim = veri.basarimlar.Find(x => x.id == id);
        if (basarim == null || basarim.acildi) return;

        basarim.acildi = true;
        basarim.acilmaTarihi = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        VeriyiKaydet();
        OnBasarimAcildi?.Invoke(basarim);
    }

    private void VeriyiYukleVeyaOlustur()
    {
        if (PlayerPrefs.HasKey(KAYIT_ANAHTARI))
        {
            string json = PlayerPrefs.GetString(KAYIT_ANAHTARI);
            veri = JsonUtility.FromJson<TumOyunVerisi>(json);
        }

        if (veri == null)
        {
            veri = new TumOyunVerisi
            {
                oyuncuOturumId = Guid.NewGuid().ToString("N").Substring(0, 12)
            };
        }

        for (int i = 1; i <= 7; i++)
            BolumStatGetir(i);

        VarsayilanBasarimlariEkle();
        VeriyiKaydet();
    }

    private void VarsayilanBasarimlariEkle()
    {
        BasarimTanimla("ilk_esik", "Ilk Esik", "First Threshold", "1. Perde (Oburluk) tamamlandi.", "Completed Veil 1 (Gluttony).");
        BasarimTanimla("olculu_goz", "Olculu Goz", "Discerning Eye", "Ilk kamufle gizli oda kesfedildi.", "Discovered your first hidden room.");
        BasarimTanimla("dokunulmaz", "Dokunulmaz", "Untouchable", "Bir bolumu 0 olumle tamamladin.", "Completed a level with 0 deaths.");
        BasarimTanimla("golge_ustasi", "Golge Ustasi", "Shadow Master", "Hedef speedrun suresinin altinda bolum bitirdin.", "Beat a level under the target speedrun time.");
        BasarimTanimla("tam_koleksiyoncu", "Tam Koleksiyoncu", "Completionist", "Bir bolumdeki tum parcalari topladin.", "Collected all fragments in a level.");
        BasarimTanimla("gunahlarin_itirafi", "Gunahlarin Itirafi", "Confession of Sins", "7 Gunah Yazitinin tamamini okudun.", "Read all 7 Sin Inscriptions.");
        BasarimTanimla("hakikat_arayicisi", "Hakikat Arayicisi", "Seeker of Truth", "7 Erdem Sunaginin tamamini bulup okudun.", "Found and read all 7 Virtue Altars.");
        BasarimTanimla("inatci_ruh", "Inatci Ruh", "Relentless Soul", "50 olume ragmen yolculuga devam ettin.", "Persisted through 50 deaths.");
        BasarimTanimla("cehenneme_inis", "Cehenneme Inis", "Descent to Hell", "7. Perde (Ofke / Cehennem) katina ulastin.", "Reached Veil 7 (Wrath / Hell).");
        BasarimTanimla("golgelerin_ardindan", "Golgelerin Ardindan", "Behind the Shadows", "Normal sonla perdeleri araladin.", "Reached the Normal Ending.");
        BasarimTanimla("yedi_perde_aralandi", "Yedi Perde Aralandi", "Seven Veils Lifted", "%100 koleksiyonla Secret Ending'e ulastin.", "Unlocked the Secret Ending with 100% collection.");
    }

    private void BasarimTanimla(string id, string bTR, string bEN, string aTR, string aEN)
    {
        if (veri.basarimlar.Exists(x => x.id == id)) return;
        veri.basarimlar.Add(new BasarimVerisi
        {
            id = id,
            baslikTR = bTR,
            baslikEN = bEN,
            aciklamaTR = aTR,
            aciklamaEN = aEN,
            acildi = false
        });
    }

    public void VeriyiKaydet()
    {
        string json = JsonUtility.ToJson(veri);
        PlayerPrefs.SetString(KAYIT_ANAHTARI, json);
        PlayerPrefs.Save();
    }

    public void TumVerileriSifirla()
    {
        PlayerPrefs.DeleteKey(KAYIT_ANAHTARI);
        veri = null;
        VeriyiYukleVeyaOlustur();
    }

    private void TelemetriOlayiGonder(string olayTipi, int bolumNo, Vector2 konum, string detay, int ardisikSayi)
    {
        if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseAnonKey))
            return;

        string payload = $"{{\"session_id\":\"{veri.oyuncuOturumId}\",\"event_type\":\"{olayTipi}\",\"level\":{bolumNo},\"pos_x\":{konum.x.ToString(CultureInfo.InvariantCulture)},\"pos_y\":{konum.y.ToString(CultureInfo.InvariantCulture)},\"detail\":\"{detay}\",\"consecutive_count\":{ardisikSayi},\"elapsed_sec\":{aktifBolumToplamSure.ToString("F2", CultureInfo.InvariantCulture)}}}";
        StartCoroutine(SupabasePostCoroutine(payload));
    }

    private IEnumerator SupabasePostCoroutine(string jsonPayload)
    {
        string endpoint = $"{supabaseUrl.TrimEnd('/')}/rest/v1/{tabloAdi}";
        using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", supabaseAnonKey);
            req.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
            req.SetRequestHeader("Prefer", "return=minimal");
            yield return req.SendWebRequest();
        }
    }
}
