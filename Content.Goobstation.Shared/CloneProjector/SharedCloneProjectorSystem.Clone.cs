// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Goobstation.Shared.CloneProjector;

public partial class SharedCloneProjectorSystem
{
    public void InitializeClone()
    {
        SubscribeLocalEvent<CloneComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<CloneComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<CloneComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMobStateChanged(Entity<CloneComponent> clone, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsIncapacitated(clone)
            || clone.Comp.HostProjector is not { } projector
            || _net.IsClient)
            return;

        TryInsertClone(projector, true);
        RaiseLocalEvent(clone, new RejuvenateEvent(true, false));

        if (clone.Comp.HostEntity is not { } host)
            return;

        var destroyedPopup = Loc.GetString("gemini-projector-clone-destroyed");
        _popup.PopupEntity(destroyedPopup, host, host, PopupType.LargeCaution);
        _stun.TryParalyze(host, projector.Comp.StunDuration, true);
    }

    private void OnMeleeHit(Entity<CloneComponent> clone, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || clone.Comp.HostEntity is not { } host)
            return;

        // Stop clones from punching their host.
        // Don't be a shitter.
        foreach (var hitEntity in args.HitEntities)
        {
            if (hitEntity != host)
                continue;

            args.BonusDamage = -args.BaseDamage;
        }
    }
    private void OnExamined(Entity<CloneComponent> clone, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || clone.Comp.HostProjector is not { } projector)
            return;

        var flavor = Loc.GetString(projector.Comp.FlavorText);
        args.PushMarkup(flavor);
    }
}
