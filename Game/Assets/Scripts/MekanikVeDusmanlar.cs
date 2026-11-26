using System.Collections;
using UnityEngine;

public class TehlikeEngeli : MonoBehaviour
{
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private string olumNedeni = "Dikenli Tuzak";
    [SerializeField] private bool radardaGoster = true;

    private void Awake()
    {
        if (radardaGoster && GetComponent<RadarHedefi>() == null)
        {
            RadarHedefi rh = gameObject.AddComponent<RadarHedefi>();
            rh.tip = RadarHedefi.HedefTipi.Tehlike;
        }
    }

    public void Ayarla(int bNo, string neden, bool radar = true)
    {
        bolumNo = Mathf.Clamp(bNo, 1, 7);
        olumNedeni = neden;
        radardaGoster = radar;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Tetikle(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Tetikle(collision.collider);
    }

    private void Tetikle(Collider2D other)
    {
        if (other == null) return;
        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k != null)
        {
            k.OlumVeYenidenDogus(bolumNo, olumNedeni);
        }
    }
}

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class DevriyeDusman : MonoBehaviour
{
    public enum DusmanArketipi
    {
        EzilebilirMuhafiz,
        MenzilliKahin,
        OndenZirhliBekci
    }

    [Header("Dusman Ayarlari")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private string dusmanAdi = "Ziyafet Muhafizi";
    [SerializeField] private DusmanArketipi arketip = DusmanArketipi.EzilebilirMuhafiz;
    [SerializeField] private float devriyeMesafesi = 3.5f;
    [SerializeField] private float hiz = 2.6f;
    [SerializeField] private bool stompEdilebilir = true;
    [SerializeField] private float atisAraligi = 2.4f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Vector2 baslangicKonumu;
    private int yon = 1;
    private bool yenildi;
    private float atisSayaci;
    private SevenVeilsKontrolcu oyuncu;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        baslangicKonumu = transform.position;
        atisSayaci = 1.2f;

        if (GetComponent<RadarHedefi>() == null)
        {
            RadarHedefi rh = gameObject.AddComponent<RadarHedefi>();
            rh.tip = RadarHedefi.HedefTipi.Dusman;
        }
    }

    private void Start()
    {
        oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
    }

    public void Ayarla(int bNo, string ad, float mesafe, float hareketHizi, bool ezilebilir, DusmanArketipi tip = DusmanArketipi.EzilebilirMuhafiz)
    {
        bolumNo = Mathf.Clamp(bNo, 1, 7);
        dusmanAdi = ad;
        devriyeMesafesi = mesafe;
        hiz = hareketHizi;
        stompEdilebilir = ezilebilir;
        arketip = tip;
    }

    private void FixedUpdate()
    {
        if (yenildi) return;

        if (devriyeMesafesi > 0.05f && hiz > 0.05f)
        {
            Vector2 yeniKonum = rb.position + Vector2.right * (yon * hiz * Time.fixedDeltaTime);
            rb.MovePosition(yeniKonum);

            float fark = yeniKonum.x - baslangicKonumu.x;
            if (fark >= devriyeMesafesi && yon > 0)
            {
                yon = -1;
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
            }
            else if (fark <= -devriyeMesafesi && yon < 0)
            {
                yon = 1;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
            }
        }
    }

    private void Update()
    {
        if (yenildi || arketip != DusmanArketipi.MenzilliKahin) return;

        if (oyuncu == null)
        {
            oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
            return;
        }

        float mesafe = Vector2.Distance(transform.position, oyuncu.transform.position);
        if (mesafe <= 16f)
        {
            int hedefYon = oyuncu.transform.position.x >= transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(hedefYon * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);

            atisSayaci -= Time.deltaTime;
            if (atisSayaci <= 0f)
            {
                atisSayaci = atisAraligi;
                MermiAtesle(hedefYon);
            }
        }
    }

    private void MermiAtesle(int atisYonu)
    {
        GameObject mermi = new GameObject("Kahin_EnerjiKuresi");
        mermi.transform.position = transform.position + new Vector3(atisYonu * 0.7f, 0.1f, 0f);
        mermi.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

        SpriteRenderer sr = mermi.AddComponent<SpriteRenderer>();
        sr.sprite = SevenVeilsGorselFabrikasi.ParcaSpriteOlustur(bolumNo);
        sr.color = new Color(1f, 0.35f, 0.15f, 1f);
        sr.sortingOrder = 8;

        CircleCollider2D c = mermi.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.42f;

        DusmanMermisi dm = mermi.AddComponent<DusmanMermisi>();
        dm.Ayarla(bolumNo, $"{dusmanAdi} Atesi", atisYonu * 7.8f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yenildi || other == null) return;

        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k == null) return;

        if (stompEdilebilir && k.UsttenEzebilirMi(col))
        {
            yenildi = true;
            k.StompSekmesiUygula();
            if (SevenVeilsTelemetriVeStats.Instance != null)
                SevenVeilsTelemetriVeStats.Instance.MekanikKullanildi("stomp");
            gameObject.SetActive(false);
        }
        else
        {
            k.OlumVeYenidenDogus(bolumNo, dusmanAdi);
        }
    }
}

