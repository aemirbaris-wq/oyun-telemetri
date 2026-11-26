using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TasTabletVeErdem : MonoBehaviour
{
    public enum TabletTuru { GunahTableti, GizliErdemSunagi }

    [Header("Tablet Turu (Gunah = Ana Rota | GizliErdem = Kadinin Hikayesi)")]
    public TabletTuru tur = TabletTuru.GunahTableti;

    [Header("Turkce Icerik")]
    public string baslikTr = "Çatlak Taş Yazıt";
    [TextArea(3, 6)] public string metinTr = "Onu karanlıktan kurtarmak için her bedeli ödemeye hazırım.";

    [Header("Ingilizce Icerik")]
    public string baslikEn = "Cracked Stone Inscription";
    [TextArea(3, 6)] public string metinEn = "To save her from the dark, I am willing to pay any price.";

    [Header("Imza ve Ozel Gorsel (Easter Egg)")]
    public string imza = "— Emir Barış Al";
    public Sprite ozelGorsel;

    private bool oyuncuYakin = false;
    private bool okunuyor = false;
    private GameObject uyariCanvas;
    private GameObject okumaCanvas;

    private void Update()
    {
        if (!oyuncuYakin) return;
        Keyboard klavye = Keyboard.current;
        if (klavye == null) return;

        if (klavye.eKey.wasPressedThisFrame)
        {
            if (!okunuyor) YazitiAc();
            else YazitiKapat();
        }
        else if (okunuyor && klavye.escapeKey.wasPressedThisFrame)
        {
            YazitiKapat();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        oyuncuYakin = true;
        UyariGoster();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        oyuncuYakin = false;
        UyariKapat();
        YazitiKapat();
    }

    private void UyariGoster()
    {
        if (uyariCanvas != null) return;
        uyariCanvas = new GameObject("TabletUyariCanvas");
        Canvas canvas = uyariCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = uyariCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject bant = new GameObject("Bant");
        bant.transform.SetParent(uyariCanvas.transform, false);
        bant.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f, 0.85f);
        RectTransform r = bant.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0f);
        r.anchorMax = new Vector2(0.5f, 0f);
        r.anchoredPosition = new Vector2(0f, 90f);
        r.sizeDelta = new Vector2(360f, 46f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        string metin = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("etkilesim_oku") : "[ E ]  Yazıtı İncele";
        Color renk = tur == TabletTuru.GizliErdemSunagi ? new Color(0.65f, 0.92f, 1f) : new Color(0.96f, 0.84f, 0.52f);
        Text txt = MetinOlustur("UyariTxt", bant.transform, font, metin, 20, FontStyle.Bold, renk);
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 40f);
    }

    private void UyariKapat()
    {
        if (uyariCanvas != null)
        {
            Destroy(uyariCanvas);
            uyariCanvas = null;
        }
    }

    private void YazitiAc()
    {
        okunuyor = true;
        UyariKapat();

        if (SesYoneticisi.Ornek != null)
        {
            SesYoneticisi.Ornek.TabletSesiCal(tur == TabletTuru.GizliErdemSunagi);
        }

        bool tr = DilYoneticisi.Ornek == null || DilYoneticisi.Ornek.TurkceMi;
        string baslik = tr ? baslikTr : baslikEn;
        string govde = tr ? metinTr : metinEn;
        string kapatStr = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("etkilesim_kapat") : "[ E ] Kapat";

        okumaCanvas = new GameObject("TabletOkumaCanvas");
        Canvas canvas = okumaCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        CanvasScaler scaler = okumaCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        bool erdemMi = tur == TabletTuru.GizliErdemSunagi;
        Color arkaRenk = erdemMi ? new Color(0.08f, 0.14f, 0.18f, 0.96f) : new Color(0.11f, 0.08f, 0.09f, 0.96f);
        Color baslikRenk = erdemMi ? new Color(0.65f, 0.95f, 1f) : new Color(0.96f, 0.78f, 0.42f);

        GameObject kart = new GameObject("TabletKart");
        kart.transform.SetParent(okumaCanvas.transform, false);
        kart.AddComponent<Image>().color = arkaRenk;
        RectTransform kRect = kart.GetComponent<RectTransform>();
        kRect.sizeDelta = ozelGorsel != null ? new Vector2(1020f, 360f) : new Vector2(900f, 320f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        float kaydirX = ozelGorsel != null ? 105f : 0f;
        float genislik = ozelGorsel != null ? 720f : 800f;

        if (ozelGorsel != null)
        {
            GameObject imgObj = new GameObject("Gorsel");
            imgObj.transform.SetParent(kart.transform, false);
            Image img = imgObj.AddComponent<Image>();
            img.sprite = ozelGorsel;
            img.preserveAspect = true;
            imgObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-375f, 0f);
            imgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(190f, 190f);
        }

        Text bTxt = MetinOlustur("Baslik", kart.transform, font, baslik, 28, FontStyle.Bold, baslikRenk);
        bTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(kaydirX, 105f);
        bTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(genislik, 45f);

        Text mTxt = MetinOlustur("Metin", kart.transform, font, govde, 22, FontStyle.Normal, Color.white);
        mTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(kaydirX, 10f);
        mTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(genislik, 130f);

        Text iTxt = MetinOlustur("Imza", kart.transform, font, imza, 18, FontStyle.Italic, new Color(0.72f, 0.76f, 0.84f));
        iTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(kaydirX, -85f);
        iTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(genislik, 35f);

        Text kTxt = MetinOlustur("Kapat", kart.transform, font, kapatStr, 17, FontStyle.Bold, new Color(0.60f, 0.65f, 0.72f));
        kTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(kaydirX, -125f);
        kTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(genislik, 30f);
    }

    private void YazitiKapat()
    {
        okunuyor = false;
        if (okumaCanvas != null)
        {
            Destroy(okumaCanvas);
            okumaCanvas = null;
        }
        if (oyuncuYakin) UyariGoster();
    }

    private Text MetinOlustur(string ad, Transform ebeveyn, Font font, string icerik, int boyut, FontStyle stil, Color renk)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(ebeveyn, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = font;
        txt.text = icerik;
        txt.fontSize = boyut;
        txt.fontStyle = stil;
        txt.color = renk;
        txt.alignment = TextAnchor.MiddleCenter;
        return txt;
    }
}
