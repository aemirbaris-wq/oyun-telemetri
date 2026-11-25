using System.Collections;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BolumBitisKapisi : MonoBehaviour
{
    public int bolumNo = 1;

    private bool tamamlandi = false;
    private bool gizliSonAktif = false;
    private PlayerController oyuncu;
    private BoxCollider2D boxCol;

    private static readonly string[] GunahAdlariTr =
    {
        "OBURLUK", "TEMBELLİK", "ŞEHVET", "AÇGÖZLÜLÜK", "KİBİR", "KISKANÇLIK", "ÖFKE"
    };

    private static readonly string[] GunahAdlariEn =
    {
        "GLUTTONY", "SLOTH", "LUST", "GREED", "PRIDE", "ENVY", "WRATH"
    };

    private void Awake()
    {
        // Eski microgame VictoryZone cakismasini devre disi birak
        VictoryZone eskiVictory = GetComponent<VictoryZone>();
        if (eskiVictory != null) eskiVictory.enabled = false;

        boxCol = GetComponent<BoxCollider2D>();
        if (boxCol == null) boxCol = gameObject.AddComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
    }

    private void Update()
    {
        if (tamamlandi)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                SonrakiSahneyeGec();
            }
            return;
        }

        if (oyuncu == null)
        {
            oyuncu = FindFirstObjectByType<PlayerController>();
            return;
        }

        if (!oyuncu.isActiveAndEnabled) return;

        bool icinde = (boxCol != null && boxCol.bounds.Contains(oyuncu.transform.position)) ||
                      Vector2.Distance(oyuncu.transform.position, transform.position + Vector3.up * 1.2f) <= 1.85f;

        if (icinde)
        {
            BolumuTamamla();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tamamlandi) return;
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            BolumuTamamla();
        }
    }

    private void BolumuTamamla()
    {
        tamamlandi = true;

        if (oyuncu != null)
        {
            oyuncu.controlEnabled = false;
        }

        if (KoleksiyonYoneticisi.Ornek != null)
        {
            KoleksiyonYoneticisi.Ornek.IlerlemeyiKaydet();
        }

        // Secret Ending SADECE 7. bolumun sonunda ve 7/7 tam koleksiyon varsa acilir
        if (bolumNo >= 7)
        {
            gizliSonAktif = KoleksiyonYoneticisi.TumOyunTamKoleksiyonMu();
        }

        if (TematikSesYoneticisi.Ornek != null)
        {
            TematikSesYoneticisi.Ornek.BolumBitisSesiCal();
        }

        Telemetri.OlayKaydet(gizliSonAktif ? "secret_ending" : "bolum_tamam", bolumNo);

        if (bolumNo < 7)
        {
            StartCoroutine(OtomatikGecisSayaci());
        }
    }

    private IEnumerator OtomatikGecisSayaci()
    {
        yield return new WaitForSeconds(2.8f);
        if (tamamlandi) SonrakiSahneyeGec();
    }

    private void SonrakiSahneyeGec()
    {
        if (bolumNo < 7)
        {
            string hedefSahne = "Bolum" + (bolumNo + 1);
            if (Application.CanStreamedLevelBeLoaded(hedefSahne))
            {
                SceneManager.LoadScene(hedefSahne);
                return;
            }
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (!tamamlandi) return;

        bool ingilizce = PlayerPrefs.GetString("Dil", "TR") == "EN";
        int idx = Mathf.Clamp(bolumNo - 1, 0, 6);
        string gunah = ingilizce ? GunahAdlariEn[idx] : GunahAdlariTr[idx];

        Color eskiRenk = GUI.color;
        GUI.color = gizliSonAktif
            ? new Color(0.96f, 0.93f, 0.84f, 0.94f)
            : new Color(0.04f, 0.05f, 0.08f, 0.90f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = eskiRenk;

        GUIStyle baslikStil = new GUIStyle(GUI.skin.label);
        baslikStil.alignment = TextAnchor.MiddleCenter;
        baslikStil.fontSize = 32;
        baslikStil.fontStyle = FontStyle.Bold;
        baslikStil.normal.textColor = gizliSonAktif ? new Color(0.24f, 0.18f, 0.08f, 1f) : new Color(0.95f, 0.80f, 0.42f, 1f);

        GUIStyle altStil = new GUIStyle(GUI.skin.label);
        altStil.alignment = TextAnchor.MiddleCenter;
        altStil.fontSize = 18;
        altStil.wordWrap = true;
        altStil.normal.textColor = gizliSonAktif ? new Color(0.18f, 0.16f, 0.14f, 1f) : new Color(0.88f, 0.90f, 0.94f, 1f);

        string baslik;
        string aciklama;
        string devamNotu;

        if (bolumNo >= 7 && gizliSonAktif)
        {
            baslik = ingilizce ? "SECRET ENDING — THE SEVEN VEILS PARTED" : "GİZLİ SON — YEDİ PERDE ARALANDI";
            aciklama = ingilizce
                ? "You gathered every single fragment across all seven trials. Beyond the deepest dark, she was waiting in the light."
                : "Yedi eşikteki tüm parçaları eksiksiz topladın. En derin karanlığın ötesinde, o seni ışıkta bekliyordu.";
            devamNotu = ingilizce ? "Press SPACE to play again" : "Yeniden oynamak için SPACE tuşuna bas";
        }
        else if (bolumNo >= 7)
        {
            baslik = ingilizce ? "THE END — BEYOND THE SHADOWS" : "SON — GÖLGELERİN ARDINDAN";
            aciklama = ingilizce
                ? "You crossed all seven thresholds, yet some forgotten fragments remained in the dark."
                : "Yedi eşiği de aştın, ancak bazı unutulmuş parçalar karanlığın içinde kaldı.";
            devamNotu = ingilizce ? "Press SPACE to restart" : "Baştan başlamak için SPACE tuşuna bas";
        }
        else
        {
            baslik = ingilizce
                ? $"VEIL {bolumNo} CLEARED — {gunah}"
                : $"{bolumNo}. PERDE TAMAMLANDI — {gunah}";

            string koleksiyonBilgi = "";
            if (KoleksiyonYoneticisi.Ornek != null && KoleksiyonYoneticisi.Ornek.Toplam > 0)
            {
                koleksiyonBilgi = $"{KoleksiyonYoneticisi.Ornek.AktifEsyaAdi()}: {KoleksiyonYoneticisi.Ornek.Toplanan} / {KoleksiyonYoneticisi.Ornek.Toplam}";
            }

            bool sonrakiVar = Application.CanStreamedLevelBeLoaded("Bolum" + (bolumNo + 1));
            aciklama = koleksiyonBilgi;
            devamNotu = sonrakiVar
                ? (ingilizce ? "Advancing to the next veil... (or press SPACE)" : "Sonraki perdeye geçiliyor... (veya SPACE'e bas)")
                : (ingilizce ? "Chapter 2 is not built yet — press SPACE to replay Chapter 1" : "2. Bölüm henüz kurulmadı — 1. Bölümü tekrar oynamak için SPACE'e bas");
        }

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.Label(new Rect(cx - 380f, cy - 90f, 760f, 50f), baslik, baslikStil);
        GUI.Label(new Rect(cx - 340f, cy - 25f, 680f, 65f), aciklama, altStil);
        GUI.Label(new Rect(cx - 340f, cy + 55f, 680f, 35f), devamNotu, altStil);
    }
}
