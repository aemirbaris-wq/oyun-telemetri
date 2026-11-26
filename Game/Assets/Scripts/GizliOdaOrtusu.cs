using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GizliOdaOrtusu : MonoBehaviour
{
    [Header("Gizli Oda Ayarlari")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private float acilmaHizi = 2.8f;
    [SerializeField] private Color perdeRengi = new Color(0.03f, 0.02f, 0.04f, 1f);
    [SerializeField] private int siralamaKatmani = 50;

    private BoxCollider2D odaAlani;
    private SpriteRenderer perdeGoruntusu;
    private bool kesfedildi;
    private float mevcutAlfa = 1f;

    public bool KesfedildiMi => kesfedildi;

    private void Awake()
    {
        odaAlani = GetComponent<BoxCollider2D>();
        odaAlani.isTrigger = true;
        PerdeGorseliniHazirla();
    }

    public void BolumNumarasiAyarla(int no)
    {
        bolumNo = Mathf.Clamp(no, 1, 7);
    }

    private void PerdeGorseliniHazirla()
    {
        Transform cocuk = transform.Find("KaranlikPerdeGorseli");
        GameObject gorselObj;

        if (cocuk != null)
        {
            gorselObj = cocuk.gameObject;
        }
        else
        {
            gorselObj = new GameObject("KaranlikPerdeGorseli");
            gorselObj.transform.SetParent(transform, false);
        }

        gorselObj.transform.localPosition = new Vector3(odaAlani.offset.x, odaAlani.offset.y, -0.5f);
        gorselObj.transform.localScale = new Vector3(odaAlani.size.x, odaAlani.size.y, 1f);

        perdeGoruntusu = gorselObj.GetComponent<SpriteRenderer>();
        if (perdeGoruntusu == null)
            perdeGoruntusu = gorselObj.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        perdeGoruntusu.sprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        perdeGoruntusu.color = perdeRengi;
        perdeGoruntusu.sortingOrder = siralamaKatmani;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (kesfedildi) return;

        if (other.GetComponent<SevenVeilsKontrolcu>() != null || other.CompareTag("Player"))
        {
            kesfedildi = true;
            if (SevenVeilsTelemetriVeStats.Instance != null)
            {
                SevenVeilsTelemetriVeStats.Instance.GizliOdaKesfedildi(bolumNo, transform.position);
            }
        }
    }

    private void Update()
    {
        if (!kesfedildi || perdeGoruntusu == null) return;

        if (mevcutAlfa > 0f)
        {
            mevcutAlfa = Mathf.MoveTowards(mevcutAlfa, 0f, Time.deltaTime * acilmaHizi);
            Color c = perdeRengi;
            c.a = mevcutAlfa;
            perdeGoruntusu.color = c;

            if (mevcutAlfa <= 0.001f)
            {
                perdeGoruntusu.enabled = false;
            }
        }
    }

    public float OyuncuyaEnYakinMesafe(Vector2 oyuncuKonum)
    {
        if (odaAlani == null) return float.MaxValue;
        Vector2 enYakinNokta = odaAlani.ClosestPoint(oyuncuKonum);
        return Vector2.Distance(oyuncuKonum, enYakinNokta);
    }
}
