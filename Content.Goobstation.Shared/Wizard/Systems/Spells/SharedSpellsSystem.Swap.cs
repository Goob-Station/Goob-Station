using Content.Goobstation.Common.Wizard.Components;
using Content.Goobstation.Common.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    [Dependency] private readonly TheSwapSystem _theSwap = default!;

    private void OnSwap(SwapSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (ev.Performer == ev.Target)
            return;

        if (!TryComp(ev.Action, out SwapSpellComponent? swap))
            return;

        if (!ev.ThroughWalls && !_examine.InRangeUnOccluded(ev.Performer, ev.Target, ev.Range))
            return;

        var userXform = Transform(ev.Performer);
        var targetXform = Transform(ev.Target);

        _theSwap.Swap(ev.Performer, userXform, ev.Target, targetXform, ev.Sound, ev.Effect);

        if (swap.SecondaryTarget != null && Exists(swap.SecondaryTarget) &&
            swap.SecondaryTarget.Value != ev.Target && swap.SecondaryTarget.Value != ev.Performer)
        {
            var secondaryTarget = swap.SecondaryTarget.Value;
            var secondaryTargetXform = Transform(secondaryTarget);

            if (secondaryTargetXform.MapID == userXform.MapID &&
                _xform.InRange((ev.Performer, userXform), (secondaryTarget, secondaryTargetXform), ev.Range))
                _theSwap.Swap(secondaryTarget, secondaryTargetXform, ev.Target, targetXform, ev.Sound, ev.Effect, false);
        }

        swap.SecondaryTarget = null;
        Dirty(ev.Action, swap);
        if (_net.IsServer)
            RaiseNetworkEvent(new StopTargetingEvent(), ev.Performer); // Just in case

        ev.Handled = true;
    }

    private void OnSwapSecondaryTarget(SetSwapSecondaryTarget ev)
    {
        var action = GetEntity(ev.Action);
        var target = GetEntity(ev.Target);

        if (!TryComp(action, out SwapSpellComponent? swap))
            return;

        if (!swap.AllowSecondaryTarget)
            return;

        swap.SecondaryTarget = target;
        Dirty(action, swap);
    }
}