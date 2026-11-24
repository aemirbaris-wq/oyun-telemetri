using System.Collections;
using UnityEngine;

public class SesYoneticisi : MonoBehaviour
{
    public static SesYoneticisi Ornek { get; private set; }

    [Header("Ozel Ses Dosyalari (Opsiyonel - Bos Birakilirsa Sentetik Ses Calar)")]
    public AudioClip butonUzerineGelmeSesi;
    public AudioClip butonTiklamaSesi;
    public AudioClip dashBoostSesi;
    public AudioClip tabletAcilisSesi;
    public AudioClip erdemSunağıSesi;
    public AudioClip bolumGirisSesi;

    [Header("Bolumlere Gore Ambiyans Muzikleri (1-7)")]
    public AudioClip[] bolumAmbiyanslari = new AudioClip[7];

    private AudioSource efektKaynagi;
    private AudioSource ambiyansKaynagi;

    private void Awake()
    {
        if (Ornek != null && Ornek != this)
        {
            Destroy(gameObject);
            return;
        }
        Ornek = this;
        DontDestroyOnLoad(gameObject);

        efektKaynagi = gameObject.AddComponent<AudioSource>();
        efektKaynagi.playOnAwake = false;

        ambiyansKaynagi = gameObject.AddComponent<AudioSource>();
        ambiyansKaynagi.loop = true;
        ambiyansKaynagi.playOnAwake = false;
        ambiyansKaynagi.volume = 0.22f;
    }

    public void ButonHoverCal()
    {
        if (butonUzerineGelmeSesi != null)
            efektKaynagi.PlayOneShot(butonUzerineGelmeSesi, 0.45f);
        else
            efektKaynagi.PlayOneShot(SentetikTonUret(660f, 0.045f, 0.12f, false));
    }

    public void ButonTiklaCal()
    {
        if (butonTiklamaSesi != null)
            efektKaynagi.PlayOneShot(butonTiklamaSesi, 0.7f);
        else
            efektKaynagi.PlayOneShot(SentetikTonUret(440f, 0.09f, 0.25f, true));
    }

    public void DashSesiCal()
    {
        if (dashBoostSesi != null)
            efektKaynagi.PlayOneShot(dashBoostSesi, 0.75f);
        else
            efektKaynagi.PlayOneShot(SentetikSupurmeUret(520f, 140f, 0.18f, 0.30f));
    }

    public void TabletSesiCal(bool erdemMi)
    {
        if (erdemMi && erdemSunağıSesi != null)
        {
            efektKaynagi.PlayOneShot(erdemSunağıSesi, 0.8f);
            return;
        }
        if (!erdemMi && tabletAcilisSesi != null)
        {
            efektKaynagi.PlayOneShot(tabletAcilisSesi, 0.8f);
            return;
        }

        if (erdemMi)
            efektKaynagi.PlayOneShot(SentetikTonUret(880f, 0.45f, 0.22f, false));
        else
            efektKaynagi.PlayOneShot(SentetikSupurmeUret(190f, 95f, 0.35f, 0.28f));
    }

    public void BolumAtmosferSesiBaslat(int bolumNo)
    {
        int indeks = Mathf.Clamp(bolumNo - 1, 0, 6);

        if (bolumGirisSesi != null)
            efektKaynagi.PlayOneShot(bolumGirisSesi, 0.75f);
        else
        {
            float baslangicFrekans = Mathf.Lerp(520f, 110f, indeks / 6f);
            efektKaynagi.PlayOneShot(SentetikTonUret(baslangicFrekans, 0.85f, 0.24f, indeks >= 4));
        }

        if (bolumAmbiyanslari != null && indeks < bolumAmbiyanslari.Length && bolumAmbiyanslari[indeks] != null)
        {
            if (ambiyansKaynagi.clip != bolumAmbiyanslari[indeks])
            {
                ambiyansKaynagi.clip = bolumAmbiyanslari[indeks];
                ambiyansKaynagi.Play();
            }
        }
        else
        {
            // Safe (aydinlik harmonik) -> Creepy (dusuk frekansli ugultu) gecisi
            float tabanFrekans = Mathf.Lerp(220f, 58f, indeks / 6f);
            ambiyansKaynagi.clip = SentetikAmbiyansUret(tabanFrekans, indeks >= 4);
            ambiyansKaynagi.Play();
        }
    }

    private AudioClip SentetikTonUret(float frekans, float sure, float genlik, bool karanlik)
    {
        int ornekHizi = 44100;
        int uzunluk = Mathf.CeilToInt(ornekHizi * sure);
        float[] veri = new float[uzunluk];
        for (int i = 0; i < uzunluk; i++)
        {
            float t = (float)i / ornekHizi;
            float zarf = Mathf.Exp(-t * (6f / sure));
            float dalga = Mathf.Sin(2f * Mathf.PI * frekans * t);
            if (karanlik)
                dalga = (dalga + 0.5f * Mathf.Sin(2f * Mathf.PI * (frekans * 0.5f) * t)) * 0.7f;
            veri[i] = dalga * zarf * genlik;
        }
        AudioClip klip = AudioClip.Create("SentetikTon", uzunluk, 1, ornekHizi, false);
        klip.SetData(veri, 0);
        return klip;
    }

    private AudioClip SentetikSupurmeUret(float baslangicFrek, float bitisFrek, float sure, float genlik)
    {
        int ornekHizi = 44100;
        int uzunluk = Mathf.CeilToInt(ornekHizi * sure);
        float[] veri = new float[uzunluk];
        float faz = 0f;
        for (int i = 0; i < uzunluk; i++)
        {
            float oran = (float)i / uzunluk;
            float frek = Mathf.Lerp(baslangicFrek, bitisFrek, oran);
            faz += 2f * Mathf.PI * frek / ornekHizi;
            float zarf = (1f - oran) * genlik;
            veri[i] = Mathf.Sin(faz) * zarf;
        }
        AudioClip klip = AudioClip.Create("SentetikSupurme", uzunluk, 1, ornekHizi, false);
        klip.SetData(veri, 0);
        return klip;
    }

    private AudioClip SentetikAmbiyansUret(float tabanFrek, bool creepyMod)
    {
        int ornekHizi = 44100;
        int uzunluk = ornekHizi * 4;
        float[] veri = new float[uzunluk];
        for (int i = 0; i < uzunluk; i++)
        {
            float t = (float)i / ornekHizi;
            float nefes = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
            float ana = Mathf.Sin(2f * Mathf.PI * tabanFrek * t);
            float armonik = creepyMod
                ? Mathf.Sin(2f * Mathf.PI * (tabanFrek * 1.06f) * t) * 0.6f
                : Mathf.Sin(2f * Mathf.PI * (tabanFrek * 1.5f) * t) * 0.4f;
            veri[i] = (ana + armonik) * 0.08f * nefes;
        }
        AudioClip klip = AudioClip.Create("SentetikAmbiyans", uzunluk, 1, ornekHizi, false);
        klip.SetData(veri, 0);
        return klip;
    }
}
