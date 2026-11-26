using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class Checkpoint : MonoBehaviour
    {
        private bool aktifEdildi = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (aktifEdildi) return;
            if (other.GetComponent<PlayerController>() == null) return;

            aktifEdildi = true;
            PlatformerModel model = Simulation.GetModel<PlatformerModel>();
            if (model != null && model.spawnPoint != null)
            {
                model.spawnPoint.position = transform.position;
            }

            SpriteRenderer gorunum = GetComponent<SpriteRenderer>();
            if (gorunum != null)
            {
                gorunum.color = Color.green;
            }

            Telemetri.OlayKaydet("checkpoint", Telemetri.AktifBolum, transform.position.x, transform.position.y);
        }
    }
}
