using System.Collections.Generic;
using UnityEngine;

public class RadarHedefi : MonoBehaviour
{
    public enum HedefTipi
    {
        Dusman,
        Toplanabilir,
        Checkpoint,
        BitisKapisi,
        Tehlike
    }

    public HedefTipi tip = HedefTipi.Toplanabilir;
}

public class PerdeRadari : MonoBehaviour
{
    [Header("Radar Ayarlari")]
    [SerializeField] private float yatayMenzil = 28f;
    [SerializeField] private float dikeyMenzil = 16f;
    [SerializeField] private float gizliOdaParazitMesafesi = 7.0f;

    private Transform oyuncu;
    private Texture2D duzDoku;
    private GizliOdaOrtusu[] gizliOdalar;
    private AraKontrolNoktasi checkpoint;
    private BolumBitisKapisi bitisKapisi;
    private float taramaSayaci;
    private readonly List<BoxCollider2D> yakindakiZeminler = new List<BoxCollider2D>();
    private readonly List<RadarHedefi> yakindakiHedefler = new List<RadarHedefi>();

    private void Awake()
    {
        duzDoku = new Texture2D(1, 1);
        duzDoku.SetPixel(0, 0, Color.white);
        duzDoku.Apply();
    }

    private void Start()
    {
        OyuncuyuBul();
        gizliOdalar = FindObjectsByType<GizliOdaOrtusu>(FindObjectsSortMode.None);
        checkpoint = FindFirstObjectByType<AraKontrolNoktasi>();
        bitisKapisi = FindFirstObjectByType<BolumBitisKapisi>();
    }

    private void OyuncuyuBul()
    {
        SevenVeilsKontrolcu k = FindFirstObjectByType<SevenVeilsKontrolcu>();
        if (k != null) oyuncu = k.transform;
    }

    private void Update()
    {
        if (oyuncu == null)
        {
            OyuncuyuBul();
            if (oyuncu == null) return;
        }

        taramaSayaci -= Time.deltaTime;
        if (taramaSayaci <= 0f)
        {
            taramaSayaci = 0.10f;
            CevreyiTara();
        }
    }

    private void CevreyiTara()
    {
        yakindakiZeminler.Clear();
        yakindakiHedefler.Clear();

        Vector2 merkez = oyuncu.position;
        Vector2 boyut = new Vector2(yatayMenzil * 2f, dikeyMenzil * 2f);
        Collider2D[] bulunanlar = Physics2D.OverlapBoxAll(merkez, boyut, 0f);

        foreach (Collider2D c in bulunanlar)
        {
            if (c == null || c.gameObject == oyuncu.gameObject) continue;
            if (GizliOdaIcindeVeKapaliMi(c.bounds.center)) continue;

            RadarHedefi hedef = c.GetComponent<RadarHedefi>();
            if (hedef != null)
            {
                yakindakiHedefler.Add(hedef);
            }
            else if (!c.isTrigger && c is BoxCollider2D box)
            {
                yakindakiZeminler.Add(box);
            }
        }
    }

    private bool GizliOdaIcindeVeKapaliMi(Vector2 nokta)
    {
        if (gizliOdalar == null) return false;
        foreach (GizliOdaOrtusu oda in gizliOdalar)
        {
            if (oda == null || oda.KesfedildiMi) continue;
            BoxCollider2D alan = oda.GetComponent<BoxCollider2D>();
            if (alan != null && alan.bounds.Contains(nokta))
                return true;
        }
        return false;
    }

    private float EnYakinKapaliGizliOdaMesafesi()
    {
        if (gizliOdalar == null || oyuncu == null) return float.MaxValue;
        float minMesafe = float.MaxValue;
        foreach (GizliOdaOrtusu oda in gizliOdalar)
        {
            if (oda == null || oda.KesfedildiMi) continue;
            float d = oda.OyuncuyaEnYakinMesafe(oyuncu.position);
            if (d < minMesafe) minMesafe = d;
        }
        return minMesafe;
    }

