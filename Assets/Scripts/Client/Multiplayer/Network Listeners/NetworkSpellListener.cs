﻿using Common;
using Core;

using EventHandler = Common.EventHandler;

namespace Client
{
    public partial class PhotonBoltClientListener
    {
        public override void OnEvent(SpellCastRequestAnswerEvent answer)
        {
            base.OnEvent(answer);

            if (answer.Result == (int) SpellCastResult.Success)
            {
                var token = answer.ProcessingEntries as SpellProcessingToken;
                EventHandler.ExecuteEvent(EventHandler.GlobalDispatcher, GameEvents.SpellLaunched, (Unit)LocalPlayer, answer.SpellId, token, answer.Source);
            }
            else
                EventHandler.ExecuteEvent(EventHandler.GlobalDispatcher, GameEvents.ClientSpellFailed, (SpellCastResult) answer.Result);
        }

        public override void OnEvent(SpellDamageDoneEvent spellDamageEvent)
        {
            base.OnEvent(spellDamageEvent);

            if (LocalPlayer == null || !World.UnitManager.TryFind(spellDamageEvent.Target.PackedValue, out Unit target))
                return;

            EventHandler.ExecuteEvent(EventHandler.GlobalDispatcher, GameEvents.SpellDamageDone, (Unit)LocalPlayer, target, spellDamageEvent.DamageAmount, spellDamageEvent.IsCrit);
        }

        public override void OnEvent(SpellHealingDoneEvent spellHealingEvent)
        {
            base.OnEvent(spellHealingEvent);

            if (LocalPlayer == null || !World.UnitManager.TryFind(spellHealingEvent.Target.PackedValue, out Unit target))
                return;

            EventHandler.ExecuteEvent(EventHandler.GlobalDispatcher, GameEvents.SpellHealingDone, (Unit)LocalPlayer, target, spellHealingEvent.HealAmount, spellHealingEvent.IsCrit);
        }

        public override void OnEvent(SpellPlayerTeleportEvent teleportEvent)
        {
            base.OnEvent(teleportEvent);

            if (LocalPlayer != null)
            {
                LocalPlayer.Position = teleportEvent.TargetPosition;
                LocalPlayer.MovementInfo.RemoveMovementFlag(MovementFlags.Ascending);
            }
        }

        public override void OnEvent(SpellCooldownEvent cooldownEvent)
        {
            base.OnEvent(cooldownEvent);

            if (LocalPlayer.ExistsIn(World))
                LocalPlayer.SpellHistory.Handle(cooldownEvent);
        }
    }
}
