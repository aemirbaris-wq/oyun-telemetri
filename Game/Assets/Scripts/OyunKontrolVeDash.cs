using System.Collections;
using System.Reflection;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OyunKontrolVeDash : MonoBehaviour
{
    [Header("Space Ruh Atilimi (Dash Boost) Ayarlari")]
    public float atilimHizi = 8.4f;
    public float atilimSuresi = 0.095f;
    public float beklemeSuresi = 1.15f;

    [Header("Kontrol Ayarlari")]
    public bool yukariOkZiplaAktif = true;
    public bool asagiOkSertInisAktif = true;

    private PlayerController oyuncu;
    private SpriteRenderer spriteRenderer;
    private bool atilimYapiliyor = false;
    private float sonAtilimZamani = -10f;
    private int sonYon = 1;

    private FieldInfo moveField;
    private FieldInfo targetVelocityField;

    private GameObject pauseCanvas;
    private bool duraklatildi = false;

    private void Start()
    {
        OyuncuReferansiniGuncelle(true);
    }

    private void OyuncuReferansiniGuncelle(bool zorla = false)
    {
        if (!zorla && oyuncu != null && oyuncu.isActiveAndEnabled) return;

        oyuncu = FindFirstObjectByType<PlayerController>();
        if (oyuncu != null)
        {
            spriteRenderer = oyuncu.GetComponentInChildren<SpriteRenderer>();
            moveField = typeof(PlayerController).GetField("move", BindingFlags.Instance | BindingFlags.NonPublic);
            targetVelocityField = typeof(KinematicObject).GetField("targetVelocity", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        else
        {
            spriteRenderer = null;
        }
    }

    private void Update()
    {
        Keyboard klavye = Keyboard.current;
        if (klavye == null) return;

        if (klavye.escapeKey.wasPressedThisFrame)
        {
            if (Telemetri.Ornek == null || Telemetri.Ornek.OturumAktifMi)
            {
                DuraklatmaDegistir();
            }
        }

        if (duraklatildi) return;

        OyuncuReferansiniGuncelle(false);
        if (oyuncu == null || !oyuncu.isActiveAndEnabled || !oyuncu.controlEnabled) return;

        if (klavye.leftArrowKey.isPressed || klavye.aKey.isPressed) sonYon = -1;
        else if (klavye.rightArrowKey.isPressed || klavye.dKey.isPressed) sonYon = 1;
        else if (spriteRenderer != null) sonYon = spriteRenderer.flipX ? -1 : 1;

        // YUKARI OK / W: Basinca zipla, erken birakinca kisa zipla
        if (yukariOkZiplaAktif)
        {
            if (klavye.upArrowKey.wasPressedThisFrame || klavye.wKey.wasPressedThisFrame)
            {
                oyuncu.TetikleZiplaHarici();
            }
            if ((klavye.upArrowKey.wasReleasedThisFrame || klavye.wKey.wasReleasedThisFrame) && !klavye.upArrowKey.isPressed && !klavye.wKey.isPressed)
            {
                oyuncu.DurdurZiplaHarici();
            }
        }

        // ASAGI OK / S: Havadayken kontrollu hizli inis (yerde titreme/scale bozulmasi yok)
        if (asagiOkSertInisAktif)
        {
            bool asagiBasili = klavye.downArrowKey.isPressed || klavye.sKey.isPressed;
            oyuncu.UygulaAsagiOkEtkisi(asagiBasili);
        }

        // SPACE: Sakin ve kontrollu yatay ruh atilimi
        if (klavye.spaceKey.wasPressedThisFrame && !atilimYapiliyor && Time.time >= sonAtilimZamani + beklemeSuresi)
        {
            StartCoroutine(RuhAtilimiYap());
        }
    }

    private IEnumerator RuhAtilimiYap()
    {
        atilimYapiliyor = true;
        sonAtilimZamani = Time.time;

        if (SesYoneticisi.Ornek != null)
        {
            SesYoneticisi.Ornek.DashSesiCal();
        }

        Color orijinalRenk = spriteRenderer != null ? spriteRenderer.color : Color.white;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.75f, 0.94f, 1f, 0.96f);
        }

        float gecen = 0f;
        while (gecen < atilimSuresi)
        {
            OyuncuReferansiniGuncelle(false);
            if (oyuncu == null || !oyuncu.isActiveAndEnabled || !oyuncu.controlEnabled) break;

            gecen += Time.deltaTime;
            float yumusakOran = 1f - (0.35f * Mathf.Clamp01(gecen / atilimSuresi));
            oyuncu.velocity = new Vector2(sonYon * atilimHizi * yumusakOran, oyuncu.velocity.y);

            if (targetVelocityField != null)
            {
                targetVelocityField.SetValue(oyuncu, new Vector2(sonYon * atilimHizi * yumusakOran, 0f));
            }
            if (moveField != null)
            {
                moveField.SetValue(oyuncu, new Vector2(sonYon * 0.65f, 0f));
            }

            yield return null;
        }

        if (oyuncu != null && oyuncu.isActiveAndEnabled && targetVelocityField != null)
        {
            targetVelocityField.SetValue(oyuncu, Vector2.zero);
        }

        if (spriteRenderer != null && oyuncu != null && oyuncu.isActiveAndEnabled)
        {
            spriteRenderer.color = orijinalRenk;
        }
        atilimYapiliyor = false;
    }

    public void DuraklatmaDegistir()
    {
        duraklatildi = !duraklatildi;
        if (duraklatildi)
        {
            Time.timeScale = 0f;
            Telemetri.OlayKaydet("duraklat", Telemetri.AktifBolum);
            DuraklatmaMenusuAc();
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseCanvas != null)
            {
                Destroy(pauseCanvas);
                pauseCanvas = null;
            }
        }
    }

    private void DuraklatmaMenusuAc()
    {
        if (pauseCanvas != null) Destroy(pauseCanvas);

        pauseCanvas = new GameObject("PauseCanvas");
        Canvas canvas = pauseCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = pauseCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        pauseCanvas.AddComponent<GraphicRaycaster>();

        GameObject perde = PanelOlustur("Perde", pauseCanvas.transform, new Color(0.03f, 0.03f, 0.05f, 0.86f));
        TamEkran(perde.GetComponent<RectTransform>());

        GameObject kart = PanelOlustur("Kart", perde.transform, new Color(0.09f, 0.10f, 0.14f, 0.98f));
        kart.GetComponent<RectTransform>().sizeDelta = new Vector2(540, 440);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        string baslikStr = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("pause_baslik") : "ZAMAN DURDURULDU";
        string devamStr = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("pause_devam") : "DEVAM ET";
        string yenidenStr = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("pause_yeniden") : "BÖLÜMÜ YENİDEN BAŞLAT";
        string menuStr = DilYoneticisi.Ornek != null ? DilYoneticisi.Ornek.Metin("pause_menu") : "ANA MENÜYE DÖN";

        Text baslik = MetinOlustur("Baslik", kart.transform, font, baslikStr, 30, FontStyle.Bold, new Color(0.95f, 0.88f, 0.72f));
        baslik.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 145);
        baslik.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 50);

        ButonOlustur("DevamBtn", kart.transform, font, devamStr, new Vector2(0, 45), new Color(0.18f, 0.55f, 0.42f), () => DuraklatmaDegistir());
        ButonOlustur("YenidenBtn", kart.transform, font, yenidenStr, new Vector2(0, -40), new Color(0.24f, 0.27f, 0.36f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        ButonOlustur("MenuBtn", kart.transform, font, menuStr, new Vector2(0, -125), new Color(0.38f, 0.18f, 0.22f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });
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

    private void ButonOlustur(string ad, Transform ebeveyn, Font font, string etiket, Vector2 pozisyon, Color renk, UnityEngine.Events.UnityAction tiklama)
    {
        GameObject obj = PanelOlustur(ad, ebeveyn, renk);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = pozisyon;
        rect.sizeDelta = new Vector2(400, 62);

        Button btn = obj.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (SesYoneticisi.Ornek != null) SesYoneticisi.Ornek.ButonTiklaCal();
            tiklama?.Invoke();
        });

        Text txt = MetinOlustur("Metin", obj.transform, font, etiket, 20, FontStyle.Bold, Color.white);
        TamEkran(txt.GetComponent<RectTransform>());
    }

    private void TamEkran(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