    private void OnGUI()
    {
        if (oyuncu == null) return;

        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));

        Rect radarRect = new Rect(1596f, 22f, 300f, 174f);
        KutuCiz(radarRect, new Color(0.05f, 0.04f, 0.08f, 0.88f));

        // Usturlap izgara cizgileri
        Color izgaraRengi = new Color(0.85f, 0.68f, 0.35f, 0.12f);
        KutuCiz(new Rect(radarRect.x, radarRect.center.y, radarRect.width, 1f), izgaraRengi);
        KutuCiz(new Rect(radarRect.center.x, radarRect.y, 1f, radarRect.height), izgaraRengi);

        float gizliMesafe = EnYakinKapaliGizliOdaMesafesi();
        bool perdeTitresimi = gizliMesafe <= gizliOdaParazitMesafesi;
        Color cerceveRengi = perdeTitresimi
            ? Color.Lerp(new Color(0.62f, 0.38f, 0.95f, 0.95f), new Color(1f, 0.90f, 1f, 1f), Mathf.PingPong(Time.time * 7f, 1f))
            : new Color(0.82f, 0.65f, 0.34f, 0.78f);
        CerceveCiz(radarRect, cerceveRengi, perdeTitresimi ? 3f : 2f);

        // Kose susleri
        KutuCiz(new Rect(radarRect.x - 2f, radarRect.y - 2f, 10f, 10f), cerceveRengi);
        KutuCiz(new Rect(radarRect.xMax - 8f, radarRect.y - 2f, 10f, 10f), cerceveRengi);
        KutuCiz(new Rect(radarRect.x - 2f, radarRect.yMax - 8f, 10f, 10f), cerceveRengi);
        KutuCiz(new Rect(radarRect.xMax - 8f, radarRect.yMax - 8f, 10f, 10f), cerceveRengi);

        Vector2 oyuncuPos = oyuncu.position;

        // Platformlar
        foreach (BoxCollider2D box in yakindakiZeminler)
        {
            if (box == null) continue;
            Bounds b = box.bounds;
            Vector2 solUst = DunyadanRadara(new Vector2(b.min.x, b.max.y), oyuncuPos, radarRect);
            Vector2 sagAlt = DunyadanRadara(new Vector2(b.max.x, b.min.y), oyuncuPos, radarRect);

            float w = Mathf.Max(2f, sagAlt.x - solUst.x);
            float h = Mathf.Max(2f, sagAlt.y - solUst.y);
            Rect r = RectKesistir(new Rect(solUst.x, solUst.y, w, h), radarRect);
            if (r.width > 0f && r.height > 0f)
                KutuCiz(r, new Color(0.55f, 0.48f, 0.42f, 0.72f));
        }

        // Yakindaki hedefler
        foreach (RadarHedefi hedef in yakindakiHedefler)
        {
            if (hedef == null || !hedef.gameObject.activeInHierarchy) continue;
            Vector2 p = DunyadanRadara(hedef.transform.position, oyuncuPos, radarRect);
            if (!radarRect.Contains(p)) continue;

            Color noktaRengi = Color.white;
            float boyut = 6f;

            switch (hedef.tip)
            {
                case RadarHedefi.HedefTipi.Dusman:
                    noktaRengi = new Color(0.98f, 0.22f, 0.22f, 1f);
                    boyut = 7f;
                    break;
                case RadarHedefi.HedefTipi.Toplanabilir:
                    noktaRengi = new Color(1f, 0.85f, 0.25f, 1f);
                    boyut = 5f;
                    break;
                case RadarHedefi.HedefTipi.Checkpoint:
                    noktaRengi = new Color(0.28f, 0.90f, 1f, 1f);
                    boyut = 9f;
                    break;
                case RadarHedefi.HedefTipi.BitisKapisi:
                    noktaRengi = new Color(0.35f, 1f, 0.55f, 1f);
                    boyut = 10f;
                    break;
                case RadarHedefi.HedefTipi.Tehlike:
                    noktaRengi = new Color(1f, 0.48f, 0.16f, 0.95f);
                    boyut = 5f;
                    break;
            }

            KutuCiz(new Rect(p.x - boyut * 0.5f, p.y - boyut * 0.5f, boyut, boyut), noktaRengi);
        }

        // Menzil disindaki Checkpoint ve Bitis Kapisi icin radar kenari yon isaretcisi
        if (checkpoint != null)
            KenarIsaretcisiCiz(checkpoint.transform.position, oyuncuPos, radarRect, new Color(0.28f, 0.90f, 1f, 0.95f));
        if (bitisKapisi != null)
            KenarIsaretcisiCiz(bitisKapisi.transform.position, oyuncuPos, radarRect, new Color(0.35f, 1f, 0.55f, 0.95f));

        // Merkezde oyuncu simgesi
        Vector2 merkez = radarRect.center;
        KutuCiz(new Rect(merkez.x - 4f, merkez.y - 4f, 8f, 8f), Color.white);

        GUIStyle radarEtiket = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };
        radarEtiket.normal.textColor = perdeTitresimi ? new Color(0.88f, 0.74f, 1f) : new Color(0.88f, 0.78f, 0.56f);
        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";
        string etiket = perdeTitresimi
            ? (ingilizce ? "VEIL ASTROLABE [SHIMMER]" : "PERDE USTURLABI [TITRESIM]")
            : (ingilizce ? "VEIL ASTROLABE" : "PERDE USTURLABI");
        GUI.Label(new Rect(radarRect.x + 10f, radarRect.y + 5f, 250f, 18f), etiket, radarEtiket);

        GUI.matrix = eskiMatris;
    }

    private void KenarIsaretcisiCiz(Vector2 hedefDunya, Vector2 oyuncuPos, Rect radarRect, Color renk)
    {
        Vector2 p = DunyadanRadara(hedefDunya, oyuncuPos, radarRect);
        if (radarRect.Contains(p)) return;

        float cx = Mathf.Clamp(p.x, radarRect.xMin + 4f, radarRect.xMax - 4f);
        float cy = Mathf.Clamp(p.y, radarRect.yMin + 4f, radarRect.yMax - 4f);
        KutuCiz(new Rect(cx - 3.5f, cy - 3.5f, 7f, 7f), renk);
    }

    private Vector2 DunyadanRadara(Vector2 dunyaNokta, Vector2 oyuncuPos, Rect radarRect)
    {
        float normX = (dunyaNokta.x - oyuncuPos.x) / yatayMenzil;
        float normY = (dunyaNokta.y - oyuncuPos.y) / dikeyMenzil;

        float rx = radarRect.center.x + normX * (radarRect.width * 0.5f);
        float ry = radarRect.center.y - normY * (radarRect.height * 0.5f);
        return new Vector2(rx, ry);
    }

    private Rect RectKesistir(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);
        return new Rect(xMin, yMin, Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
    }

    private void KutuCiz(Rect rect, Color renk)
    {
        Color eski = GUI.color;
        GUI.color = renk;
        GUI.DrawTexture(rect, duzDoku);
        GUI.color = eski;
    }

    private void CerceveCiz(Rect rect, Color renk, float kalinlik)
    {
        KutuCiz(new Rect(rect.x, rect.y, rect.width, kalinlik), renk);
        KutuCiz(new Rect(rect.x, rect.yMax - kalinlik, rect.width, kalinlik), renk);
        KutuCiz(new Rect(rect.x, rect.y, kalinlik, rect.height), renk);
        KutuCiz(new Rect(rect.xMax - kalinlik, rect.y, kalinlik, rect.height), renk);
    }
}
