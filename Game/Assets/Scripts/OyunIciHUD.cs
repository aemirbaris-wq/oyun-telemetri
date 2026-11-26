using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OyunIciHUD : MonoBehaviour
{
    [Header("Bolum Bilgileri")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private int toplananParca = 0;
    [SerializeField] private int toplamParca = 10;

    private SevenVeilsKontrolcu oyuncu;
    private Texture2D duzDoku;
    private bool duraklatildi;
    private float basarimBildirimSuresi;
    private string basarimBildirimBaslik = "";
    private string basarimBildirimAciklama = "";

    private static readonly string[] BolumBaslikTR =
    {
        "I. PERDE — OBURLUK",
        "II. PERDE — TEMBELLIK",
        "III. PERDE — SEHVET",
        "IV. PERDE — ACGOZLULUK",
        "V. PERDE — KIBIR",
        "VI. PERDE — KISKANCLIK",
        "VII. PERDE — OFKE (CEHENNEM)"
    };

    private static readonly string[] BolumBaslikEN =
    {
        "VEIL I — GLUTTONY",
        "VEIL II — SLOTH",
        "VEIL III — LUST",
        "VEIL IV — GREED",
        "VEIL V — PRIDE",
        "VEIL VI — ENVY",
        "VEIL VII — WRATH (HELL)"
    };

    private static readonly string[] MottoTR =
    {
        "\"Her lokma acligi buyutur.\"",
        "\"Duraksayani sis yutar.\"",
        "\"Gordugun her parilti bir golgedir.\"",
        "\"Parilti agirlastikca elin bosalir.\"",
        "\"Zirveye tirmanan dususu unutur.\"",
        "\"Baskasinin isigi kendi golgeni uzatir.\"",
        "\"Kendi atesinde yanan kul olur.\""
    };

    private static readonly string[] MottoEN =
    {
        "\"Every bite feeds the hunger.\"",
        "\"The mist swallows those who stall.\"",
        "\"Every shimmer you see is a shadow.\"",
        "\"The heavier the gleam, the emptier the hand.\"",
        "\"Those who climb the peak forget the fall.\"",
        "\"Another's light lengthens your own shadow.\"",
        "\"He who burns in his own fire turns to ash.\""
    };

    private static readonly string[] BoostTR =
    {
        "BOOST — Doyumsuz Adim: Parca alinca 2.5 sn hiz ve sicrama artar.",
        "BOOST — Agir Suzulus: Havada ziplamayi basili tutmak dususu yavaslatir.",
        "BOOST — Serap Sicrayisi: Havada 1 ekstra Cift Ziplama hakki verir.",
        "BOOST — Altin Miknatisi: Parcalari ceker, alinca Dash mesafesini uzatir.",
        "BOOST — Gok Sicrayisi: Yuksek dikey sicrama ve guclu duvar sekmesi.",
        "BOOST — Golge Yankisi: Dash bekleme suresi yariya iner.",
        "BOOST — Kor Ofkesi: Yuksek temel hiz ve daha uzun alevli Dash."
    };

    private static readonly string[] BoostEN =
    {
        "BOOST — Insatiable Step: Collecting fragments boosts speed & jump for 2.5s.",
        "BOOST — Heavy Glide: Hold Jump in mid-air to slow your descent.",
        "BOOST — Mirage Leap: Grants 1 extra Double Jump in mid-air.",
        "BOOST — Gold Magnet: Pulls nearby fragments & extends Dash distance.",
        "BOOST — Sky Leap: Higher vertical jump & stronger wall kick.",
        "BOOST — Shadow Echo: Cuts Dash cooldown in half.",
        "BOOST — Ember Wrath: Faster base speed & extended fiery Dash."
    };

    private void Awake()
    {
        duzDoku = new Texture2D(1, 1);
        duzDoku.SetPixel(0, 0, Color.white);
        duzDoku.Apply();
    }

    private void Start()
    {
        oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
        if (SevenVeilsTelemetriVeStats.Instance != null)
        {
            SevenVeilsTelemetriVeStats.Instance.OnBasarimAcildi += BasarimGoster;
        }
    }

    private void OnDestroy()
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
        {
            SevenVeilsTelemetriVeStats.Instance.OnBasarimAcildi -= BasarimGoster;
        }
        Time.timeScale = 1f;
    }

    public void BolumBilgisiAyarla(int yeniBolumNo, int toplam)
    {
        bolumNo = Mathf.Clamp(yeniBolumNo, 1, 7);
        toplamParca = Mathf.Max(1, toplam);
    }

    public void ParcaSayisiGuncelle(int toplanan, int toplam)
    {
        toplananParca = toplanan;
        toplamParca = Mathf.Max(1, toplam);
    }

    private void BasarimGoster(SevenVeilsTelemetriVeStats.BasarimVerisi b)
    {
        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";
        basarimBildirimBaslik = ingilizce ? b.baslikEN : b.baslikTR;
        basarimBildirimAciklama = ingilizce ? b.aciklamaEN : b.aciklamaTR;
        basarimBildirimSuresi = 4.2f;
    }

    private void Update()
    {
        if (basarimBildirimSuresi > 0f)
            basarimBildirimSuresi -= Time.unscaledDeltaTime;

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            duraklatildi = !duraklatildi;
            Time.timeScale = duraklatildi ? 0f : 1f;
        }
    }

    private void OnGUI()
    {
        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));

        int idx = Mathf.Clamp(bolumNo - 1, 0, 6);
        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";

        // Sol ust panel arka plani
        Rect panel = new Rect(24f, 22f, 540f, 168f);
        KutuCiz(panel, new Color(0.06f, 0.05f, 0.08f, 0.82f));
        CerceveCiz(panel, new Color(0.78f, 0.62f, 0.35f, 0.65f), 2f);

        GUIStyle baslikStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        baslikStil.normal.textColor = new Color(0.95f, 0.80f, 0.48f);

        GUIStyle mottoStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Italic
        };
        mottoStil.normal.textColor = new Color(0.86f, 0.84f, 0.90f);

        GUIStyle boostStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        bool boostAktif = oyuncu != null && oyuncu.BoostAktifMi;
        boostStil.normal.textColor = boostAktif ? new Color(1f, 0.45f, 0.25f) : new Color(0.62f, 0.84f, 0.96f);

        GUIStyle statStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        statStil.normal.textColor = Color.white;

        GUI.Label(new Rect(40f, 30f, 510f, 28f), ingilizce ? BolumBaslikEN[idx] : BolumBaslikTR[idx], baslikStil);
        GUI.Label(new Rect(40f, 58f, 510f, 24f), ingilizce ? MottoEN[idx] : MottoTR[idx], mottoStil);
        GUI.Label(new Rect(40f, 84f, 510f, 44f), ingilizce ? BoostEN[idx] : BoostTR[idx], boostStil);

        float sure = SevenVeilsTelemetriVeStats.Instance != null ? SevenVeilsTelemetriVeStats.Instance.AktifBolumSuresi : Time.timeSinceLevelLoad;
        int olum = SevenVeilsTelemetriVeStats.Instance != null ? SevenVeilsTelemetriVeStats.Instance.AktifBolumOlumu : 0;

        string sureMetni = SevenVeilsTelemetriVeStats.SureFormatla(sure);
        string altSatir = ingilizce
            ? $"TIME: {sureMetni}    |    DEATHS: {olum}    |    SHARDS: {toplananParca}/{toplamParca}"
            : $"SURE: {sureMetni}    |    OLUM: {olum}    |    PARCA: {toplananParca}/{toplamParca}";

        KutuCiz(new Rect(36f, 134f, 516f, 1f), new Color(1f, 1f, 1f, 0.16f));
        GUI.Label(new Rect(40f, 142f, 510f, 28f), altSatir, statStil);

        // Basarim acildi bildirimi (sag alt)
        if (basarimBildirimSuresi > 0f)
        {
            Rect toastRect = new Rect(1460f, 940f, 430f, 105f);
            KutuCiz(toastRect, new Color(0.08f, 0.06f, 0.10f, 0.94f));
            CerceveCiz(toastRect, new Color(0.95f, 0.75f, 0.30f, 0.95f), 2f);

            GUIStyle toastUst = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            toastUst.normal.textColor = new Color(0.95f, 0.78f, 0.35f);
            GUI.Label(new Rect(1478f, 948f, 395f, 22f), ingilizce ? "ACHIEVEMENT UNLOCKED" : "BASARIM ACILDI", toastUst);
            GUI.Label(new Rect(1478f, 970f, 395f, 26f), basarimBildirimBaslik, baslikStil);
            GUI.Label(new Rect(1478f, 996f, 395f, 40f), basarimBildirimAciklama, mottoStil);
        }

        // ESC Duraklatma Menusu
        if (duraklatildi)
        {
            KutuCiz(new Rect(0f, 0f, 1920f, 1080f), new Color(0f, 0f, 0f, 0.68f));
            Rect kutu = new Rect(735f, 360f, 450f, 330f);
            KutuCiz(kutu, new Color(0.09f, 0.07f, 0.12f, 0.96f));
            CerceveCiz(kutu, new Color(0.85f, 0.68f, 0.38f, 0.9f), 2f);

            GUIStyle ortaBaslik = new GUIStyle(baslikStil) { alignment = TextAnchor.MiddleCenter, fontSize = 24 };
            GUI.Label(new Rect(755f, 385f, 410f, 36f), ingilizce ? "PAUSED" : "OYUN DURAKLATILDI", ortaBaslik);

            if (GUI.Button(new Rect(795f, 450f, 330f, 52f), ingilizce ? "Resume (ESC)" : "Devam Et (ESC)"))
            {
                duraklatildi = false;
                Time.timeScale = 1f;
            }

            if (GUI.Button(new Rect(795f, 520f, 330f, 52f), ingilizce ? "Restart Level" : "Bolumu Yeniden Baslat"))
            {
                duraklatildi = false;
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(795f, 590f, 330f, 52f), ingilizce ? "Main Menu" : "Ana Menuye Don"))
            {
                duraklatildi = false;
                Time.timeScale = 1f;
                if (SevenVeilsTelemetriVeStats.Instance != null)
                {
                    float oran = toplamParca > 0 ? (toplananParca * 100f) / toplamParca : 0f;
                    SevenVeilsTelemetriVeStats.Instance.BolumTerkEdildi(oran);
                }
                if (Application.CanStreamedLevelBeLoaded("AnaMenu"))
                    SceneManager.LoadScene("AnaMenu");
                else
                    SceneManager.LoadScene(0);
            }
        }

        GUI.matrix = eskiMatris;
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
