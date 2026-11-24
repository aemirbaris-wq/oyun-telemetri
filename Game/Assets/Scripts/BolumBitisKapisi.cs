using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BolumBitisKapisi : MonoBehaviour
{
    public int bolumNo = 1;
    public float tetiklemeYaricapi = 1.65f;

    private bool tamamlandi = false;
    private float baslangicZamani;
    private PlayerController oyuncu;
    private GameObject bitisCanvas;

    private void Start()
    {
        baslangicZamani = Time.time;
    }

    private void Update()
    {
        if (tamamlandi)
        {
            Keyboard klavye = Keyboard.current;
            if (klavye != null && (klavye.enterKey.wasPressedThisFrame || klavye.spaceKey.wasPressedThisFrame))
            {
                SonrakiBolumeGec();
            }
            return;
        }

        if (oyuncu == null)
        {
            oyuncu = FindFirstObjectByType<PlayerController>();
            return;
        }

        if (!oyuncu.isActiveAndEnabled) return;

        Vector3 kapiMerkez = transform.position + new Vector3(0f, 1.2f, 0f);
        if (Vector2.Distance(kapiMerkez, oyuncu.transform.position) <= tetiklemeYaricapi)
        {
            BolumuTamamla();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tamamlandi) return;
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            oyuncu = pc;
            BolumuTamamla();
        }
    }

    private void BolumuTamamla()
    {
        if (tamamlandi) return;
        tamamlandi = true;

        if (oyuncu != null)
        {
            oyuncu.controlEnabled = false;
            oyuncu.velocity = Vector2.zero;
        }

        Telemetri.OlayKaydet("bolum_tamamla", Telemetri.AktifBolum);

        if (SesYoneticisi.Ornek != null)
        {
            SesYoneticisi.Ornek.SendMessage("ErdemSunagiCal", SendMessageOptions.DontRequireReceiver);
        }

        BitisEkraniniAc();
    }

    private bool IngilizceMi()
    {
        if (DilYoneticisi.Ornek != null)
        {
            return DilYoneticisi.Ornek.Metin("pause_devam") == "RESUME";
        }
        return false;
    }

    private void BitisEkraniniAc()
    {
        if (bitisCanvas != null) Destroy(bitisCanvas);

        bitisCanvas = new GameObject("BolumBitisCanvas");
        Canvas canvas = bitisCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = bitisCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        bitisCanvas.AddComponent<GraphicRaycaster>();

        GameObject perde = PanelYap("Perde", bitisCanvas.transform, new Color(0.02f, 0.02f, 0.04f, 0.86f));
        TamEkran(perde.GetComponent<RectTransform>());

        GameObject cerceve = PanelYap("Cerceve", perde.transform, new Color(0.88f, 0.72f, 0.34f, 0.95f));
        cerceve.GetComponent<RectTransform>().sizeDelta = new Vector2(646f, 476f);

        GameObject kart = PanelYap("Kart", cerceve.transform, new Color(0.07f, 0.08f, 0.12f, 0.98f));
        kart.GetComponent<RectTransform>().sizeDelta = new Vector2(640f, 470f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bool en = IngilizceMi();

        int gecenSaniye = Mathf.Max(1, Mathf.RoundToInt(Time.time - baslangicZamani));
        int dk = gecenSaniye / 60;
        int sn = gecenSaniye % 60;
        int erdemSayisi = AnlatimNesnesi.BulunanErdemSayisi;

        string ustEtiket = en ? $"THRESHOLD {bolumNo} CROSSED" : $"{bolumNo}. EŞİK AŞILDI";
        string anaBaslik = en ? "CHAPTER COMPLETED" : "BÖLÜM TAMAMLANDI";
        string ozetMetin = en
            ? $"Elapsed Time: {dk:00}:{sn:00}   •   Hidden Virtues: {erdemSayisi} / 7"
            : $"Geçen Süre: {dk:00}:{sn:00}   •   Bulunan Erdem: {erdemSayisi} / 7";

        Text tUst = MetinYap("Ust", kart.transform, font, ustEtiket, 17, FontStyle.Bold, new Color(0.90f, 0.74f, 0.34f));
        tUst.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 175f);
        tUst.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 34f);

        Text tBaslik = MetinYap("Baslik", kart.transform, font, anaBaslik, 34, FontStyle.Bold, new Color(0.96f, 0.94f, 0.88f));
        tBaslik.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 125f);
        tBaslik.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 52f);

        Text tOzet = MetinYap("Ozet", kart.transform, font, ozetMetin, 20, FontStyle.Normal, new Color(0.78f, 0.82f, 0.88f));
        tOzet.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 65f);
        tOzet.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 40f);

        int aktifIndex = SceneManager.GetActiveScene().buildIndex;
        bool sonrakiVar = (aktifIndex + 1) < SceneManager.sceneCountInBuildSettings;

        string sonrakiEtiket = sonrakiVar
            ? (en ? "NEXT CHAPTER" : "SONRAKİ BÖLÜM")
            : (en ? "NEXT CHAPTER (SOON — REPLAY)" : "SONRAKİ BÖLÜM (YAKINDA — TEKRAR OYNA)");

        ButonYap("SonrakiBtn", kart.transform, font, sonrakiEtiket, new Vector2(0f, -15f), new Color(0.18f, 0.54f, 0.40f), SonrakiBolumeGec);
        ButonYap("TekrarBtn", kart.transform, font, en ? "REPLAY CHAPTER" : "BÖLÜMÜ TEKRAR OYNA", new Vector2(0f, -92f), new Color(0.24f, 0.27f, 0.36f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(aktifIndex);
        });
        ButonYap("MenuBtn", kart.transform, font, en ? "MAIN MENU" : "ANA MENÜYE DÖN", new Vector2(0f, -169f), new Color(0.38f, 0.18f, 0.22f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });
    }

    private void SonrakiBolumeGec()
    {
        Time.timeScale = 1f;
        int aktifIndex = SceneManager.GetActiveScene().buildIndex;
        if (aktifIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(aktifIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(aktifIndex);
        }
    }

    private GameObject PanelYap(string ad, Transform parent, Color renk)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = renk;
        return obj;
    }

    private Text MetinYap(string ad, Transform parent, Font font, string yazi, int boyut, FontStyle stil, Color renk)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = font;
        txt.text = yazi;
        txt.fontSize = boyut;
        txt.fontStyle = stil;
        txt.color = renk;
        txt.alignment = TextAnchor.MiddleCenter;
        return txt;
    }

    private void ButonYap(string ad, Transform parent, Font font, string etiket, Vector2 pos, Color renk, UnityEngine.Events.UnityAction tikla)
    {
        GameObject obj = PanelYap(ad, parent, renk);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(440f, 58f);

        Button btn = obj.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (SesYoneticisi.Ornek != null) SesYoneticisi.Ornek.ButonTiklaCal();
            tikla?.Invoke();
        });

        Text txt = MetinYap("Yazi", obj.transform, font, etiket, 19, FontStyle.Bold, Color.white);
        TamEkran(txt.GetComponent<RectTransform>());
    }

    private void TamEkran(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
