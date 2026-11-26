using UnityEngine;

public class BolumTemaUygulayici : MonoBehaviour
{
    [SerializeField] private int bolumNo = 1;

    public void BolumNumarasiAyarla(int no)
    {
        bolumNo = Mathf.Clamp(no, 1, 7);
    }

    private void Awake()
    {
        var tema = SevenVeilsGorselFabrikasi.TemaGetir(bolumNo);
        if (Camera.main != null)
            Camera.main.backgroundColor = tema.gokyuzuRengi;

        string k = $"Art/Veil{bolumNo}";
        Sprite platformSp = Resources.Load<Sprite>($"{k}/Platform");
        Sprite karakterSp = Resources.Load<Sprite>($"{k}/Ressam");
        Sprite muhafizSp = Resources.Load<Sprite>($"{k}/Dusman_Muhafiz");
        Sprite kahinSp = Resources.Load<Sprite>($"{k}/Dusman_Kahin");
        Sprite zirhliSp = Resources.Load<Sprite>($"{k}/Dusman_Zirhli");
        Sprite parcaSp = Resources.Load<Sprite>($"{k}/Parca");
        Sprite kapiSp = Resources.Load<Sprite>($"{k}/Kapi");
        Sprite tablo0Sp = Resources.Load<Sprite>($"{k}/Tablo_0");
        Sprite tablo1Sp = Resources.Load<Sprite>($"{k}/Tablo_1");
        Sprite esSp = Resources.Load<Sprite>($"{k}/EsPortresi");
        Sprite yaySp = Resources.Load<Sprite>("Art/Ortak/Yay_Pad");
        Sprite dikenSp = Resources.Load<Sprite>("Art/Ortak/Diken_Tuzak");

        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr.gameObject.name.Contains("KaranlikPerdeGorseli")) continue;

            if (sr.GetComponent<SevenVeilsKontrolcu>() != null && karakterSp != null)
                sr.sprite = karakterSp;
            else if (sr.GetComponent<DevriyeDusman>() != null)
            {
                if (sr.gameObject.name.Contains("Kahin") && kahinSp != null) sr.sprite = kahinSp;
                else if (sr.gameObject.name.Contains("Zirhli") && zirhliSp != null) sr.sprite = zirhliSp;
                else if (muhafizSp != null) sr.sprite = muhafizSp;
            }
            else if (sr.GetComponent<ToplanabilirParca>() != null && parcaSp != null)
                sr.sprite = parcaSp;
            else if (sr.GetComponent<BolumBitisKapisi>() != null && kapiSp != null)
                sr.sprite = kapiSp;
            else if (sr.GetComponent<ZiplamaYayi>() != null && yaySp != null)
                sr.sprite = yaySp;
            else if (sr.gameObject.name.StartsWith("Diken") && dikenSp != null)
                sr.sprite = dikenSp;
            else if (sr.gameObject.name.StartsWith("Tablo_0") && tablo0Sp != null)
                sr.sprite = tablo0Sp;
            else if (sr.gameObject.name.StartsWith("Tablo_1") && tablo1Sp != null)
                sr.sprite = tablo1Sp;
            else if (sr.gameObject.name.StartsWith("EsPortresi") && esSp != null)
                sr.sprite = esSp;
            else if ((sr.gameObject.name.StartsWith("Zemin") || sr.gameObject.name.StartsWith("Coken") || sr.gameObject.name.StartsWith("Hareketli") || sr.gameObject.name.StartsWith("Duvar")) && platformSp != null)
                sr.sprite = platformSp;
        }
    }
}
