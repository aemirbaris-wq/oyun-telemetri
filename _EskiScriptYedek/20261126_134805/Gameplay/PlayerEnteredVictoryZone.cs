using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;

            int bolum = Telemetri.AktifBolum;
            Telemetri.OlayKaydet("bolum_bitir", bolum);
            if (bolum >= 6)
            {
                Telemetri.OlayKaydet("oyun_bitir", 6);
            }
            Telemetri.BolumSonuGecisiBaslat();
        }
    }
}
