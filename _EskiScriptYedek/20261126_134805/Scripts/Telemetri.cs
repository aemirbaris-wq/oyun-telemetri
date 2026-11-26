using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Telemetri : MonoBehaviour
{
    public static Telemetri Ornek { get; private set; }

    [Header("Supabase Baglanti Ayarlari")]
    [SerializeField] private string supabaseUrl = "https://vdhrsfklinmzffhpvpco.supabase.co";
    [SerializeField] private string publishableKey = "sb_publishable_eEryzTt8DbXN0jOihTRDdw_mXUBqR7g";

    [Header("Oyun Surumu ve Test")]
    [SerializeField] private string surum = "v1";
    [SerializeField] private bool testModuOtomatikBaslat = false;

    private string oturumId;
    private string oyuncuId;
    private int olaySirasi = 0;
    private float oturumBaslangicZamani;
    private bool oturumAktif = false;
    private string bekleyenOlumNedeni = "cukur";

    public bool OturumAktifMi => oturumAktif;

    public static int AktifBolum
    {
        get
        {
            string sahneAdi = SceneManager.GetActiveScene().name;
            Match eslesme = Regex.Match(sahneAdi, @"(\d+)$");
            if (eslesme.Success && int.TryParse(eslesme.Groups[1].Value, out int no))
            {
                return no;
            }
            return 1;
        }
    }

    private void Awake()
    {
        if (Ornek != null && Ornek != this)
        {
            Destroy(gameObject);
            return;
        }
        Ornek = this;
        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey("oyuncu_id"))
        {
            PlayerPrefs.SetString("oyuncu_id", Guid.NewGuid().ToString());
            PlayerPrefs.Save();
        }
        oyuncuId = PlayerPrefs.GetString("oyuncu_id");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SahneYuklendi;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    private void Start()
    {
        if (testModuOtomatikBaslat && !oturumAktif)
        {
            OturumuBaslat();
        }
    }

    private void SahneYuklendi(Scene sahne, LoadSceneMode mod)
    {
        if (oturumAktif)
        {
            OlayKaydet("bolum_basla", AktifBolum);
        }
    }

    public void OturumuBaslat()
    {
        oturumId = Guid.NewGuid().ToString();
        olaySirasi = 0;
        oturumBaslangicZamani = Time.realtimeSinceStartup;
        oturumAktif = true;

        OlayKaydet("oturum_basla", 0);
        OlayKaydet("bolum_basla", AktifBolum);
    }

    public static void SonOlumNedeniAta(string neden)
    {
        if (Ornek != null && !string.IsNullOrEmpty(neden))
        {
            Ornek.bekleyenOlumNedeni = neden;
        }
    }

    public static string SonOlumNedeniTuket()
    {
        if (Ornek == null) return "cukur";
        string sonuc = Ornek.bekleyenOlumNedeni;
        Ornek.bekleyenOlumNedeni = "cukur";
        return sonuc;
    }

    public static void BolumSonuGecisiBaslat()
    {
        if (Ornek != null)
        {
            Ornek.StartCoroutine(Ornek.SonrakiSahneyeGec());
        }
    }

    private IEnumerator SonrakiSahneyeGec()
    {
        yield return new WaitForSeconds(1.5f);
        int mevcutIndex = SceneManager.GetActiveScene().buildIndex;
        int toplamSahne = SceneManager.sceneCountInBuildSettings;
        if (mevcutIndex + 1 < toplamSahne)
        {
            SceneManager.LoadScene(mevcutIndex + 1);
        }
        else
        {
            Debug.Log("[Telemetri] Son sahne tamamlandi.");
        }
    }

    public static void OlayKaydet(string tur, int bolum, float? x = null, float? y = null, string neden = null)
    {
        if (Ornek == null) return;
        Ornek.Gonder(tur, bolum, x, y, neden);
    }

    private void Gonder(string tur, int bolum, float? x, float? y, string neden)
    {
        if (!oturumAktif) return;

        olaySirasi++;
        int gonderilenSira = olaySirasi;
        int gecenMs = Mathf.Max(0, Mathf.RoundToInt((Time.realtimeSinceStartup - oturumBaslangicZamani) * 1000f));

        string xDeger = x.HasValue ? x.Value.ToString("F2", CultureInfo.InvariantCulture) : "null";
        string yDeger = y.HasValue ? y.Value.ToString("F2", CultureInfo.InvariantCulture) : "null";
        string nedenDeger = string.IsNullOrEmpty(neden) ? "null" : $"\"{JsonKacis(neden)}\"";

        string json = "{"
            + $"\"oturum_id\":\"{oturumId}\","
            + $"\"oyuncu_id\":\"{oyuncuId}\","
            + $"\"surum\":\"{JsonKacis(surum)}\","
            + $"\"sira\":{gonderilenSira},"
            + $"\"t_ms\":{gecenMs},"
            + $"\"tur\":\"{JsonKacis(tur)}\","
            + $"\"bolum\":{bolum},"
            + $"\"x\":{xDeger},"
            + $"\"y\":{yDeger},"
            + $"\"neden\":{nedenDeger}"
            + "}";

        StartCoroutine(PostIstegiAt(json, tur, gonderilenSira, neden));
    }

    private IEnumerator PostIstegiAt(string jsonGovde, string tur, int siraNo, string neden)
    {
        string ucNokta = supabaseUrl.TrimEnd('/') + "/rest/v1/olay";
        byte[] veri = Encoding.UTF8.GetBytes(jsonGovde);

        using (UnityWebRequest istek = new UnityWebRequest(ucNokta, "POST"))
        {
            istek.uploadHandler = new UploadHandlerRaw(veri);
            istek.downloadHandler = new DownloadHandlerBuffer();
            istek.SetRequestHeader("Content-Type", "application/json");
            istek.SetRequestHeader("apikey", publishableKey);
            istek.SetRequestHeader("Prefer", "return=minimal");

            if (publishableKey.StartsWith("eyJ"))
            {
                istek.SetRequestHeader("Authorization", "Bearer " + publishableKey);
            }

            yield return istek.SendWebRequest();

            if (istek.result == UnityWebRequest.Result.Success)
            {
                string nedenBilgi = string.IsNullOrEmpty(neden) ? "" : $" [neden: {neden}]";
                Debug.Log($"[Telemetri] Kaydedildi -> {tur}{nedenBilgi} (bolum: {AktifBolum}, sira: {siraNo})");
            }
            else
            {
                Debug.LogWarning($"[Telemetri] Hata ({istek.responseCode}): {istek.error} | {istek.downloadHandler.text}");
            }
        }
    }

    private string JsonKacis(string metin)
    {
        return metin.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
