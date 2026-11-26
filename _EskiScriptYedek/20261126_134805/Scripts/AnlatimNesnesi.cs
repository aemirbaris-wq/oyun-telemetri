using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnlatimNesnesi : MonoBehaviour
{
    public enum NesneTuru
    {
        GunahTableti,
        ErdemSunagi,
        YapimciImzasi
    }

    [Header("Kimlik ve Tur")]
    public NesneTuru tur = NesneTuru.GunahTableti;
    public string kayitKodu = "bolum1_tablet_1";
    [Range(1, 7)] public int erdemSirasi = 1;

    [Header("Turkce Metinler")]
    public string baslikTr = "TAŞ YAZIT";
    [TextArea(2, 5)]
    public string icerikTr = "...";

    [Header("Ingilizce Metinler")]
    public string baslikEn = "STONE INSCRIPTION";
    [TextArea(2, 5)]
    public string icerikEn = "...";

    [Header("Etkilesim Ayarlari")]
    public float etkilesimMesafesi = 1.75f;

    private Transform oyuncuTransform;
    private PlayerController oyuncuKontrol;
    private Transform gorselTransform;
    private Vector3 gorselBaslangicPos;
    private GameObject ipucuObjesi;
    private Text ipucuMetni;
    private GameObject okumaCanvas;
    private bool okumaAcik = false;
    private bool dahaOnceOkundu = false;

    public static int BulunanErdemSayisi
    {
        get
        {
            int toplam = 0;
            for (int i = 1; i <= 7; i++)
            {
                if (PlayerPrefs.GetInt("ErdemBulundu_" + i, 0) == 1) toplam++;
            }
            return toplam;
        }
    }

    private void Start()
    {
        GorselOlusturYoksa();
        IpucuOlustur();
    }

    private void GorselOlusturYoksa()
    {
        SpriteRenderer mevcut = GetComponentInChildren<SpriteRenderer>();
        if (mevcut != null)
        {
            gorselTransform = mevcut.transform;
            gorselBaslangicPos = gorselTransform.localPosition;
            return;
        }

        GameObject gorsel = new GameObject("ObeliskGorsel");
        gorsel.transform.SetParent(transform, false);
        gorselTransform = gorsel.transform;
        gorselBaslangicPos = Vector3.zero;

        SpriteRenderer sr = gorsel.AddComponent<SpriteRenderer>();
        int w = 24, h = 34;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color disRenk = tur == NesneTuru.ErdemSunagi
            ? new Color(0.92f, 0.78f, 0.36f, 1f)
            : (tur == NesneTuru.YapimciImzasi ? new Color(0.48f, 0.74f, 0.90f, 1f) : new Color(0.42f, 0.45f, 0.50f, 1f));
        Color icRenk = new Color(0.14f, 0.15f, 0.19f, 1f);
        Color runRenk = tur == NesneTuru.ErdemSunagi
            ? new Color(1f, 0.94f, 0.65f, 1f)
            : (tur == NesneTuru.YapimciImzasi ? new Color(0.72f, 0.90f, 1f, 1f) : new Color(0.76f, 0.78f, 0.84f, 1f));

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int daralma = y > 26 ? (y - 26) / 2 : 0;
                if (x < daralma || x >= w - daralma)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }
                bool kenar = x == daralma || x == (w - daralma - 1) || y == 0 || y == h - 1;
                bool merkezRun = (x >= 10 && x <= 13 && y >= 10 && y <= 22 && y % 4 != 0);
                tex.SetPixel(x, y, kenar ? disRenk : (merkezRun ? runRenk : icRenk));
            }
        }
        tex.Apply();

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 28f);
        sr.sortingOrder = 2;
    }

    private void IpucuOlustur()
    {
        ipucuObjesi = new GameObject("EtkilesimIpucu");
        ipucuObjesi.transform.SetParent(transform, false);
        ipucuObjesi.transform.localPosition = new Vector3(0f, 1.55f, 0f);

        Canvas c = ipucuObjesi.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 25;

        RectTransform rt = ipucuObjesi.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(170f, 38f);
        rt.localScale = Vector3.one * 0.011f;

        GameObject bg = new GameObject("Arka");
        bg.transform.SetParent(ipucuObjesi.transform, false);
        bg.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.88f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject txtObj = new GameObject("Yazi");
        txtObj.transform.SetParent(bg.transform, false);
        ipucuMetni = txtObj.AddComponent<Text>();
        ipucuMetni.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ipucuMetni.fontSize = 19;
        ipucuMetni.fontStyle = FontStyle.Bold;
        ipucuMetni.alignment = TextAnchor.MiddleCenter;
        ipucuMetni.color = tur == NesneTuru.ErdemSunagi ? new Color(1f, 0.88f, 0.48f) : new Color(0.88f, 0.91f, 0.96f);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        ipucuObjesi.SetActive(false);
    }

    private bool IngilizceMi()
    {
        if (DilYoneticisi.Ornek != null)
        {
            return DilYoneticisi.Ornek.Metin("pause_devam") == "RESUME";
        }
        return false;
    }

    private void Update()
    {
        if (gorselTransform != null && tur == NesneTuru.ErdemSunagi)
        {
            gorselTransform.localPosition = gorselBaslangicPos + new Vector3(0f, Mathf.Sin(Time.time * 2.4f) * 0.06f, 0f);
        }

        if (oyuncuTransform == null)
        {
            oyuncuKontrol = FindFirstObjectByType<PlayerController>();
            if (oyuncuKontrol != null) oyuncuTransform = oyuncuKontrol.transform;
            return;
        }

        Keyboard klavye = Keyboard.current;
        if (klavye == null) return;

        if (okumaAcik)
        {
            if (klavye.eKey.wasPressedThisFrame || klavye.escapeKey.wasPressedThisFrame || klavye.enterKey.wasPressedThisFrame)
            {
                Kapat();
            }
            return;
        }

        float mesafe = Vector2.Distance(transform.position, oyuncuTransform.position);
        bool yakinda = mesafe <= etkilesimMesafesi && oyuncuKontrol.isActiveAndEnabled;

        if (ipucuObjesi != null)
        {
            ipucuObjesi.SetActive(yakinda);
            if (yakinda && ipucuMetni != null)
            {
                ipucuMetni.text = IngilizceMi() ? "[E] READ" : "[E] OKU";
            }
        }

        if (yakinda && klavye.eKey.wasPressedThisFrame)
        {
            Ac();
        }
    }

    private void Ac()
    {
        okumaAcik = true;
        if (oyuncuKontrol != null) oyuncuKontrol.controlEnabled = false;
        if (ipucuObjesi != null) ipucuObjesi.SetActive(false);

        if (SesYoneticisi.Ornek != null)
        {
            string sesMetodu = tur == NesneTuru.ErdemSunagi ? "ErdemSunagiCal" : "TabletAcilisCal";
            SesYoneticisi.Ornek.SendMessage(sesMetodu, SendMessageOptions.DontRequireReceiver);
        }

        if (!dahaOnceOkundu)
        {
            dahaOnceOkundu = true;
            string olayAdi = tur == NesneTuru.ErdemSunagi ? "erdem_sunagi" : (tur == NesneTuru.YapimciImzasi ? "yapimci_imzasi" : "tablet_oku");
            Telemetri.OlayKaydet(olayAdi, Telemetri.AktifBolum);

            if (tur == NesneTuru.ErdemSunagi)
            {
                PlayerPrefs.SetInt("ErdemBulundu_" + erdemSirasi, 1);
                PlayerPrefs.Save();
            }
        }

        PencereOlustur();
    }

    private void Kapat()
    {
        okumaAcik = false;
        if (oyuncuKontrol != null) oyuncuKontrol.controlEnabled = true;
        if (okumaCanvas != null)
        {
            Destroy(okumaCanvas);
            okumaCanvas = null;
        }
    }

    private void PencereOlustur()
    {
        if (okumaCanvas != null) Destroy(okumaCanvas);

        okumaCanvas = new GameObject("TabletOkumaCanvas");
        Canvas canvas = okumaCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 105;

        CanvasScaler scaler = okumaCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        okumaCanvas.AddComponent<GraphicRaycaster>();

        Color kenarRenk = tur == NesneTuru.ErdemSunagi
            ? new Color(0.90f, 0.74f, 0.34f, 0.95f)
            : (tur == NesneTuru.YapimciImzasi ? new Color(0.46f, 0.72f, 0.90f, 0.95f) : new Color(0.46f, 0.49f, 0.56f, 0.90f));

        // Ekranin alt ortasinda zarif, kompakt diyalog penceresi
        GameObject cerceve = new GameObject("Cerceve");
        cerceve.transform.SetParent(okumaCanvas.transform, false);
        cerceve.AddComponent<Image>().color = kenarRenk;
        RectTransform cRt = cerceve.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0f);
        cRt.anchorMax = new Vector2(0.5f, 0f);
        cRt.pivot = new Vector2(0.5f, 0f);
        cRt.anchoredPosition = new Vector2(0f, 48f);
        cRt.sizeDelta = new Vector2(764f, 214f);

        GameObject kart = new GameObject("Kart");
        kart.transform.SetParent(cerceve.transform, false);
        kart.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f, 0.95f);
        RectTransform kRt = kart.GetComponent<RectTransform>();
        kRt.sizeDelta = new Vector2(758f, 208f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bool en = IngilizceMi();

        string turEtiketi = tur == NesneTuru.ErdemSunagi
            ? (en ? $"HIDDEN VIRTUE ({BulunanErdemSayisi}/7)" : $"GİZLİ ERDEM ({BulunanErdemSayisi}/7)")
            : (tur == NesneTuru.YapimciImzasi
                ? (en ? "ARCHITECT'S NOTE" : "MİMARIN NOTU")
                : (en ? "STONE INSCRIPTION" : "TAŞ YAZIT"));

        Text ustBilgi = MetinYap("UstBilgi", kart.transform, font, $"{turEtiketi}  •  {(en ? baslikEn : baslikTr)}", 17, FontStyle.Bold, kenarRenk);
        ustBilgi.alignment = TextAnchor.MiddleLeft;
        ustBilgi.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 68f);
        ustBilgi.GetComponent<RectTransform>().sizeDelta = new Vector2(690f, 30f);

        Text icerik = MetinYap("Icerik", kart.transform, font, en ? icerikEn : icerikTr, 19, FontStyle.Italic, new Color(0.88f, 0.90f, 0.94f));
        icerik.alignment = TextAnchor.UpperLeft;
        icerik.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -6f);
        icerik.GetComponent<RectTransform>().sizeDelta = new Vector2(690f, 96f);

        Text altKapat = MetinYap("AltKapat", kart.transform, font, en ? "[E] Close" : "[E] Kapat", 15, FontStyle.Normal, new Color(0.58f, 0.62f, 0.70f));
        altKapat.alignment = TextAnchor.MiddleRight;
        altKapat.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -74f);
        altKapat.GetComponent<RectTransform>().sizeDelta = new Vector2(690f, 26f);
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
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        return txt;
    }
}
