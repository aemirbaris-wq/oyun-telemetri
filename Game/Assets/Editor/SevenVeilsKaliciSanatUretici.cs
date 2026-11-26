#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SevenVeilsKaliciSanatUretici
{
    public static void TumSanatVarliklariniUret(int bolumNo = 1)
    {
        string ortakKlasor = "Assets/Resources/Art/Ortak";
        string bolumKlasor = $"Assets/Resources/Art/Veil{bolumNo}";
        Directory.CreateDirectory(ortakKlasor);
        Directory.CreateDirectory(bolumKlasor);

        var tema = SevenVeilsGorselFabrikasi.TemaGetir(bolumNo);

        // Ortak varliklar (Tam 1.0 x 1.0 birim: 128px @ 128 PPU)
        PngKaydet($"{ortakKlasor}/Samdan_Pasif.png", SamdanCiz(false), 128);
        PngKaydet($"{ortakKlasor}/Samdan_Aktif.png", SamdanCiz(true), 128);
        PngKaydet($"{ortakKlasor}/Yay_Pad.png", YayCiz(tema), 128);
        PngKaydet($"{ortakKlasor}/Yazit_Gunah.png", YazitCiz(false), 128);
        PngKaydet($"{ortakKlasor}/Sunak_Erdem.png", YazitCiz(true), 128);
        PngKaydet($"{ortakKlasor}/Diken_Tuzak.png", DikenCiz(), 128);

        // Bolume ozel yuksek detayli varliklar
        PngKaydet($"{bolumKlasor}/Ressam.png", GercekciRessamCiz(bolumNo, tema), 128);
        PngKaydet($"{bolumKlasor}/Platform.png", DetayliPlatformCiz(bolumNo, tema), 128);
        PngKaydet($"{bolumKlasor}/Dusman_Muhafiz.png", DusmanCiz(tema, 0), 128);
        PngKaydet($"{bolumKlasor}/Dusman_Kahin.png", DusmanCiz(tema, 1), 128);
        PngKaydet($"{bolumKlasor}/Dusman_Zirhli.png", DusmanCiz(tema, 2), 128);
        PngKaydet($"{bolumKlasor}/Parca.png", ParcaCiz(tema), 128);
        PngKaydet($"{bolumKlasor}/Kapi.png", KapiCiz(tema), 96);
        PngKaydet($"{bolumKlasor}/Tablo_0.png", TabloCiz(tema, 0), 64);
        PngKaydet($"{bolumKlasor}/Tablo_1.png", TabloCiz(tema, 1), 64);
        PngKaydet($"{bolumKlasor}/EsPortresi.png", EsPortresiCiz(tema), 64);
        PngKaydet($"{bolumKlasor}/Parallax_Uzak.png", ParallaxUzakCiz(tema), 32);
        PngKaydet($"{bolumKlasor}/Parallax_Orta.png", ParallaxOrtaCiz(tema), 32);

        AssetDatabase.Refresh();
    }

    private static void PngKaydet(string assetYolu, Texture2D tex, float ppu)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(assetYolu, bytes);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(assetYolu, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(assetYolu) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D BosDoku(int w, int h)
    {
        Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color seffaf = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                t.SetPixel(x, y, seffaf);
        return t;
    }

    private static void KutuDoldur(Texture2D t, int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = Mathf.Max(0, y0); y <= Mathf.Min(t.height - 1, y1); y++)
            for (int x = Mathf.Max(0, x0); x <= Mathf.Min(t.width - 1, x1); x++)
                t.SetPixel(x, y, c);
    }

    private static void DaireDoldur(Texture2D t, float cx, float cy, float r, Color c)
    {
        for (int y = Mathf.Max(0, Mathf.FloorToInt(cy - r)); y <= Mathf.Min(t.height - 1, Mathf.CeilToInt(cy + r)); y++)
        {
            for (int x = Mathf.Max(0, Mathf.FloorToInt(cx - r)); x <= Mathf.Min(t.width - 1, Mathf.CeilToInt(cx + r)); x++)
            {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) <= r)
                    t.SetPixel(x, y, c);
            }
        }
    }

    // 128x128 Detayli Ressam Silueti (Dalgalanan pelerin, kirmizi atki, palet ve firca)
    private static Texture2D GercekciRessamCiz(int bolumNo, SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(128, 128);
        Color pelerinAna = p.karakterRengi;
        Color pelerinGolge = Color.Lerp(pelerinAna, Color.black, 0.34f);
        Color pelerinIsik = Color.Lerp(pelerinAna, p.zeminDamarRenk, 0.25f);
        Color atki = bolumNo >= 6 ? new Color(0.96f, 0.28f, 0.14f) : new Color(0.78f, 0.18f, 0.24f);
        Color atkiGolge = Color.Lerp(atki, Color.black, 0.32f);

        // Cizmeler (y: 0..18)
        KutuDoldur(t, 48, 0, 58, 18, new Color(0.12f, 0.10f, 0.14f));
        KutuDoldur(t, 68, 0, 78, 18, new Color(0.12f, 0.10f, 0.14f));

        // Cok katmanli ressam pelerini (y: 16..88)
        for (int y = 16; y < 88; y++)
        {
            float oran = (y - 16f) / 72f;
            int sol = Mathf.RoundToInt(30 + oran * 20 + Mathf.Sin(y * 0.18f) * 2f);
            int sag = Mathf.RoundToInt(92 - oran * 12);
            for (int x = sol; x <= sag; x++)
            {
                Color c = pelerinAna;
                if (x < sol + 7 || (x - sol) % 14 < 3) c = pelerinGolge;
                else if (x > sag - 5) c = pelerinIsik;
                t.SetPixel(x, y, c);
            }
        }

        // Boyunda ruzgarda dalgalanan uzun ressam atkisi
        KutuDoldur(t, 44, 82, 82, 91, atki);
        KutuDoldur(t, 44, 82, 82, 84, atkiGolge);
        // Arkaya sarkan iki uclu atki kuyrugu
        for (int i = 0; i < 28; i++)
        {
            int ax = 44 - i;
            int ay = 84 - Mathf.RoundToInt(i * 0.45f + Mathf.Sin(i * 0.28f) * 3f);
            KutuDoldur(t, ax, ay - 4, ax + 1, ay + 4, i % 5 == 0 ? atkiGolge : atki);
        }

        // Bas, yuz silueti ve parlayan gozler (y: 91..114)
        KutuDoldur(t, 50, 91, 78, 113, pelerinAna);
        KutuDoldur(t, 50, 91, 56, 113, pelerinGolge);
        KutuDoldur(t, 68, 100, 75, 105, p.karakterGozRengi);

        // Egik ressam beresi (y: 112..126)
        KutuDoldur(t, 42, 112, 84, 119, pelerinGolge);
        KutuDoldur(t, 46, 118, 78, 125, pelerinIsik);
        KutuDoldur(t, 58, 124, 66, 127, pelerinGolge);

        // Sol elde ahsap boya paleti, sag elde firca
        DaireDoldur(t, 42, 54, 9, new Color(0.54f, 0.34f, 0.18f));
        DaireDoldur(t, 39, 57, 2, p.parcaRengi);
        DaireDoldur(t, 45, 56, 2, atki);
        DaireDoldur(t, 42, 50, 2, p.karakterGozRengi);

        KutuDoldur(t, 84, 46, 88, 78, new Color(0.46f, 0.28f, 0.14f));
        KutuDoldur(t, 82, 78, 90, 86, p.zeminDamarRenk);

        t.Apply();
        return t;
    }

    // 128x128 Kabartmali Ziyafet Tasi Platformu
    private static Texture2D DetayliPlatformCiz(int bolumNo, SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(128, 128);
        Color koyu = Color.Lerp(p.zeminAnaRenk, Color.black, 0.38f);
        Color orta = p.zeminAnaRenk;
        Color parlak = Color.Lerp(p.zeminAnaRenk, p.zeminDamarRenk, 0.85f);

        KutuDoldur(t, 0, 0, 127, 127, orta);
        // Ust altin-tas pervaz
        KutuDoldur(t, 0, 118, 127, 127, parlak);
        KutuDoldur(t, 0, 113, 127, 117, koyu);
        KutuDoldur(t, 0, 0, 127, 7, koyu);

        for (int y = 8; y < 113; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                if (y % 32 == 0 || (x + (y / 32) * 24) % 64 == 0)
                    t.SetPixel(x, y, koyu);
                else if (bolumNo == 1 && (y % 32 == 16) && (x % 16 == 8))
                    t.SetPixel(x, y, p.zeminDamarRenk);
            }
        }
        t.Apply();
        return t;
    }

    // 128x128 3 Farkli Dusman Arketipi
    private static Texture2D DusmanCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p, int tip)
    {
        Texture2D t = BosDoku(128, 128);
        Color koyu = Color.Lerp(p.dusmanRengi, Color.black, 0.38f);

        if (tip == 0) // Ezilebilir Ziyafet Muhafizi
        {
            KutuDoldur(t, 20, 0, 107, 86, p.dusmanRengi);
            KutuDoldur(t, 28, 86, 99, 108, koyu);
            KutuDoldur(t, 38, 62, 52, 72, Color.white);
            KutuDoldur(t, 76, 62, 90, 72, Color.white);
            KutuDoldur(t, 42, 30, 86, 42, p.zeminDamarRenk);
        }
        else if (tip == 1) // Menzilli Kahin
        {
            for (int y = 8; y < 118; y++)
            {
                int w = (122 - y) / 2;
                KutuDoldur(t, 64 - w, y, 64 + w, y, y % 12 < 3 ? p.dusmanRengi : koyu);
            }
            KutuDoldur(t, 52, 86, 76, 96, new Color(1f, 0.35f, 0.12f));
            DaireDoldur(t, 96, 64, 12, p.zeminDamarRenk);
            DaireDoldur(t, 96, 64, 6, new Color(1f, 0.45f, 0.15f));
        }
        else // Onden Zirhli Bekci (Tepesi dikenli)
        {
            KutuDoldur(t, 16, 0, 111, 92, koyu);
            KutuDoldur(t, 24, 8, 103, 84, p.dusmanRengi);
            for (int x = 22; x <= 96; x += 18)
            {
                for (int y = 93; y < 122; y++)
                {
                    int w = (122 - y) / 4;
                    KutuDoldur(t, x + 5 - w, y, x + 5 + w, y, new Color(0.95f, 0.24f, 0.18f));
                }
            }
            KutuDoldur(t, 74, 54, 94, 64, p.zeminDamarRenk);
        }

        t.Apply();
        return t;
    }

    private static Texture2D ParcaCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(128, 128);
        DaireDoldur(t, 64, 64, 46, new Color(p.parcaRengi.r, p.parcaRengi.g, p.parcaRengi.b, 0.28f));
        for (int y = 22; y < 106; y++)
        {
            int r = 42 - Mathf.Abs(64 - y);
            for (int x = 64 - r; x <= 64 + r; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                Color c = d < 12f ? Color.white : (d < 26f ? p.parcaRengi : p.zeminDamarRenk);
                t.SetPixel(x, y, c);
            }
        }
        t.Apply();
        return t;
    }

    private static Texture2D SamdanCiz(bool aktif)
    {
        Texture2D t = BosDoku(128, 128);
        Color pirinc = new Color(0.88f, 0.68f, 0.26f);
        Color pirincKoyu = new Color(0.54f, 0.38f, 0.12f);

        KutuDoldur(t, 34, 0, 93, 12, pirinc);
        KutuDoldur(t, 58, 12, 69, 84, pirinc);
        KutuDoldur(t, 30, 50, 97, 58, pirincKoyu);
        KutuDoldur(t, 30, 50, 38, 76, pirinc);
        KutuDoldur(t, 89, 50, 97, 76, pirinc);

        KutuDoldur(t, 30, 77, 38, 92, new Color(0.96f, 0.92f, 0.84f));
        KutuDoldur(t, 58, 85, 69, 102, new Color(0.96f, 0.92f, 0.84f));
        KutuDoldur(t, 89, 77, 97, 92, new Color(0.96f, 0.92f, 0.84f));

        if (aktif)
        {
            Color alev = new Color(0.32f, 0.92f, 1.0f);
            DaireDoldur(t, 34, 100, 7, alev);
            DaireDoldur(t, 64, 112, 10, alev);
            DaireDoldur(t, 64, 111, 5, Color.white);
            DaireDoldur(t, 93, 100, 7, alev);
        }
        t.Apply();
        return t;
    }

    private static Texture2D YayCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(128, 128);
        KutuDoldur(t, 12, 0, 115, 16, new Color(0.24f, 0.20f, 0.24f));
        KutuDoldur(t, 34, 17, 93, 28, p.zeminDamarRenk);
        KutuDoldur(t, 42, 29, 85, 40, p.zeminDamarRenk);
        KutuDoldur(t, 10, 41, 117, 56, new Color(0.95f, 0.44f, 0.18f));
        t.Apply();
        return t;
    }

    private static Texture2D YazitCiz(bool erdemMi)
    {
        Texture2D t = BosDoku(128, 128);
        Color tas = erdemMi ? new Color(0.24f, 0.38f, 0.46f) : new Color(0.34f, 0.22f, 0.16f);
        Color parilti = erdemMi ? new Color(0.42f, 0.94f, 1f) : new Color(0.98f, 0.62f, 0.24f);

        KutuDoldur(t, 22, 0, 105, 14, tas);
        KutuDoldur(t, 34, 15, 93, 106, tas);
        KutuDoldur(t, 42, 34, 85, 92, new Color(0.90f, 0.84f, 0.70f));
        KutuDoldur(t, 50, 46, 77, 51, parilti);
        KutuDoldur(t, 50, 60, 77, 65, parilti);
        KutuDoldur(t, 50, 74, 77, 79, parilti);
        t.Apply();
        return t;
    }

    private static Texture2D DikenCiz()
    {
        Texture2D t = BosDoku(128, 128);
        for (int i = 0; i < 4; i++)
        {
            int merkez = 16 + i * 32;
            for (int y = 0; y < 68; y++)
            {
                int w = (68 - y) / 5;
                KutuDoldur(t, merkez - w, y, merkez + w, y, new Color(0.90f, 0.20f, 0.18f));
            }
        }
        t.Apply();
        return t;
    }

    private static Texture2D KapiCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(192, 288);
        KutuDoldur(t, 12, 0, 179, 270, p.zeminDamarRenk);
        KutuDoldur(t, 32, 0, 159, 246, Color.Lerp(p.gokyuzuRengi, p.parcaRengi, 0.22f));
        KutuDoldur(t, 54, 18, 137, 218, Color.Lerp(p.parcaRengi, Color.white, 0.35f));
        t.Apply();
        return t;
    }

    // 256x192 Cerceveli Ziyafet Tablolari
    private static Texture2D TabloCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p, int varyant)
    {
        Texture2D t = BosDoku(256, 192);
        Color altinCerceve = new Color(0.86f, 0.66f, 0.24f);
        Color cerceveGolge = new Color(0.48f, 0.34f, 0.10f);

        KutuDoldur(t, 0, 0, 255, 191, altinCerceve);
        KutuDoldur(t, 8, 8, 247, 183, cerceveGolge);
        KutuDoldur(t, 16, 16, 239, 175, p.tabloTuvalRengi);

        if (varyant == 0)
        {
            // Iki kisilik sofra: sol taraf tasan yiyecekler ve altin kadehler, sag taraf bos sandalye
            KutuDoldur(t, 44, 48, 210, 58, altinCerceve);
            KutuDoldur(t, 58, 24, 66, 48, altinCerceve);
            KutuDoldur(t, 186, 24, 194, 48, altinCerceve);
            DaireDoldur(t, 84, 72, 18, p.parcaRengi);
            DaireDoldur(t, 108, 68, 14, new Color(0.85f, 0.24f, 0.28f));
            KutuDoldur(t, 194, 24, 204, 112, Color.Lerp(p.tabloTuvalRengi, Color.black, 0.45f));
            KutuDoldur(t, 172, 62, 198, 68, Color.Lerp(p.tabloTuvalRengi, Color.black, 0.45f));
        }
        else
        {
            // Dev tasan altin kadeh ve uzum salkimlari freski
            KutuDoldur(t, 102, 28, 152, 38, altinCerceve);
            KutuDoldur(t, 120, 38, 134, 84, altinCerceve);
            KutuDoldur(t, 84, 84, 170, 138, altinCerceve);
            DaireDoldur(t, 128, 136, 26, p.parcaRengi);
            DaireDoldur(t, 104, 130, 14, new Color(0.76f, 0.22f, 0.34f));
            DaireDoldur(t, 152, 130, 14, new Color(0.76f, 0.22f, 0.34f));
        }

        t.Apply();
        return t;
    }

    // 192x256 Esin Tul Perdeli Portresi
    private static Texture2D EsPortresiCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(192, 256);
        Color altinCerceve = new Color(0.88f, 0.68f, 0.26f);

        KutuDoldur(t, 0, 0, 191, 255, altinCerceve);
        KutuDoldur(t, 14, 14, 177, 241, new Color(0.09f, 0.07f, 0.13f));

        // Esin omuz, boyun ve bas portresi
        Color tenSiluet = Color.Lerp(p.parcaRengi, Color.white, 0.58f);
        Color sacGolge = Color.Lerp(tenSiluet, Color.black, 0.35f);

        DaireDoldur(t, 96, 154, 42, sacGolge);
        KutuDoldur(t, 44, 24, 147, 86, tenSiluet);
        KutuDoldur(t, 82, 86, 110, 118, tenSiluet);
        DaireDoldur(t, 96, 148, 30, tenSiluet);

        // Yari saydam tul duvak katmanlari
        Color tul = new Color(0.90f, 0.88f, 0.98f, 0.34f);
        for (int y = 36; y < 208; y += 3)
            KutuDoldur(t, 48, y, 143, y + 1, tul);

        t.Apply();
        return t;
    }

    // 512x256 Uzak Mahzen Kemerleri ve Mum Isigi Vitraylari
    private static Texture2D ParallaxUzakCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(512, 256);
        Color arka = Color.Lerp(p.gokyuzuRengi, Color.black, 0.22f);
        Color kemer = Color.Lerp(p.gokyuzuRengi, p.zeminAnaRenk, 0.35f);
        Color vitray = Color.Lerp(p.gokyuzuRengi, p.zeminDamarRenk, 0.32f);

        KutuDoldur(t, 0, 0, 511, 255, arka);
        for (int k = 0; k < 2; k++)
        {
            int x0 = 64 + k * 256;
            KutuDoldur(t, x0, 48, x0 + 116, 224, vitray);
            DaireDoldur(t, x0 + 58, 136, 48, new Color(1f, 0.78f, 0.36f, 0.22f));
            KutuDoldur(t, x0 + 54, 48, x0 + 62, 224, kemer);
            KutuDoldur(t, x0, 132, x0 + 116, 140, kemer);
        }
        t.Apply();
        return t;
    }

    // 512x256 Orta Katman Sutunlar, Ziyafet Masalari ve Avizeler
    private static Texture2D ParallaxOrtaCiz(SevenVeilsGorselFabrikasi.BolumTemaPaleti p)
    {
        Texture2D t = BosDoku(512, 256);
        Color sutun = Color.Lerp(p.gokyuzuRengi, p.zeminAnaRenk, 0.52f);
        Color ahsapKoyu = Color.Lerp(p.zeminAnaRenk, Color.black, 0.45f);
        Color mum = new Color(1f, 0.84f, 0.40f, 0.85f);

        // Sutunlar
        KutuDoldur(t, 28, 0, 68, 255, sutun);
        KutuDoldur(t, 284, 0, 324, 255, sutun);

        // Arka planda ziyafet masasi ve dev sarap/bal ficisi siluetleri
        KutuDoldur(t, 108, 0, 240, 36, ahsapKoyu);
        KutuDoldur(t, 118, 36, 230, 44, sutun);
        DaireDoldur(t, 404, 34, 34, ahsapKoyu);

        // Tavandan sarkan avizeler ve yanan mumlar
        for (int a = 0; a < 2; a++)
        {
            int cx = 174 + a * 256;
            for (int y = 148; y < 256; y += 6)
                KutuDoldur(t, cx - 2, y, cx + 2, y + 3, ahsapKoyu);

            KutuDoldur(t, cx - 34, 140, cx + 34, 148, ahsapKoyu);
            KutuDoldur(t, cx - 28, 148, cx - 24, 158, mum);
            KutuDoldur(t, cx - 2, 148, cx + 2, 160, mum);
            KutuDoldur(t, cx + 24, 148, cx + 28, 158, mum);
        }

        t.Apply();
        return t;
    }
}
#endif
