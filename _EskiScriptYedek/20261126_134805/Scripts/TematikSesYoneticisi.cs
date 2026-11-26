using UnityEngine;
using UnityEngine.SceneManagement;

public class TematikSesYoneticisi : MonoBehaviour
{
    public static TematikSesYoneticisi Ornek { get; private set; }

    private AudioSource muzikKaynak;
    private AudioSource efektKaynak;
    private int aktifBolum = 1;
    private AudioClip aktifToplanabilirSesi;
    private AudioClip checkpointSesi;
    private AudioClip bitisSesi;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OtomatikBaslat()
    {
        if (Ornek != null) return;
        GameObject obj = new GameObject("TematikSesYoneticisi");
        DontDestroyOnLoad(obj);
        obj.AddComponent<TematikSesYoneticisi>();
    }

    private void Awake()
    {
        if (Ornek != null && Ornek != this)
        {
            Destroy(gameObject);
            return;
        }
        Ornek = this;
        DontDestroyOnLoad(gameObject);

        muzikKaynak = gameObject.AddComponent<AudioSource>();
        muzikKaynak.loop = true;
        muzikKaynak.playOnAwake = false;
        muzikKaynak.volume = 0.24f;

        efektKaynak = gameObject.AddComponent<AudioSource>();
        efektKaynak.loop = false;
        efektKaynak.playOnAwake = false;
        efektKaynak.volume = 0.55f;

        checkpointSesi = CheckpointKlibiUret();
        bitisSesi = BitisKlibiUret();

        SceneManager.sceneLoaded += SahneYuklendi;
        BolumSesleriniHazirla(AktifBolumuBul());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    private void SahneYuklendi(Scene sahne, LoadSceneMode mod)
    {
        BolumSesleriniHazirla(AktifBolumuBul());
    }

    public int AktifBolumuBul()
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

    public void BolumSesleriniHazirla(int bolumNo)
    {
        aktifBolum = Mathf.Clamp(bolumNo, 1, 7);
        aktifToplanabilirSesi = ToplanabilirKlibiUret(aktifBolum);

        AudioClip yeniMuzik = AmbientMuzikUret(aktifBolum);
        if (muzikKaynak.clip != yeniMuzik)
        {
            muzikKaynak.clip = yeniMuzik;
            muzikKaynak.Play();
        }
    }

    public void ToplanabilirSesiCal()
    {
        if (efektKaynak == null) return;
        if (aktifToplanabilirSesi == null)
            aktifToplanabilirSesi = ToplanabilirKlibiUret(aktifBolum);
        efektKaynak.PlayOneShot(aktifToplanabilirSesi, 0.72f);
    }

    public void CheckpointSesiCal()
    {
        if (efektKaynak != null && checkpointSesi != null)
            efektKaynak.PlayOneShot(checkpointSesi, 0.75f);
    }

    public void BolumBitisSesiCal()
    {
        if (efektKaynak != null && bitisSesi != null)
            efektKaynak.PlayOneShot(bitisSesi, 0.85f);
    }

    private AudioClip AmbientMuzikUret(int bolum)
    {
        int sampleRate = 22050;
        float sure = 6.0f;
        int ornekSayisi = Mathf.RoundToInt(sampleRate * sure);
        float[] veri = new float[ornekSayisi];

        // Bolum 1'den 7'ye dustukce frekans kararir ve araliklar gerginlesir
        float[] kokFrekanslar = { 146.83f, 130.81f, 116.54f, 110.00f, 98.00f, 87.31f, 73.42f };
        float[] ikinciOran    = { 1.498f,  1.414f,  1.335f,  1.260f,  1.189f,  1.414f, 1.059f };
        float[] nabizHizi     = { 0.35f,   0.28f,   0.45f,   0.55f,   0.40f,   0.75f,  1.15f };

        float kok = kokFrekanslar[bolum - 1];
        float ust = kok * ikinciOran[bolum - 1];
        float nabiz = nabizHizi[bolum - 1];
        float karanlikOrani = (bolum - 1) / 6f;

        for (int i = 0; i < ornekSayisi; i++)
        {
            float t = (float)i / sampleRate;
            float lfo = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * nabiz * t);

            float temel = Mathf.Sin(2f * Mathf.PI * kok * t);
            float harmonik = Mathf.Sin(2f * Mathf.PI * ust * t) * Mathf.Lerp(0.42f, 0.22f, karanlikOrani);
            float altBas = Mathf.Sin(2f * Mathf.PI * (kok * 0.5f) * t) * Mathf.Lerp(0.20f, 0.62f, karanlikOrani);
            float gerginTitresim = Mathf.Sin(2f * Mathf.PI * (kok * 1.012f) * t) * (0.12f + 0.22f * karanlikOrani);

            float s = (temel * 0.45f + harmonik + altBas + gerginTitresim) * lfo * 0.36f;

            // Dongu bas/son gecisinde tik sesini onlemek icin yumusak gecis
            float kenar = Mathf.Min(t / 0.25f, (sure - t) / 0.25f, 1f);
            veri[i] = Mathf.Clamp(s * kenar, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("AmbientBolum_" + bolum, ornekSayisi, 1, sampleRate, false);
        clip.SetData(veri, 0);
        return clip;
    }

    private AudioClip ToplanabilirKlibiUret(int bolum)
    {
        int sampleRate = 22050;
        float sure = 0.36f;
        int ornekSayisi = Mathf.RoundToInt(sampleRate * sure);
        float[] veri = new float[ornekSayisi];

        // 1: Sicak can -> 7: Derin kor tinisi
        float[] basFrekans = { 660f, 740f, 587f, 880f, 987f, 1046f, 330f };
        float[] bitFrekans = { 990f, 1110f, 880f, 659f, 740f, 554f, 220f };

        float f1 = basFrekans[bolum - 1];
        float f2 = bitFrekans[bolum - 1];

        for (int i = 0; i < ornekSayisi; i++)
        {
            float t = (float)i / sampleRate;
            float oran = t / sure;
            float zarf = Mathf.Exp(-7.5f * oran) * Mathf.Min(t / 0.01f, 1f);
            float frek = oran < 0.35f ? f1 : f2;

            float s = Mathf.Sin(2f * Mathf.PI * frek * t) * 0.65f
                    + Mathf.Sin(2f * Mathf.PI * (frek * 1.5f) * t) * 0.25f;

            veri[i] = Mathf.Clamp(s * zarf * 0.55f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("ToplanabilirSFX_" + bolum, ornekSayisi, 1, sampleRate, false);
        clip.SetData(veri, 0);
        return clip;
    }

    private AudioClip CheckpointKlibiUret()
    {
        int sampleRate = 22050;
        float sure = 0.55f;
        int ornekSayisi = Mathf.RoundToInt(sampleRate * sure);
        float[] veri = new float[ornekSayisi];

        for (int i = 0; i < ornekSayisi; i++)
        {
            float t = (float)i / sampleRate;
            float oran = t / sure;
            float zarf = Mathf.Exp(-5.0f * oran) * Mathf.Min(t / 0.015f, 1f);
            float f = oran < 0.4f ? 440f : 659.25f;
            float s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * (f * 2f) * t) * 0.2f;
            veri[i] = Mathf.Clamp(s * zarf * 0.5f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("CheckpointSFX", ornekSayisi, 1, sampleRate, false);
        clip.SetData(veri, 0);
        return clip;
    }

    private AudioClip BitisKlibiUret()
    {
        int sampleRate = 22050;
        float sure = 0.85f;
        int ornekSayisi = Mathf.RoundToInt(sampleRate * sure);
        float[] veri = new float[ornekSayisi];

        for (int i = 0; i < ornekSayisi; i++)
        {
            float t = (float)i / sampleRate;
            float oran = t / sure;
            float zarf = Mathf.Exp(-3.5f * oran) * Mathf.Min(t / 0.02f, 1f);
            float f = oran < 0.33f ? 392f : (oran < 0.66f ? 493.88f : 587.33f);
            float s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.55f + Mathf.Sin(2f * Mathf.PI * (f * 0.5f) * t) * 0.35f;
            veri[i] = Mathf.Clamp(s * zarf * 0.55f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("BolumBitisSFX", ornekSayisi, 1, sampleRate, false);
        clip.SetData(veri, 0);
        return clip;
    }
}
