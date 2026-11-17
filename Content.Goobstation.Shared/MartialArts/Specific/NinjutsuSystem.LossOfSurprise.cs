using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class NinjutsuSystem
{
    [SubscribeLocalEvent]
    private void OnBeforeGunShot(Entity<NinjutsuSneakAttackComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        ResetDebuff(ent);
    }

    /*[SubscribeLocalEvent]
    private void OnAlertEffectEnded(Entity<NinjutsuSneakAttackComponent> ent, ref StatusEffectEndedEvent args)
    {
        if (args.Key == "LossOfSurprise")
            _alerts.ShowAlert(ent.Owner, ent.Comp.Alert);
    }*/

    [SubscribeLocalEvent]
    private void OnSneakAttackRemove(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _alerts.ClearAlertCategory(ent.Owner, NinjutsuAlertCategory);
    }

    [SubscribeLocalEvent]
    private void OnSneakAttackInit(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentInit args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.Alert);
    }

    [SubscribeLocalEvent]
    private void OnThrow(Entity<NinjutsuSneakAttackComponent> ent, ref ThrowEvent ev)
    {
        ResetDebuff(ent);
    }
}
