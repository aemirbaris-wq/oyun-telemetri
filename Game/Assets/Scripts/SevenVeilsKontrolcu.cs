using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SevenVeilsKontrolcu : MonoBehaviour
{
    public enum GunahBoostTipi
    {
        Yok,
        DoyumsuzAdim,
        AgirSuzulus,
        SerapSicrayisi,
        AltinMiknatisi,
        GokSicrayisi,
        GolgeYankisi,
        KorOfkesi
    }

    [Header("Temel Hareket")]
    [SerializeField] private float hareketHizi = 8.4f;
    [SerializeField] private float ziplamaGucu = 15.0f;
    [SerializeField] private float kisaZiplamaCarpani = 0.5f;
    [SerializeField] private float maksimumDusmeHizi = -22f;

    [Header("Mario & Celeste Hissi")]
    [SerializeField] private float coyoteSuresi = 0.14f;
    [SerializeField] private float jumpBufferSuresi = 0.15f;
    [SerializeField] private float tepeNoktasiYerCekimiCarpani = 0.65f;
    [SerializeField] private float inisYerCekimiCarpani = 1.35f;

    [Header("Shadow Dash & Duvar")]
    [SerializeField] private float dashHizi = 24f;
    [SerializeField] private float dashSuresi = 0.18f;
    [SerializeField] private float dashBeklemeSuresi = 0.35f;
    [SerializeField] private float duvarKaymaHizi = -2.2f;
    [SerializeField] private Vector2 duvarZiplamaGucu = new Vector2(11.8f, 14.6f);

    [Header("Bolume Ozel Gunah Boost")]
    [SerializeField] private GunahBoostTipi aktifBoost = GunahBoostTipi.Yok;
    [SerializeField] private float boostSuresi = 2.5f;

    [Header("Zemin ve Duvar Kontrolu")]
    [SerializeField] private LayerMask zeminKatmani;
    [SerializeField] private float ayakMesafesi = 0.16f;
    [SerializeField] private float yanMesafe = 0.14f;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Color temelSpriteRengi = Color.white;
    private PhysicsMaterial2D surtunmesizMateryal;
    private float yatayGirdi;
    private bool ziplamaBasiliTutuluyor;
    private float coyoteSayaci;
    private float jumpBufferSayaci;
    private float dashSayaci;
    private float dashBeklemeSayaci;
    private float boostKalanSure;
    private int kalanHavaDash = 1;
    private int kalanEkstraZiplama;
    private int bakilanYon = 1;
    private bool yerde;
    private bool duvarda;
    private int duvarYonu;
    private bool kilitli;
    private Vector3 dogusNoktasi;
    private Vector2 aktifPlatformHizi;
    private float izOlusturmaSayaci;

    public bool YerdeMi => yerde;
    public bool DashAtiyorMu => dashSayaci > 0f;
    public bool BoostAktifMi => boostKalanSure > 0f;
    public int BakilanYon => bakilanYon;
    public GunahBoostTipi AktifBoost => aktifBoost;

    public event Action OnZipladi;
    public event Action OnDuvarZipladi;
    public event Action<Vector3, int> OnDashAtildi;
    public event Action OnBoostTetiklendi;
    public event Action<int, Vector3, string> OnOlumGerceklesti;
    public event Action OnYenidenDogdu;

    private void Awake()
    {
        if (InputSystem.settings != null)
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) temelSpriteRengi = sr.color;

        surtunmesizMateryal = new PhysicsMaterial2D("SevenVeils_Surtunmesiz")
        {
            friction = 0f,
            bounciness = 0f
        };
        dogusNoktasi = transform.position;
        GuvenliFizikVeDurumSifirla();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SahneYuklendi;
        GuvenliFizikVeDurumSifirla();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    private void SahneYuklendi(Scene sahne, LoadSceneMode mod)
    {
        GuvenliFizikVeDurumSifirla();
    }

    public void GuvenliFizikVeDurumSifirla()
    {
        CancelInvoke(nameof(DogusNoktasinaDon));
        Time.timeScale = 1f;

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3.2f;
            rb.linearVelocity = Vector2.zero;
        }

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
            if (surtunmesizMateryal != null)
                col.sharedMaterial = surtunmesizMateryal;
        }

        if (sr != null)
        {
            temelSpriteRengi = Color.white;
            sr.color = temelSpriteRengi;
        }

        kilitli = false;
        dashSayaci = 0f;
        dashBeklemeSayaci = 0f;
        coyoteSayaci = 0f;
        jumpBufferSayaci = 0f;
        boostKalanSure = 0f;
        aktifPlatformHizi = Vector2.zero;
        kalanHavaDash = 1;
        kalanEkstraZiplama = aktifBoost == GunahBoostTipi.SerapSicrayisi ? 1 : 0;
    }

    public void AktifBoostuAyarla(GunahBoostTipi yeniBoost)
    {
        aktifBoost = yeniBoost;
        kalanEkstraZiplama = aktifBoost == GunahBoostTipi.SerapSicrayisi ? 1 : 0;
    }

    public void ToplanabilirBoostTetikle()
    {
        if (aktifBoost == GunahBoostTipi.DoyumsuzAdim)
        {
            boostKalanSure = boostSuresi;
            OnBoostTetiklendi?.Invoke();
        }
        else if (aktifBoost == GunahBoostTipi.AltinMiknatisi)
        {
            boostKalanSure = boostSuresi;
            kalanHavaDash = 1;
            OnBoostTetiklendi?.Invoke();
        }
    }

    public void CheckpointGuncelle(Vector3 yeniNokta)
    {
        dogusNoktasi = yeniNokta;
    }

    public void HariciZiplamaUygula(float dikeyHiz)
    {
        kalanHavaDash = 1;
        if (aktifBoost == GunahBoostTipi.SerapSicrayisi) kalanEkstraZiplama = 1;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, dikeyHiz);
    }

    public void OlumVeYenidenDogus(int bolumNo = 1, string olumNedeni = "Tehlike")
    {
        if (kilitli) return;
        kilitli = true;
        aktifPlatformHizi = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        OnOlumGerceklesti?.Invoke(bolumNo, transform.position, olumNedeni);
        Invoke(nameof(DogusNoktasinaDon), 0.35f);
    }

    private void DogusNoktasinaDon()
    {
        transform.position = dogusNoktasi;
        GuvenliFizikVeDurumSifirla();
        OnYenidenDogdu?.Invoke();
    }

    private void Update()
    {
        if (kilitli || Time.timeScale <= 0.001f) return;

        GirdileriOku(out bool ziplamaBasildi, out bool ziplamaBirakildi, out bool dashBasildi);
        YuzeyKontrolleri();

        if (yerde)
        {
            coyoteSayaci = coyoteSuresi;
            kalanHavaDash = 1;
            kalanEkstraZiplama = aktifBoost == GunahBoostTipi.SerapSicrayisi ? 1 : 0;
        }
        else
        {
            coyoteSayaci -= Time.deltaTime;
        }

        if (ziplamaBasildi) jumpBufferSayaci = jumpBufferSuresi;
        else jumpBufferSayaci -= Time.deltaTime;

        if (dashBeklemeSayaci > 0f) dashBeklemeSayaci -= Time.deltaTime;
        if (boostKalanSure > 0f) boostKalanSure -= Time.deltaTime;

        if (Mathf.Abs(yatayGirdi) > 0.01f && dashSayaci <= 0f)
        {
            bakilanYon = yatayGirdi > 0f ? 1 : -1;
            transform.localScale = new Vector3(bakilanYon, 1f, 1f);
        }

        if (dashBasildi && dashSayaci <= 0f && dashBeklemeSayaci <= 0f)
        {
            if (yerde || kalanHavaDash > 0)
            {
                if (!yerde) kalanHavaDash--;
                float beklemeCarpani = aktifBoost == GunahBoostTipi.GolgeYankisi ? 0.5f : 1f;
                dashSayaci = dashSuresi;
                dashBeklemeSayaci = dashBeklemeSuresi * beklemeCarpani;
                izOlusturmaSayaci = 0f;
                OnDashAtildi?.Invoke(transform.position, bakilanYon);
                if (aktifBoost == GunahBoostTipi.GolgeYankisi || aktifBoost == GunahBoostTipi.KorOfkesi)
                    OnBoostTetiklendi?.Invoke();
            }
        }

        if (jumpBufferSayaci > 0f)
        {
            if (coyoteSayaci > 0f)
            {
                ZiplamaUygula(ziplamaGucu * AktifZiplamaCarpani());
                jumpBufferSayaci = 0f;
                coyoteSayaci = 0f;
                OnZipladi?.Invoke();
                if (aktifBoost == GunahBoostTipi.GokSicrayisi) OnBoostTetiklendi?.Invoke();
            }
            else if (duvarda && !yerde)
            {
                float duvarCarpani = aktifBoost == GunahBoostTipi.GokSicrayisi ? 1.18f : 1f;
                rb.linearVelocity = new Vector2(-duvarYonu * duvarZiplamaGucu.x, duvarZiplamaGucu.y * duvarCarpani);
                bakilanYon = -duvarYonu;
                transform.localScale = new Vector3(bakilanYon, 1f, 1f);
                jumpBufferSayaci = 0f;
                OnDuvarZipladi?.Invoke();
            }
            else if (kalanEkstraZiplama > 0)
            {
                kalanEkstraZiplama--;
                ZiplamaUygula(ziplamaGucu * 0.92f);
                jumpBufferSayaci = 0f;
                OnZipladi?.Invoke();
                OnBoostTetiklendi?.Invoke();
            }
        }

        if (ziplamaBirakildi && rb.linearVelocity.y > 0f && dashSayaci <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * kisaZiplamaCarpani);
        }

        if (dashSayaci > 0f && sr != null)
        {
            izOlusturmaSayaci -= Time.deltaTime;
            if (izOlusturmaSayaci <= 0f)
            {
                izOlusturmaSayaci = 0.04f;
                GolgeIziBirak();
            }
        }
    }

    private void GolgeIziBirak()
    {
        if (sr == null || sr.sprite == null) return;
        GameObject iz = new GameObject("Dash_FircaIzi");
        iz.transform.position = transform.position;
        iz.transform.localScale = transform.localScale;
        SpriteRenderer izSr = iz.AddComponent<SpriteRenderer>();
        izSr.sprite = sr.sprite;
        izSr.sortingOrder = sr.sortingOrder - 1;
        izSr.color = new Color(0.95f, 0.72f, 0.32f, 0.45f);
        Destroy(iz, 0.16f);
    }

    private void FixedUpdate()
    {
        if (kilitli) return;

        if (dashSayaci > 0f)
        {
            dashSayaci -= Time.fixedDeltaTime;
            float mesafeCarpani = (aktifBoost == GunahBoostTipi.AltinMiknatisi && boostKalanSure > 0f) ? 1.25f :
                                  (aktifBoost == GunahBoostTipi.KorOfkesi ? 1.18f : 1f);
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(bakilanYon * dashHizi * mesafeCarpani, 0f);
            return;
        }

        float hiz = hareketHizi * AktifHizCarpani();
        float hedefX = (yatayGirdi * hiz) + (yerde ? aktifPlatformHizi.x : 0f);
        rb.linearVelocity = new Vector2(hedefX, rb.linearVelocity.y);

        float tabanYerCekimi = 3.2f;
        if (rb.linearVelocity.y < 0f)
        {
            float suzulmeCarpani = (aktifBoost == GunahBoostTipi.AgirSuzulus && ziplamaBasiliTutuluyor) ? 0.32f : inisYerCekimiCarpani;
            rb.gravityScale = tabanYerCekimi * suzulmeCarpani;
        }
        else if (Mathf.Abs(rb.linearVelocity.y) < 1.8f && !yerde)
        {
            rb.gravityScale = tabanYerCekimi * tepeNoktasiYerCekimiCarpani;
        }
        else
        {
            rb.gravityScale = tabanYerCekimi;
        }

        if (duvarda && !yerde && rb.linearVelocity.y < duvarKaymaHizi)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, duvarKaymaHizi);
        }

        if (rb.linearVelocity.y < maksimumDusmeHizi)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maksimumDusmeHizi);
        }
    }

    private void ZiplamaUygula(float guc)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, guc);
    }

    private float AktifHizCarpani()
    {
        float carpani = aktifBoost == GunahBoostTipi.KorOfkesi ? 1.15f : 1f;
        if (aktifBoost == GunahBoostTipi.DoyumsuzAdim && boostKalanSure > 0f) carpani *= 1.25f;
        return carpani;
    }

    private float AktifZiplamaCarpani()
    {
        float carpani = aktifBoost == GunahBoostTipi.GokSicrayisi ? 1.22f : 1f;
        if (aktifBoost == GunahBoostTipi.DoyumsuzAdim && boostKalanSure > 0f) carpani *= 1.12f;
        return carpani;
    }

    private void YuzeyKontrolleri()
    {
        Bounds b = col.bounds;
        Vector2 ayakMerkez = new Vector2(b.center.x, b.min.y - ayakMesafesi * 0.5f);
        Vector2 ayakBoyut = new Vector2(b.size.x * 0.80f, ayakMesafesi);
        yerde = ZeminTemasKontrol(ayakMerkez, ayakBoyut, false, out aktifPlatformHizi);

        Vector2 sagMerkez = new Vector2(b.max.x + yanMesafe * 0.5f, b.center.y);
        Vector2 solMerkez = new Vector2(b.min.x - yanMesafe * 0.5f, b.center.y);
        Vector2 yanBoyut = new Vector2(yanMesafe, b.size.y * 0.65f);

        bool sagDuvar = ZeminTemasKontrol(sagMerkez, yanBoyut, true, out _);
        bool solDuvar = ZeminTemasKontrol(solMerkez, yanBoyut, true, out _);

        duvarda = sagDuvar || solDuvar;
        duvarYonu = sagDuvar ? 1 : (solDuvar ? -1 : 0);
    }

    private bool ZeminTemasKontrol(Vector2 merkez, Vector2 boyut, bool duvarKontrolu, out Vector2 platformHiz)
    {
        platformHiz = Vector2.zero;
        Collider2D[] hits = zeminKatmani.value != 0
            ? Physics2D.OverlapBoxAll(merkez, boyut, 0f, zeminKatmani)
            : Physics2D.OverlapBoxAll(merkez, boyut, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D h = hits[i];
            if (h == null || h.gameObject == gameObject || h.isTrigger) continue;
            if (duvarKontrolu && h.usedByEffector) continue;

            HareketliPlatform hp = h.GetComponent<HareketliPlatform>();
            if (hp != null) platformHiz = hp.MevcutHiz;
            return true;
        }
        return false;
    }

    private void GirdileriOku(out bool ziplamaBasildi, out bool ziplamaBirakildi, out bool dashBasildi)
    {
        yatayGirdi = 0f;
        ziplamaBasildi = false;
        ziplamaBirakildi = false;
        dashBasildi = false;
        ziplamaBasiliTutuluyor = false;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) yatayGirdi -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) yatayGirdi += 1f;

            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                ziplamaBasildi = true;
            if (kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame)
                ziplamaBirakildi = true;
            if (kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed)
                ziplamaBasiliTutuluyor = true;

            if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame || kb.shiftKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame)
                dashBasildi = true;
        }

        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            float lx = gp.leftStick.x.ReadValue();
            if (Mathf.Abs(lx) > 0.2f) yatayGirdi = Mathf.Clamp(yatayGirdi + lx, -1f, 1f);
            if (gp.dpad.left.isPressed) yatayGirdi = -1f;
            if (gp.dpad.right.isPressed) yatayGirdi = 1f;

            if (gp.buttonSouth.wasPressedThisFrame) ziplamaBasildi = true;
            if (gp.buttonSouth.wasReleasedThisFrame) ziplamaBirakildi = true;
            if (gp.buttonSouth.isPressed) ziplamaBasiliTutuluyor = true;

            if (gp.buttonEast.wasPressedThisFrame || gp.rightTrigger.wasPressedThisFrame) dashBasildi = true;
        }

        yatayGirdi = Mathf.Clamp(yatayGirdi, -1f, 1f);
    }

    public bool UsttenEzebilirMi(Collider2D dusmanCollider)
    {
        if (dusmanCollider == null || col == null) return false;
        return rb.linearVelocity.y <= 0.5f && col.bounds.min.y >= dusmanCollider.bounds.center.y - 0.08f;
    }

    public void StompSekmesiUygula()
    {
        kalanHavaDash = 1;
        if (aktifBoost == GunahBoostTipi.SerapSicrayisi) kalanEkstraZiplama = 1;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, ziplamaGucu * 0.85f);
    }
}
