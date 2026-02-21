using Content.Shared._ES.PainFlash;
using Content.Shared._ES.PainFlash.Components;
using Content.Shared.Damage.Systems;

namespace Content.Server._ES.PainFlash;

/// <inheritdoc/>
public sealed class ESPainFlashSystem : ESSharedPainFlashSystem
{
    protected override void OnDamageChanged(Entity<ESPainFlashComponent> ent, ref DamageChangedEvent args)
    {
        if (!IsPainFlashTrigger(args, out var damage))
            return;

        var ev = new ESPainFlashMessage(damage, Timing.CurTick);
        RaiseNetworkEvent(ev, ent);
    }
}
