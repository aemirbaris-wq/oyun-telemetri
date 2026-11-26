using UnityEngine;
using UnityEngine.SceneManagement;

public class BolumAkisYoneticisi : MonoBehaviour
{
    [Header("Bolum Kimligi")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private float hedefSpeedrunSuresi = 90f;

    private SevenVeilsKontrolcu oyuncu;
    private OyunIciHUD hud;
    private int toplananParca;
    private int toplamParca;
    private bool bolumBitti;
    private float gecisSayaci;

    public int BolumNo => bolumNo;
    public int ToplananParca => toplananParca;
    public int ToplamParca => toplamParca;

    public void BolumAyarla(int no, float speedrunHedef = 90f)
    {
        bolumNo = Mathf.Clamp(no, 1, 7);
        hedefSpeedrunSuresi = speedrunHedef;
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        if (SevenVeilsTelemetriVeStats.Instance == null)
        {
            GameObject telObj = new GameObject("SevenVeilsTelemetri");
            telObj.AddComponent<SevenVeilsTelemetriVeStats>();
        }
    }

    private void Start()
    {
        oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
        hud = FindFirstObjectByType<OyunIciHUD>();

        ToplanabilirParca[] parcalar = FindObjectsByType<ToplanabilirParca>(FindObjectsSortMode.None);
        toplamParca = Mathf.Max(1, parcalar.Length);
        toplananParca = 0;

        if (hud != null)
        {
            hud.BolumBilgisiAyarla(bolumNo, toplamParca);
            hud.ParcaSayisiGuncelle(toplananParca, toplamParca);
        }

        if (SevenVeilsTelemetriVeStats.Instance != null)
        {
            SevenVeilsTelemetriVeStats.Instance.BolumBaslat(bolumNo, toplamParca);
        }

        if (oyuncu != null)
        {
            oyuncu.OnZipladi += ZiplamaKaydet;
            oyuncu.OnDuvarZipladi += DuvarZiplamaKaydet;
            oyuncu.OnDashAtildi += DashKaydet;
            oyuncu.OnBoostTetiklendi += BoostKaydet;
            oyuncu.OnOlumGerceklesti += OlumKaydet;
        }

        ToplanabilirParca.OnParcaToplandi += ParcaToplandi;
        BolumBitisKapisi.OnBolumKapisiGecildi += KapiGecildi;
    }

    private void OnDestroy()
    {
        if (oyuncu != null)
        {
            oyuncu.OnZipladi -= ZiplamaKaydet;
            oyuncu.OnDuvarZipladi -= DuvarZiplamaKaydet;
            oyuncu.OnDashAtildi -= DashKaydet;
            oyuncu.OnBoostTetiklendi -= BoostKaydet;
            oyuncu.OnOlumGerceklesti -= OlumKaydet;
        }

        ToplanabilirParca.OnParcaToplandi -= ParcaToplandi;
        BolumBitisKapisi.OnBolumKapisiGecildi -= KapiGecildi;
    }

    private void ZiplamaKaydet()
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.MekanikKullanildi("ziplama");
    }

    private void DuvarZiplamaKaydet()
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.MekanikKullanildi("wall_jump");
    }

    private void DashKaydet(Vector3 pos, int yon)
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.MekanikKullanildi("dash");
    }

    private void BoostKaydet()
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.MekanikKullanildi("boost");
    }

    private void OlumKaydet(int bNo, Vector3 konum, string neden)
    {
        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.OlumKaydet(bolumNo, konum, neden);
    }

    private void ParcaToplandi(int bNo)
    {
        toplananParca++;
        if (hud != null)
            hud.ParcaSayisiGuncelle(toplananParca, toplamParca);

        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.ToplanabilirGuncelle(bolumNo, toplananParca, toplamParca);
    }

    private void KapiGecildi(int bNo)
    {
        if (bolumBitti) return;
        bolumBitti = true;
        gecisSayaci = 2.4f;

        if (SevenVeilsTelemetriVeStats.Instance != null)
            SevenVeilsTelemetriVeStats.Instance.BolumTamamlandi(bolumNo, toplananParca, toplamParca, hedefSpeedrunSuresi);
    }

    private void Update()
    {
        if (!bolumBitti) return;

        gecisSayaci -= Time.deltaTime;
        if (gecisSayaci <= 0f)
        {
            bolumBitti = false;
            string sonrakiSahne = $"Bolum{bolumNo + 1}";
            if (bolumNo < 7 && Application.CanStreamedLevelBeLoaded(sonrakiSahne))
            {
                SceneManager.LoadScene(sonrakiSahne);
            }
            else if (Application.CanStreamedLevelBeLoaded("AnaMenu"))
            {
                SceneManager.LoadScene("AnaMenu");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private void OnGUI()
    {
        if (!bolumBitti) return;

        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));

        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";
        GUIStyle stil = new GUIStyle(GUI.skin.box)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        stil.normal.textColor = new Color(0.98f, 0.84f, 0.45f);

        string metin;
        if (bolumNo >= 7)
        {
            bool secret = SevenVeilsTelemetriVeStats.Instance != null && SevenVeilsTelemetriVeStats.Instance.Veri.secretEndingAcildi;
            metin = secret
                ? (ingilizce ? "SECRET ENDING — ALL SEVEN VEILS LIFTED IN LIGHT" : "GIZLI SON — YEDI PERDE AYDINLIKTA ARALANDI")
                : (ingilizce ? "NORMAL ENDING — SHADOWS REMAIN" : "NORMAL SON — GOLGELERIN ARDINDA");
        }
        else
        {
            metin = ingilizce
                ? $"VEIL {bolumNo} CLEARED ({toplananParca}/{toplamParca})"
                : $"{bolumNo}. PERDE ARALANDI ({toplananParca}/{toplamParca})";
        }

        GUI.Box(new Rect(510f, 470f, 900f, 110f), metin, stil);
        GUI.matrix = eskiMatris;
    }
}
