using UnityEngine;

public static class SevenVeilsGorselFabrikasi
{
    public struct BolumTemaPaleti
    {
        public Color gokyuzuRengi;
        public Color zeminAnaRenk;
        public Color zeminDamarRenk;
        public Color karakterRengi;
        public Color karakterGozRengi;
        public Color dusmanRengi;
        public Color parcaRengi;
        public Color tabloCerceveRengi;
        public Color tabloTuvalRengi;
        public float toplamaSesFrekansi;
    }

    public static BolumTemaPaleti TemaGetir(int bolumNo)
    {
        switch (Mathf.Clamp(bolumNo, 1, 7))
        {
            case 1: // Oburluk
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.20f, 0.13f, 0.08f),
                    zeminAnaRenk = new Color(0.58f, 0.38f, 0.18f),
                    zeminDamarRenk = new Color(0.84f, 0.62f, 0.26f),
                    karakterRengi = new Color(0.96f, 0.94f, 0.88f),
                    karakterGozRengi = new Color(0.35f, 0.78f, 0.95f),
                    dusmanRengi = new Color(0.45f, 0.22f, 0.14f),
                    parcaRengi = new Color(1.00f, 0.82f, 0.28f),
                    tabloCerceveRengi = new Color(0.85f, 0.65f, 0.22f),
                    tabloTuvalRengi = new Color(0.36f, 0.24f, 0.16f),
                    toplamaSesFrekansi = 523.25f
                };
            case 2: // Tembellik
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.11f, 0.17f, 0.24f),
                    zeminAnaRenk = new Color(0.26f, 0.38f, 0.48f),
                    zeminDamarRenk = new Color(0.48f, 0.66f, 0.76f),
                    karakterRengi = new Color(0.84f, 0.88f, 0.94f),
                    karakterGozRengi = new Color(0.55f, 0.85f, 1.00f),
                    dusmanRengi = new Color(0.20f, 0.30f, 0.42f),
                    parcaRengi = new Color(0.52f, 0.90f, 1.00f),
                    tabloCerceveRengi = new Color(0.46f, 0.58f, 0.66f),
                    tabloTuvalRengi = new Color(0.18f, 0.26f, 0.34f),
                    toplamaSesFrekansi = 440.00f
                };
            case 3: // Sehvet
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.22f, 0.08f, 0.18f),
                    zeminAnaRenk = new Color(0.46f, 0.16f, 0.34f),
                    zeminDamarRenk = new Color(0.82f, 0.34f, 0.62f),
                    karakterRengi = new Color(0.76f, 0.66f, 0.78f),
                    karakterGozRengi = new Color(0.96f, 0.42f, 0.74f),
                    dusmanRengi = new Color(0.62f, 0.14f, 0.36f),
                    parcaRengi = new Color(0.98f, 0.52f, 0.84f),
                    tabloCerceveRengi = new Color(0.76f, 0.36f, 0.56f),
                    tabloTuvalRengi = new Color(0.30f, 0.12f, 0.24f),
                    toplamaSesFrekansi = 587.33f
                };
            case 4: // Acgozluluk
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.07f, 0.15f, 0.11f),
                    zeminAnaRenk = new Color(0.16f, 0.34f, 0.24f),
                    zeminDamarRenk = new Color(0.88f, 0.72f, 0.22f),
                    karakterRengi = new Color(0.66f, 0.68f, 0.62f),
                    karakterGozRengi = new Color(1.00f, 0.84f, 0.20f),
                    dusmanRengi = new Color(0.24f, 0.38f, 0.22f),
                    parcaRengi = new Color(1.00f, 0.88f, 0.18f),
                    tabloCerceveRengi = new Color(0.86f, 0.68f, 0.18f),
                    tabloTuvalRengi = new Color(0.12f, 0.22f, 0.16f),
                    toplamaSesFrekansi = 659.25f
                };
            case 5: // Kibir
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.16f, 0.18f, 0.24f),
                    zeminAnaRenk = new Color(0.56f, 0.60f, 0.68f),
                    zeminDamarRenk = new Color(0.88f, 0.90f, 0.96f),
                    karakterRengi = new Color(0.52f, 0.54f, 0.62f),
                    karakterGozRengi = new Color(0.82f, 0.88f, 1.00f),
                    dusmanRengi = new Color(0.36f, 0.40f, 0.52f),
                    parcaRengi = new Color(0.90f, 0.94f, 1.00f),
                    tabloCerceveRengi = new Color(0.75f, 0.78f, 0.86f),
                    tabloTuvalRengi = new Color(0.24f, 0.26f, 0.34f),
                    toplamaSesFrekansi = 698.46f
                };
            case 6: // Kiskanclik
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.06f, 0.16f, 0.16f),
                    zeminAnaRenk = new Color(0.14f, 0.38f, 0.36f),
                    zeminDamarRenk = new Color(0.32f, 0.88f, 0.76f),
                    karakterRengi = new Color(0.38f, 0.46f, 0.46f),
                    karakterGozRengi = new Color(0.25f, 1.00f, 0.75f),
                    dusmanRengi = new Color(0.12f, 0.46f, 0.38f),
                    parcaRengi = new Color(0.36f, 1.00f, 0.84f),
                    tabloCerceveRengi = new Color(0.24f, 0.64f, 0.56f),
                    tabloTuvalRengi = new Color(0.08f, 0.22f, 0.22f),
                    toplamaSesFrekansi = 493.88f
                };
            default: // 7. Ofke (CEHENNEM)
                return new BolumTemaPaleti
                {
                    gokyuzuRengi = new Color(0.14f, 0.03f, 0.03f),
                    zeminAnaRenk = new Color(0.18f, 0.12f, 0.14f),
                    zeminDamarRenk = new Color(0.98f, 0.32f, 0.10f),
                    karakterRengi = new Color(0.16f, 0.12f, 0.14f),
                    karakterGozRengi = new Color(1.00f, 0.28f, 0.08f),
                    dusmanRengi = new Color(0.78f, 0.14f, 0.08f),
                    parcaRengi = new Color(1.00f, 0.45f, 0.12f),
                    tabloCerceveRengi = new Color(0.45f, 0.14f, 0.08f),
                    tabloTuvalRengi = new Color(0.10f, 0.04f, 0.04f),
                    toplamaSesFrekansi = 392.00f
                };
        }
    }

    public static Sprite PlatformSpriteOlustur(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < boyut; y++)
        {
            for (int x = 0; x < boyut; x++)
            {
                Color c = p.zeminAnaRenk;

                // Ust yuzey parlak seridi
                if (y >= boyut - 3)
                    c = Color.Lerp(p.zeminAnaRenk, p.zeminDamarRenk, 0.75f);
                // Bolume ozel doku desenleri
                else if (bolumNo == 1 && (y % 8 == 0 || (x + (y / 8) * 6) % 16 == 0))
                    c = Color.Lerp(p.zeminAnaRenk, Color.black, 0.25f); // Yontma tas tugla
                else if (bolumNo == 2 && (y > 22 && (x % 4 == 0)))
                    c = Color.Lerp(p.zeminAnaRenk, new Color(0.28f, 0.56f, 0.42f), 0.55f); // Yosunlu kemer
                else if (bolumNo == 3 && ((x + y) % 8 == 0))
                    c = Color.Lerp(p.zeminAnaRenk, p.zeminDamarRenk, 0.35f); // Aynali karo
                else if (bolumNo == 4 && ((x == 4 || x == 27) || (y == 15)))
                    c = p.zeminDamarRenk; // Altin isleme
                else if (bolumNo == 5 && (x % 6 == 0))
                    c = Color.Lerp(p.zeminAnaRenk, p.zeminDamarRenk, 0.30f); // Mermer yiv
                else if (bolumNo == 6 && (Mathf.Abs(x - y) == 4 || Mathf.Abs((31 - x) - y) == 6))
                    c = p.zeminDamarRenk; // Kirik ayna catlagi
                else if (bolumNo == 7 && ((x * 3 + y * 5) % 17 == 0 || y < 3))
                    c = p.zeminDamarRenk; // Cehennem lav catlaklari

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite KarakterSpriteOlustur(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        Color seffaf = new Color(0, 0, 0, 0);
        for (int y = 0; y < boyut; y++)
            for (int x = 0; x < boyut; x++)
                tex.SetPixel(x, y, seffaf);

        // Govde ve pelerin silueti
        for (int y = 2; y < 22; y++)
        {
            int genislik = y < 10 ? 12 - (y / 3) : 8;
            for (int x = 16 - genislik; x <= 16 + genislik; x++)
            {
                if (x >= 0 && x < boyut)
                    tex.SetPixel(x, y, p.karakterRengi);
            }
        }

        // Bas
        for (int y = 21; y < 30; y++)
        {
            for (int x = 11; x <= 21; x++)
            {
                tex.SetPixel(x, y, p.karakterRengi);
            }
        }

        // Parlayan gozler (7. bolumde kor kirmizi)
        tex.SetPixel(18, 25, p.karakterGozRengi);
        tex.SetPixel(19, 25, p.karakterGozRengi);
        tex.SetPixel(18, 26, p.karakterGozRengi);

        // 7. bolumde boynuz/alev silueti
        if (bolumNo >= 6)
        {
            tex.SetPixel(11, 30, p.karakterGozRengi);
            tex.SetPixel(10, 31, p.karakterGozRengi);
            tex.SetPixel(21, 30, p.karakterGozRengi);
            tex.SetPixel(22, 31, p.karakterGozRengi);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite DusmanSpriteOlustur(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        Color seffaf = new Color(0, 0, 0, 0);
        for (int y = 0; y < boyut; y++)
            for (int x = 0; x < boyut; x++)
                tex.SetPixel(x, y, seffaf);

        for (int y = 2; y < 24; y++)
        {
            for (int x = 5; x < 27; x++)
            {
                tex.SetPixel(x, y, p.dusmanRengi);
            }
        }

        // Gozler
        tex.SetPixel(10, 15, Color.white);
        tex.SetPixel(21, 15, Color.white);

        // 7. bolumde tepede kor dikenler (ezilemez)
        if (bolumNo == 7)
        {
            for (int x = 6; x < 26; x += 4)
            {
                tex.SetPixel(x, 24, p.zeminDamarRenk);
                tex.SetPixel(x + 1, 26, p.zeminDamarRenk);
                tex.SetPixel(x, 28, Color.yellow);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite ParcaSpriteOlustur(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        Color seffaf = new Color(0, 0, 0, 0);
        for (int y = 0; y < boyut; y++)
            for (int x = 0; x < boyut; x++)
                tex.SetPixel(x, y, seffaf);

        // Elmas / kristal formu
        for (int y = 4; y < 28; y++)
        {
            int yaricap = 12 - Mathf.Abs(16 - y);
            for (int x = 16 - yaricap; x <= 16 + yaricap; x++)
            {
                Color c = Mathf.Abs(16 - x) < 3 ? Color.white : p.parcaRengi;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite KapiSpriteOlustur(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < boyut; y++)
        {
            for (int x = 0; x < boyut; x++)
            {
                bool cerceve = x < 4 || x >= 28 || y >= 28;
                if (bolumNo == 7 && y >= 24 && (x % 4 == 0))
                    cerceve = true; // Cehennem disli kemer

                Color c = cerceve ? p.zeminDamarRenk : Color.Lerp(p.gokyuzuRengi, p.parcaRengi, 0.25f);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite PortreVeyaTabloOlustur(int bolumNo, bool esPortresiMi, int tabloVaryant = 0)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int boyut = 32;
        Texture2D tex = new Texture2D(boyut, boyut);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < boyut; y++)
        {
            for (int x = 0; x < boyut; x++)
            {
                bool cerceve = x < 3 || x >= 29 || y < 3 || y >= 29;
                if (cerceve)
                {
                    tex.SetPixel(x, y, p.tabloCerceveRengi);
                    continue;
                }

                Color c = p.tabloTuvalRengi;

                if (esPortresiMi)
                {
                    // Merkezde esin silueti (bolum ilerledikce carpilir)
                    int kayma = bolumNo >= 4 ? ((y / 4) % 2 == 0 ? 1 : -1) * (bolumNo - 3) : 0;
                    int dx = Mathf.Abs((x + kayma) - 16);
                    if ((y >= 6 && y <= 16 && dx <= 7) || (y >= 17 && y <= 24 && dx <= 4))
                        c = Color.Lerp(p.parcaRengi, Color.white, 0.45f);
                    if (bolumNo >= 6 && Mathf.Abs(x - y) <= 1)
                        c = p.zeminDamarRenk; // Kirik / yanmis catlak
                }
                else
                {
                    // Arka plan sembolik sanatci tablosu
                    if (tabloVaryant % 2 == 0 && y >= 8 && y <= 18 && x >= 8 && x <= 23)
                        c = Color.Lerp(p.tabloTuvalRengi, p.zeminDamarRenk, 0.45f);
                    else if (tabloVaryant % 2 == 1 && (x + y) % 7 == 0)
                        c = Color.Lerp(p.tabloTuvalRengi, p.parcaRengi, 0.35f);
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), boyut);
    }

    public static Sprite DuzKareSpriteOlustur()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    public static AudioClip ToplamaSesiUret(int bolumNo)
    {
        BolumTemaPaleti p = TemaGetir(bolumNo);
        int ornekHizi = 22050;
        int ornekSayisi = Mathf.RoundToInt(ornekHizi * 0.16f);
        float[] veri = new float[ornekSayisi];

        for (int i = 0; i < ornekSayisi; i++)
        {
            float t = (float)i / ornekHizi;
            float zarf = 1f - ((float)i / ornekSayisi);
            float frekans = p.toplamaSesFrekansi * (1f + t * 2.2f);
            veri[i] = Mathf.Sin(2f * Mathf.PI * frekans * t) * zarf * 0.25f;
        }

        AudioClip klip = AudioClip.Create($"Collect_Veil_{bolumNo}", ornekSayisi, 1, ornekHizi, false);
        klip.SetData(veri, 0);
        return klip;
    }
}
