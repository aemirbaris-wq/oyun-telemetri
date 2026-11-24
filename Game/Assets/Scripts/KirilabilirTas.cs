using System.Collections;
using Platformer.Mechanics;
using UnityEngine;

public class KirilabilirTas : MonoBehaviour
{
    public float cokmeGecikmesi = 0.85f;
    public float yenilenmeSuresi = 3.2f;

    private BoxCollider2D boxCol;
    private SpriteRenderer sr;
    private PlayerController oyuncu;
    private Vector3 orijinalPos;
    private bool tetiklendi = false;

    private void Start()
    {
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        orijinalPos = transform.position;
    }

    private void Update()
    {
        if (tetiklendi || boxCol == null || !boxCol.enabled) return;

        if (oyuncu == null) oyuncu = FindFirstObjectByType<PlayerController>();
        if (oyuncu == null || !oyuncu.isActiveAndEnabled || !oyuncu.IsGrounded) return;

        Bounds pBounds = oyuncu.Bounds;
        Bounds tasBounds = boxCol.bounds;
        bool yatayTemas = pBounds.max.x > tasBounds.min.x + 0.05f && pBounds.min.x < tasBounds.max.x - 0.05f;
        bool ustunde = Mathf.Abs(pBounds.min.y - tasBounds.max.y) <= 0.16f;

        if (yatayTemas && ustunde)
        {
            StartCoroutine(CokmeDongusu());
        }
    }

    private IEnumerator CokmeDongusu()
    {
        tetiklendi = true;
        float gecen = 0f;

        // Titreme uyarisi
        while (gecen < cokmeGecikmesi)
        {
            gecen += Time.deltaTime;
            float sarsinti = Mathf.Sin(gecen * 55f) * 0.06f;
            transform.position = orijinalPos + new Vector3(sarsinti, 0f, 0f);
            yield return null;
        }

        // Cokus ve kaybolma
        boxCol.enabled = false;
        float dusus = 0f;
        Color baslangicRenk = sr != null ? sr.color : Color.white;
        while (dusus < 0.35f)
        {
            dusus += Time.deltaTime;
            transform.position += Vector3.down * 4.5f * Time.deltaTime;
            if (sr != null)
            {
                Color c = baslangicRenk;
                c.a = Mathf.Lerp(1f, 0f, dusus / 0.35f);
                sr.color = c;
            }
            yield return null;
        }

        if (sr != null) sr.enabled = false;
        yield return new WaitForSeconds(yenilenmeSuresi);

        // Yeniden olusma
        transform.position = orijinalPos;
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = baslangicRenk;
        }
        boxCol.enabled = true;
        tetiklendi = false;
    }
}
