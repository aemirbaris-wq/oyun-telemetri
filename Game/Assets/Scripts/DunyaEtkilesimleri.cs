using System;
using UnityEngine;

public class ToplanabilirParca : MonoBehaviour
{
    [Header("Parca Ayarlari")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private float salinimHizi = 3.2f;
    [SerializeField] private float salinimYuksekligi = 0.14f;
    [SerializeField] private float miknatisMenzili = 5.5f;
    [SerializeField] private float miknatisHizi = 11f;
    [SerializeField] private AudioClip toplamaSesi;

    private Vector3 baslangicKonumu;
    private SevenVeilsKontrolcu oyuncu;
    private bool toplandi;

    public static event Action<int> OnParcaToplandi;

    private void Awake()
    {
        baslangicKonumu = transform.position;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (GetComponent<RadarHedefi>() == null)
        {
            RadarHedefi rh = gameObject.AddComponent<RadarHedefi>();
            rh.tip = RadarHedefi.HedefTipi.Toplanabilir;
        }
    }

    private void Start()
    {
        oyuncu = FindFirstObjectByType<SevenVeilsKontrolcu>();
        if (toplamaSesi == null)
            toplamaSesi = SevenVeilsGorselFabrikasi.ToplamaSesiUret(bolumNo);
    }

    public void BolumNumarasiAyarla(int no)
    {
        bolumNo = Mathf.Clamp(no, 1, 7);
    }

    private void Update()
    {
        if (toplandi) return;

        float yeniY = baslangicKonumu.y + Mathf.Sin(Time.time * salinimHizi + baslangicKonumu.x) * salinimYuksekligi;
        transform.position = new Vector3(transform.position.x, yeniY, transform.position.z);

        if (oyuncu != null && oyuncu.AktifBoost == SevenVeilsKontrolcu.GunahBoostTipi.AltinMiknatisi)
        {
            float mesafe = Vector2.Distance(transform.position, oyuncu.transform.position);
            if (mesafe <= miknatisMenzili)
            {
                transform.position = Vector3.MoveTowards(transform.position, oyuncu.transform.position, miknatisHizi * Time.deltaTime);
                baslangicKonumu = transform.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (toplandi) return;

        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k != null || other.CompareTag("Player"))
        {
            toplandi = true;
            if (k != null) k.ToplanabilirBoostTetikle();

            if (toplamaSesi != null)
                AudioSource.PlayClipAtPoint(toplamaSesi, transform.position, 0.7f);

            OnParcaToplandi?.Invoke(bolumNo);
            gameObject.SetActive(false);
        }
    }
}

public class AraKontrolNoktasi : MonoBehaviour
{
    [SerializeField] private Sprite pasifSamdan;
    [SerializeField] private Sprite aktifSamdan;

    private SpriteRenderer sr;
    private bool yakildi;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (pasifSamdan == null) pasifSamdan = Resources.Load<Sprite>("Art/Ortak/Samdan_Pasif");
        if (aktifSamdan == null) aktifSamdan = Resources.Load<Sprite>("Art/Ortak/Samdan_Aktif");

        if (sr != null && pasifSamdan != null)
        {
            sr.sprite = pasifSamdan;
            sr.color = Color.white;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (GetComponent<RadarHedefi>() == null)
        {
            RadarHedefi rh = gameObject.AddComponent<RadarHedefi>();
            rh.tip = RadarHedefi.HedefTipi.Checkpoint;
        }
    }

    public void SpritelariAyarla(Sprite pasif, Sprite aktif)
    {
        pasifSamdan = pasif;
        aktifSamdan = aktif;
        if (sr != null && pasifSamdan != null) sr.sprite = pasifSamdan;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yakildi) return;

        SevenVeilsKontrolcu k = other.GetComponent<SevenVeilsKontrolcu>();
        if (k != null || other.CompareTag("Player"))
        {
            yakildi = true;
            if (k != null) k.CheckpointGuncelle(transform.position + Vector3.up * 0.35f);
            if (sr != null)
            {
                if (aktifSamdan != null) sr.sprite = aktifSamdan;
                sr.color = Color.white;
            }

            if (SevenVeilsTelemetriVeStats.Instance != null)
                SevenVeilsTelemetriVeStats.Instance.CheckpointGecildi();
        }
    }
}

public class AnlatimYaziti : MonoBehaviour
{
    [Header("Yazit veya Erdem Sunagi")]
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private bool erdemSunagiMi = false;
    [SerializeField] private string baslikTR = "GUNAH YAZITI";
    [SerializeField] private string baslikEN = "SIN INSCRIPTION";
    [TextArea(2, 4)] [SerializeField] private string metinTR = "";
    [TextArea(2, 4)] [SerializeField] private string metinEN = "";

