using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnThrownLightning(ThrownLightningEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var packet = PredictedSpawnItemInHands(ev.Performer, ev.Proto, ev.Action);
        if (packet == null)
            return;

        if (_net.IsServer)
            _audio.PlayPvs(ev.Sound, packet.Value);

        ev.Handled = true;
    }
}