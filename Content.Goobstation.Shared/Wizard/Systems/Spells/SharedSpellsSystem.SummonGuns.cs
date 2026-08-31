
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard.LesserSummonGuns;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnLesserSummonGuns(LesserSummonGunsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var gun = PredictedSpawnItemInHands(ev.Performer, ev.Proto, ev.Action);
        if (gun == null)
            return;

        var comp = EnsureComp<EnchantedBoltActionRifleComponent>(gun.Value);
        ev.Handled = true;
    }
}