    private bool oyuncuYakinda;
    private bool okunduKaydedildi;
    private Texture2D duzDoku;

    private void Awake()
    {
        duzDoku = new Texture2D(1, 1);
        duzDoku.SetPixel(0, 0, Color.white);
        duzDoku.Apply();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void IcerikAyarla(int bNo, bool erdemMi, string bTR, string bEN, string mTR, string mEN)
    {
        bolumNo = Mathf.Clamp(bNo, 1, 7);
        erdemSunagiMi = erdemMi;
        baslikTR = bTR;
        baslikEN = bEN;
        metinTR = mTR;
        metinEN = mEN;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SevenVeilsKontrolcu>() != null || other.CompareTag("Player"))
        {
            oyuncuYakinda = true;
            if (!okunduKaydedildi)
            {
                okunduKaydedildi = true;
                if (SevenVeilsTelemetriVeStats.Instance != null)
                    SevenVeilsTelemetriVeStats.Instance.YazitVeyaSunakOkundu(bolumNo, erdemSunagiMi);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<SevenVeilsKontrolcu>() != null || other.CompareTag("Player"))
        {
            oyuncuYakinda = false;
        }
    }

    private void OnGUI()
    {
        if (!oyuncuYakinda) return;

        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));

        bool ingilizce = PlayerPrefs.GetString("SevenVeils_Dil", "TR") == "EN";

        Rect kutu = new Rect(460f, 865f, 1000f, 165f);
        Color arkaRenk = erdemSunagiMi ? new Color(0.06f, 0.11f, 0.14f, 0.94f) : new Color(0.11f, 0.05f, 0.07f, 0.94f);
        Color kenarRenk = erdemSunagiMi ? new Color(0.45f, 0.88f, 0.95f, 0.92f) : new Color(0.92f, 0.55f, 0.30f, 0.92f);

        KutuCiz(kutu, arkaRenk);
        KutuCiz(new Rect(kutu.x, kutu.y, kutu.width, 2f), kenarRenk);
        KutuCiz(new Rect(kutu.x, kutu.yMax - 2f, kutu.width, 2f), kenarRenk);
        KutuCiz(new Rect(kutu.x, kutu.y, 2f, kutu.height), kenarRenk);
        KutuCiz(new Rect(kutu.xMax - 2f, kutu.y, 2f, kutu.height), kenarRenk);

        GUIStyle baslikStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        baslikStil.normal.textColor = kenarRenk;

        GUIStyle metinStil = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.UpperCenter,
            wordWrap = true
        };
        metinStil.normal.textColor = new Color(0.95f, 0.93f, 0.90f);

        GUI.Label(new Rect(kutu.x + 24f, kutu.y + 14f, kutu.width - 48f, 30f), ingilizce ? baslikEN : baslikTR, baslikStil);
        GUI.Label(new Rect(kutu.x + 36f, kutu.y + 52f, kutu.width - 72f, 96f), ingilizce ? metinEN : metinTR, metinStil);

        GUI.matrix = eskiMatris;
    }

    private void KutuCiz(Rect rect, Color renk)
    {
        Color eski = GUI.color;
        GUI.color = renk;
        GUI.DrawTexture(rect, duzDoku);
        GUI.color = eski;
    }
}

public class BolumBitisKapisi : MonoBehaviour
{
    [SerializeField] private int bolumNo = 1;
    [SerializeField] private float hedefSpeedrunSuresi = 240f;

    private bool gecildi;

    public static event Action<int> OnBolumKapisiGecildi;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (GetComponent<RadarHedefi>() == null)
        {
            RadarHedefi rh = gameObject.AddComponent<RadarHedefi>();
            rh.tip = RadarHedefi.HedefTipi.BitisKapisi;
        }
    }

    public void Ayarla(int bNo, float speedrunHedef)
    {
        bolumNo = Mathf.Clamp(bNo, 1, 7);
        hedefSpeedrunSuresi = speedrunHedef;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gecildi) return;

        if (other.GetComponent<SevenVeilsKontrolcu>() != null || other.CompareTag("Player"))
        {
            gecildi = true;
            OnBolumKapisiGecildi?.Invoke(bolumNo);
        }
    }
}
