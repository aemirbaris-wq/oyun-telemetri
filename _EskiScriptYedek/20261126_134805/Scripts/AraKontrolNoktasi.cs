using Platformer.Mechanics;
using UnityEngine;

public class AraKontrolNoktasi : MonoBehaviour
{
    private bool aktifEdildi = false;
    private SpriteRenderer sr;
    private PlayerController oyuncu;
    private float yaziZamanlayici = 0f;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = MesaleSpriteOlustur(false);
        sr.sortingOrder = 2;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0f, 1.0f);
        col.size = new Vector2(2.2f, 2.6f);
    }

    private void Update()
    {
        if (yaziZamanlayici > 0f) yaziZamanlayici -= Time.deltaTime;
        if (aktifEdildi) return;

        if (oyuncu == null)
        {
            oyuncu = FindFirstObjectByType<PlayerController>();
            return;
        }

        if (Vector2.Distance(oyuncu.transform.position, transform.position + Vector3.up * 0.8f) <= 1.55f)
        {
            AktifEt();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (aktifEdildi) return;
        if (other.GetComponentInParent<PlayerController>() != null)
        {
            AktifEt();
        }
    }

    private void AktifEt()
    {
        aktifEdildi = true;
        yaziZamanlayici = 2.6f;

        if (sr != null) sr.sprite = MesaleSpriteOlustur(true);

        GameObject spawnObj = GameObject.Find("SpawnPoint");
        if (spawnObj == null) spawnObj = new GameObject("SpawnPoint");
        spawnObj.transform.position = transform.position + new Vector3(0f, 0.6f, 0f);

        GameController gc = FindFirstObjectByType<GameController>();
        if (gc != null && gc.model != null)
        {
            gc.model.spawnPoint = spawnObj.transform;
        }

        if (TematikSesYoneticisi.Ornek != null)
            TematikSesYoneticisi.Ornek.CheckpointSesiCal();

        Telemetri.OlayKaydet("checkpoint", Telemetri.AktifBolum);
    }

    private void OnGUI()
    {
        if (yaziZamanlayici <= 0f || Camera.main == null) return;

        Vector3 ekranPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.3f);
        if (ekranPos.z < 0f) return;

        bool ingilizce = PlayerPrefs.GetString("Dil", "TR") == "EN";
        string metin = ingilizce ? "CHECKPOINT SAVED" : "ARA NOKTA KAYDEDİLDİ";

        GUIStyle stil = new GUIStyle(GUI.skin.label);
        stil.alignment = TextAnchor.MiddleCenter;
        stil.fontSize = 15;
        stil.fontStyle = FontStyle.Bold;
        stil.normal.textColor = new Color(1f, 0.88f, 0.48f, Mathf.Clamp01(yaziZamanlayici));

        float w = 240f;
        GUI.Label(new Rect(ekranPos.x - w * 0.5f, Screen.height - ekranPos.y - 20f, w, 30f), metin, stil);
    }

    private Sprite MesaleSpriteOlustur(bool yanik)
    {
        int w = 24, h = 44;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color tas = new Color(0.36f, 0.39f, 0.45f, 1f);
        Color tasKoyu = new Color(0.22f, 0.24f, 0.28f, 1f);
        Color alevDis = yanik ? new Color(0.96f, 0.52f, 0.14f, 1f) : new Color(0.18f, 0.18f, 0.20f, 0.6f);
        Color alevIc = yanik ? new Color(1f, 0.92f, 0.55f, 1f) : Color.clear;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = Color.clear;
                if (y < 24 && x >= 7 && x <= 16)
                {
                    c = (x == 7 || x == 16 || y % 6 == 0) ? tasKoyu : tas;
                }
                else if (y >= 24 && y < 28 && x >= 5 && x <= 18)
                {
                    c = tasKoyu;
                }
                else if (y >= 28 && y < 40)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(11.5f, 33f));
                    if (dist < 3.0f && yanik) c = alevIc;
                    else if (dist < 5.2f) c = alevDis;
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 24f);
    }
}