public class DusmanMermisi : MonoBehaviour
{
    private int bolumNo = 1;
    private string neden = "Kahin Atesi";
    private float yatayHiz = 7.5f;
    private float omur = 3.2f;

    public void Ayarla(int bNo, string olumNedeni, float hiz)
    {
        bolumNo = bNo;
        neden = olumNedeni;
        yatayHiz = hiz;
    }

    private void Update()
    {
        transform.position += Vector3.right * (yatayHiz * Time.deltaTime);
        omur -= Time.deltaTime;
        if (omur <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k != null)
        {
            if (!k.DashAtiyorMu)
            {
                k.OlumVeYenidenDogus(bolumNo, neden);
            }
            Destroy(gameObject);
        }
        else if (!other.isTrigger && other.GetComponent<DevriyeDusman>() == null)
        {
            Destroy(gameObject);
        }
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class ZiplamaYayi : MonoBehaviour
{
    [SerializeField] private float firlatmaGucu = 21.5f;

    private SpriteRenderer sr;
    private Vector3 orijinalScale;

    private void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
        orijinalScale = transform.localScale;
    }

    public void GucAyarla(float guc)
    {
        firlatmaGucu = guc;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k != null)
        {
            k.HariciZiplamaUygula(firlatmaGucu);
            StopAllCoroutines();
            StartCoroutine(YayAnimasyonu());
        }
    }

    private IEnumerator YayAnimasyonu()
    {
        transform.localScale = new Vector3(orijinalScale.x * 1.25f, orijinalScale.y * 0.65f, 1f);
        if (sr != null) sr.color = new Color(1f, 0.90f, 0.35f, 1f);
        yield return new WaitForSeconds(0.14f);
        transform.localScale = orijinalScale;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class CokenTasPlatform : MonoBehaviour
{
    [SerializeField] private float cokmeGecikmesi = 0.65f;
    [SerializeField] private float yenilenmeSuresi = 3.0f;

    private BoxCollider2D col;
    private SpriteRenderer sr;
    private Vector3 orijinalKonum;
    private bool tetiklendi;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;
        sr = GetComponent<SpriteRenderer>();
        orijinalKonum = transform.position;
    }

    public void Ayarla(float gecikme, float yenilenme)
    {
        cokmeGecikmesi = gecikme;
        yenilenmeSuresi = yenilenme;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tetiklendi) return;

        if (collision.collider.GetComponent<SevenVeilsKontrolcu>() != null)
        {
            foreach (ContactPoint2D nokta in collision.contacts)
            {
                if (nokta.normal.y < -0.45f)
                {
                    StartCoroutine(CokmeRutini());
                    break;
                }
            }
        }
    }

    private IEnumerator CokmeRutini()
    {
        tetiklendi = true;
        float t = 0f;
        while (t < cokmeGecikmesi)
        {
            t += Time.deltaTime;
            float titresim = Mathf.Sin(t * 55f) * 0.06f;
            transform.position = orijinalKonum + new Vector3(titresim, 0f, 0f);
            yield return null;
        }

        transform.position = orijinalKonum;
        col.enabled = false;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.15f;
            sr.color = c;
        }

        yield return new WaitForSeconds(yenilenmeSuresi);

