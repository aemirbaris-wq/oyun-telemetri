using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class AnaMenuVeProtokol : MonoBehaviour
{
    private GameObject menuCanvas;
    private GameObject anaPanel;
    private GameObject protokolPanel;
    private GameObject kontrolBilgiKutusu;

    private Text oyunAdiTxt;
    private Text altBaslikTxt;
    private Text oynaBtnTxt;
    private Text kontrolBtnTxt;
    private Text kontrolDetayTxt;
    private Text cikisBtnTxt;
    private Text imzaTxt;

    private Text protokolBaslikTxt;
    private Text protokolMetinTxt;
    private Text onayBtnTxt;
    private Text geriBtnTxt;

    private void Start()
    {
        if (Telemetri.Ornek != null && Telemetri.Ornek.OturumAktifMi)
        {
            return;
        }

        Time.timeScale = 0f;
        MenuyuKur();
        MetinleriGuncelle();
        DilYoneticisi.DilDegisti += MetinleriGuncelle;
        StartCoroutine(AcilisAnimasyonu());
    }

    private void OnDestroy()
    {
        DilYoneticisi.DilDegisti -= MetinleriGuncelle;
    }

    private void MenuyuKur()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        menuCanvas = new GameObject("SinematikAnaMenuCanvas");
        Canvas canvas = menuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = menuCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        menuCanvas.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Derin atmosferik arka plan
        GameObject arkaPlan = PanelOlustur("ArkaPlan", menuCanvas.transform, new Color(0.05f, 0.06f, 0.09f, 0.96f));
        TamEkran(arkaPlan.GetComponent<RectTransform>());

        // Ust bar: Sag ustte [ TR | EN ] dil degistirici
        ButonOlustur("DilTR", arkaPlan.transform, font, "TR", new Vector2(820, 470), new Vector2(68, 42), new Color(0.18f, 0.22f, 0.30f), out _, () =>
        {
            if (DilYoneticisi.Ornek != null) DilYoneticisi.Ornek.DilDegistir(true);
        });
        ButonOlustur("DilEN", arkaPlan.transform, font, "EN", new Vector2(898, 470), new Vector2(68, 42), new Color(0.18f, 0.22f, 0.30f), out _, () =>
        {
            if (DilYoneticisi.Ornek != null) DilYoneticisi.Ornek.DilDegistir(false);
        });

        // 1. ANA MENU PANELI
        anaPanel = new GameObject("AnaMenuPaneli");
        anaPanel.transform.SetParent(arkaPlan.transform, false);
        RectTransform anaRect = anaPanel.AddComponent<RectTransform>();
        TamEkran(anaRect);
        anaPanel.AddComponent<CanvasGroup>().alpha = 0f;

        oyunAdiTxt = MetinOlustur("OyunAdi", anaPanel.transform, font, "YEDİ PERDE", 76, FontStyle.Bold, new Color(0.96f, 0.92f, 0.84f));
        oyunAdiTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 230);
        oyunAdiTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 110);

        altBaslikTxt = MetinOlustur("AltBaslik", anaPanel.transform, font, "", 24, FontStyle.Italic, new Color(0.72f, 0.76f, 0.84f));
        altBaslikTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 145);
        altBaslikTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(1200, 50);

        ButonOlustur("OynaBtn", anaPanel.transform, font, "", new Vector2(0, 20), new Vector2(420, 72), new Color(0.18f, 0.52f, 0.42f), out oynaBtnTxt, () =>
        {
            anaPanel.SetActive(false);
            protokolPanel.SetActive(true);
        });

        ButonOlustur("KontrolBtn", anaPanel.transform, font, "", new Vector2(0, -72), new Vector2(420, 64), new Color(0.16f, 0.19f, 0.26f), out kontrolBtnTxt, () =>
        {
            kontrolBilgiKutusu.SetActive(!kontrolBilgiKutusu.activeSelf);
        });

        ButonOlustur("CikisBtn", anaPanel.transform, font, "", new Vector2(0, -156), new Vector2(420, 60), new Color(0.28f, 0.14f, 0.18f), out cikisBtnTxt, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        // Kontrol bilgi bandi
        kontrolBilgiKutusu = PanelOlustur("KontrolKutusu", anaPanel.transform, new Color(0.10f, 0.12f, 0.17f, 0.95f));
        kontrolBilgiKutusu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -255);
        kontrolBilgiKutusu.GetComponent<RectTransform>().sizeDelta = new Vector2(1250, 64);
        kontrolDetayTxt = MetinOlustur("KontrolDetay", kontrolBilgiKutusu.transform, font, "", 20, FontStyle.Normal, new Color(0.88f, 0.92f, 0.98f));
        TamEkran(kontrolDetayTxt.GetComponent<RectTransform>());
        kontrolBilgiKutusu.SetActive(false);

        imzaTxt = MetinOlustur("Imza", anaPanel.transform, font, "", 18, FontStyle.Italic, new Color(0.55f, 0.60f, 0.70f));
        imzaTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -475);
        imzaTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 40);

        // 2. ARASTIRMA PROTOKOLU (KVKK / GDPR) PANELI
        protokolPanel = PanelOlustur("ProtokolPaneli", arkaPlan.transform, new Color(0.09f, 0.11f, 0.16f, 0.98f));
        protokolPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(980, 600);
        protokolPanel.SetActive(false);

        protokolBaslikTxt = MetinOlustur("ProtokolBaslik", protokolPanel.transform, font, "", 30, FontStyle.Bold, new Color(0.95f, 0.86f, 0.66f));
        protokolBaslikTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 215);
        protokolBaslikTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 60);

        protokolMetinTxt = MetinOlustur("ProtokolMetin", protokolPanel.transform, font, "", 21, FontStyle.Normal, new Color(0.88f, 0.90f, 0.94f));
        protokolMetinTxt.alignment = TextAnchor.UpperLeft;
        protokolMetinTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
        protokolMetinTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(860, 280);

        ButonOlustur("OnayBtn", protokolPanel.transform, font, "", new Vector2(160, -200), new Vector2(360, 66), new Color(0.18f, 0.56f, 0.42f), out onayBtnTxt, () =>
        {
            Time.timeScale = 1f;
            Destroy(menuCanvas);
            if (Telemetri.Ornek != null)
            {
                Telemetri.Ornek.OturumuBaslat();
            }
        });

        ButonOlustur("GeriBtn", protokolPanel.transform, font, "", new Vector2(-210, -200), new Vector2(260, 66), new Color(0.22f, 0.25f, 0.32f), out geriBtnTxt, () =>
        {
            protokolPanel.SetActive(false);
            anaPanel.SetActive(true);
        });
    }

    private IEnumerator AcilisAnimasyonu()
    {
        CanvasGroup cg = anaPanel.GetComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.75f)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / 0.75f);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private void MetinleriGuncelle()
    {
        DilYoneticisi d = DilYoneticisi.Ornek;
        if (d == null) return;

        oyunAdiTxt.text = d.Metin("oyun_adi");
        altBaslikTxt.text = d.Metin("menu_alt_baslik");
        oynaBtnTxt.text = d.Metin("menu_oyna");
        kontrolBtnTxt.text = d.Metin("menu_kontroller");
        kontrolDetayTxt.text = d.Metin("menu_kontroller_detay");
        cikisBtnTxt.text = d.Metin("menu_cikis");
        imzaTxt.text = d.Metin("imza");

        protokolBaslikTxt.text = d.Metin("kvkk_baslik");
        protokolMetinTxt.text = d.Metin("kvkk_metin");
        onayBtnTxt.text = d.Metin("kvkk_onay");
        geriBtnTxt.text = d.Metin("kvkk_geri");
    }

    private GameObject PanelOlustur(string ad, Transform ebeveyn, Color renk)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(ebeveyn, false);
        obj.AddComponent<Image>().color = renk;
        return obj;
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

    private void ButonOlustur(string ad, Transform ebeveyn, Font font, string etiket, Vector2 pozisyon, Vector2 boyut, Color renk, out Text etiketTxt, UnityEngine.Events.UnityAction tiklama)
    {
        GameObject obj = PanelOlustur(ad, ebeveyn, renk);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = pozisyon;
        rect.sizeDelta = boyut;

        Button btn = obj.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (SesYoneticisi.Ornek != null) SesYoneticisi.Ornek.ButonTiklaCal();
            tiklama?.Invoke();
        });

        EventTrigger tetik = obj.AddComponent<EventTrigger>();
        EventTrigger.Entry hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hover.callback.AddListener((_) =>
        {
            rect.localScale = new Vector3(1.04f, 1.04f, 1f);
            if (SesYoneticisi.Ornek != null) SesYoneticisi.Ornek.ButonHoverCal();
        });
        tetik.triggers.Add(hover);

        EventTrigger.Entry cikis = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        cikis.callback.AddListener((_) => rect.localScale = Vector3.one);
        tetik.triggers.Add(cikis);

        etiketTxt = MetinOlustur("Metin", obj.transform, font, etiket, 22, FontStyle.Bold, Color.white);
        TamEkran(etiketTxt.GetComponent<RectTransform>());
    }

    private void TamEkran(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
