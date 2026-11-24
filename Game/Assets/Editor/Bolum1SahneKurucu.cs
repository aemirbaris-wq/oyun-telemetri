using System.Collections.Generic;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Bolum1SahneKurucu
{
    private const string OTO_KURULUM_ANAHTARI = "Bolum1_OtoKurulum_v30";

    private struct PlatformTanimi
    {
        public int xBas;
        public int xBit;
        public int yBas;
        public int yBit;
        public float SolKenarX;
        public float SagKenarX;
        public float YuzeyY;
        public float MerkezX => (SolKenarX + SagKenarX) * 0.5f;

        public PlatformTanimi(Tilemap tm, int xb, int xbit, int yb, int ybit)
        {
            xBas = xb; xBit = xbit; yBas = yb; yBit = ybit;
            SolKenarX = tm.CellToWorld(new Vector3Int(xb, 0, 0)).x;
            SagKenarX = tm.CellToWorld(new Vector3Int(xbit + 1, 0, 0)).x;
            YuzeyY = tm.CellToWorld(new Vector3Int(0, ybit + 1, 0)).y;
        }
    }

    [InitializeOnLoadMethod]
    private static void DerlemeSonrasiTekSeferKur()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool(OTO_KURULUM_ANAHTARI, false)) return;

        SessionState.SetBool(OTO_KURULUM_ANAHTARI, true);
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Bolum1Kur();
            }
        };
    }

    [MenuItem("Yedi Perde (Tez)/1. Bolum Sahnesini Kur (Bolum1)")]
    public static void Bolum1Kur()
    {
        string samplePath = "Assets/Scenes/SampleScene.unity";
        string bolum1Path = "Assets/Scenes/Bolum1.unity";

        if (!System.IO.File.Exists(samplePath))
        {
            Debug.LogError("[Bolum1Kurucu] Assets/Scenes/SampleScene.unity bulunamadi!");
            return;
        }

        AssetDatabase.CopyAsset(samplePath, bolum1Path);
        AssetDatabase.Refresh();

        Scene sahne = EditorSceneManager.OpenScene(bolum1Path, OpenSceneMode.Single);

        Tilemap[] tumTilemapler = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        Tilemap zeminTilemap = null;
        TileBase zeminTile = null;

        foreach (var tm in tumTilemapler)
        {
            if (tm.GetComponent<TilemapCollider2D>() != null)
            {
                zeminTilemap = tm;
                break;
            }
        }
        if (zeminTilemap == null && tumTilemapler.Length > 0) zeminTilemap = tumTilemapler[0];

        if (zeminTilemap != null) zeminTile = IlkDoluKaroyuBul(zeminTilemap);
        if (zeminTile == null)
        {
            foreach (var tm in tumTilemapler)
            {
                zeminTile = IlkDoluKaroyuBul(tm);
                if (zeminTile != null) break;
            }
        }
        if (zeminTile == null)
        {
            string[] guidler = AssetDatabase.FindAssets("t:TileBase");
            if (guidler.Length > 0)
            {
                zeminTile = AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(guidler[0]));
            }
        }

        foreach (var tm in tumTilemapler) tm.ClearAllTiles();

        if (zeminTilemap == null || zeminTile == null)
        {
            Debug.LogError("[Bolum1Kurucu] Tilemap veya TileBase bulunamadi!");
            return;
        }

        if (zeminTilemap.transform.parent != null)
        {
            zeminTilemap.transform.parent.position = Vector3.zero;
            zeminTilemap.transform.parent.localScale = Vector3.one;
        }
        zeminTilemap.transform.localPosition = Vector3.zero;
        zeminTilemap.transform.localScale = Vector3.one;

        TilemapCollider2D tmCol = zeminTilemap.GetComponent<TilemapCollider2D>();
        if (tmCol == null) tmCol = zeminTilemap.gameObject.AddComponent<TilemapCollider2D>();
        tmCol.enabled = true;
        tmCol.isTrigger = false;

        int zeminLayer = zeminTilemap.gameObject.layer;

        // =========================================================================
        // CESITLENDIRILMIS 5 ASAMALI BOLUM MIMARISI
        // =========================================================================
        // BOLGE I: Uyanis Avlusu -> Ruzgar Koprusu (Hareketli Platform gecisi)
        PlatformTanimi pBaslangic     = new PlatformTanimi(zeminTilemap, 0, 14, -3, -1);     // Baslangic avlusu
        PlatformTanimi pAda1          = new PlatformTanimi(zeminTilemap, 19, 25, -2, 0);     // Ilk sicrama adasi
        // (x: 26..37 arasinda yatay hareketli tas kopru!)
        PlatformTanimi pMuhafizKopru1 = new PlatformTanimi(zeminTilemap, 38, 50, -1, 1);     // Muhafiz 1

        // BOLGE II: Catlak Kemerler (Alt yolda kirilabilir taslar, ustte yuksek sutun rotasi)
        PlatformTanimi pAlt1          = new PlatformTanimi(zeminTilemap, 55, 65, -2, 0);     // Muhafiz 2
        // (x: 66..80 arasinda 2 adet catlak coken tas platform!)
        PlatformTanimi pAlt2          = new PlatformTanimi(zeminTilemap, 82, 94, -1, 1);     // Muhafiz 3

        // Ust Harabe Sutunlari (Gizli odaya giden zorlu yuksek yolun ilk adimlari)
        PlatformTanimi pUst1          = new PlatformTanimi(zeminTilemap, 53, 59, 2, 2);
        PlatformTanimi pUst2          = new PlatformTanimi(zeminTilemap, 64, 71, 3, 3);      // Muhafiz 4
        PlatformTanimi pUst3          = new PlatformTanimi(zeminTilemap, 76, 83, 4, 4);

        // BOLGE III: Yanki Kuyusu & TEK GUNAH YAZITI
        PlatformTanimi pKuyuGiris     = new PlatformTanimi(zeminTilemap, 99, 108, 0, 2);     // Muhafiz 5
        // (x: 110..114 dikey asansor tasi -> y=3'ten y=6'ya cikarir)
        PlatformTanimi pGozlemKulesi  = new PlatformTanimi(zeminTilemap, 117, 132, 3, 5);    // TEK GUNAH YAZITI (Ana rota) + Muhafiz 6

        // BOLGE IV: Golgeli Ucurum (Alt Ana Yol) vs. GIZLI GECIT MABEDI (Ust Gizli Oda)
        // Alt Ana Yol:
        PlatformTanimi pAltGecit1     = new PlatformTanimi(zeminTilemap, 137, 148, 1, 3);    // Muhafiz 7
        // (x: 149..162 arasinda coken tas + yatay hareketli kopru!)
        PlatformTanimi pAltGecit2     = new PlatformTanimi(zeminTilemap, 164, 177, 1, 3);    // Muhafiz 8

        // Ust Gizli Rota (pGozlemKulesi'nden geri/yukari sicrayarak ulasilir, sonunda sahte duvar var!):
        PlatformTanimi pGizliOnBalkon = new PlatformTanimi(zeminTilemap, 137, 144, 6, 6);    // Zorlu yuksek cikinti
        // (x: 145..149 arasinda tek coken tas!)
        PlatformTanimi pGizliEsik     = new PlatformTanimi(zeminTilemap, 151, 155, 7, 7);    // Sahte duvarin hemen onu
        PlatformTanimi pGizliMabet   = new PlatformTanimi(zeminTilemap, 156, 174, 7, 7);    // GIZLI ODA: Esinin Siluet Portresi + 1 Erdem Sunagi

        // BOLGE V: Muhur Merdivenleri & Final Tapinak Kapisi
        PlatformTanimi pSonBasamak1   = new PlatformTanimi(zeminTilemap, 182, 192, 2, 4);    // Muhafiz 9
        // (x: 193..203 arasinda yatay hareketli kopru!)
        PlatformTanimi pFinalMeydan   = new PlatformTanimi(zeminTilemap, 205, 224, 3, 5);    // Muhafiz 10 + Bitis Kapisi

        PlatformTanimi[] tumPlatformlar = new[]
        {
            pBaslangic, pAda1, pMuhafizKopru1,
            pAlt1, pAlt2, pUst1, pUst2, pUst3,
            pKuyuGiris, pGozlemKulesi,
            pAltGecit1, pAltGecit2,
            pGizliOnBalkon, pGizliEsik, pGizliMabet,
            pSonBasamak1, pFinalMeydan
        };

        // Sinir duvarlari ve Gizli Mabedin ust/arka tas cercevesi (On girisi acik, oraya GizliGecitDuvari konacak!)
        BlokCiz(zeminTilemap, zeminTile, -4, -1, -3, 10);
        BlokCiz(zeminTilemap, zeminTile, 225, 228, 3, 13);
        BlokCiz(zeminTilemap, zeminTile, 155, 175, 13, 14); // Gizli mabet tavani
        BlokCiz(zeminTilemap, zeminTile, 174, 175, 8, 13);  // Gizli mabet sag arka duvari

        foreach (var p in tumPlatformlar)
        {
            BlokCiz(zeminTilemap, zeminTile, p.xBas, p.xBit, p.yBas, p.yBit);
        }

        zeminTilemap.RefreshAllTiles();
        CompositeCollider2D compCollider = zeminTilemap.GetComponent<CompositeCollider2D>();
        if (compCollider != null)
        {
            compCollider.isTrigger = false;
            compCollider.GenerateGeometry();
        }
        Physics2D.SyncTransforms();

        // 2. OZEL PLATFORMLAR: Hareketli Kopruler, Asansorler, Catlak Coken Taslar ve Gizli Gecit Duvari
        GameObject ozelKlasor = new GameObject("Bolum1_OzelPlatformlar");

        // Bolge I Yatay Hareketli Tas Kopru
        Vector3 kopru1Bas = zeminTilemap.CellToWorld(new Vector3Int(27, 0, 0)) + new Vector3(0.5f, 0.35f, 0f);
        Vector3 kopru1Son = zeminTilemap.CellToWorld(new Vector3Int(35, 0, 0)) + new Vector3(0.5f, 0.35f, 0f);
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_1", kopru1Bas, kopru1Son - kopru1Bas, 2.4f, 2.4f, zeminLayer);

        // Bolge II Catlak Coken Taslar (Alt gecit)
        Vector3 coken1Pos = zeminTilemap.CellToWorld(new Vector3Int(69, 0, 0)) + new Vector3(0.75f, 0.35f, 0f);
        Vector3 coken2Pos = zeminTilemap.CellToWorld(new Vector3Int(76, 0, 0)) + new Vector3(0.75f, 0.35f, 0f);
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_1", coken1Pos, 2.1f, zeminLayer);
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_2", coken2Pos, 2.1f, zeminLayer);

        // Bolge III Dikey Asansor Tasi (Yanki Kuyusu)
        Vector3 asansorBas = zeminTilemap.CellToWorld(new Vector3Int(111, 2, 0)) + new Vector3(0.5f, 0.35f, 0f);
        Vector3 asansorSon = zeminTilemap.CellToWorld(new Vector3Int(111, 5, 0)) + new Vector3(0.5f, 0.35f, 0f);
        HareketliPlatformOlustur(ozelKlasor.transform, "DikeyAsansor_1", asansorBas, asansorSon - asansorBas, 2.0f, 2.2f, zeminLayer);

        // Bolge IV Alt Gecit: Coken Tas + Yatay Hareketli Kopru
        Vector3 coken3Pos = zeminTilemap.CellToWorld(new Vector3Int(151, 3, 0)) + new Vector3(0.75f, 0.35f, 0f);
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_3", coken3Pos, 2.0f, zeminLayer);

        Vector3 kopru2Bas = zeminTilemap.CellToWorld(new Vector3Int(155, 3, 0)) + new Vector3(0.5f, 0.35f, 0f);
        Vector3 kopru2Son = zeminTilemap.CellToWorld(new Vector3Int(161, 3, 0)) + new Vector3(0.5f, 0.35f, 0f);
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_2", kopru2Bas, kopru2Son - kopru2Bas, 2.3f, 2.2f, zeminLayer);

        // Gizli Oda Onundeki Coken Tuzak Tas
        Vector3 gizliCokenPos = zeminTilemap.CellToWorld(new Vector3Int(147, 7, 0)) + new Vector3(0.75f, 0.35f, 0f);
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_GizliYol", gizliCokenPos, 1.8f, zeminLayer);

        // Bolge V Final Oncesi Hareketli Kopru
        Vector3 kopru3Bas = zeminTilemap.CellToWorld(new Vector3Int(194, 4, 0)) + new Vector3(0.5f, 0.35f, 0f);
        Vector3 kopru3Son = zeminTilemap.CellToWorld(new Vector3Int(202, 5, 0)) + new Vector3(0.5f, 0.35f, 0f);
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_3", kopru3Bas, kopru3Son - kopru3Bas, 2.6f, 2.3f, zeminLayer);

        // GIZLI GECIT ILLUZYON DUVARI (Gizli mabedin girisini disaridan duz kaya duvari gibi kapatir!)
        GizliGecitDuvariOlustur(ozelKlasor.transform, zeminTilemap, 155, 156, 8, 12);

        // 3. ARKA PLAN
        GameObject bg = GameObject.Find("Background");
        if (bg != null)
        {
            bg.SetActive(true);
            bg.transform.position = new Vector3(10f, 1.5f, bg.transform.position.z);
        }

        // 4. JETONLAR (Gizli gecide bariz ok cikarmadan ana ve ust rotaya dengeli dagitim)
        List<Vector3> jetonNoktalari = new List<Vector3>();
        KavisJetonEkle(jetonNoktalari, pBaslangic, pAda1);
        YuzeyJetonEkle(jetonNoktalari, pAda1, 0.5f);
        YuzeyJetonEkle(jetonNoktalari, pUst1, 0.5f);
        YuzeyJetonEkle(jetonNoktalari, pUst3, 0.5f);
        KavisJetonEkle(jetonNoktalari, pUst1, pUst2);
        KavisJetonEkle(jetonNoktalari, pUst2, pUst3);
        YuzeyJetonEkle(jetonNoktalari, pGizliOnBalkon, 0.5f);
        YuzeyJetonEkle(jetonNoktalari, pAltGecit1, 0.5f);
        YuzeyJetonEkle(jetonNoktalari, pSonBasamak1, 0.5f);
        JetonlariKur(jetonNoktalari);

        // 5. AKTIF KOPRU MUHAFIZLARI (10 Devriye Dusman)
        (string ad, PlatformTanimi plat)[] dusmanPlan = new[]
        {
            ("GolgeMuhafiz_01", pMuhafizKopru1),
            ("GolgeMuhafiz_02", pAlt1),
            ("GolgeMuhafiz_03", pAlt2),
            ("GolgeMuhafiz_04", pUst2),
            ("GolgeMuhafiz_05", pKuyuGiris),
            ("GolgeMuhafiz_06", pGozlemKulesi),
            ("GolgeMuhafiz_07", pAltGecit1),
            ("GolgeMuhafiz_08", pAltGecit2),
            ("GolgeMuhafiz_09", pSonBasamak1),
            ("GolgeMuhafiz_10", pFinalMeydan)
        };
        DusmanlariKur(dusmanPlan);

        // 6. HIKAYE ANLATIMI: TAM 1 GUNAH YAZITI (Ana Yol) + 1 GIZLI ERDEM SUNAGI & ESININ SILUET PORTRESI
        foreach (var eski in Object.FindObjectsByType<AnlatimNesnesi>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(eski.gameObject);
        }

        // A) ANA ROTADAKI TEK GUNAH YAZITI (Oyuncularin cogunlugunun karsilasacagi yazit)
        float gunahX = Mathf.Lerp(pGozlemKulesi.SolKenarX, pGozlemKulesi.SagKenarX, 0.65f);
        float gunahY = ZeminYuzeyiBul(gunahX, pGozlemKulesi.YuzeyY);
        TabletEkle("GunahTableti_Bolum1", new Vector3(gunahX, gunahY, 0f), AnlatimNesnesi.NesneTuru.GunahTableti,
            "bolum1_gunah_1", 1,
            "BÖLÜM I — AĞIRLAŞAN GÖLGE",
            "Adımlarım her geçen an daha da ağırlaşıyor. Yukarıdaki ışık çoktan geride kaldı; yine de durmayacağım, onu bu karanlıktan çekip alacağım.",
            "CHAPTER I — THE HEAVY SHADOW",
            "My steps grow heavier with every passing moment. The light above is long behind me; still I will not stop, I will pull her out of this dark.");

        // B) GIZLI ODADAKI ESININ SILUET PORTRESI VE YANINDAKI TEK ERDEM SUNAGI
        float portreX = Mathf.Lerp(pGizliMabet.SolKenarX, pGizliMabet.SagKenarX, 0.42f);
        float portreY = ZeminYuzeyiBul(portreX, pGizliMabet.YuzeyY);
        EsininSiluetPortresiOlustur(new Vector3(portreX, portreY, 0f));

        float erdemX = portreX + 1.85f;
        float erdemY = ZeminYuzeyiBul(erdemX, pGizliMabet.YuzeyY);
        TabletEkle("ErdemSunagi_Bolum1", new Vector3(erdemX, erdemY, 0f), AnlatimNesnesi.NesneTuru.ErdemSunagi,
            "bolum1_erdem_1", 1,
            "I. ERDEM — UYANIŞ VE ÇABA",
            "\"Beni karanlığın içinde aramaktan vazgeç sevgilim... Bu gölgeler beni hapsetmiyor; senin kendi ağırlığınla yüzleşmen için bekliyor.\"",
            "I. VIRTUE — AWAKENING & EFFORT",
            "\"Stop searching for me inside the dark, my love... These shadows do not imprison me; they wait for you to face your own weight.\"");

        // 7. BASLANGIC NOKTASI VE OYUNCU
        float spawnX = zeminTilemap.CellToWorld(new Vector3Int(4, 0, 0)).x;
        float gercekZeminY = ZeminYuzeyiBul(spawnX, pBaslangic.YuzeyY);
        Vector3 baslangicPos = new Vector3(spawnX, gercekZeminY + 0.55f, 0f);

        GameObject spawnObj = GameObject.Find("SpawnPoint");
        if (spawnObj == null) spawnObj = new GameObject("SpawnPoint");
        spawnObj.transform.position = baslangicPos;

        GameController gc = Object.FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            SerializedObject so = new SerializedObject(gc);
            SerializedProperty spProp = so.FindProperty("model.spawnPoint");
            if (spProp != null)
            {
                spProp.objectReferenceValue = spawnObj.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player != null)
        {
            player.gameObject.SetActive(true);
            player.transform.position = baslangicPos;
            var sr = player.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) sr.enabled = true;
        }

        // 8. BOLUM SONU KAPISI
        VictoryZone victory = Object.FindFirstObjectByType<VictoryZone>(FindObjectsInactive.Include);
        if (victory == null)
        {
            GameObject vObj = new GameObject("VictoryZone");
            victory = vObj.AddComponent<VictoryZone>();
        }
        float kapiX = pFinalMeydan.SagKenarX - 2.0f;
        float kapiY = ZeminYuzeyiBul(kapiX, pFinalMeydan.YuzeyY);
        victory.gameObject.SetActive(true);
        victory.transform.SetParent(null);
        victory.transform.position = new Vector3(kapiX, kapiY, 0f);
        victory.transform.localScale = Vector3.one;

        BoxCollider2D vBox = victory.GetComponent<BoxCollider2D>();
        if (vBox == null) vBox = victory.gameObject.AddComponent<BoxCollider2D>();
        vBox.isTrigger = true;
        vBox.offset = new Vector2(0f, 1.5f);
        vBox.size = new Vector2(3.0f, 3.8f);

        BolumBitisKapisi bitisKapisi = victory.GetComponent<BolumBitisKapisi>();
        if (bitisKapisi == null) bitisKapisi = victory.gameObject.AddComponent<BolumBitisKapisi>();
        bitisKapisi.bolumNo = 1;

        BitisKapisiGorseliEkle(victory.transform);

        // 9. TEK UCURUM OLUM ALANI
        foreach (var death in Object.FindObjectsByType<DeathZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(death.gameObject);
        }
        float altSinirY = zeminTilemap.CellToWorld(new Vector3Int(0, -10, 0)).y;
        GameObject tekDeathObj = new GameObject("UcurumDeathZone");
        tekDeathObj.transform.position = new Vector3(pFinalMeydan.SagKenarX * 0.5f, altSinirY, 0f);
        BoxCollider2D tekBox = tekDeathObj.AddComponent<BoxCollider2D>();
        tekBox.isTrigger = true;
        tekBox.size = new Vector2(pFinalMeydan.SagKenarX + 60f, 4f);
        tekDeathObj.AddComponent<DeathZone>();

        // 10. KAMERA SINIRLARI
        float solKameraX = zeminTilemap.CellToWorld(new Vector3Int(-2, 0, 0)).x;
        float sagKameraX = zeminTilemap.CellToWorld(new Vector3Int(226, 0, 0)).x;
        float ustKameraY = zeminTilemap.CellToWorld(new Vector3Int(0, 16, 0)).y;

        GameObject confinerObj = GameObject.Find("CinemachineConfiner");
        if (confinerObj != null)
        {
            confinerObj.transform.position = Vector3.zero;
            confinerObj.transform.localScale = Vector3.one;
            PolygonCollider2D poly = confinerObj.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                poly.offset = Vector2.zero;
                poly.points = new Vector2[]
                {
                    new Vector2(solKameraX, altSinirY + 1f),
                    new Vector2(sagKameraX, altSinirY + 1f),
                    new Vector2(sagKameraX, ustKameraY),
                    new Vector2(solKameraX, ustKameraY)
                };
            }
        }

        if (Camera.main != null) Camera.main.transform.position = new Vector3(spawnX, gercekZeminY + 1.5f, -10f);
        GameObject vcam = GameObject.Find("CM vcam1");
        if (vcam != null)
        {
            vcam.transform.position = new Vector3(spawnX, gercekZeminY + 1.5f, -10f);
            foreach (var vComp in vcam.GetComponents<Component>())
            {
                if (vComp == null) continue;
                var m1 = vComp.GetType().GetMethod("InvalidateCache");
                if (m1 != null) m1.Invoke(vComp, null);
                var m2 = vComp.GetType().GetMethod("InvalidatePathCache");
                if (m2 != null) m2.Invoke(vComp, null);
            }
        }

        BolumAtmosferi atmosfer = Object.FindFirstObjectByType<BolumAtmosferi>();
        if (atmosfer != null) atmosfer.bolumNumarasi = 1;

        List<EditorBuildSettingsScene> sahneListesi = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(samplePath, true),
            new EditorBuildSettingsScene(bolum1Path, true)
        };
        EditorBuildSettings.scenes = sahneListesi.ToArray();

        EditorSceneManager.SaveScene(sahne);
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.LookAt(baslangicPos, Quaternion.identity, 14f);
        }
        Debug.Log("[Bolum1Kurucu] Bolum1 hareketli/coken platformlar, 1 Gunah Yaziti ve Gizli Gecit arkasinda Es Portresi + 1 Erdem Sunagi ile kuruldu!");
    }

    private static void HareketliPlatformOlustur(Transform parent, string ad, Vector3 basPos, Vector2 mesafe, float genislik, float hiz, int layer)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        obj.transform.position = basPos;
        obj.layer = layer;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = TasBlokSpriteOlustur(48, 14, new Color(0.56f, 0.62f, 0.70f, 1f), new Color(0.82f, 0.86f, 0.92f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(genislik, 0.55f);
        sr.sortingOrder = 2;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(genislik, 0.55f);

        HareketliPlatform hp = obj.AddComponent<HareketliPlatform>();
        hp.hareketMesafesi = mesafe;
        hp.hiz = hiz;
    }

    private static void KirilabilirTasOlustur(Transform parent, string ad, Vector3 pos, float genislik, int layer)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        obj.transform.position = pos;
        obj.layer = layer;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = TasBlokSpriteOlustur(44, 14, new Color(0.68f, 0.54f, 0.44f, 1f), new Color(0.32f, 0.24f, 0.20f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(genislik, 0.52f);
        sr.sortingOrder = 2;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(genislik, 0.52f);

        obj.AddComponent<KirilabilirTas>();
    }

    private static void GizliGecitDuvariOlustur(Transform parent, Tilemap tm, int xBas, int xBit, int yBas, int yBit)
    {
        Vector3 solAlt = tm.CellToWorld(new Vector3Int(xBas, yBas, 0));
        Vector3 sagUst = tm.CellToWorld(new Vector3Int(xBit + 1, yBit + 1, 0));
        Vector3 merkez = (solAlt + sagUst) * 0.5f;
        Vector2 boyut = new Vector2(sagUst.x - solAlt.x, sagUst.y - solAlt.y);

        GameObject duvar = new GameObject("GizliGecit_SahteDuvar");
        duvar.transform.SetParent(parent, false);
        duvar.transform.position = merkez;

        SpriteRenderer sr = duvar.AddComponent<SpriteRenderer>();
        sr.sprite = TasBlokSpriteOlustur(32, 64, new Color(0.46f, 0.50f, 0.56f, 1f), new Color(0.36f, 0.40f, 0.46f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = boyut;
        sr.sortingOrder = 6; // Oyuncu icinden gecerken onu gizler, sonra seffaflasir

        duvar.AddComponent<GizliGecitDuvari>();
    }

    private static void EsininSiluetPortresiOlustur(Vector3 zeminPos)
    {
        GameObject portreObj = new GameObject("EsininSiluetPortresi_1");
        portreObj.transform.position = zeminPos + new Vector3(0f, 0.1f, 0f);

        int w = 44, h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color altinCerceve = new Color(0.86f, 0.70f, 0.34f, 1f);
        Color icArkaPlan = new Color(0.11f, 0.13f, 0.19f, 1f);
        Color ayIsigiHale = new Color(0.96f, 0.90f, 0.72f, 0.35f);
        Color siluetKoyu = new Color(0.04f, 0.04f, 0.07f, 1f);
        Color siluetKenar = new Color(0.95f, 0.86f, 0.62f, 0.90f);

        Vector2 kafaMerkez = new Vector2(22f, 43f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool cerceve = x < 3 || x >= w - 3 || y < 3 || y >= h - 3;
                if (cerceve)
                {
                    tex.SetPixel(x, y, altinCerceve);
                    continue;
                }

                float kafaMesafe = Vector2.Distance(new Vector2(x, y), kafaMerkez);
                bool kafa = kafaMesafe <= 6.2f;
                bool duvak = (y >= 24 && y <= 46 && Mathf.Abs(x - 22) <= (6.5f + (46 - y) * 0.18f));
                bool omuz = (y >= 6 && y < 26 && Mathf.Abs(x - 22) <= (8f + (26 - y) * 0.32f));
                bool siluet = kafa || duvak || omuz;

                if (siluet)
                {
                    bool kenarParilti = (kafaMesafe > 5.0f && kafa) || (Mathf.Abs(x - 22) >= (5.5f + (46 - y) * 0.18f) && y > 24);
                    tex.SetPixel(x, y, kenarParilti ? siluetKenar : siluetKoyu);
                }
                else
                {
                    float haleMesafe = Vector2.Distance(new Vector2(x, y), new Vector2(22f, 38f));
                    tex.SetPixel(x, y, haleMesafe < 15f ? Color.Lerp(ayIsigiHale, icArkaPlan, haleMesafe / 15f) : icArkaPlan);
                }
            }
        }
        tex.Apply();

        SpriteRenderer sr = portreObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 26f);
        sr.sortingOrder = 2;
    }

    private static Sprite TasBlokSpriteOlustur(int w, int h, Color anaRenk, Color catlakRenk)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool kenar = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                bool catlak = ((x + y * 2) % 13 == 0 && y > 2 && y < h - 2);
                tex.SetPixel(x, y, kenar ? catlakRenk : (catlak ? catlakRenk : anaRenk));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f, 0, SpriteMeshType.FullRect);
    }

    private static float ZeminYuzeyiBul(float x, float varsayilanY)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, varsayilanY + 4f), Vector2.down, 12f);
        if (hit.collider != null) return hit.point.y;
        return varsayilanY;
    }

    private static void YuzeyJetonEkle(List<Vector3> liste, PlatformTanimi plat, float oran)
    {
        float x = Mathf.Lerp(plat.SolKenarX, plat.SagKenarX, oran);
        float y = ZeminYuzeyiBul(x, plat.YuzeyY);
        liste.Add(new Vector3(x, y + 0.70f, 0f));
    }

    private static void KavisJetonEkle(List<Vector3> liste, PlatformTanimi solPlat, PlatformTanimi sagPlat)
    {
        float ortaX = (solPlat.SagKenarX + sagPlat.SolKenarX) * 0.5f;
        float tepeY = Mathf.Max(solPlat.YuzeyY, sagPlat.YuzeyY) + 1.25f;
        liste.Add(new Vector3(ortaX, tepeY, 0f));
    }

    private static void JetonlariKur(List<Vector3> noktalar)
    {
        GameObject sablon = null;
        TokenInstance[] sahneJetonlar = Object.FindObjectsByType<TokenInstance>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (sahneJetonlar.Length > 0) sablon = sahneJetonlar[0].gameObject;

        if (sablon == null)
        {
            string[] prefabGuidler = AssetDatabase.FindAssets("t:Prefab");
            foreach (string g in prefabGuidler)
            {
                string yol = AssetDatabase.GUIDToAssetPath(g);
                if (yol.ToLower().Contains("token"))
                {
                    sablon = AssetDatabase.LoadAssetAtPath<GameObject>(yol);
                    break;
                }
            }
        }
        if (sablon == null) return;

        GameObject jetonKlasoru = new GameObject("Bolum1_Jetonlar");
        foreach (Vector3 p in noktalar)
        {
            GameObject j = (GameObject)PrefabUtility.InstantiatePrefab(sablon, jetonKlasoru.transform);
            if (j == null) j = Object.Instantiate(sablon, jetonKlasoru.transform);
            j.transform.position = p;
            j.SetActive(true);
            j.name = "Jeton";
        }

        foreach (var eski in sahneJetonlar)
        {
            if (eski != null && eski.transform.parent != jetonKlasoru.transform)
                Object.DestroyImmediate(eski.gameObject);
        }
    }

    private static void DusmanlariKur((string ad, PlatformTanimi plat)[] plan)
    {
        GameObject sablon = null;
        EnemyController[] sahneDusmanlar = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (sahneDusmanlar.Length > 0) sablon = sahneDusmanlar[0].gameObject;

        if (sablon == null)
        {
            string[] prefabGuidler = AssetDatabase.FindAssets("t:Prefab");
            foreach (string g in prefabGuidler)
            {
                string yol = AssetDatabase.GUIDToAssetPath(g);
                if (yol.ToLower().Contains("enemy"))
                {
                    sablon = AssetDatabase.LoadAssetAtPath<GameObject>(yol);
                    break;
                }
            }
        }
        if (sablon == null) return;

        GameObject dusmanKlasoru = new GameObject("Bolum1_Dusmanlar");

        foreach (var item in plan)
        {
            float solLimit = item.plat.SolKenarX + 0.8f;
            float sagLimit = item.plat.SagKenarX - 0.8f;
            float merkezX = (solLimit + sagLimit) * 0.5f;
            float yariMesafe = Mathf.Max(0.9f, (sagLimit - solLimit) * 0.5f);
            float zeminY = ZeminYuzeyiBul(merkezX, item.plat.YuzeyY);

            GameObject dusmanObj = (GameObject)PrefabUtility.InstantiatePrefab(sablon, dusmanKlasoru.transform);
            if (dusmanObj == null) dusmanObj = Object.Instantiate(sablon, dusmanKlasoru.transform);
            dusmanObj.name = item.ad;
            dusmanObj.SetActive(true);
            dusmanObj.transform.position = new Vector3(merkezX, zeminY + 0.4f, 0f);

            Collider2D col = dusmanObj.GetComponent<Collider2D>();
            if (col != null)
            {
                Physics2D.SyncTransforms();
                float altFark = zeminY - col.bounds.min.y;
                dusmanObj.transform.position += new Vector3(0f, altFark + 0.02f, 0f);
            }

            GameObject rotaObj = new GameObject(item.ad + "_Rota");
            rotaObj.transform.SetParent(dusmanKlasoru.transform, false);
            rotaObj.transform.position = dusmanObj.transform.position;

            PatrolPath rota = rotaObj.AddComponent<PatrolPath>();
            rota.startPosition = new Vector2(-yariMesafe, 0f);
            rota.endPosition = new Vector2(yariMesafe, 0f);

            EnemyController ec = dusmanObj.GetComponent<EnemyController>();
            if (ec != null) ec.path = rota;
        }

        foreach (var eski in sahneDusmanlar)
        {
            if (eski != null && eski.transform.parent != dusmanKlasoru.transform)
                Object.DestroyImmediate(eski.gameObject);
        }
        foreach (var rota in Object.FindObjectsByType<PatrolPath>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (rota != null && rota.transform.parent != dusmanKlasoru.transform)
                Object.DestroyImmediate(rota.gameObject);
        }
    }

    private static void BitisKapisiGorseliEkle(Transform parent)
    {
        Transform eski = parent.Find("BitisGecidiGorsel");
        if (eski != null) Object.DestroyImmediate(eski.gameObject);

        GameObject gorsel = new GameObject("BitisGecidiGorsel");
        gorsel.transform.SetParent(parent, false);
        gorsel.transform.localPosition = Vector3.zero;

        Texture2D tex = new Texture2D(48, 84);
        tex.filterMode = FilterMode.Point;
        Color cerceve = new Color(0.95f, 0.80f, 0.38f, 1f);
        Color icIsik = new Color(1f, 0.96f, 0.78f, 0.88f);

        for (int y = 0; y < 84; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                bool sutun = (x < 6 || x >= 42 || y >= 76);
                bool icAlan = (x >= 6 && x < 42 && y < 76);
                tex.SetPixel(x, y, sutun ? cerceve : (icAlan ? icIsik : Color.clear));
            }
        }
        tex.Apply();

        SpriteRenderer sr = gorsel.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 48, 84), new Vector2(0.5f, 0f), 24f);
        sr.sortingOrder = 1;
    }

    private static TileBase IlkDoluKaroyuBul(Tilemap tm)
    {
        tm.CompressBounds();
        foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)
        {
            TileBase t = tm.GetTile(pos);
            if (t != null) return t;
        }
        return null;
    }

    private static void BlokCiz(Tilemap map, TileBase tile, int xBas, int xBit, int yBas, int yBit)
    {
        for (int x = xBas; x <= xBit; x++)
        {
            for (int y = yBas; y <= yBit; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private static void TabletEkle(string ad, Vector3 pos, AnlatimNesnesi.NesneTuru tur, string kod, int erdemNo,
        string bTr, string iTr, string bEn, string iEn)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.position = pos;
        AnlatimNesnesi a = obj.AddComponent<AnlatimNesnesi>();
        a.tur = tur;
        a.kayitKodu = kod;
        a.erdemSirasi = erdemNo;
        a.baslikTr = bTr;
        a.icerikTr = iTr;
        a.baslikEn = bEn;
        a.icerikEn = iEn;
    }
}