        col.enabled = true;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        tetiklendi = false;
    }
}

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class HareketliPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 hareketYonu = Vector2.right;
    [SerializeField] private float mesafe = 4f;
    [SerializeField] private float hiz = 2.2f;
    [SerializeField] private float fazOfseti = 0f;

    private Rigidbody2D rb;
    private Vector2 baslangicKonumu;
    private Vector2 oncekiKonum;

    public Vector2 MevcutHiz { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;

        baslangicKonumu = transform.position;
        oncekiKonum = baslangicKonumu;
    }

    public void Ayarla(Vector2 yon, float uzaklik, float platformHizi, float faz = 0f)
    {
        hareketYonu = yon.normalized;
        mesafe = uzaklik;
        hiz = platformHizi;
        fazOfseti = faz;
    }

    private void FixedUpdate()
    {
        float t = (Mathf.Sin(Time.time * hiz + fazOfseti) + 1f) * 0.5f;
        Vector2 hedef = baslangicKonumu + hareketYonu * (mesafe * t);
        MevcutHiz = (hedef - oncekiKonum) / Time.fixedDeltaTime;
        rb.MovePosition(hedef);
        oncekiKonum = hedef;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class SahteSerapPlatform : MonoBehaviour
{
    [SerializeField] private float tetikMesafesi = 3.2f;

    private SpriteRenderer sr;
    private SevenVeilsKontrolcu oyuncu;
    private Color orijinalRenk;

    private void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) orijinalRenk = sr.color;
    }

    private void Start()
    {
        oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
    }

    private void Update()
    {
        if (sr == null || oyuncu == null) return;
        float d = Vector2.Distance(transform.position, oyuncu.transform.position);
        float hedefAlfa = d <= tetikMesafesi ? 0.14f : orijinalRenk.a;
        Color c = sr.color;
        c.a = Mathf.MoveTowards(c.a, hedefAlfa, Time.deltaTime * 4f);
        sr.color = c;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class AlttanOdulBlok : MonoBehaviour
{
    [SerializeField] private GameObject acilacakHedef;
    [SerializeField] private bool gizliGecitAciciMi = false;

    private SpriteRenderer sr;
    private Vector3 orijinalKonum;
    private bool vuruldu;

    private void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;
        sr = GetComponent<SpriteRenderer>();
        orijinalKonum = transform.position;
    }

    public void HedefBagla(GameObject hedef, bool gecitAcici)
    {
        acilacakHedef = hedef;
        gizliGecitAciciMi = gecitAcici;
        if (acilacakHedef != null && !gizliGecitAciciMi)
            acilacakHedef.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (vuruldu) return;
        if (collision.collider.GetComponent<SevenVeilsKontrolcu>() == null) return;

        foreach (ContactPoint2D nokta in collision.contacts)
        {
            if (nokta.normal.y > 0.45f)
            {
                vuruldu = true;
                StartCoroutine(VurusAnimasyonu());
                break;
            }
        }
    }

    private IEnumerator VurusAnimasyonu()
    {
        transform.position = orijinalKonum + Vector3.up * 0.25f;
        if (sr != null) sr.color = new Color(0.55f, 0.50f, 0.45f, 1f);

        if (acilacakHedef != null)
        {
            acilacakHedef.SetActive(!gizliGecitAciciMi);
        }

        yield return new WaitForSeconds(0.12f);
        transform.position = orijinalKonum;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class BasincPlakasiVeKapi : MonoBehaviour
{
    [SerializeField] private Transform hedefKapi;
    [SerializeField] private Vector3 acikOfset = new Vector3(0f, 3.2f, 0f);
    [SerializeField] private bool kaliciAcilsin = true;

    private Vector3 kapaliKonum;
    private SpriteRenderer sr;
    private bool acik;

    private void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
    }

    public void KapiBagla(Transform kapi, Vector3 ofset, bool kalici = true)
    {
        hedefKapi = kapi;
        acikOfset = ofset;
        kaliciAcilsin = kalici;
        if (hedefKapi != null) kapaliKonum = hedefKapi.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SevenVeilsKontrolcu>() != null)
        {
            acik = true;
            if (sr != null) sr.color = new Color(0.30f, 0.90f, 0.55f, 1f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (kaliciAcilsin) return;
        if (other.GetComponent<SevenVeilsKontrolcu>() != null)
        {
            acik = false;
            if (sr != null) sr.color = new Color(0.85f, 0.45f, 0.25f, 1f);
        }
    }

    private void Update()
    {
        if (hedefKapi == null) return;
        Vector3 hedef = acik ? (kapaliKonum + acikOfset) : kapaliKonum;
        hedefKapi.position = Vector3.MoveTowards(hedefKapi.position, hedef, Time.deltaTime * 6f);
    }
}
