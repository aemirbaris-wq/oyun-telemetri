using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenuVeStatsDashboard : MonoBehaviour
{
    private enum AktifSekme
    {
        AnaMenu,
        BolumSecimi,
        StatsDashboard,
        Basarimlar
    }

    private AktifSekme sekme = AktifSekme.AnaMenu;
    private Texture2D duzDoku;
    private Vector2 basarimKaydirma;
    private Vector2[] tozParcaciklari;

    private static readonly string[] PerdeAdlariTR =
    {
        "I. PERDE — OBURLUK (Olcululuk)",
        "II. PERDE — TEMBELLIK (Gayret)",
        "III. PERDE — SEHVET (Ozdenetim)",
        "IV. PERDE — ACGOZLULUK (Comertlik)",
        "V. PERDE — KIBIR (Alcakgonulluluk)",
        "VI. PERDE — KISKANCLIK (Sukur)",
        "VII. PERDE — OFKE / CEHENNEM (Sukunet)"
    };

    private static readonly string[] PerdeAdlariEN =
    {
        "VEIL I — GLUTTONY (Temperance)",
        "VEIL II — SLOTH (Diligence)",
        "VEIL III — LUST (Self-Control)",
        "VEIL IV — GREED (Generosity)",
        "VEIL V — PRIDE (Humility)",
        "VEIL VI — ENVY (Gratitude)",
        "VEIL VII — WRATH / HELL (Serenity)"
    };

    private static readonly Color[] PerdeRenkleri =
    {
        new Color(0.95f, 0.68f, 0.24f),
        new Color(0.45f, 0.72f, 0.88f),
        new Color(0.86f, 0.34f, 0.68f),
        new Color(0.28f, 0.84f, 0.54f),
        new Color(0.84f, 0.88f, 0.96f),
        new Color(0.25f, 0.88f, 0.74f),
        new Color(0.96f, 0.28f, 0.18f)
    };

    private void Awake()
    {
        Time.timeScale = 1f;
        duzDoku = new Texture2D(1, 1);
        duzDoku.SetPixel(0, 0, Color.white);
        duzDoku.Apply();

        tozParcaciklari = new Vector2[38];
        for (int i = 0; i < tozParcaciklari.Length; i++)
        {
            tozParcaciklari[i] = new Vector2(Random.Range(0f, 1920f), Random.Range(0f, 1080f));
        }

        if (SevenVeilsTelemetriVeStats.Instance == null)
        {
            GameObject telObj = new GameObject("SevenVeilsTelemetri");
            telObj.AddComponent<SevenVeilsTelemetriVeStats>();
        }
    }

    private void Update()
    {
        if (tozParcaciklari == null) return;
        for (int i = 0; i < tozParcaciklari.Length; i++)
        {
            float hizY = 14f + (i % 5) * 6f;
            float salinimX = Mathf.Sin(Time.time * 0.9f + i) * 12f * Time.deltaTime;
            tozParcaciklari[i].y -= hizY * Time.deltaTime;
            tozParcaciklari[i].x += salinimX;
            if (tozParcaciklari[i].y < -10f)
            {
                tozParcaciklari[i].y = 1090f;
                tozParcaciklari[i].x = Random.Range(0f, 1920f);
            }
        }
    }

    private void OnGUI()
    {
        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));

        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";

        // Atolye karanlik arka plani ve ust-alt sinematik seritler
        KutuCiz(new Rect(0f, 0f, 1920f, 1080f), new Color(0.045f, 0.035f, 0.065f, 1f));
        KutuCiz(new Rect(0f, 0f, 1920f, 18f), new Color(0.02f, 0.015f, 0.03f, 1f));
        KutuCiz(new Rect(0f, 1062f, 1920f, 18f), new Color(0.02f, 0.015f, 0.03f, 1f));

        // Suzulen altin toz / kor parcaciklari
        if (tozParcaciklari != null)
        {
            for (int i = 0; i < tozParcaciklari.Length; i++)
            {
                float alfa = 0.18f + (i % 4) * 0.08f;
                float boyut = 2f + (i % 3);
                KutuCiz(new Rect(tozParcaciklari[i].x, tozParcaciklari[i].y, boyut, boyut), new Color(0.95f, 0.76f, 0.38f, alfa));
            }
        }

        // Sag ustte TR / EN Dil Secimi (Yalnizca Ana Menude)
        if (AltinButonCiz(new Rect(1715f, 34f, 76f, 38f), "TR", !ingilizce))
        {
            PlayerPrefs.SetString("SevenVeils_Dil", "TR");
            PlayerPrefs.Save();
        }
        if (AltinButonCiz(new Rect(1802f, 34f, 76f, 38f), "EN", ingilizce))
        {
            PlayerPrefs.SetString("SevenVeils_Dil", "EN");
            PlayerPrefs.Save();
        }

        // Baslik alani
        GUIStyle ustEtiket = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        ustEtiket.normal.textColor = new Color(0.76f, 0.60f, 0.34f);

        GUIStyle oyunBaslik = new GUIStyle(GUI.skin.label)
        {
            fontSize = 60,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        oyunBaslik.normal.textColor = new Color(0.96f, 0.80f, 0.44f);

        GUIStyle altBaslik = new GUIStyle(GUI.skin.label)
        {
            fontSize = 19,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        altBaslik.normal.textColor = new Color(0.76f, 0.72f, 0.82f);

        GUI.Label(new Rect(360f, 26f, 1200f, 24f), "ATELIER OF SHADOWS  •  MMXXVI", ustEtiket);
        GUI.Label(new Rect(360f, 48f, 1200f, 72f), ingilizce ? "S E V E N   V E I L S" : "S E V E N   V E I L S  •  Y E D I   P E R D E", oyunBaslik);
        GUI.Label(new Rect(360f, 118f, 1200f, 32f),
            ingilizce ? "Seven layers of paint, seven shadows over truth." : "Tuvaldeki yedi kat boya, hakikatin uzerindeki yedi golge.", altBaslik);

        if (sekme == AktifSekme.AnaMenu)
            CizAnaMenu(ingilizce);
        else if (sekme == AktifSekme.BolumSecimi)
            CizBolumSecimi(ingilizce);
        else if (sekme == AktifSekme.StatsDashboard)
            CizStatsDashboard(ingilizce);
        else if (sekme == AktifSekme.Basarimlar)
            CizBasarimlar(ingilizce);

        GUI.matrix = eskiMatris;
    }

    private void CizAnaMenu(bool ingilizce)
    {
        // Sol tarafta Ana Menu Dugmeleri
        float x = 260f;
        float w = 470f;
        float h = 68f;

        if (AltinButonCiz(new Rect(x, 275f, w, h), ingilizce ? "ENTER THE CANVAS  (VEIL I)" : "TUVALE ADIM AT  (I. PERDE)", true))
            BolumYukle(1);

        if (AltinButonCiz(new Rect(x, 365f, w, h), ingilizce ? "SELECT VEIL  (I - VII)" : "PERDE SECIMI  (I - VII)", false))
            sekme = AktifSekme.BolumSecimi;

        if (AltinButonCiz(new Rect(x, 455f, w, h), ingilizce ? "TELEMETRY & STATS DASHBOARD" : "TELEMETRI & ISTATISTIK PANELI", false))
            sekme = AktifSekme.StatsDashboard;

        if (AltinButonCiz(new Rect(x, 545f, w, h), ingilizce ? "ACHIEVEMENTS & SEALS" : "BASARIMLAR & MUHURLER", false))
            sekme = AktifSekme.Basarimlar;

        // Sag tarafta Yedi Perdeli Tuval Vitrini
        Rect tuvalDis = new Rect(860f, 225f, 800f, 520f);
        KutuCiz(tuvalDis, new Color(0.08f, 0.06f, 0.11f, 0.96f));
        CerceveCiz(tuvalDis, new Color(0.84f, 0.66f, 0.34f, 0.85f), 3f);

        GUIStyle vitrinBaslik = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        vitrinBaslik.normal.textColor = new Color(0.95f, 0.78f, 0.42f);
        GUI.Label(new Rect(tuvalDis.x, tuvalDis.y + 18f, tuvalDis.width, 30f),
            ingilizce ? "THE SEVEN VEILS OF THE UNFINISHED PORTRAIT" : "YARIM KALMIS PORTREDEKI YEDI PERDE", vitrinBaslik);

        SevenVeilsTelemetriVeStats tel = SevenVeilsTelemetriVeStats.Instance;
        float toplamYuzde = tel != null ? tel.YuzdeKacTamamlandi() : 0f;

        for (int i = 0; i < 7; i++)
        {
            int bNo = i + 1;
            var stat = tel != null ? tel.BolumStatGetir(bNo) : null;
            bool tamamlandi = stat != null && stat.tamamlandi;

            Rect serit = new Rect(tuvalDis.x + 44f, tuvalDis.y + 68f + i * 56f, tuvalDis.width - 88f, 44f);
            KutuCiz(serit, tamamlandi ? new Color(0.16f, 0.14f, 0.20f, 1f) : new Color(0.06f, 0.05f, 0.08f, 0.95f));
            CerceveCiz(serit, tamamlandi ? PerdeRenkleri[i] : new Color(0.35f, 0.32f, 0.40f, 0.5f), 1f);

            // Sol renk muhru
            KutuCiz(new Rect(serit.x + 8f, serit.y + 8f, 10f, 28f), tamamlandi ? PerdeRenkleri[i] : new Color(0.25f, 0.25f, 0.28f));

            GUIStyle satir = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            satir.normal.textColor = tamamlandi ? Color.white : new Color(0.62f, 0.60f, 0.66f);

            string ad = ingilizce ? PerdeAdlariEN[i] : PerdeAdlariTR[i];
            string durum = tamamlandi
                ? (ingilizce ? $"LIFTED  ({stat.toplananParca}/{stat.toplamParca})" : $"ARALANDI  ({stat.toplananParca}/{stat.toplamParca})")
                : (ingilizce ? "VEILED" : "ORTULU");

            GUI.Label(new Rect(serit.x + 28f, serit.y + 10f, 470f, 26f), ad, satir);
            satir.alignment = TextAnchor.MiddleRight;
            satir.normal.textColor = tamamlandi ? PerdeRenkleri[i] : new Color(0.48f, 0.46f, 0.52f);
            GUI.Label(new Rect(serit.xMax - 210f, serit.y + 10f, 195f, 26f), durum, satir);
        }

        // Alt toplam ilerleme cubugu
        Rect barArka = new Rect(tuvalDis.x + 44f, tuvalDis.yMax - 42f, tuvalDis.width - 88f, 20f);
        KutuCiz(barArka, new Color(0.04f, 0.03f, 0.05f, 1f));
        KutuCiz(new Rect(barArka.x, barArka.y, barArka.width * (toplamYuzde / 100f), barArka.height), new Color(0.95f, 0.76f, 0.36f, 0.9f));

        GUIStyle barYazi = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        barYazi.normal.textColor = Color.white;
        GUI.Label(barArka, ingilizce ? $"TRUTH UNVEILED: %{toplamYuzde:F1}" : $"HAKIKAT ARALANMA ORANI: %{toplamYuzde:F1}", barYazi);

        // Kontroller alt bilgisi
        GUIStyle kontrolBilgi = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
        kontrolBilgi.normal.textColor = new Color(0.68f, 0.65f, 0.74f);
        GUI.Label(new Rect(260f, 820f, 1400f, 34f),
            ingilizce
                ? "CONTROLS:  A / D or Arrows = Move   |   Space / W = Jump & Wall Jump   |   Left Shift / J = Shadow Dash   |   ESC = Pause"
                : "KONTROLLER:  A / D veya Yon Tuslari = Hareket   |   Space / W = Ziplama & Duvar Sekmesi   |   Sol Shift / J = Shadow Dash   |   ESC = Duraklat",
            kontrolBilgi);
    }

    private void CizBolumSecimi(bool ingilizce)
    {
        for (int i = 0; i < 7; i++)
        {
            int bolumNo = i + 1;
            string sahneAdi = $"Bolum{bolumNo}";
            bool hazir = Application.CanStreamedLevelBeLoaded(sahneAdi);
            string etiket = ingilizce ? PerdeAdlariEN[i] : PerdeAdlariTR[i];
            if (!hazir) etiket += ingilizce ? "   [In Development]" : "   [Siradaki Perde]";

            GUI.enabled = hazir;
            if (AltinButonCiz(new Rect(610f, 200f + i * 76f, 700f, 60f), etiket, hazir))
                BolumYukle(bolumNo);
            GUI.enabled = true;
        }

        if (AltinButonCiz(new Rect(810f, 765f, 300f, 54f), ingilizce ? "BACK TO ATELIER" : "ATOLYEYE DON", false))
            sekme = AktifSekme.AnaMenu;
    }

    private void CizStatsDashboard(bool ingilizce)
    {
        SevenVeilsTelemetriVeStats tel = SevenVeilsTelemetriVeStats.Instance;
        if (tel == null) return;

        var veri = tel.Veri;
        Rect anaKutu = new Rect(180f, 175f, 1560f, 770f);
        KutuCiz(anaKutu, new Color(0.07f, 0.06f, 0.10f, 0.97f));
        CerceveCiz(anaKutu, new Color(0.84f, 0.66f, 0.35f, 0.85f), 2f);

        GUIStyle kartBaslik = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
        kartBaslik.normal.textColor = new Color(0.95f, 0.78f, 0.42f);

        GUIStyle kartDeger = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, wordWrap = true };
        kartDeger.normal.textColor = Color.white;

        CizStatKarti(new Rect(210f, 200f, 355f, 115f),
            ingilizce ? "TOTAL PLAYTIME" : "TOPLAM OYNAMA SURESI",
            SevenVeilsTelemetriVeStats.SureFormatla(veri.toplamOynamaSuresi), kartBaslik, kartDeger);

        CizStatKarti(new Rect(590f, 200f, 355f, 115f),
            ingilizce ? "TOTAL DEATHS & COMPLETION" : "TOPLAM OLUM & TAMAMLANMA",
            $"{veri.toplamOlum}   |   %{tel.YuzdeKacTamamlandi():F1}", kartBaslik, kartDeger);

        var enCokNeden = tel.EnCokOldurenNeden();
        string nedenMetni = enCokNeden != null ? $"{enCokNeden.neden} ({enCokNeden.sayi}x)" : "-";
        CizStatKarti(new Rect(970f, 200f, 355f, 115f),
            ingilizce ? "NEMESIS HAZARD" : "EN COK OLDUREN ENGEL",
            nedenMetni, kartBaslik, kartDeger);

        var ragebait = tel.EnCokRagebaitYenenNokta();
        string rbMetni = ragebait != null
            ? (ingilizce ? $"Veil {ragebait.bolumNo} ({ragebait.x},{ragebait.y}) - {ragebait.toplamOlum}x" : $"{ragebait.bolumNo}. Perde ({ragebait.x},{ragebait.y}) - {ragebait.toplamOlum}x")
            : "-";
        CizStatKarti(new Rect(1350f, 200f, 360f, 115f),
            ingilizce ? "TOP RAGEBAIT HOTSPOT" : "EN COK TAKILINAN NOKTA",
            rbMetni, kartBaslik, kartDeger);

        GUIStyle satirStil = new GUIStyle(GUI.skin.label) { fontSize = 15 };
        satirStil.normal.textColor = new Color(0.84f, 0.88f, 0.94f);
        string mekanikOzet = ingilizce
            ? $"Jumps: {veri.toplamZiplama}   |   Shadow Dash: {veri.toplamDash}   |   Wall Jumps: {veri.toplamWallJump}   |   Stomps: {veri.toplamEzilenDusman}   |   Sin Boosts: {veri.toplamBoostTetikleme}"
            : $"Ziplama: {veri.toplamZiplama}   |   Shadow Dash: {veri.toplamDash}   |   Duvar Sekmesi: {veri.toplamWallJump}   |   Ezilen Dusman: {veri.toplamEzilenDusman}   |   Gunah Boost: {veri.toplamBoostTetikleme}";
        GUI.Label(new Rect(215f, 332f, 1490f, 28f), mekanikOzet, satirStil);

        float tabloY = 375f;
        GUI.Label(new Rect(215f, tabloY, 340f, 28f), ingilizce ? "VEIL / SIN" : "PERDE / GUNAH", kartBaslik);
        GUI.Label(new Rect(570f, tabloY, 200f, 28f), ingilizce ? "BEST TIME" : "EN IYI SURE", kartBaslik);
        GUI.Label(new Rect(780f, tabloY, 150f, 28f), ingilizce ? "DEATHS" : "OLUM", kartBaslik);
        GUI.Label(new Rect(940f, tabloY, 200f, 28f), ingilizce ? "0-DEATH BADGE" : "0 OLUM ROZETI", kartBaslik);
        GUI.Label(new Rect(1160f, tabloY, 180f, 28f), ingilizce ? "SHARDS" : "PARCALAR", kartBaslik);
        GUI.Label(new Rect(1360f, tabloY, 340f, 28f), ingilizce ? "SECRET ROOM / LORE" : "GIZLI ODA / YAZIT", kartBaslik);

        for (int i = 1; i <= 7; i++)
        {
            var b = tel.BolumStatGetir(i);
            float y = tabloY + 38f + (i - 1) * 56f;
            KutuCiz(new Rect(210f, y - 4f, 1500f, 46f), new Color(1f, 1f, 1f, i % 2 == 0 ? 0.04f : 0.08f));

            string perdeAdi = ingilizce ? PerdeAdlariEN[i - 1] : PerdeAdlariTR[i - 1];
            string enIyi = SevenVeilsTelemetriVeStats.SureFormatla(b.enIyiSure);
            string rozet = b.sifirOlumRozeti ? (ingilizce ? "UNTOUCHABLE" : "DOKUNULMAZ") : "-";
            string parca = $"{b.toplananParca} / {b.toplamParca}";
            string kesif = $"{(b.gizliOdaBulundu ? "Oda: +" : "Oda: -")}  |  {(b.gunahYazitiOkundu ? "Gunah: +" : "Gunah: -")}  |  {(b.erdemSunagiOkundu ? "Erdem: +" : "Erdem: -")}";

            GUI.Label(new Rect(215f, y + 6f, 345f, 30f), perdeAdi, satirStil);
            GUI.Label(new Rect(570f, y + 6f, 200f, 30f), enIyi, satirStil);
            GUI.Label(new Rect(780f, y + 6f, 150f, 30f), b.toplamOlum.ToString(), satirStil);
            GUI.Label(new Rect(940f, y + 6f, 200f, 30f), rozet, satirStil);
            GUI.Label(new Rect(1160f, y + 6f, 180f, 30f), parca, satirStil);
            GUI.Label(new Rect(1360f, y + 6f, 350f, 30f), kesif, satirStil);
        }

        if (AltinButonCiz(new Rect(700f, 975f, 240f, 52f), ingilizce ? "BACK TO ATELIER" : "ATOLYEYE DON", false))
            sekme = AktifSekme.AnaMenu;

        if (AltinButonCiz(new Rect(970f, 975f, 260f, 52f), ingilizce ? "RESET TELEMETRY" : "VERILERI SIFIRLA", false))
            tel.TumVerileriSifirla();
    }

    private void CizStatKarti(Rect rect, string baslik, string deger, GUIStyle bStil, GUIStyle dStil)
    {
        KutuCiz(rect, new Color(0.11f, 0.09f, 0.15f, 1f));
        CerceveCiz(rect, new Color(0.80f, 0.62f, 0.32f, 0.65f), 1f);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 26f), baslik, bStil);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 42f, rect.width - 28f, 62f), deger, dStil);
    }

    private void CizBasarimlar(bool ingilizce)
    {
        SevenVeilsTelemetriVeStats tel = SevenVeilsTelemetriVeStats.Instance;
        if (tel == null) return;

        Rect kutu = new Rect(360f, 185f, 1200f, 750f);
        KutuCiz(kutu, new Color(0.07f, 0.06f, 0.10f, 0.97f));
        CerceveCiz(kutu, new Color(0.84f, 0.66f, 0.35f, 0.85f), 2f);

        var liste = tel.Veri.basarimlar;
        basarimKaydirma = GUI.BeginScrollView(new Rect(385f, 210f, 1150f, 695f), basarimKaydirma, new Rect(0f, 0f, 1110f, liste.Count * 82f));

        GUIStyle bStil = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
        GUIStyle aStil = new GUIStyle(GUI.skin.label) { fontSize = 15 };

        for (int i = 0; i < liste.Count; i++)
        {
            var item = liste[i];
            float y = i * 82f;
            KutuCiz(new Rect(0f, y, 1110f, 72f), item.acildi ? new Color(0.18f, 0.15f, 0.10f, 0.95f) : new Color(0.10f, 0.09f, 0.13f, 0.78f));

            bStil.normal.textColor = item.acildi ? new Color(0.98f, 0.80f, 0.35f) : new Color(0.55f, 0.55f, 0.60f);
            aStil.normal.textColor = item.acildi ? Color.white : new Color(0.50f, 0.50f, 0.55f);

            string durum = item.acildi ? (ingilizce ? $"[UNLOCKED {item.acilmaTarihi}]" : $"[ACILDI {item.acilmaTarihi}]") : (ingilizce ? "[LOCKED]" : "[KILITLI]");
            GUI.Label(new Rect(18f, y + 8f, 820f, 28f), ingilizce ? item.baslikEN : item.baslikTR, bStil);
            GUI.Label(new Rect(840f, y + 10f, 250f, 26f), durum, aStil);
            GUI.Label(new Rect(18f, y + 38f, 1060f, 26f), ingilizce ? item.aciklamaEN : item.aciklamaTR, aStil);
        }

        GUI.EndScrollView();

        if (AltinButonCiz(new Rect(810f, 965f, 300f, 54f), ingilizce ? "BACK TO ATELIER" : "ATOLYEYE DON", false))
            sekme = AktifSekme.AnaMenu;
    }

    private bool AltinButonCiz(Rect rect, string metin, bool vurgulu)
    {
        Color dolgu = vurgulu ? new Color(0.22f, 0.16f, 0.09f, 0.96f) : new Color(0.11f, 0.09f, 0.15f, 0.94f);
        Color cerceve = vurgulu ? new Color(0.96f, 0.78f, 0.36f, 0.95f) : new Color(0.75f, 0.58f, 0.30f, 0.65f);

        KutuCiz(rect, dolgu);
        CerceveCiz(rect, cerceve, vurgulu ? 2f : 1f);

        GUIStyle stil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        stil.normal.textColor = vurgulu ? new Color(1f, 0.88f, 0.52f) : new Color(0.92f, 0.88f, 0.80f);

        return GUI.Button(rect, metin, stil);
    }

    private void BolumYukle(int bolumNo)
    {
        string sahneAdi = $"Bolum{bolumNo}";
        if (Application.CanStreamedLevelBeLoaded(sahneAdi))
            SceneManager.LoadScene(sahneAdi);
    }

    private void KutuCiz(Rect rect, Color renk)
    {
        Color eski = GUI.color;
        GUI.color = renk;
        GUI.DrawTexture(rect, duzDoku);
        GUI.color = eski;
    }

    private void CerceveCiz(Rect rect, Color renk, float kalinlik)
    {
        KutuCiz(new Rect(rect.x, rect.y, rect.width, kalinlik), renk);
        KutuCiz(new Rect(rect.x, rect.yMax - kalinlik, rect.width, kalinlik), renk);
        KutuCiz(new Rect(rect.x, rect.y, kalinlik, rect.height), renk);
        KutuCiz(new Rect(rect.xMax - kalinlik, rect.y, kalinlik, rect.height), renk);
    }
}
