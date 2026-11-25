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
    private const string OTO_KURULUM_ANAHTARI = "Bolum1_Oburluk_v32";

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

    [MenuItem("Yedi Perde (Tez)/1. Bolum Sahnesini Kur (Oburluk)")]
    public static void Bolum1Kur()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Bolum1Kurucu] Play modundan ciktiktan sonra kurulum calistirilabilir.");
            return;
        }

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
        // BOLUM 1: OBURLUK (GLUTTONY) — 3-4 DAKIKALIK 6 BOLGELI MIMARI (~445 KARO)
        // Tum bosluklar <= 4 karo, tum adimlar <= 2 karo (%100 gecilebilir)
        // =========================================================================

        // BOLGE 1: Uyanis Avlusu ve Ilk Tas Kopruler (x: 0..68)
        PlatformTanimi p1_Avlu      = new PlatformTanimi(zeminTilemap, 0, 15, -4, -1);     // Yuzey: y=0 karosu ustu
        PlatformTanimi p1_Ada1      = new PlatformTanimi(zeminTilemap, 19, 27, -3, 0);     // 3 karo bosluk, +1 yukseklik
        PlatformTanimi p1_Ada2      = new PlatformTanimi(zeminTilemap, 31, 40, -3, 0);     // 3 karo bosluk
        // (x: 41..51 arasinda yatay hareketli tas kopru - ust yuzeyi p1_Ada2 ile birebir ayni!)
        PlatformTanimi p1_KopruSonu = new PlatformTanimi(zeminTilemap, 52, 66, -2, 1);     // Muhafiz 1

        // BOLGE 2: Ziyafet Mahzenleri — Oburluk Tuzaklari & Ust Kemer Yolu (x: 70..146)
        PlatformTanimi p2_Alt1      = new PlatformTanimi(zeminTilemap, 70, 81, -2, 1);     // Muhafiz 2
        // (x: 83..86 arasinda 1. Oburluk Coken Tasi - uzerinde 3'lu kirinti cebi)
        PlatformTanimi p2_Alt2      = new PlatformTanimi(zeminTilemap, 88, 100, -2, 1);    // Muhafiz 3
        // (x: 102..105 arasinda 2. Oburluk Coken Tasi - uzerinde 3'lu kirinti cebi)
        PlatformTanimi p2_Alt3      = new PlatformTanimi(zeminTilemap, 107, 121, -1, 2);   // Muhafiz 4

        // Ust Alternatif Kemer Rotasi (p2_Alt1'den +2 basamaklarla tirmanilir, p2_Alt3'e iner)
        PlatformTanimi p2_Ust1      = new PlatformTanimi(zeminTilemap, 74, 81, 3, 4);
        PlatformTanimi p2_Ust2      = new PlatformTanimi(zeminTilemap, 85, 93, 4, 5);      // Muhafiz 5
        PlatformTanimi p2_Ust3      = new PlatformTanimi(zeminTilemap, 97, 105, 3, 4);

        // BOLGE 3: Sarnic Kuyusu Tirmanisi & TEK GUNAH YAZITI (x: 125..198)
        PlatformTanimi p3_KuyuTaban = new PlatformTanimi(zeminTilemap, 125, 136, -1, 2);
        // (x: 138..141 dikey asansor tasi: y=2 yuzeyinden y=5 yuzeyine cikarir)
        PlatformTanimi p3_Teras1    = new PlatformTanimi(zeminTilemap, 143, 154, 2, 5);    // Muhafiz 6
        PlatformTanimi p3_Teras2    = new PlatformTanimi(zeminTilemap, 158, 169, 3, 6);
        PlatformTanimi p3_YazitAlani= new PlatformTanimi(zeminTilemap, 173, 192, 3, 6);    // TEK GUNAH YAZITI (I. Gunah: Oburluk) + Muhafiz 7

        // BOLGE 4: BOLUM ORTASI TEK CHECKPOINT MEYDANI (x: 196..220)
        PlatformTanimi p4_Checkpoint= new PlatformTanimi(zeminTilemap, 196, 218, 2, 5);    // Ortada 1 Checkpoint Mesalesi

        // BOLGE 5: Ikiz Gecitler & KAMUFLE GIZLI ERDEM MABEDI (x: 222..326)
        // Alt Ana Yol:
        PlatformTanimi p5_Alt1      = new PlatformTanimi(zeminTilemap, 222, 234, 1, 4);    // Muhafiz 8
        // (x: 236..245 yatay hareketli kopru)
        PlatformTanimi p5_Alt2      = new PlatformTanimi(zeminTilemap, 246, 260, 1, 4);    // Muhafiz 9
        // (x: 262..265 coken tas)
        PlatformTanimi p5_Alt3      = new PlatformTanimi(zeminTilemap, 267, 282, 0, 3);    // Muhafiz 10
        // (x: 284..294 yatay hareketli kopru)
        PlatformTanimi p5_Alt4      = new PlatformTanimi(zeminTilemap, 295, 312, 0, 3);    // Muhafiz 11

        // Ust Gizli Mabet Rotasi (p4_Checkpoint sag ucundan yukari basamaklarla cikar):
        PlatformTanimi p5_Gizli1    = new PlatformTanimi(zeminTilemap, 221, 227, 6, 7);    // +2 basamak
        PlatformTanimi p5_Gizli2    = new PlatformTanimi(zeminTilemap, 231, 237, 7, 8);    // +1 basamak
        // (x: 239..242 coken tas)
        PlatformTanimi p5_GizliEsik = new PlatformTanimi(zeminTilemap, 244, 248, 8, 9);    // Kamufle duvarin hemen onu
        PlatformTanimi p5_GizliMabet= new PlatformTanimi(zeminTilemap, 249, 268, 8, 9);    // GIZLI ODA (yuzey y=10 cizgisi, tavan y=15..17)

        // BOLGE 6: Olcululuk Merdivenleri & Final Tapinak Meydani (x: 316..425)
        PlatformTanimi p6_Basamak1  = new PlatformTanimi(zeminTilemap, 316, 328, 1, 4);    // Muhafiz 12
        // (x: 330..339 yatay hareketli kopru)
        PlatformTanimi p6_Basamak2  = new PlatformTanimi(zeminTilemap, 340, 353, 2, 5);    // Muhafiz 13
        // (x: 355..358 coken tas)
        PlatformTanimi p6_Basamak3  = new PlatformTanimi(zeminTilemap, 360, 373, 3, 6);    // Muhafiz 14
        // (x: 375..385 son yatay hareketli kopru)
        PlatformTanimi p6_Final     = new PlatformTanimi(zeminTilemap, 386, 412, 3, 6);    // Muhafiz 15 + Bolum Bitis Kapisi

        PlatformTanimi[] tumPlatformlar = new[]
        {
            p1_Avlu, p1_Ada1, p1_Ada2, p1_KopruSonu,
            p2_Alt1, p2_Alt2, p2_Alt3, p2_Ust1, p2_Ust2, p2_Ust3,
            p3_KuyuTaban, p3_Teras1, p3_Teras2, p3_YazitAlani,
            p4_Checkpoint,
            p5_Alt1, p5_Alt2, p5_Alt3, p5_Alt4,
            p5_Gizli1, p5_Gizli2, p5_GizliEsik, p5_GizliMabet,
            p6_Basamak1, p6_Basamak2, p6_Basamak3, p6_Final
        };

        // Sol ve Sag sinir duvarlari + Gizli Mabedin kapali tas tavani ve sag arka duvari
        BlokCiz(zeminTilemap, zeminTile, -4, -1, -4, 12);
        BlokCiz(zeminTilemap, zeminTile, 413, 416, 3, 15);
        BlokCiz(zeminTilemap, zeminTile, 249, 269, 15, 17); // Gizli mabet kalin tavani
        BlokCiz(zeminTilemap, zeminTile, 268, 269, 10, 15); // Gizli mabet sag arka duvari

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

        // 2. OZEL PLATFORMLAR (Tum ust yuzeyler komsu zeminin YuzeyY degeriyle milimetrik hizali!)
        GameObject ozelKlasor = new GameObject("Bolum1_OzelPlatformlar");
        float kalinlik = 0.52f;

        // Bolge 1 Yatay Kopru (p1_Ada2 -> p1_KopruSonu)
        float k1SolX = zeminTilemap.CellToWorld(new Vector3Int(42, 0, 0)).x + 1.1f;
        float k1SagX = zeminTilemap.CellToWorld(new Vector3Int(50, 0, 0)).x - 1.1f;
        float k1MerkezY = p1_Ada2.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_1",
            new Vector3(k1SolX, k1MerkezY, 0f), new Vector2(k1SagX - k1SolX, 0f), 2.4f, 2.1f, kalinlik, zeminLayer);

        // Bolge 2 Oburluk Tuzagi 1 (p2_Alt1 -> p2_Alt2 arasi)
        float c1X = (p2_Alt1.SagKenarX + p2_Alt2.SolKenarX) * 0.5f;
        float c1Y = p2_Alt1.YuzeyY - kalinlik * 0.5f;
        KirilabilirTasOlustur(ozelKlasor.transform, "OburlukCokenTas_1", new Vector3(c1X, c1Y, 0f), 2.2f, kalinlik, zeminLayer);

        // Bolge 2 Oburluk Tuzagi 2 (p2_Alt2 -> p2_Alt3 arasi)
        float c2X = (p2_Alt2.SagKenarX + p2_Alt3.SolKenarX) * 0.5f;
        float c2Y = p2_Alt2.YuzeyY - kalinlik * 0.5f;
        KirilabilirTasOlustur(ozelKlasor.transform, "OburlukCokenTas_2", new Vector3(c2X, c2Y, 0f), 2.2f, kalinlik, zeminLayer);

        // Bolge 3 Dikey Asansor Tasi (p3_KuyuTaban yuzeyinden p3_Teras1 yuzeyine tam hizali!)
        float asansorX = (p3_KuyuTaban.SagKenarX + p3_Teras1.SolKenarX) * 0.5f;
        float asansorAltY = p3_KuyuTaban.YuzeyY - kalinlik * 0.5f;
        float asansorUstY = p3_Teras1.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "DikeyAsansor_1",
            new Vector3(asansorX, asansorAltY, 0f), new Vector2(0f, asansorUstY - asansorAltY), 2.2f, 2.0f, kalinlik, zeminLayer);

        // Bolge 5 Alt Yol Yatay Kopru 2 (p5_Alt1 -> p5_Alt2)
        float k2SolX = zeminTilemap.CellToWorld(new Vector3Int(237, 0, 0)).x + 1.0f;
        float k2SagX = zeminTilemap.CellToWorld(new Vector3Int(245, 0, 0)).x - 1.0f;
        float k2MerkezY = p5_Alt1.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_2",
            new Vector3(k2SolX, k2MerkezY, 0f), new Vector2(k2SagX - k2SolX, 0f), 2.3f, 2.2f, kalinlik, zeminLayer);

        // Bolge 5 Alt Yol Coken Tas (p5_Alt2 -> p5_Alt3)
        float c3X = (p5_Alt2.SagKenarX + p5_Alt3.SolKenarX) * 0.5f;
        float c3Y = p5_Alt3.YuzeyY - kalinlik * 0.5f;
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_Bolge5", new Vector3(c3X, c3Y, 0f), 2.0f, kalinlik, zeminLayer);

        // Bolge 5 Alt Yol Yatay Kopru 3 (p5_Alt3 -> p5_Alt4)
        float k3SolX = zeminTilemap.CellToWorld(new Vector3Int(285, 0, 0)).x + 1.0f;
        float k3SagX = zeminTilemap.CellToWorld(new Vector3Int(294, 0, 0)).x - 1.0f;
        float k3MerkezY = p5_Alt3.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_3",
            new Vector3(k3SolX, k3MerkezY, 0f), new Vector2(k3SagX - k3SolX, 0f), 2.3f, 2.3f, kalinlik, zeminLayer);

        // Gizli Yol Coken Tasi (p5_Gizli2 -> p5_GizliEsik)
        float cGizliX = (p5_Gizli2.SagKenarX + p5_GizliEsik.SolKenarX) * 0.5f;
        float cGizliY = p5_GizliEsik.YuzeyY - kalinlik * 0.5f;
        KirilabilirTasOlustur(ozelKlasor.transform, "GizliYolCokenTas", new Vector3(cGizliX, cGizliY, 0f), 1.9f, kalinlik, zeminLayer);

        // KAMUFLE GIZLI GECIT DUVARI (x: 249, y: 10..14 — Gercek tile karolariyla birebir ayni!)
        KamufleGizliDuvarOlustur(ozelKlasor.transform, zeminTilemap, zeminTile, 249, 249, 10, 14);

        // Bolge 6 Yatay Kopru 4 (p6_Basamak1 -> p6_Basamak2)
        float k4SolX = zeminTilemap.CellToWorld(new Vector3Int(331, 0, 0)).x + 1.0f;
        float k4SagX = zeminTilemap.CellToWorld(new Vector3Int(339, 0, 0)).x - 1.0f;
        float k4MerkezY = p6_Basamak1.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_4",
            new Vector3(k4SolX, k4MerkezY, 0f), new Vector2(k4SagX - k4SolX, 0f), 2.3f, 2.3f, kalinlik, zeminLayer);

        // Bolge 6 Coken Tas (p6_Basamak2 -> p6_Basamak3)
        float c4X = (p6_Basamak2.SagKenarX + p6_Basamak3.SolKenarX) * 0.5f;
        float c4Y = p6_Basamak2.YuzeyY - kalinlik * 0.5f;
        KirilabilirTasOlustur(ozelKlasor.transform, "CatlakTas_Bolge6", new Vector3(c4X, c4Y, 0f), 2.0f, kalinlik, zeminLayer);

        // Bolge 6 Son Yatay Kopru 5 (p6_Basamak3 -> p6_Final)
        float k5SolX = zeminTilemap.CellToWorld(new Vector3Int(376, 0, 0)).x + 1.0f;
        float k5SagX = zeminTilemap.CellToWorld(new Vector3Int(385, 0, 0)).x - 1.0f;
        float k5MerkezY = p6_Basamak3.YuzeyY - kalinlik * 0.5f;
        HareketliPlatformOlustur(ozelKlasor.transform, "HareketliKopru_5",
            new Vector3(k5SolX, k5MerkezY, 0f), new Vector2(k5SagX - k5SolX, 0f), 2.4f, 2.4f, kalinlik, zeminLayer);

        // 3. ARKA PLAN
        GameObject bg = GameObject.Find("Background");
        if (bg != null)
        {
            bg.SetActive(true);
            bg.transform.position = new Vector3(12f, 2.0f, bg.transform.position.z);
        }

        // 4. %100 ERISILEBILIR ZIYAFET KIRINTILARI (Toplam 18 Adet — Hepsi %100 toplanabilir!)
        List<Vector3> kirintiNoktalari = new List<Vector3>();

        // Bolge 1 (3 adet)
        KavisJetonEkle(kirintiNoktalari, p1_Avlu, p1_Ada1);
        KavisJetonEkle(kirintiNoktalari, p1_Ada1, p1_Ada2);
        YuzeyJetonEkle(kirintiNoktalari, p1_Ada2, 0.5f);

        // Bolge 2: Oburluk Tuzaklari (Coken taslarin ustunde 3'er adet = 6 adet) + Ust Kemer (2 adet)
        float c1Ust = p2_Alt1.YuzeyY + 0.65f;
        kirintiNoktalari.Add(new Vector3(c1X - 0.55f, c1Ust, 0f));
        kirintiNoktalari.Add(new Vector3(c1X,         c1Ust + 0.25f, 0f));
        kirintiNoktalari.Add(new Vector3(c1X + 0.55f, c1Ust, 0f));

        float c2Ust = p2_Alt2.YuzeyY + 0.65f;
        kirintiNoktalari.Add(new Vector3(c2X - 0.55f, c2Ust, 0f));
        kirintiNoktalari.Add(new Vector3(c2X,         c2Ust + 0.25f, 0f));
        kirintiNoktalari.Add(new Vector3(c2X + 0.55f, c2Ust, 0f));

        YuzeyJetonEkle(kirintiNoktalari, p2_Ust1, 0.5f);
        YuzeyJetonEkle(kirintiNoktalari, p2_Ust3, 0.5f);

        // Bolge 3 (2 adet)
        KavisJetonEkle(kirintiNoktalari, p3_Teras1, p3_Teras2);
        YuzeyJetonEkle(kirintiNoktalari, p3_Teras2, 0.5f);

        // Bolge 5 Alt ve Ust Yol + Gizli Oda Ici (4 adet)
        YuzeyJetonEkle(kirintiNoktalari, p5_Alt2, 0.5f);
        YuzeyJetonEkle(kirintiNoktalari, p5_Gizli2, 0.5f);
        // Gizli oda ici (Dogrudan p5_GizliMabet.YuzeyY uzerine konur, tavan ustune cikmaz!)
        kirintiNoktalari.Add(new Vector3(Mathf.Lerp(p5_GizliMabet.SolKenarX, p5_GizliMabet.SagKenarX, 0.22f), p5_GizliMabet.YuzeyY + 0.65f, 0f));
        kirintiNoktalari.Add(new Vector3(Mathf.Lerp(p5_GizliMabet.SolKenarX, p5_GizliMabet.SagKenarX, 0.82f), p5_GizliMabet.YuzeyY + 0.65f, 0f));

        // Bolge 6 (1 adet)
        YuzeyJetonEkle(kirintiNoktalari, p6_Basamak2, 0.5f);

        JetonlariKur(kirintiNoktalari);

        // 5. AKTIF KOPRU MUHAFIZLARI (15 Dengeli Devriye)
        (string ad, PlatformTanimi plat)[] dusmanPlan = new[]
        {
            ("GolgeMuhafiz_01", p1_KopruSonu),
            ("GolgeMuhafiz_02", p2_Alt1),
            ("GolgeMuhafiz_03", p2_Alt2),
            ("GolgeMuhafiz_04", p2_Alt3),
            ("GolgeMuhafiz_05", p2_Ust2),
            ("GolgeMuhafiz_06", p3_Teras1),
            ("GolgeMuhafiz_07", p3_YazitAlani),
            ("GolgeMuhafiz_08", p5_Alt1),
            ("GolgeMuhafiz_09", p5_Alt2),
            ("GolgeMuhafiz_10", p5_Alt3),
            ("GolgeMuhafiz_11", p5_Alt4),
            ("GolgeMuhafiz_12", p6_Basamak1),
            ("GolgeMuhafiz_13", p6_Basamak2),
            ("GolgeMuhafiz_14", p6_Basamak3),
            ("GolgeMuhafiz_15", p6_Final)
        };
        DusmanlariKur(dusmanPlan);

        // 6. BOLUM ORTASI TEK CHECKPOINT (AraKontrolNoktasi)
        foreach (var eskiCp in Object.FindObjectsByType<AraKontrolNoktasi>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(eskiCp.gameObject);
        }
        float cpX = p4_Checkpoint.MerkezX;
        GameObject cpObj = new GameObject("Bolum1_TekCheckpoint");
        cpObj.transform.position = new Vector3(cpX, p4_Checkpoint.YuzeyY, 0f);
        cpObj.AddComponent<AraKontrolNoktasi>();

        // 7. HIKAYE ANLATIMI: ANA ROTADA 1 GUNAH YAZITI + DOLU GIZLI ODADA ESININ SILUET PORTRESI & 1 ERDEM SUNAGI
        foreach (var eski in Object.FindObjectsByType<AnlatimNesnesi>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(eski.gameObject);
        }

        // A) ANA ROTADAKI TEK GUNAH YAZITI (I. Gunah: Oburluk)
        float gunahX = Mathf.Lerp(p3_YazitAlani.SolKenarX, p3_YazitAlani.SagKenarX, 0.35f);
        TabletEkle("GunahTableti_Bolum1_Oburluk", new Vector3(gunahX, p3_YazitAlani.YuzeyY, 0f), AnlatimNesnesi.NesneTuru.GunahTableti,
            "bolum1_gunah_oburluk", 1,
            "I. GÜNAH — OBURLUK",
            "Hiçbir kırıntıyı geride bırakmamak için taşların çöküşünü bile görmezden geldim. Doymak bilmeyen bu telaş, hem beni hem onu bu derinliğe çekti.",
            "I. SIN — GLUTTONY",
            "Refusing to leave a single crumb behind, I ignored even the crumbling stones beneath me. This insatiable rush dragged both me and her into these depths.");

        // B) DOLU VE KAPALI GIZLI ODA (Tavan altinda, tam odanin zemininde!)
        GizliOdaAtmosferiKur(zeminTilemap, p5_GizliMabet);

        float portreX = Mathf.Lerp(p5_GizliMabet.SolKenarX, p5_GizliMabet.SagKenarX, 0.42f);
        EsininSiluetPortresiOlustur(new Vector3(portreX, p5_GizliMabet.YuzeyY, 0f));

        float erdemX = portreX + 2.1f;
        TabletEkle("ErdemSunagi_Bolum1_Olcululuk", new Vector3(erdemX, p5_GizliMabet.YuzeyY, 0f), AnlatimNesnesi.NesneTuru.ErdemSunagi,
            "bolum1_erdem_olcululuk", 1,
            "I. ERDEM — ÖLÇÜLÜLÜK",
            "\"Her şeyi bir anda avuçlamaya çalışma sevgilim... Durup nefes alan ve ölçülü yürüyen göz, taşların ardındaki hakikati görür.\"",
            "I. VIRTUE — TEMPERANCE",
            "\"Do not try to grasp everything at once, my love... The eye that pauses to breathe and walks with measure sees the truth behind the stones.\"");

        // 8. BASLANGIC NOKTASI VE OYUNCU
        float spawnX = zeminTilemap.CellToWorld(new Vector3Int(4, 0, 0)).x;
        Vector3 baslangicPos = new Vector3(spawnX, p1_Avlu.YuzeyY + 0.55f, 0f);

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

        // 9. KESIN CALISAN BOLUM SONU KAPISI
        VictoryZone victory = Object.FindFirstObjectByType<VictoryZone>(FindObjectsInactive.Include);
        if (victory == null)
        {
            GameObject vObj = new GameObject("VictoryZone");
            victory = vObj.AddComponent<VictoryZone>();
        }
        float kapiX = p6_Final.SagKenarX - 2.2f;
        victory.gameObject.SetActive(true);
        victory.transform.SetParent(null);
        victory.transform.position = new Vector3(kapiX, p6_Final.YuzeyY, 0f);
        victory.transform.localScale = Vector3.one;
        victory.enabled = false;

        BoxCollider2D vBox = victory.GetComponent<BoxCollider2D>();
        if (vBox == null) vBox = victory.gameObject.AddComponent<BoxCollider2D>();
        vBox.isTrigger = true;
        vBox.offset = new Vector2(0f, 1.5f);
        vBox.size = new Vector2(3.0f, 3.8f);

        BolumBitisKapisi bitisKapisi = victory.GetComponent<BolumBitisKapisi>();
        if (bitisKapisi == null) bitisKapisi = victory.gameObject.AddComponent<BolumBitisKapisi>();
        bitisKapisi.bolumNo = 1;

        BitisKapisiGorseliEkle(victory.transform);

        // 10. TEK UCURUM OLUM ALANI
        foreach (var death in Object.FindObjectsByType<DeathZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(death.gameObject);
        }
        float altSinirY = zeminTilemap.CellToWorld(new Vector3Int(0, -11, 0)).y;
        GameObject tekDeathObj = new GameObject("UcurumDeathZone");
        tekDeathObj.transform.position = new Vector3(p6_Final.SagKenarX * 0.5f, altSinirY, 0f);
        BoxCollider2D tekBox = tekDeathObj.AddComponent<BoxCollider2D>();
        tekBox.isTrigger = true;
        tekBox.size = new Vector2(p6_Final.SagKenarX + 80f, 4f);
        tekDeathObj.AddComponent<DeathZone>();

        // 11. KAMERA SINIRLARI
        float solKameraX = zeminTilemap.CellToWorld(new Vector3Int(-2, 0, 0)).x;
        float sagKameraX = zeminTilemap.CellToWorld(new Vector3Int(414, 0, 0)).x;
        float ustKameraY = zeminTilemap.CellToWorld(new Vector3Int(0, 19, 0)).y;

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

        if (Camera.main != null) Camera.main.transform.position = new Vector3(spawnX, p1_Avlu.YuzeyY + 1.5f, -10f);
        GameObject vcam = GameObject.Find("CM vcam1");
        if (vcam != null)
        {
            vcam.transform.position = new Vector3(spawnX, p1_Avlu.YuzeyY + 1.5f, -10f);
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
        Debug.Log("[Bolum1Kurucu] Bolum 1 (Oburluk) 6 bolgeli, milimetrik hizali ve gizli oda ici dolu olarak kuruldu!");
    }

    private static void KamufleGizliDuvarOlustur(Transform parent, Tilemap tm, TileBase zeminTile, int xBas, int xBit, int yBas, int yBit)
    {
        GameObject grup = new GameObject("GizliGecit_KamufleDuvar");
        grup.transform.SetParent(parent, false);

        Sprite karoSprite = (zeminTile as Tile)?.sprite;
        Color karoRenk = (zeminTile as Tile)?.color ?? Color.white;
        Vector3 hucreBoyutu = tm.cellSize;

        for (int x = xBas; x <= xBit; x++)
        {
            for (int y = yBas; y <= yBit; y++)
            {
                Vector3 merkez = tm.GetCellCenterWorld(new Vector3Int(x, y, 0));
                GameObject parca = new GameObject($"KamufleKaro_{x}_{y}");
                parca.transform.SetParent(grup.transform, false);
                parca.transform.position = merkez;

                SpriteRenderer sr = parca.AddComponent<SpriteRenderer>();
                if (karoSprite != null)
                {
                    sr.sprite = karoSprite;
                    sr.color = karoRenk;
                    float olcekX = hucreBoyutu.x / Mathf.Max(0.01f, karoSprite.bounds.size.x);
                    float olcekY = hucreBoyutu.y / Mathf.Max(0.01f, karoSprite.bounds.size.y);
                    parca.transform.localScale = new Vector3(olcekX, olcekY, 1f);
                }
                else
                {
                    sr.sprite = TasBlokSpriteOlustur(24, 24, new Color(0.45f, 0.50f, 0.56f, 1f), new Color(0.36f, 0.40f, 0.46f, 1f));
                }
                sr.sortingOrder = 5;
                parca.AddComponent<GizliGecitDuvari>();
            }
        }
    }

    private static void GizliOdaAtmosferiKur(Tilemap tm, PlatformTanimi mabet)
    {
        GameObject odaKlasor = new GameObject("Bolum1_GizliOdaAtmosferi");

        Vector3 solAlt = tm.CellToWorld(new Vector3Int(mabet.xBas, mabet.yBit + 1, 0));
        Vector3 sagUst = tm.CellToWorld(new Vector3Int(mabet.xBit + 1, 15, 0));
        Vector3 merkez = (solAlt + sagUst) * 0.5f;
        Vector2 boyut = new Vector2(sagUst.x - solAlt.x, sagUst.y - solAlt.y);

        GameObject arkaDuvar = new GameObject("GizliMabet_ArkaDuvar");
        arkaDuvar.transform.SetParent(odaKlasor.transform, false);
        arkaDuvar.transform.position = merkez;

        SpriteRenderer arkaSr = arkaDuvar.AddComponent<SpriteRenderer>();
        arkaSr.sprite = TasBlokSpriteOlustur(64, 48, new Color(0.14f, 0.12f, 0.16f, 0.98f), new Color(0.24f, 0.20f, 0.26f, 0.98f));
        arkaSr.drawMode = SpriteDrawMode.Sliced;
        arkaSr.size = boyut;
        arkaSr.sortingOrder = -1;

        float[] sutunOranlari = { 0.14f, 0.88f };
        for (int i = 0; i < sutunOranlari.Length; i++)
        {
            float sx = Mathf.Lerp(mabet.SolKenarX, mabet.SagKenarX, sutunOranlari[i]);

            GameObject sutun = new GameObject("MabetSutunu_" + (i + 1));
            sutun.transform.SetParent(odaKlasor.transform, false);
            sutun.transform.position = new Vector3(sx, mabet.YuzeyY, 0f);

            Texture2D tex = new Texture2D(18, 66);
            tex.filterMode = FilterMode.Point;
            Color tas = new Color(0.32f, 0.28f, 0.26f, 1f);
            Color altinSerit = new Color(0.86f, 0.66f, 0.28f, 1f);
            Color mesaleIsik = new Color(1f, 0.78f, 0.34f, 1f);

            for (int y = 0; y < 66; y++)
            {
                for (int x = 0; x < 18; x++)
                {
                    Color c = Color.clear;
                    if (y < 50 && x >= 4 && x <= 13)
                    {
                        c = (y == 6 || y == 44) ? altinSerit : tas;
                    }
                    else if (y >= 50 && y < 62)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(8.5f, 55f));
                        if (d < 4.5f) c = mesaleIsik;
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            SpriteRenderer sSr = sutun.AddComponent<SpriteRenderer>();
            sSr.sprite = Sprite.Create(tex, new Rect(0, 0, 18, 66), new Vector2(0.5f, 0f), 24f);
            sSr.sortingOrder = 0;
        }
    }

    private static void HareketliPlatformOlustur(Transform parent, string ad, Vector3 basPos, Vector2 mesafe, float genislik, float hiz, float kalinlik, int layer)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        obj.transform.position = basPos;
        obj.layer = layer;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = TasBlokSpriteOlustur(48, 14, new Color(0.56f, 0.62f, 0.70f, 1f), new Color(0.82f, 0.86f, 0.92f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(genislik, kalinlik);
        sr.sortingOrder = 2;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(genislik, kalinlik);

        HareketliPlatform hp = obj.AddComponent<HareketliPlatform>();
        hp.hareketMesafesi = mesafe;
        hp.hiz = hiz;
    }

    private static void KirilabilirTasOlustur(Transform parent, string ad, Vector3 pos, float genislik, float kalinlik, int layer)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        obj.transform.position = pos;
        obj.layer = layer;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = TasBlokSpriteOlustur(44, 14, new Color(0.72f, 0.56f, 0.42f, 1f), new Color(0.32f, 0.24f, 0.20f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(genislik, kalinlik);
        sr.sortingOrder = 2;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(genislik, kalinlik);

        obj.AddComponent<KirilabilirTas>();
    }

    private static void EsininSiluetPortresiOlustur(Vector3 zeminPos)
    {
        GameObject portreObj = new GameObject("EsininSiluetPortresi_1");
        portreObj.transform.position = zeminPos + new Vector3(0f, 0.05f, 0f);

        int w = 44, h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color altinCerceve = new Color(0.88f, 0.72f, 0.34f, 1f);
        Color icArkaPlan = new Color(0.11f, 0.13f, 0.19f, 1f);
        Color ayIsigiHale = new Color(0.96f, 0.90f, 0.72f, 0.38f);
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

    private static void YuzeyJetonEkle(List<Vector3> liste, PlatformTanimi plat, float oran)
    {
        float x = Mathf.Lerp(plat.SolKenarX, plat.SagKenarX, oran);
        liste.Add(new Vector3(x, plat.YuzeyY + 0.65f, 0f));
    }

    private static void KavisJetonEkle(List<Vector3> liste, PlatformTanimi solPlat, PlatformTanimi sagPlat)
    {
        float ortaX = (solPlat.SagKenarX + sagPlat.SolKenarX) * 0.5f;
        float tepeY = Mathf.Max(solPlat.YuzeyY, sagPlat.YuzeyY) + 1.05f;
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

        GameObject jetonKlasoru = new GameObject("Bolum1_ZiyafetKirintilari");
        foreach (Vector3 p in noktalar)
        {
            GameObject j = (GameObject)PrefabUtility.InstantiatePrefab(sablon, jetonKlasoru.transform);
            if (j == null) j = Object.Instantiate(sablon, jetonKlasoru.transform);
            j.transform.position = p;
            j.SetActive(true);
            j.name = "ZiyafetKirintisi";
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
            float zeminY = item.plat.YuzeyY;

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
