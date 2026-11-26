using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SevenVeilsKontrolcu : MonoBehaviour
{
    public enum GunahBoostTipi
    {
        Yok,
        DoyumsuzAdim,     // 1. Oburluk: Toplanabilir alinca 2.5 sn hiz ve ziplama artisi
        AgirSuzulus,      // 2. Tembellik: Havada ziplama basili tutulursa yavas dusus
        SerapSicrayisi,   // 3. Sehvet: Havada 1 ekstra cift ziplama
        AltinMiknatisi,   // 4. Acgozluluk: Yakindaki toplanabilirleri cekme + dash uzatma
        GokSicrayisi,     // 5. Kibir: Daha yuksek dikey ziplama ve guclu duvar sekmesi
        GolgeYankisi,     // 6. Kiskanclik: Dash bekleme suresi yariya iner
        KorOfkesi         // 7. Ofke (Cehennem): Yuksek temel hiz + guclu dash
    }

    [Header("Temel Hareket")]
    [SerializeField] private float hareketHizi = 8.2f;
    [SerializeField] private float ziplamaGucu = 14.5f;
    [SerializeField] private float kisaZiplamaCarpani = 0.5f;
    [SerializeField] private float maksimumDusmeHizi = -22f;

    [Header("Mario & Celeste Hissi")]
    [SerializeField] private float coyoteSuresi = 0.12f;
    [SerializeField] private float jumpBufferSuresi = 0.14f;
    [SerializeField] private float tepeNoktasiYerCekimiCarpani = 0.65f;
    [SerializeField] private float inisYerCekimiCarpani = 1.35f;

    [Header("Shadow Dash & Duvar")]
    [SerializeField] private float dashHizi = 20f;
    [SerializeField] private float dashSuresi = 0.16f;
    [SerializeField] private float dashBeklemeSuresi = 0.45f;
    [SerializeField] private float duvarKaymaHizi = -2.4f;
    [SerializeField] private Vector2 duvarZiplamaGucu = new Vector2(11.5f, 14.2f);

    [Header("Bolume Ozel Gunah Boost")]
    [SerializeField] private GunahBoostTipi aktifBoost = GunahBoostTipi.Yok;
    [SerializeField] private float boostSuresi = 2.5f;

    [Header("Zemin ve Duvar Kontrolu")]
    [SerializeField] private LayerMask zeminKatmani;
    [SerializeField] private float ayakMesafesi = 0.12f;
    [SerializeField] private float yanMesafe = 0.10f;

    private Rigidbody2D rb;
    private Collider2D col;
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

    public bool BoostAktifMi => boostKalanSure > 0f;
    public GunahBoostTipi AktifBoost => aktifBoost;

    public event Action<Vector3, int> OnDashAtildi;
    public event Action<int> OnOlumGerceklesti;
    public event Action OnYenidenDogdu;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        dogusNoktasi = transform.position;
        GuvenliFizikVeDurumSifirla();
    }

    private void OnEnable()
    {
        GuvenliFizikVeDurumSifirla();
    }

    public void GuvenliFizikVeDurumSifirla()
    {
        if (Time.timeScale <= 0.001f) Time.timeScale = 1f;

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3.2f;
            rb.linearVelocity = Vector2.zero;
        }

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
            col.sharedMaterial = new PhysicsMaterial2D("SevenVeils_Surtunmesiz")
            {
                friction = 0f,
                bounciness = 0f
            };
        }

        kilitli = false;
        dashSayaci = 0f;
        dashBeklemeSayaci = 0f;
        coyoteSayaci = 0f;
        jumpBufferSayaci = 0f;
        boostKalanSure = 0f;
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
        }
        else if (aktifBoost == GunahBoostTipi.AltinMiknatisi)
        {
            boostKalanSure = boostSuresi;
            kalanHavaDash = 1;
        }
    }

    public void CheckpointGuncelle(Vector3 yeniNokta)
    {
        dogusNoktasi = yeniNokta;
    }

    public void OlumVeYenidenDogus(int bolumNo = 1)
    {
        if (kilitli) return;
        kilitli = true;
        rb.linearVelocity = Vector2.zero;
        OnOlumGerceklesti?.Invoke(bolumNo);
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
        if (kilitli) return;

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
                OnDashAtildi?.Invoke(transform.position, bakilanYon);
            }
        }

        if (jumpBufferSayaci > 0f)
        {
            if (coyoteSayaci > 0f)
            {
                ZiplamaUygula(ziplamaGucu * AktifZiplamaCarpani());
                jumpBufferSayaci = 0f;
                coyoteSayaci = 0f;
            }
            else if (duvarda && !yerde)
            {
                float duvarCarpani = aktifBoost == GunahBoostTipi.GokSicrayisi ? 1.18f : 1f;
                rb.linearVelocity = new Vector2(-duvarYonu * duvarZiplamaGucu.x, duvarZiplamaGucu.y * duvarCarpani);
                bakilanYon = -duvarYonu;
                transform.localScale = new Vector3(bakilanYon, 1f, 1f);
                jumpBufferSayaci = 0f;
            }
            else if (kalanEkstraZiplama > 0)
            {
                kalanEkstraZiplama--;
                ZiplamaUygula(ziplamaGucu * 0.92f);
                jumpBufferSayaci = 0f;
            }
        }

        if (ziplamaBirakildi && rb.linearVelocity.y > 0f && dashSayaci <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * kisaZiplamaCarpani);
        }
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
        rb.linearVelocity = new Vector2(yatayGirdi * hiz, rb.linearVelocity.y);

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
        Vector2 ayakBoyut = new Vector2(b.size.x * 0.8f, ayakMesafesi);
        Collider2D ayakHit = zeminKatmani.value != 0
            ? Physics2D.OverlapBox(ayakMerkez, ayakBoyut, 0f, zeminKatmani)
            : Physics2D.OverlapBox(ayakMerkez, ayakBoyut, 0f);
        yerde = ayakHit != null && ayakHit.gameObject != gameObject && !ayakHit.isTrigger;

        Vector2 sagMerkez = new Vector2(b.max.x + yanMesafe * 0.5f, b.center.y);
        Vector2 solMerkez = new Vector2(b.min.x - yanMesafe * 0.5f, b.center.y);
        Vector2 yanBoyut = new Vector2(yanMesafe, b.size.y * 0.65f);

        Collider2D sagHit = zeminKatmani.value != 0
            ? Physics2D.OverlapBox(sagMerkez, yanBoyut, 0f, zeminKatmani)
            : Physics2D.OverlapBox(sagMerkez, yanBoyut, 0f);
        Collider2D solHit = zeminKatmani.value != 0
            ? Physics2D.OverlapBox(solMerkez, yanBoyut, 0f, zeminKatmani)
            : Physics2D.OverlapBox(solMerkez, yanBoyut, 0f);

        bool sagDuvar = sagHit != null && sagHit.gameObject != gameObject && !sagHit.isTrigger;
        bool solDuvar = solHit != null && solHit.gameObject != gameObject && !solHit.isTrigger;

        duvarda = sagDuvar || solDuvar;
        duvarYonu = sagDuvar ? 1 : (solDuvar ? -1 : 0);
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

            if (kb.shiftKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame)
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

    public bool DusebilirMi(Collider2D dusmanCollider)
    {
        if (dusmanCollider == null || col == null) return false;
        return rb.linearVelocity.y <= 0.5f && col.bounds.min.y >= dusmanCollider.bounds.center.y - 0.05f;
    }

    public void StompSekmesiUygula()
    {
        kalanHavaDash = 1;
        if (aktifBoost == GunahBoostTipi.SerapSicrayisi) kalanEkstraZiplama = 1;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, ziplamaGucu * 0.85f);
    }
}
