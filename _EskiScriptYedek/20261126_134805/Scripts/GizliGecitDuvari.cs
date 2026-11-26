using Platformer.Mechanics;
using UnityEngine;

public class GizliGecitDuvari : MonoBehaviour
{
    private SpriteRenderer sr;
    private PlayerController oyuncu;
    private bool kesfedildi = false;
    private float hedefAlfa = 1f;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (oyuncu == null)
        {
            oyuncu = FindFirstObjectByType<PlayerController>();
            return;
        }

        if (sr == null) return;

        Bounds b = sr.bounds;
        Vector3 p = oyuncu.transform.position;
        bool icinde = p.x >= b.min.x - 0.6f && p.x <= b.max.x + 1.2f && p.y >= b.min.y - 0.5f && p.y <= b.max.y + 0.5f;

        if (icinde)
        {
            hedefAlfa = 0.22f;
            if (!kesfedildi)
            {
                kesfedildi = true;
                Telemetri.OlayKaydet("gizli_gecit", Telemetri.AktifBolum);
                if (SesYoneticisi.Ornek != null)
                {
                    SesYoneticisi.Ornek.SendMessage("TabletAcilisCal", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else if (!kesfedildi)
        {
            hedefAlfa = 1f;
        }

        Color c = sr.color;
        c.a = Mathf.Lerp(c.a, hedefAlfa, Time.deltaTime * 6f);
        sr.color = c;
    }
}
