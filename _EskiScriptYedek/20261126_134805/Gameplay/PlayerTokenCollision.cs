using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision>
    {
        public PlayerController player;
        public TokenInstance token;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);
            Telemetri.OlayKaydet("jeton", Telemetri.AktifBolum, token.transform.position.x, token.transform.position.y);
        }
    }
}
