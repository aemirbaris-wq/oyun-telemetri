using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerEnteredDeathZone : Simulation.Event<PlayerEnteredDeathZone>
    {
        public DeathZone deathzone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            string nesneAdi = deathzone != null ? deathzone.gameObject.name.ToLowerInvariant() : "";
            string ustAdi = (deathzone != null && deathzone.transform.parent != null)
                ? deathzone.transform.parent.name.ToLowerInvariant()
                : "";

            if (nesneAdi.Contains("diken") || nesneAdi.Contains("spike") || ustAdi.Contains("diken") || ustAdi.Contains("spike"))
            {
                Telemetri.SonOlumNedeniAta("diken");
            }
            else
            {
                Telemetri.SonOlumNedeniAta("cukur");
            }

            Simulation.Schedule<PlayerDeath>(0);
        }
    }
}
