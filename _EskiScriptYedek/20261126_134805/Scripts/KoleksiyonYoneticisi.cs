using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KoleksiyonYoneticisi : MonoBehaviour
{
    public static KoleksiyonYoneticisi Ornek { get; private set; }

    private int aktifBolum = 1;
    private int toplamAdet = 0;
    private int toplananAdet = 0;
    private readonly List<TokenInstance> takipEdilenler = new List<TokenInstance>();
    private readonly HashSet<int> toplananIdler = new HashSet<int>();
    private PlayerController oyuncu;

    private static readonly string[] AdlarTr =
    {
        "Ziyafet Kırıntısı",
        "Uyanış Kıvılcımı",
        "Serap İncisi",
        "Lanetli Sikke",
        "Kırık Taç Parçası",
        "Ayna Kıymığı",
        "Kor Parçası"
    };

    private static readonly string[] AdlarEn =
    {
        "Banquet Crumb",
        "Spark of Awakening",
        "Mirage Pearl",
        "Cursed Coin",
        "Shattered Crown",
        "Mirror Splinter",
        "Ember Fragment"
    };

    private static readonly Color[] TemaRenkleri =
    {
        new Color(0.96f, 0.72f, 0.28f, 1f), // 1. Oburluk (Kehribar)
        new Color(0.42f, 0.74f, 0.94f, 1f), // 2. Tembellik (Sis Mavisi)
        new Color(0.82f, 0.45f, 0.78f, 1f), // 3. Şehvet (Bordo/Mor)
        new Color(0.36f, 0.80f, 0.54f, 1f), // 4. Açgözlülük (Zümrüt Altın)
        new Color(0.84f, 0.86f, 0.90f, 1f), // 5. Kibir (Soğuk Gümüş)
        new Color(0.32f, 0.82f, 0.74f, 1f), // 6. Kıskançlık (Zehir Turkuaz)
        new Color(0.94f, 0.36f, 0.30f, 1f)  // 7. Öfke (Kor Kırmızısı)
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SahneBaslangicKurulumu()
    {
        if (FindFirstObjectByType<KoleksiyonYoneticisi>() != null) return;
        GameObject obj = new GameObject("KoleksiyonYoneticisi");
        obj.AddComponent<KoleksiyonYoneticisi>();
    }

    private void Awake()
    {
        Ornek = this;
        aktifBolum = BolumNoBul();
    }

    private void Start()
    {
        TokenInstance[] tumJetonlar = FindObjectsByType<TokenInstance>(FindObjectsSortMode.None);
        takipEdilenler.Clear();
        toplananIdler.Clear();

        foreach (var j in tumJetonlar)
        {
            if (j != null && j.gameObject.activeInHierarchy)
            {
                takipEdilenler.Add(j);
                SpriteRenderer sr = j.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = Color.Lerp(Color.white, TemaRenkleri[aktifBolum - 1], 0.45f);
            }
        }

        toplamAdet = takipEdilenler.Count;
        toplananAdet = 0;

        if (toplamAdet > 0)
        {
            int eskiToplam = PlayerPrefs.GetInt($"YediPerde_Bolum_{aktifBolum}_Toplam", 0);
            if (toplamAdet > eskiToplam)
                PlayerPrefs.SetInt($"YediPerde_Bolum_{aktifBolum}_Toplam", toplamAdet);
        }
    }

    private void Update()
    {
        if (toplamAdet == 0) return;
        if (oyuncu == null) oyuncu = FindFirstObjectByType<PlayerController>();

        for (int i = 0; i < takipEdilenler.Count; i++)
        {
            TokenInstance t = takipEdilenler[i];
            if (t == null) continue;
            int id = t.GetInstanceID();
            if (toplananIdler.Contains(id)) continue;

            SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
            bool pasifOldu = !t.gameObject.activeInHierarchy || (sr != null && !sr.enabled);
            bool yakinTemas = oyuncu != null && oyuncu.isActiveAndEnabled &&
                              Vector2.Distance(oyuncu.transform.position, t.transform.position) <= 0.85f;

            if (pasifOldu || yakinTemas)
            {
                toplananIdler.Add(id);
                toplananAdet++;
                if (yakinTemas && t.gameObject.activeInHierarchy)
                    t.gameObject.SetActive(false);

                if (TematikSesYoneticisi.Ornek != null)
                    TematikSesYoneticisi.Ornek.ToplanabilirSesiCal();

                IlerlemeyiKaydet();
            }
        }
    }

    public void IlerlemeyiKaydet()
    {
        int oncekiEnIyi = PlayerPrefs.GetInt($"YediPerde_Bolum_{aktifBolum}_Toplanan", 0);
        if (toplananAdet > oncekiEnIyi)
        {
            PlayerPrefs.SetInt($"YediPerde_Bolum_{aktifBolum}_Toplanan", toplananAdet);
        }
        if (toplamAdet > 0)
        {
            PlayerPrefs.SetInt($"YediPerde_Bolum_{aktifBolum}_Toplam", toplamAdet);
            if (toplananAdet >= toplamAdet)
            {
                PlayerPrefs.SetInt($"YediPerde_Bolum_{aktifBolum}_Tamam", 1);
            }
        }
        PlayerPrefs.Save();
    }

    public static bool TumOyunTamKoleksiyonMu()
    {
        for (int b = 1; b <= 7; b++)
        {
            int toplam = PlayerPrefs.GetInt($"YediPerde_Bolum_{b}_Toplam", 0);
            int toplanan = PlayerPrefs.GetInt($"YediPerde_Bolum_{b}_Toplanan", 0);
            if (toplam <= 0 || toplanan < toplam) return false;
        }
        return true;
    }

    public string AktifEsyaAdi()
    {
        bool ingilizce = PlayerPrefs.GetString("Dil", "TR") == "EN";
        return ingilizce ? AdlarEn[aktifBolum - 1] : AdlarTr[aktifBolum - 1];
    }

    public int Toplanan => toplananAdet;
    public int Toplam => toplamAdet;

    private void OnGUI()
    {
        if (toplamAdet <= 0) return;

        float olcek = Mathf.Clamp(Screen.height / 720f, 0.85f, 1.6f);
        float kutuW = 250f * olcek;
        float kutuH = 58f * olcek;
        Rect kutu = new Rect(Screen.width - kutuW - 20f * olcek, 18f * olcek, kutuW, kutuH);

        Color tema = TemaRenkleri[aktifBolum - 1];
        Color eskiRenk = GUI.color;

        GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.84f);
        GUI.DrawTexture(kutu, Texture2D.whiteTexture);

        GUI.color = tema;
        GUI.DrawTexture(new Rect(kutu.x, kutu.y, 4f * olcek, kutu.height), Texture2D.whiteTexture);

        float doluluk = Mathf.Clamp01((float)toplananAdet / toplamAdet);
        GUI.DrawTexture(new Rect(kutu.x + 12f * olcek, kutu.yMax - 9f * olcek, (kutu.width - 24f * olcek) * doluluk, 4f * olcek), Texture2D.whiteTexture);
        GUI.color = eskiRenk;

        GUIStyle baslikStil = new GUIStyle(GUI.skin.label);
        baslikStil.fontSize = Mathf.RoundToInt(13f * olcek);
        baslikStil.fontStyle = FontStyle.Bold;
        baslikStil.normal.textColor = tema;

        GUIStyle sayiStil = new GUIStyle(GUI.skin.label);
        sayiStil.fontSize = Mathf.RoundToInt(17f * olcek);
        sayiStil.fontStyle = FontStyle.Bold;
        sayiStil.alignment = TextAnchor.MiddleRight;
        sayiStil.normal.textColor = Color.white;

        GUI.Label(new Rect(kutu.x + 12f * olcek, kutu.y + 6f * olcek, kutu.width - 24f * olcek, 22f * olcek), AktifEsyaAdi(), baslikStil);
        GUI.Label(new Rect(kutu.x + 12f * olcek, kutu.y + 22f * olcek, kutu.width - 24f * olcek, 26f * olcek), $"{toplananAdet} / {toplamAdet}", sayiStil);
    }

    private int BolumNoBul()
    {
        BolumAtmosferi atm = FindFirstObjectByType<BolumAtmosferi>();
        if (atm != null && atm.bolumNumarasi >= 1 && atm.bolumNumarasi <= 7)
            return atm.bolumNumarasi;

        string ad = SceneManager.GetActiveScene().name;
        for (int i = 1; i <= 7; i++)
        {
            if (ad.Contains(i.ToString())) return i;
        }
        return Mathf.Clamp(Telemetri.AktifBolum, 1, 7);
    }
}
