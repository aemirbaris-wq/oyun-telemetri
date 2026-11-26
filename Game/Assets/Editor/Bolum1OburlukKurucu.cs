#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Bolum1OburlukKurucu
{
    [MenuItem("Seven Veils/Ana Menu + 1. Bolum (Oburluk) Sahnesini Kur")]
    public static void HepsiniKur()
    {
        SevenVeilsKaliciSanatUretici.TumSanatVarliklariniUret(1);

        AnaMenuSahnesiOlustur();
        Bolum1SahnesiOlustur();
        BuildAyarlariniGuncelle();
        EditorSceneManager.OpenScene("Assets/Scenes/Bolum1.unity");
        Debug.Log("Ana Menu ve 3.5-4.5 dakikalik 1. Bolum (Oburluk) 1x1 birim hassasiyetle kuruldu!");
    }

    private static void AnaMenuSahnesiOlustur()
    {
        Scene sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject kamObj = new GameObject("Main Camera");
        kamObj.tag = "MainCamera";
        Camera cam = kamObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.045f, 0.035f, 0.065f);
        kamObj.transform.position = new Vector3(0f, 0f, -10f);

        GameObject menuObj = new GameObject("AnaMenuYonetici");
        menuObj.AddComponent<AnaMenuVeStatsDashboard>();

        EditorSceneManager.SaveScene(sahne, "Assets/Scenes/AnaMenu.unity");
    }

    private static void Bolum1SahnesiOlustur()
    {
        Scene sahne = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int bolumNo = 1;
        var tema = SevenVeilsGorselFabrikasi.TemaGetir(bolumNo);

        Sprite platformSp = Resources.Load<Sprite>("Art/Veil1/Platform");
        Sprite karakterSp = Resources.Load<Sprite>("Art/Veil1/Ressam");
        Sprite muhafizSp = Resources.Load<Sprite>("Art/Veil1/Dusman_Muhafiz");
        Sprite kahinSp = Resources.Load<Sprite>("Art/Veil1/Dusman_Kahin");
        Sprite zirhliSp = Resources.Load<Sprite>("Art/Veil1/Dusman_Zirhli");
        Sprite parcaSp = Resources.Load<Sprite>("Art/Veil1/Parca");
        Sprite kapiSp = Resources.Load<Sprite>("Art/Veil1/Kapi");
        Sprite tablo0Sp = Resources.Load<Sprite>("Art/Veil1/Tablo_0");
        Sprite tablo1Sp = Resources.Load<Sprite>("Art/Veil1/Tablo_1");
        Sprite esSp = Resources.Load<Sprite>("Art/Veil1/EsPortresi");
        Sprite parallaxUzakSp = Resources.Load<Sprite>("Art/Veil1/Parallax_Uzak");
        Sprite parallaxOrtaSp = Resources.Load<Sprite>("Art/Veil1/Parallax_Orta");
        Sprite yaySp = Resources.Load<Sprite>("Art/Ortak/Yay_Pad");
        Sprite dikenSp = Resources.Load<Sprite>("Art/Ortak/Diken_Tuzak");
        Sprite samdanPasifSp = Resources.Load<Sprite>("Art/Ortak/Samdan_Pasif");
        Sprite samdanAktifSp = Resources.Load<Sprite>("Art/Ortak/Samdan_Aktif");
        Sprite yazitGunahSp = Resources.Load<Sprite>("Art/Ortak/Yazit_Gunah");
        Sprite sunakErdemSp = Resources.Load<Sprite>("Art/Ortak/Sunak_Erdem");

        // 1. Sistemler ve HUD (Toplam 24 Ziyafet Kirintisi, Hedef Sure 270 sn = 4.5 dk)
        GameObject sistemler = new GameObject("Bolum1_Sistemler");
        sistemler.AddComponent<BolumAkisYoneticisi>().BolumAyarla(bolumNo, 270f);
        sistemler.AddComponent<OyunIciHUD>().BolumBilgisiAyarla(bolumNo, 24);
        sistemler.AddComponent<PerdeRadari>();
        sistemler.AddComponent<BolumTemaUygulayici>().BolumNumarasiAyarla(bolumNo);

        // 2. Kamera
        GameObject kamObj = new GameObject("Main Camera");
        kamObj.tag = "MainCamera";
        Camera cam = kamObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.2f;
        cam.backgroundColor = tema.gokyuzuRengi;
        kamObj.AddComponent<AudioListener>();
        SevenVeilsKamera akilliKamera = kamObj.AddComponent<SevenVeilsKamera>();
        akilliKamera.SinirlariAyarla(new Vector2(-2f, -12f), new Vector2(515f, 30f));

        // 3. Cok Katmanli Parallax Arka Plan (16 birim genisliginde 512px @ 32 PPU)
        for (int i = -1; i <= 34; i++)
        {
            GameObject uzak = new GameObject($"Parallax_Uzak_{i}");
            uzak.transform.position = new Vector3(i * 16f, 5.5f, 8f);
            uzak.transform.localScale = new Vector3(1.01f, 2.2f, 1f);
            SpriteRenderer uSr = uzak.AddComponent<SpriteRenderer>();
            uSr.sprite = parallaxUzakSp;
            uSr.sortingOrder = -20;
            uzak.AddComponent<ParallaxArkaPlan>().Ayarla(0.42f, 0.14f, 0f);

            GameObject orta = new GameObject($"Parallax_Orta_{i}");
            orta.transform.position = new Vector3(i * 16f + 3f, 4.5f, 5f);
            orta.transform.localScale = new Vector3(1.01f, 2.1f, 1f);
            SpriteRenderer oSr = orta.AddComponent<SpriteRenderer>();
            oSr.sprite = parallaxOrtaSp;
            oSr.sortingOrder = -12;
            orta.AddComponent<ParallaxArkaPlan>().Ayarla(0.20f, 0.08f, 0f);
        }

        // Atmosferik Altin Toz / Mum Kivilcimi Partikulleri
        GameObject tozObj = new GameObject("Atmosfer_AltinToz");
        tozObj.transform.position = new Vector3(250f, 8f, -1f);
        ParticleSystem ps = tozObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = 0.45f;
        main.startSize = 0.14f;
        main.maxParticles = 450;
        main.startColor = new Color(1f, 0.80f, 0.36f, 0.55f);
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(520f, 32f, 1f);
        var psRenderer = tozObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Sprites/Default"));
        psRenderer.sortingOrder = 12;

        // 4. Oyuncu Karakteri (Ressam: 1.35 x 1.55 birim)
        GameObject oyuncu = new GameObject("Ressam_Oyuncu");
        oyuncu.tag = "Player";
        oyuncu.transform.position = new Vector3(2f, 1.2f, 0f);
        oyuncu.transform.localScale = new Vector3(1.35f, 1.55f, 1f);
        SpriteRenderer oyuncuSr = oyuncu.AddComponent<SpriteRenderer>();
        oyuncuSr.sprite = karakterSp;
        oyuncuSr.sortingOrder = 10;

        Rigidbody2D rb = oyuncu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.2f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D oyuncuCol = oyuncu.AddComponent<BoxCollider2D>();
        oyuncuCol.size = new Vector2(0.56f, 0.96f);

        SevenVeilsKontrolcu kontrolcu = oyuncu.AddComponent<SevenVeilsKontrolcu>();
        kontrolcu.AktifBoostuAyarla(SevenVeilsKontrolcu.GunahBoostTipi.DoyumsuzAdim);
        akilliKamera.HedefiAyarla(oyuncu.transform);

        // Alt Ucurum Olum Bolgesi
        GameObject ucurum = new GameObject("Ucurum_Tehlike");
        ucurum.transform.position = new Vector3(260f, -16f, 0f);
        BoxCollider2D ucurumCol = ucurum.AddComponent<BoxCollider2D>();
        ucurumCol.size = new Vector2(600f, 4f);
        ucurumCol.isTrigger = true;
        ucurum.AddComponent<TehlikeEngeli>().Ayarla(bolumNo, "Mahzen Boslugu", false);

        // =========================================================================
        // FAZ 1: ALTIN ZIYAFET AVLUSU (Hiz & Boost Zinciri, X: -2 -> 112)
        // =========================================================================
        PlatformEkle("Zemin_Giris", new Vector2(8f, -1f), new Vector2(22f, 2f), platformSp); // Ust yuzey: Y = 0.0
        TabloEkle("Tablo_0_Sofra", new Vector2(11f, 3.6f), new Vector2(1.25f, 1.25f), tablo0Sp);

        PlatformEkle("Zemin_Basamak1", new Vector2(23.5f, 0.5f), new Vector2(6.5f, 1.4f), platformSp); // Ust: 1.2
        ParcaEkle(bolumNo, new Vector2(23.5f, 2.0f), parcaSp); // 1

        PlatformEkle("Zemin_Basamak2", new Vector2(33.5f, 2.0f), new Vector2(6.5f, 1.4f), platformSp); // Ust: 2.7
        ParcaEkle(bolumNo, new Vector2(33.5f, 3.5f), parcaSp); // 2

        // Ust balkon (Ziplama Yayi ile cikilan odul rotasi)
        YayEkle(new Vector2(35.8f, 3.15f), yaySp, 22.0f);
        PlatformEkle("Zemin_UstBalkon1", new Vector2(46.5f, 8.0f), new Vector2(13f, 1.2f), platformSp); // Ust: 8.6
        ParcaEkle(bolumNo, new Vector2(43.5f, 9.4f), parcaSp); // 3
        ParcaEkle(bolumNo, new Vector2(49.5f, 9.4f), parcaSp); // 4

        // Alt avlu (Yuzey Y = 2.0 -> Dusman merkezi Y = 2.625)
        PlatformEkle("Zemin_AvluOrta", new Vector2(54f, 1.0f), new Vector2(24f, 2f), platformSp);
        DusmanEkle(bolumNo, "Ziyafet Muhafizi", new Vector2(49f, 2.65f), 4.0f, 2.8f, true, DevriyeDusman.DusmanArketipi.EzilebilirMuhafiz, muhafizSp);

        GameObject odulKutu1 = PlatformEkle("OdulBlogu_Faz1", new Vector2(56f, 5.1f), new Vector2(1.2f, 1.2f), platformSp);
        odulKutu1.GetComponent<SpriteRenderer>().color = new Color(1f, 0.86f, 0.36f);
        GameObject gizliParca1 = ParcaEkle(bolumNo, new Vector2(56f, 6.4f), parcaSp); // 5
        odulKutu1.AddComponent<AlttanOdulBlok>().HedefBagla(gizliParca1, false);

        // Ana Rota Tek Gunah Yaziti (Yuzey Y = 2.0 -> Yazit merkezi Y = 2.6)
        YazitEkle(bolumNo, false, new Vector2(62f, 2.6f), yazitGunahSp,
            "I. PERDE — OBURLUK YAZITI",
            "VEIL I — GLUTTONY INSCRIPTION",
            "\"Tuvaldeki kusursuz cizgiye duydugum aclik buyudukce, sofradaki tek kisilik sessizligi gormez oldum.\"",
            "\"As my hunger for the flawless line on canvas grew, I grew blind to the one-sided silence at our table.\"");

        // Menzilli Kahin Terasi (Yuzey Y = 2.4 -> Kahin merkezi Y = 3.05)
        PlatformEkle("Zemin_KahinTeras", new Vector2(82f, 1.4f), new Vector2(22f, 2f), platformSp);
        DusmanEkle(bolumNo, "Oburluk Kahini", new Vector2(88f, 3.05f), 1.6f, 1.5f, true, DevriyeDusman.DusmanArketipi.MenzilliKahin, kahinSp);
        ParcaEkle(bolumNo, new Vector2(78f, 3.2f), parcaSp); // 6
        ParcaEkle(bolumNo, new Vector2(91f, 3.2f), parcaSp); // 7

        PlatformEkle("Zemin_GecisKopru", new Vector2(101f, 2.0f), new Vector2(8f, 1.4f), platformSp);

        // =========================================================================
        // FAZ 2: COKEN AVIZELER & IKILI ATES-SU BULMACASI (X: 110 -> 218)
        // =========================================================================
        TabloEkle("Tablo_1_Kadeh", new Vector2(124f, 6.4f), new Vector2(1.25f, 1.25f), tablo1Sp);

        CokenTasEkle("Coken_Tas1", new Vector2(110.5f, 2.2f), new Vector2(4.2f, 0.9f), 0.62f, 2.8f, platformSp);
        CokenTasEkle("Coken_Tas2", new Vector2(118.5f, 2.9f), new Vector2(4.2f, 0.9f), 0.62f, 2.8f, platformSp);
        ParcaEkle(bolumNo, new Vector2(118.5f, 4.1f), parcaSp); // 8

        CokenTasEkle("Coken_Tas3", new Vector2(126.5f, 3.6f), new Vector2(4.2f, 0.9f), 0.62f, 2.8f, platformSp);
        CokenTasEkle("Coken_Tas4", new Vector2(134.5f, 2.7f), new Vector2(4.2f, 0.9f), 0.62f, 2.8f, platformSp);
        ParcaEkle(bolumNo, new Vector2(134.5f, 3.9f), parcaSp); // 9

        // Ates-Su Tarzi Cift Kademeli Kapi & Basinc Plakasi Bulmacasi (Yuzey Y = 2.5)
        PlatformEkle("Zemin_BulmacaSalon", new Vector2(165f, 1.5f), new Vector2(48f, 2f), platformSp);
        DusmanEkle(bolumNo, "Zirhli Sofra Bekcisi", new Vector2(156f, 3.15f), 4.2f, 2.6f, false, DevriyeDusman.DusmanArketipi.OndenZirhliBekci, zirhliSp);

        PlatformEkle("Zemin_BulmacaSol1", new Vector2(147f, 4.5f), new Vector2(4.5f, 1.0f), platformSp);
        PlatformEkle("Zemin_BulmacaSol2", new Vector2(153.5f, 6.8f), new Vector2(5.5f, 1.0f), platformSp); // Ust: 7.3
        ParcaEkle(bolumNo, new Vector2(153.5f, 8.1f), parcaSp); // 10

        GameObject kapi1 = PlatformEkle("Zemin_KilitliKapi1", new Vector2(164f, 5.0f), new Vector2(1.4f, 5.0f), platformSp);
        kapi1.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.65f, 0.25f);
        PlakaEkle("Plaka_1", new Vector2(155.2f, 7.45f), kapi1.transform, new Vector3(0f, 5.0f, 0f));

        DikenEkle(bolumNo, new Vector2(170.5f, 2.95f), dikenSp);
        YayEkle(new Vector2(175.0f, 2.95f), yaySp, 21.5f);
        PlatformEkle("Zemin_BulmacaSagBalkon", new Vector2(178f, 7.6f), new Vector2(9f, 1.0f), platformSp); // Ust: 8.1
        ParcaEkle(bolumNo, new Vector2(176.5f, 8.9f), parcaSp); // 11

        GameObject kapi2 = PlatformEkle("Zemin_KilitliKapi2", new Vector2(184f, 5.0f), new Vector2(1.4f, 5.0f), platformSp);
        kapi2.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.48f, 0.22f);
        PlakaEkle("Plaka_2", new Vector2(180.8f, 8.25f), kapi2.transform, new Vector3(0f, 5.0f, 0f));

        // =========================================================================
        // BOLUM ORTASI TEK CHECKPOINT (PIRINC SAMDAN - X: 200)
        // =========================================================================
        PlatformEkle("Zemin_Checkpoint", new Vector2(200f, 1.5f), new Vector2(18f, 2f), platformSp); // Ust: 2.5
        GameObject cp = new GameObject("BolumOrtasi_Checkpoint");
        cp.transform.position = new Vector3(200f, 3.15f, 0f);
        cp.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        SpriteRenderer cpSr = cp.AddComponent<SpriteRenderer>();
        cpSr.sprite = samdanPasifSp;
        cpSr.sortingOrder = 4;
        BoxCollider2D cpCol = cp.AddComponent<BoxCollider2D>();
        cpCol.isTrigger = true;
        cp.AddComponent<AraKontrolNoktasi>().SpritelariAyarla(samdanPasifSp, samdanAktifSp);

        // =========================================================================
        // FAZ 3: DERIN MAHZEN KUYUSU — DUVAR SEKMESI & HAREKETLI KOPRULER (X: 212 -> 325)
        // =========================================================================
        PlatformEkle("Duvar_BacaSol", new Vector2(213f, 9.5f), new Vector2(1.8f, 15f), platformSp);
        PlatformEkle("Duvar_BacaSag", new Vector2(219.5f, 7.5f), new Vector2(1.8f, 15f), platformSp);
        ParcaEkle(bolumNo, new Vector2(216.2f, 8.5f), parcaSp); // 12
        ParcaEkle(bolumNo, new Vector2(216.2f, 13.5f), parcaSp); // 13

        PlatformEkle("Zemin_BacaZirve", new Vector2(228f, 14.0f), new Vector2(14f, 2f), platformSp); // Ust: 15.0
        DusmanEkle(bolumNo, "Ziyafet Muhafizi", new Vector2(229f, 15.65f), 3.4f, 3.0f, true, DevriyeDusman.DusmanArketipi.EzilebilirMuhafiz, muhafizSp);

        // Senkronize hareketli tas kopruler (Arada bosluk birakilarak cakisma engellendi)
        HareketliPlatformEkle("Hareketli_Kopru1", new Vector2(238f, 14.0f), new Vector2(5.0f, 1f), Vector2.right, 5.2f, 2.1f, 0f, platformSp);
        HareketliPlatformEkle("Hareketli_Kopru2", new Vector2(251f, 12.6f), new Vector2(5.0f, 1f), Vector2.right, 5.2f, 2.1f, Mathf.PI, platformSp);
        ParcaEkle(bolumNo, new Vector2(253.5f, 14.2f), parcaSp); // 14

        // Orta mahzen adasi (Ust: 12.5)
        PlatformEkle("Zemin_OrtaAda", new Vector2(269f, 11.5f), new Vector2(18f, 2f), platformSp);
        DusmanEkle(bolumNo, "Oburluk Kahini", new Vector2(273f, 13.15f), 1.8f, 1.6f, true, DevriyeDusman.DusmanArketipi.MenzilliKahin, kahinSp);

        GameObject odulKutu2 = PlatformEkle("OdulBlogu_Faz3", new Vector2(266f, 15.6f), new Vector2(1.2f, 1.2f), platformSp);
        odulKutu2.GetComponent<SpriteRenderer>().color = new Color(1f, 0.86f, 0.36f);
        GameObject gizliParca2 = ParcaEkle(bolumNo, new Vector2(266f, 16.9f), parcaSp); // 15
        odulKutu2.AddComponent<AlttanOdulBlok>().HedefBagla(gizliParca2, false);

        // Dikey asansor ve ust gecit
        HareketliPlatformEkle("Hareketli_Asansor", new Vector2(283f, 10.0f), new Vector2(5.0f, 1f), Vector2.up, 4.5f, 2.1f, 0f, platformSp);
        PlatformEkle("Zemin_KirisInis", new Vector2(298f, 10.5f), new Vector2(18f, 2f), platformSp); // Ust: 11.5
        DusmanEkle(bolumNo, "Zirhli Sofra Bekcisi", new Vector2(298f, 12.15f), 4.2f, 2.8f, false, DevriyeDusman.DusmanArketipi.OndenZirhliBekci, zirhliSp);
        ParcaEkle(bolumNo, new Vector2(304f, 12.4f), parcaSp); // 16

        // =========================================================================
        // FAZ 4: KIZIL MAHZEN KOPRULERI (Ekstra Meydan Okuma, X: 312 -> 395)
        // =========================================================================
        CokenTasEkle("Coken_Kopru1", new Vector2(312f, 10.5f), new Vector2(4.0f, 0.9f), 0.58f, 2.6f, platformSp);
        CokenTasEkle("Coken_Kopru2", new Vector2(320f, 11.2f), new Vector2(4.0f, 0.9f), 0.58f, 2.6f, platformSp);
        ParcaEkle(bolumNo, new Vector2(320f, 12.4f), parcaSp); // 17

        PlatformEkle("Zemin_AraKule", new Vector2(330f, 11.8f), new Vector2(10f, 1.6f), platformSp); // Ust: 12.6
        DusmanEkle(bolumNo, "Oburluk Kahini", new Vector2(332f, 13.25f), 1.4f, 1.5f, true, DevriyeDusman.DusmanArketipi.MenzilliKahin, kahinSp);
        ParcaEkle(bolumNo, new Vector2(327f, 13.5f), parcaSp); // 18

        // Dash + Duvar sekmesi kombo kulesi
        PlatformEkle("Duvar_KuleSol", new Vector2(342f, 16.5f), new Vector2(1.6f, 8.5f), platformSp);
        PlatformEkle("Duvar_KuleSag", new Vector2(348.2f, 15.0f), new Vector2(1.6f, 8.5f), platformSp);
        ParcaEkle(bolumNo, new Vector2(345.1f, 17.5f), parcaSp); // 19

        PlatformEkle("Zemin_KuleCikis", new Vector2(358f, 19.0f), new Vector2(14f, 1.6f), platformSp); // Ust: 19.8
        DusmanEkle(bolumNo, "Ziyafet Muhafizi", new Vector2(358f, 20.45f), 3.6f, 3.2f, true, DevriyeDusman.DusmanArketipi.EzilebilirMuhafiz, muhafizSp);
        ParcaEkle(bolumNo, new Vector2(363f, 20.7f), parcaSp); // 20

        // =========================================================================
        // FAZ 5: ZIYAFET SALONU FINAL PARKURU & BITIS KAPISI (X: 370 -> 495)
        // =========================================================================
        CokenTasEkle("Coken_Final1", new Vector2(371f, 18.5f), new Vector2(4.0f, 0.9f), 0.58f, 2.6f, platformSp);
        CokenTasEkle("Coken_Final2", new Vector2(379f, 17.5f), new Vector2(4.0f, 0.9f), 0.58f, 2.6f, platformSp);
        ParcaEkle(bolumNo, new Vector2(379f, 18.7f), parcaSp); // 21

        PlatformEkle("Zemin_FinalSutun", new Vector2(388.5f, 16.5f), new Vector2(6.5f, 1.5f), platformSp);

        // Son Salon (Ust: 16.5)
        PlatformEkle("Zemin_SonSalonAlt", new Vector2(428f, 15.5f), new Vector2(62f, 2f), platformSp);
        DusmanEkle(bolumNo, "Ziyafet Muhafizi", new Vector2(406f, 17.15f), 3.8f, 3.2f, true, DevriyeDusman.DusmanArketipi.EzilebilirMuhafiz, muhafizSp);
        DikenEkle(bolumNo, new Vector2(416f, 16.95f), dikenSp);
        DusmanEkle(bolumNo, "Zirhli Sofra Bekcisi", new Vector2(426f, 17.15f), 3.5f, 2.8f, false, DevriyeDusman.DusmanArketipi.OndenZirhliBekci, zirhliSp);
        DusmanEkle(bolumNo, "Oburluk Kahini", new Vector2(440f, 17.15f), 2.0f, 1.6f, true, DevriyeDusman.DusmanArketipi.MenzilliKahin, kahinSp);

        YayEkle(new Vector2(411f, 16.95f), yaySp, 22.0f);
        PlatformEkle("Zemin_SonSalonUst", new Vector2(426f, 22.2f), new Vector2(20f, 1.1f), platformSp);
        ParcaEkle(bolumNo, new Vector2(421f, 23.5f), parcaSp); // 22
        ParcaEkle(bolumNo, new Vector2(431f, 23.5f), parcaSp); // 23

        // Bitis Kapisi (Yuzey Y = 16.5 -> Kapi merkezi Y = 18.0)
        GameObject bitisKapisi = new GameObject("Bolum1_BitisKapisi");
        bitisKapisi.transform.position = new Vector3(452f, 18.0f, 0f);
        SpriteRenderer kapiSr = bitisKapisi.AddComponent<SpriteRenderer>();
        kapiSr.sprite = kapiSp;
        kapiSr.sortingOrder = 5;
        BoxCollider2D kapiCol = bitisKapisi.AddComponent<BoxCollider2D>();
        kapiCol.isTrigger = true;
        bitisKapisi.AddComponent<BolumBitisKapisi>().Ayarla(bolumNo, 270f);

        // =========================================================================
        // MESAKKATLI & TAMAMEN KARANLIK PERDELI GIZLI ODA (OLCULULUK / TEMPERANCE)
        // Zemin_FinalSutun (X: 388.5) altindaki gizli mahzen gecidinden ulasilir
        // =========================================================================
        PlatformEkle("Zemin_GizliAltGiris", new Vector2(394f, 6.5f), new Vector2(14f, 1.2f), platformSp); // Ust: 7.1
        DikenEkle(bolumNo, new Vector2(397f, 7.55f), dikenSp);

        // Kafa atma blogu ve kapi duvari icin genis ve ferah oda (Tavan Y = 12.2, Zemin Y = 5.5, Blok Y = 9.2)
        PlatformEkle("Zemin_GizliAltKopru", new Vector2(409f, 5.5f), new Vector2(12f, 1.2f), platformSp); // Ust: 6.1
        DusmanEkle(bolumNo, "Zirhli Mahzen Gardiyani", new Vector2(407f, 6.75f), 2.0f, 2.2f, false, DevriyeDusman.DusmanArketipi.OndenZirhliBekci, zirhliSp);

        // Kilitli gizli oda muhru ve ustteki rahatca kafa atilabilen kilit blogu
        GameObject gizliKapakDuvar = PlatformEkle("Duvar_GizliOdaKapagi", new Vector2(417f, 8.8f), new Vector2(1.4f, 5.4f), platformSp);

        GameObject gizliKilitBlok = PlatformEkle("OdulBlogu_GizliGecit", new Vector2(413.2f, 9.2f), new Vector2(1.3f, 1.3f), platformSp);
        gizliKilitBlok.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.72f, 0.28f);
        gizliKilitBlok.AddComponent<AlttanOdulBlok>().HedefBagla(gizliKapakDuvar, true);

        // Gizli Oda Zemin (Ust: 6.1), Tavan (Alt: 12.4) ve Sag Duvar
        PlatformEkle("Zemin_GizliOdaTaban", new Vector2(432f, 5.5f), new Vector2(30f, 1.2f), platformSp);
        PlatformEkle("Zemin_GizliOdaTavan", new Vector2(432f, 13.0f), new Vector2(32f, 1.2f), platformSp);
        PlatformEkle("Duvar_GizliOdaSag", new Vector2(446.5f, 9.2f), new Vector2(1.4f, 7.6f), platformSp);

        // Gizli Oda Icindeki 3 Zorunlu Oge:
        TabloEkle("EsPortresi_Bolum1", new Vector2(424.5f, 9.2f), new Vector2(0.95f, 0.95f), esSp);

        YazitEkle(bolumNo, true, new Vector2(433f, 6.6f), sunakErdemSp,
            "I. ERDEM SUNAGI — OLCULULUK (TEMPERANCE)",
            "I. VIRTUE ALTAR — TEMPERANCE",
            "\"Gercek sanat tuvali doyurmak degil, yanindaki canin sesini duyabilmektir. Ilk perde aralandi.\"",
            "\"True art is not gorging the canvas, but hearing the voice beside you. The first veil parts.\"");

        ParcaEkle(bolumNo, new Vector2(441f, 6.9f), parcaSp); // 24. ve son parca

        // Gizli Odayi Disaridan Tamamen Gorunmez Kilan Opak Karanlik Perde Ortusu
        GameObject gizliPerde = new GameObject("GizliOda_PerdeOrtusu");
        gizliPerde.transform.position = new Vector3(431.8f, 9.2f, 0f);
        BoxCollider2D perdeCol = gizliPerde.AddComponent<BoxCollider2D>();
        perdeCol.size = new Vector2(28.0f, 6.3f);
        perdeCol.isTrigger = true;
        gizliPerde.AddComponent<GizliOdaOrtusu>().BolumNumarasiAyarla(bolumNo);

        EditorSceneManager.SaveScene(sahne, "Assets/Scenes/Bolum1.unity");
    }

    private static GameObject PlatformEkle(string ad, Vector2 konum, Vector2 boyut, Sprite sprite)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.position = new Vector3(konum.x, konum.y, 0f);
        obj.transform.localScale = new Vector3(boyut.x, boyut.y, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        return obj;
    }

    private static void CokenTasEkle(string ad, Vector2 konum, Vector2 boyut, float gecikme, float yenilenme, Sprite sprite)
    {
        GameObject obj = PlatformEkle(ad, konum, boyut, sprite);
        obj.GetComponent<SpriteRenderer>().color = new Color(0.95f, 0.78f, 0.55f);
        obj.AddComponent<CokenTasPlatform>().Ayarla(gecikme, yenilenme);
    }

    private static void HareketliPlatformEkle(string ad, Vector2 konum, Vector2 boyut, Vector2 yon, float mesafe, float hiz, float faz, Sprite sprite)
    {
        GameObject obj = PlatformEkle(ad, konum, boyut, sprite);
        obj.AddComponent<Rigidbody2D>();
        obj.AddComponent<HareketliPlatform>().Ayarla(yon, mesafe, hiz, faz);
    }

    private static GameObject ParcaEkle(int bolumNo, Vector2 konum, Sprite sprite)
    {
        GameObject obj = new GameObject($"ZiyafetKirintisi_{konum.x:F0}_{konum.y:F0}");
        obj.transform.position = new Vector3(konum.x, konum.y, 0f);
        obj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 6;
        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.48f;
        obj.AddComponent<ToplanabilirParca>().BolumNumarasiAyarla(bolumNo);
        return obj;
    }

    private static void DusmanEkle(int bolumNo, string ad, Vector2 konum, float mesafe, float hiz, bool ezilebilir, DevriyeDusman.DusmanArketipi tip, Sprite sprite)
    {
        GameObject obj = new GameObject($"Dusman_{ad}");
        obj.transform.position = new Vector3(konum.x, konum.y, 0f);
        obj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 7;
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.82f, 0.92f);
        obj.AddComponent<DevriyeDusman>().Ayarla(bolumNo, ad, mesafe, hiz, ezilebilir, tip);
    }

    private static void YayEkle(Vector2 konum, Sprite sprite, float guc)
    {
        GameObject obj = new GameObject($"ZiplamaYayi_{konum.x:F0}");
        obj.transform.position = new Vector3(konum.x, konum.y, 0f);
        obj.transform.localScale = new Vector3(1.1f, 0.9f, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.90f, 0.55f);
        obj.AddComponent<ZiplamaYayi>().GucAyarla(guc);
    }

    private static void DikenEkle(int bolumNo, Vector2 konum, Sprite sprite)
    {
        GameObject diken = new GameObject($"DikenliTuzak_{konum.x:F0}");
        diken.transform.position = new Vector3(konum.x, konum.y, 0f);
        diken.transform.localScale = new Vector3(1.3f, 0.9f, 1f);
        SpriteRenderer dikenSr = diken.AddComponent<SpriteRenderer>();
        dikenSr.sprite = sprite;
        dikenSr.sortingOrder = 5;
        BoxCollider2D dikenCol = diken.AddComponent<BoxCollider2D>();
        dikenCol.isTrigger = true;
        dikenCol.size = new Vector2(0.88f, 0.55f);
        diken.AddComponent<TehlikeEngeli>().Ayarla(bolumNo, "Dikenli Tuzak", true);
    }

    private static void PlakaEkle(string ad, Vector2 konum, Transform kapi, Vector3 ofset)
    {
        GameObject plaka = new GameObject(ad);
        plaka.transform.position = new Vector3(konum.x, konum.y, 0f);
        plaka.transform.localScale = new Vector3(1.8f, 0.30f, 1f);
        SpriteRenderer plakaSr = plaka.AddComponent<SpriteRenderer>();
        plakaSr.sprite = SevenVeilsGorselFabrikasi.DuzKareSpriteOlustur();
        plakaSr.color = new Color(0.92f, 0.42f, 0.22f);
        plakaSr.sortingOrder = 4;
        BoxCollider2D plakaCol = plaka.AddComponent<BoxCollider2D>();
        plakaCol.isTrigger = true;
        plaka.AddComponent<BasincPlakasiVeKapi>().KapiBagla(kapi, ofset, true);
    }

    private static void TabloEkle(string ad, Vector2 konum, Vector2 boyut, Sprite sprite)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.position = new Vector3(konum.x, konum.y, 2f);
        obj.transform.localScale = new Vector3(boyut.x, boyut.y, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -4;
    }

    private static void YazitEkle(int bolumNo, bool erdemMi, Vector2 konum, Sprite sprite, string bTR, string bEN, string mTR, string mEN)
    {
        GameObject obj = new GameObject(erdemMi ? $"ErdemSunagi_{bolumNo}" : $"GunahYaziti_{bolumNo}");
        obj.transform.position = new Vector3(konum.x, konum.y, 0f);
        obj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.6f, 1.2f);
        obj.AddComponent<AnlatimYaziti>().IcerikAyarla(bolumNo, erdemMi, bTR, bEN, mTR, mEN);
    }

    private static void BuildAyarlariniGuncelle()
    {
        List<EditorBuildSettingsScene> sahneler = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/AnaMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Bolum1.unity", true)
        };
        EditorBuildSettings.scenes = sahneler.ToArray();
    }
}
#endif
