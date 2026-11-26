using UnityEngine;

public class ParallaxArkaPlan : MonoBehaviour
{
    [SerializeField] private float parallaxCarpaniX = 0.35f;
    [SerializeField] private float parallaxCarpaniY = 0.12f;
    [SerializeField] private float tekrarGenisligi = 24f;

    private Transform kamera;
    private Vector3 sonKameraKonumu;

    public void Ayarla(float carpanX, float carpanY, float genislik)
    {
        parallaxCarpaniX = carpanX;
        parallaxCarpaniY = carpanY;
        tekrarGenisligi = genislik;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            kamera = Camera.main.transform;
            sonKameraKonumu = kamera.position;
        }
    }

    private void LateUpdate()
    {
        if (kamera == null)
        {
            if (Camera.main != null)
            {
                kamera = Camera.main.transform;
                sonKameraKonumu = kamera.position;
            }
            return;
        }

        Vector3 delta = kamera.position - sonKameraKonumu;
        transform.position += new Vector3(delta.x * parallaxCarpaniX, delta.y * parallaxCarpaniY, 0f);
        sonKameraKonumu = kamera.position;

        if (tekrarGenisligi > 0.1f)
        {
            float farkX = kamera.position.x - transform.position.x;
            if (Mathf.Abs(farkX) >= tekrarGenisligi)
            {
                float kayma = Mathf.Sign(farkX) * tekrarGenisligi;
                transform.position += new Vector3(kayma, 0f, 0f);
            }
        }
    }
}
