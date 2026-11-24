using System.Collections;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.UI;

public class BolumAtmosferi : MonoBehaviour
{
    [Header("Bolum Kimligi (1 = Tembellik ... 7 = Kibir)")]
    [Range(1, 7)] public int bolumNumarasi = 1;

    [Header("Karakter Shady/Twisted Donusumu (Fizik Degismez)")]
    public bool otomatikTwistedDonusum = true;
    public Sprite ozelKarakterGorunumu;

    [Header("Atmosfer Rengi")]
    public bool otomatikGokyuzuRengi = true;

    private static readonly Color[] GokyuzuRenkleri =
    {
        new Color(0.55f, 0.82f, 0.98f), // 1. Parlak cennet mavisi
        new Color(0.95f, 0.78f, 0.42f), // 2. Altin ikindi
        new Color(0.76f, 0.36f, 0.52f), // 3. Kizil ufuk
        new Color(0.34f, 0.22f, 0.42f), // 4. Araf moru
        new Color(0.14f, 0.24f, 0.20f), // 5. Sisli yesil-siyah
        new Color(0.32f, 0.08f, 0.08f), // 6. Volkanik kizil
        new Color(0.09f, 0.04f, 0.06f)  // 7. Cehennem zirvesi
    };

    private static readonly Color[] KarakterTwistedRenkleri =
    {
        new Color(1.00f, 1.00f, 1.00f), // 1. Saf aydinlik form
        new Color(0.94f, 0.89f, 0.78f), // 2. Hafif golgelenmis
        new Color(0.86f, 0.68f, 0.74f), // 3. Soluk kizilimsi
        new Color(0.70f, 0.58f, 0.78f), // 4. Araf golgesi
        new Color(0.50f, 0.66f, 0.54f), // 5. Zehirli golge
        new Color(0.72f, 0.28f, 0.28f), // 6. Kor kizili
        new Color(0.35f, 0.14f, 0.22f)  // 7. Tamamen Shady-Form
    };

    private void Start()
    {
        int indeks = Mathf.Clamp(bolumNumarasi - 1, 0, 6);

        if (otomatikGokyuzuRengi && Camera.main != null)
        {
            Camera.main.backgroundColor = GokyuzuRenkleri[indeks];
        }

        PlayerController oyuncu = FindFirstObjectByType<PlayerController>();
        if (oyuncu != null)
        {
            foreach (SpriteRenderer sr in oyuncu.GetComponentsInChildren<SpriteRenderer>())
            {
                if (ozelKarakterGorunumu != null) sr.sprite = ozelKarakterGorunumu;
                if (otomatikTwistedDonusum) sr.color = KarakterTwistedRenkleri[indeks];
            }
        }

        StartCoroutine(SinematikBaslikGoster());
    }

    private IEnumerator SinematikBaslikGoster()
    {
        yield return new WaitUntil(() => Telemetri.Ornek == null || Telemetri.Ornek.OturumAktifMi);

        if (SesYoneticisi.Ornek != null)
        {
            SesYoneticisi.Ornek.BolumAtmosferSesiBaslat(bolumNumarasi);
        }

        string baslikMetni = DilYoneticisi.Ornek != null
            ? DilYoneticisi.Ornek.Metin($"bolum_{bolumNumarasi}_baslik")
            : $"CHAPTER {bolumNumarasi}";
        string altMetin = DilYoneticisi.Ornek != null
            ? DilYoneticisi.Ornek.Metin($"bolum_{bolumNumarasi}_alt")
            : "";

        GameObject canvasObj = new GameObject("SinematikBolumCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        CanvasGroup grup = canvasObj.AddComponent<CanvasGroup>();
        grup.alpha = 0f;
        grup.blocksRaycasts = false;

        GameObject serit = new GameObject("SinematikSerit");
        serit.transform.SetParent(canvasObj.transform, false);
        serit.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.06f, 0.80f);
        RectTransform seritRect = serit.GetComponent<RectTransform>();
        seritRect.anchorMin = new Vector2(0f, 0.5f);
        seritRect.anchorMax = new Vector2(1f, 0.5f);
        seritRect.anchoredPosition = new Vector2(0f, 130f);
        seritRect.sizeDelta = new Vector2(0f, 185f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Text anaTxt = MetinOlustur("AnaBaslik", serit.transform, font, baslikMetni, 52, FontStyle.Bold, Color.white);
        anaTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 22f);
        anaTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(1600f, 80f);

        Text altTxt = MetinOlustur("AltBaslik", serit.transform, font, altMetin, 25, FontStyle.Italic, new Color(0.86f, 0.82f, 0.74f));
        altTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -38f);
        altTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(1600f, 50f);

        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            grup.alpha = Mathf.Clamp01(t / 0.6f);
            yield return null;
        }

        yield return new WaitForSeconds(2.3f);

        t = 0f;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            grup.alpha = 1f - Mathf.Clamp01(t / 0.8f);
            yield return null;
        }

        Destroy(canvasObj);
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
