using Platformer.Mechanics;
using UnityEngine;

public class HareketliPlatform : MonoBehaviour
{
    public Vector2 hareketMesafesi = new Vector2(4f, 0f);
    public float hiz = 2.1f;

    private Vector3 baslangicPos;
    private Vector3 sonKonum;
    private BoxCollider2D boxCol;
    private Rigidbody2D rb;
    private PlayerController oyuncu;
    private Rigidbody2D oyuncuRb;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        baslangicPos = transform.position;
        sonKonum = baslangicPos;
        rb.position = baslangicPos;
    }

    private void FixedUpdate()
    {
        float mesafe = Mathf.Max(0.5f, hareketMesafesi.magnitude);
        float t = Mathf.PingPong(Time.time * hiz / mesafe, 1f);
        t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 hedefPos = baslangicPos + (Vector3)(hareketMesafesi * t);
        Vector3 delta = hedefPos - sonKonum;

        bool oyuncuyuTasi = false;
        if (boxCol != null)
        {
            if (oyuncu == null)
            {
                oyuncu = FindFirstObjectByType<PlayerController>();
                if (oyuncu != null) oyuncuRb = oyuncu.GetComponent<Rigidbody2D>();
            }

            if (oyuncu != null && oyuncu.isActiveAndEnabled && oyuncu.velocity.y <= 0.1f)
            {
                Bounds pBounds = oyuncu.Bounds;
                Bounds platBounds = boxCol.bounds;
                bool yatayUstunde = pBounds.max.x > platBounds.min.x + 0.02f && pBounds.min.x < platBounds.max.x - 0.02f;
                bool dikeyUstunde = pBounds.min.y >= platBounds.max.y - 0.15f && pBounds.min.y <= platBounds.max.y + 0.22f;
                oyuncuyuTasi = yatayUstunde && dikeyUstunde;
            }
        }

        rb.MovePosition(hedefPos);
        sonKonum = hedefPos;

        if (oyuncuyuTasi && oyuncuRb != null && delta.sqrMagnitude > 0.0000001f)
        {
            oyuncuRb.position += (Vector2)delta;
            Physics2D.SyncTransforms();
        }
    }
}
