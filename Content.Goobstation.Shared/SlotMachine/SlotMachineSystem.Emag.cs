using Content.Shared.Coordinates;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Storage;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SlotMachine;

public sealed partial class SlotMachineSystem
{
    /// <summary>
    /// Gives a random prize on emag doafter.
    /// </summary>
    private void OnEmagged(Entity<SlotMachineComponent> ent, ref GotEmaggedEvent args)
    {
        if (ent.Comp.IsSpinning || ent.Comp.EmagPrizes is null)
            return;

        EnsureComp<EmaggedComponent>(ent);

        args.Handled = true;

        var doAfter = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.DoAfterTime,
            new SlotMachineEmagDoAfterEvent(),
            ent.Owner
        )
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            MultiplyDelay = false,
        };

        _audio.PlayPredicted(ent.Comp.SpinSound, ent, args.UserUid);
        _doAfter.TryStartDoAfter(doAfter);

        SetSpinning(ent, true);
        Dirty(ent);
    }

    private void OnSlotMachineEmagDoAfter(Entity<SlotMachineComponent> ent, ref SlotMachineEmagDoAfterEvent args)
    {
        DebugTools.AssertNotNull(ent.Comp.EmagPrizes);

        RemComp<EmaggedComponent>(ent);

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        foreach (var uid in _prize.HandlePrize(ent.Comp.EmagPrizes!, ent, random))
        {
            _adminLog.Add(
                LogType.EntitySpawn,
                LogImpact.High,
                $"{ToPrettyString(args.User)} got emag prize {ToPrettyString(uid)} from {ToPrettyString(ent)}"
            );
        }

        SetSpinning(ent, false);
        Dirty(ent);
    }
}