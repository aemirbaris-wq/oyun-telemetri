using UnityEngine;

public static class EsPortresiOlusturucu
{
    public static GameObject Olustur(int bolumNo, Vector3 zeminPos, Transform parent = null)
    {
        int b = Mathf.Clamp(bolumNo, 1, 7);
        GameObject portreObj = new GameObject($"EsininSiluetPortresi_Bolum{b}");
        if (parent != null) portreObj.transform.SetParent(parent, false);
        portreObj.transform.position = zeminPos + new Vector3(0f, 0.05f, 0f);

        int w = 44, h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color[] cerceveRenkler =
        {
            new Color(0.88f, 0.72f, 0.34f, 1f), // 1. Oburluk: Sicak altin
            new Color(0.46f, 0.62f, 0.74f, 1f), // 2. Tembellik: Soluk sis mavisi
            new Color(0.66f, 0.26f, 0.48f, 1f), // 3. Sehvet: Koyu bordo
            new Color(0.32f, 0.54f, 0.38f, 1f), // 4. Acgozluluk: Pasli zumrut
            new Color(0.74f, 0.78f, 0.84f, 1f), // 5. Kibir: Soguk gumus
            new Color(0.22f, 0.56f, 0.52f, 1f), // 6. Kiskanclik: Zehir turkuaz
            new Color(0.58f, 0.14f, 0.12f, 1f)  // 7. Ofke: Yanmis kor kirmizisi
        };

        Color[] haleRenkler =
        {
            new Color(0.96f, 0.90f, 0.72f, 0.40f),
            new Color(0.55f, 0.72f, 0.86f, 0.28f),
            new Color(0.78f, 0.32f, 0.58f, 0.32f),
            new Color(0.42f, 0.68f, 0.44f, 0.25f),
            new Color(0.85f, 0.88f, 0.95f, 0.26f),
            new Color(0.28f, 0.74f, 0.66f, 0.28f),
            new Color(0.88f, 0.22f, 0.14f, 0.34f)
        };

        Color cerceve = cerceveRenkler[b - 1];
        Color hale = haleRenkler[b - 1];
        Color icArkaPlan = Color.Lerp(new Color(0.11f, 0.13f, 0.19f, 1f), new Color(0.04f, 0.03f, 0.05f, 1f), (b - 1) / 6f);
        Color siluetKoyu = new Color(0.03f, 0.03f, 0.05f, 1f);
        Color siluetKenar = Color.Lerp(new Color(0.95f, 0.86f, 0.62f, 0.90f), new Color(0.92f, 0.26f, 0.20f, 0.95f), (b - 1) / 6f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool kenarCerceve = x < 3 || x >= w - 3 || y < 3 || y >= h - 3;
                if (kenarCerceve)
                {
                    // Bolum 4+ cercevede catlak ve kiriklar baslar
                    bool cerceveKirik = (b >= 4 && ((x * 7 + y * 13) % (29 - b * 2) == 0)) ||
                                        (b == 7 && ((x < 3 && y > 48) || (x >= w - 3 && y < 18)));
                    tex.SetPixel(x, y, cerceveKirik ? icArkaPlan : cerceve);
                    continue;
                }

                // Bolum 6 (Kiskanclik): Ortadan kirik ayna kaymasi
                int ornekY = y;
                if (b == 6)
                {
                    ornekY += (x < w / 2) ? 3 : -3;
                }
                // Bolum 7 (Ofke): Dalgalanan ve carpilan siluet ekseni
                float merkezX = 22f + (b == 7 ? Mathf.Sin(y * 0.35f) * 1.8f : 0f);
                float kafaY = 43f + (b == 5 ? 2.5f : 0f);

                float kafaMesafe = Vector2.Distance(new Vector2(x, ornekY), new Vector2(merkezX, kafaY));
                bool kafa = kafaMesafe <= (b == 5 ? 5.6f : 6.2f);
                bool duvak = (ornekY >= 24 && ornekY <= 46 && Mathf.Abs(x - merkezX) <= (6.5f + (46 - ornekY) * 0.18f));
                bool omuz = (ornekY >= 6 && ornekY < 26 && Mathf.Abs(x - merkezX) <= (8f + (26 - ornekY) * (b == 5 ? 0.24f : 0.32f)));

                // Bolum 5: Kirik tac silueti
                bool kirikTac = (b == 5 && ornekY >= 49 && ornekY <= 54 && Mathf.Abs(x - merkezX) <= 5f && ((x + ornekY) % 2 == 0));
                // Bolum 7: Golge dikenleri
                bool diken = (b == 7 && ornekY >= 14 && ornekY <= 44 && Mathf.Abs(x - merkezX) <= 11f && ((x * 3 + y) % 11 == 0));

                bool siluet = kafa || duvak || omuz || kirikTac || diken;

                // Bolum 4+: Siluet icinde dikey golge catlaklari
                if (siluet && b >= 4 && ((x == 20 && y % 4 != 0) || (b >= 6 && x == 24 && y % 3 != 0)))
                {
                    siluet = false;
                }

                if (siluet)
                {
                    // Bolum 7'de ince kor goz pariltisi
                    if (b == 7 && ornekY == 43 && (Mathf.Abs(x - (merkezX - 2f)) < 1.1f || Mathf.Abs(x - (merkezX + 2f)) < 1.1f))
                    {
                        tex.SetPixel(x, y, new Color(1f, 0.32f, 0.18f, 1f));
                    }
                    else
                    {
                        bool kenarParilti = (kafaMesafe > 5.0f && kafa) || (b == 2 && ((x + y) % 2 == 0) && kafaMesafe > 4.5f);
                        tex.SetPixel(x, y, kenarParilti ? siluetKenar : siluetKoyu);
                    }
                }
                else
                {
                    // Bolum 3 (Sehvet): Kaymis ikinci golge yankisi
                    bool ikinciGolge = (b == 3 && Vector2.Distance(new Vector2(x, y), new Vector2(26f, 41f)) <= 5.8f);
                    if (ikinciGolge)
                    {
                        tex.SetPixel(x, y, new Color(0.16f, 0.06f, 0.14f, 1f));
                    }
                    else
                    {
                        float haleMesafe = Vector2.Distance(new Vector2(x, y), new Vector2(22f, 38f));
                        tex.SetPixel(x, y, haleMesafe < 15f ? Color.Lerp(hale, icArkaPlan, haleMesafe / 15f) : icArkaPlan);
                    }
                }
            }
        }
        tex.Apply();

        SpriteRenderer sr = portreObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 26f, 0, SpriteMeshType.FullRect);
        sr.sortingOrder = 2;
        return portreObj;
    }
}
