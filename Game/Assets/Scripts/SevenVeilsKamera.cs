using UnityEngine;

public class SevenVeilsKamera : MonoBehaviour
{
    [Header("Takip Ayarlari")]
    [SerializeField] private Transform hedef;
    [SerializeField] private Vector3 temelOfset = new Vector3(0f, 1.4f, -10f);
    [SerializeField] private float yumusaklikSuresi = 0.14f;
    [SerializeField] private float lookAheadMesafesi = 2.2f;
    [SerializeField] private float lookAheadHizi = 4.5f;

    [Header("Bolum Sinirlari (Opsiyonel)")]
    [SerializeField] private bool sinirlariKullan = false;
    [SerializeField] private Vector2 minSinir = new Vector2(-10f, -15f);
    [SerializeField] private Vector2 maxSinir = new Vector2(200f, 40f);

    private SevenVeilsKontrolcu kontrolcu;
    private Vector3 mevcutHiz;
    private float mevcutLookAhead;
    private float sarsintiSuresi;
    private float sarsintiSiddeti;

    private void Start()
    {
        HedefiBagla();
    }

    public void HedefiAyarla(Transform yeniHedef)
    {
        hedef = yeniHedef;
        HedefiBagla();
    }

    public void SinirlariAyarla(Vector2 min, Vector2 max)
    {
        sinirlariKullan = true;
        minSinir = min;
        maxSinir = max;
    }

    private void HedefiBagla()
    {
        if (hedef == null)
        {
            SevenVeilsKontrolcu bulunan = FindFirstObjectByType<SevenVeilsKontrolcu>();
            if (bulunan != null) hedef = bulunan.transform;
        }

        if (hedef != null)
        {
            if (kontrolcu != null)
            {
                kontrolcu.OnDashAtildi -= DashSarsintisi;
                kontrolcu.OnOlumGerceklesti -= OlumSarsintisi;
            }

            kontrolcu = hedef.GetComponent<SevenVeilsKontrolcu>();
            if (kontrolcu != null)
            {
                kontrolcu.OnDashAtildi += DashSarsintisi;
                kontrolcu.OnOlumGerceklesti += OlumSarsintisi;
            }
        }
    }

    private void OnDestroy()
    {
        if (kontrolcu != null)
        {
            kontrolcu.OnDashAtildi -= DashSarsintisi;
            kontrolcu.OnOlumGerceklesti -= OlumSarsintisi;
        }
    }

    private void DashSarsintisi(Vector3 konum, int yon)
    {
        SarsintiUygula(0.14f, 0.12f);
    }

    private void OlumSarsintisi(int bolumNo, Vector3 konum, string neden)
    {
        SarsintiUygula(0.28f, 0.22f);
    }

    public void SarsintiUygula(float siddet, float sure)
    {
        sarsintiSiddeti = Mathf.Max(sarsintiSiddeti, siddet);
        sarsintiSuresi = Mathf.Max(sarsintiSuresi, sure);
    }

    private void LateUpdate()
    {
        if (hedef == null)
        {
            HedefiBagla();
            if (hedef == null) return;
        }

        int yon = kontrolcu != null ? kontrolcu.BakilanYon : (hedef.localScale.x >= 0f ? 1 : -1);
        float hedefLookAhead = yon * lookAheadMesafesi;
        mevcutLookAhead = Mathf.Lerp(mevcutLookAhead, hedefLookAhead, Time.deltaTime * lookAheadHizi);

        Vector3 hedefKonum = new Vector3(
            hedef.position.x + temelOfset.x + mevcutLookAhead,
            hedef.position.y + temelOfset.y,
            temelOfset.z
        );

        Vector3 yumusakKonum = Vector3.SmoothDamp(transform.position, hedefKonum, ref mevcutHiz, yumusaklikSuresi);

        if (sinirlariKullan)
        {
            yumusakKonum.x = Mathf.Clamp(yumusakKonum.x, minSinir.x, maxSinir.x);
            yumusakKonum.y = Mathf.Clamp(yumusakKonum.y, minSinir.y, maxSinir.y);
        }

        if (sarsintiSuresi > 0f)
        {
            sarsintiSuresi -= Time.deltaTime;
            Vector2 titre = Random.insideUnitCircle * sarsintiSiddeti;
            yumusakKonum.x += titre.x;
            yumusakKonum.y += titre.y;
            if (sarsintiSuresi <= 0f) sarsintiSiddeti = 0f;
        }

        transform.position = yumusakKonum;
    }
}
