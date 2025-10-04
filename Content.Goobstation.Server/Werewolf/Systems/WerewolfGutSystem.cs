// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.EntityEffects.EffectConditions;
using Content.Server.Roles;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Tag;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles the Werewolf Gut ability.
/// Rips a deceased victim apart, spilling all their chest organs and blood into the floor.
/// Has a do-after.
/// Eating Gutted Organs grants Fury, which can be spent on Mutations.
/// </summary>
public sealed class WerewolfGutSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfGutComponent, WerewolfGutEvent>(OnWerewolfGut);

        SubscribeLocalEvent<WerewolfGutComponent, WerewolfGutDoAfterEvent>(OnDoAfterWerewolfGut);
    }

    private void OnWerewolfGut(Entity<WerewolfGutComponent> ent, ref WerewolfGutEvent args)
    {
        var doAfter = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.Timer,
            new WerewolfGutDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            ColorOverride = Color.Silver,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfterWerewolfGut(Entity<WerewolfGutComponent> ent, ref WerewolfGutDoAfterEvent args)
    {
        if (args.Target is not {} target)
            return;

        TryGut(target, ent.Comp);
    }

    private void TryGut(EntityUid target, WerewolfGutComponent comp)
    {
        if (!HasOrgans(target) ||
            !_mobState.IsDead(target))
            return;

        RemoveOrgansAndSpillBlood(target, comp);
    }

    #region Helpers
    /// <summary>
    ///  Checks if a target is a robot, or is humanoid.
    /// </summary>
    /// <param name="target"></param> The target being checked
    /// <returns></returns>
    private bool HasOrgans(EntityUid target)
    {
        if (HasComp<BorgChassisComponent>(target) ||
            HasComp<SiliconComponent>(target) ||
            !HasComp<HumanoidAppearanceComponent>(target))
            return false;

        return true;
    }

    /// <summary>
    ///  Removes all organs and spills the blood of a target
    /// </summary>
    /// <param name="target"></param> The target of the Gut ability
    /// <param name="comp"></param> The component of the Gut ability
    private void RemoveOrgansAndSpillBlood(EntityUid target, WerewolfGutComponent comp)
    {
        foreach (var (organId, _) in _body.GetBodyOrgans(target))
        {
            if (!_whitelist.IsValid(comp.Whitelist, organId))
                continue;

            AttachFuryToOrgans(target, organId, comp);
            _body.TryRemoveOrgan(organId);
        }

        if (!TryComp<BloodstreamComponent>(target, out var blood))
            return;

        _bloodstream.SpillAllSolutions(target, blood);
    }

    /// <summary>
    ///  Makes the organs provide fury to the werewolf
    /// </summary>
    /// <param name="target"></param> The target of the Gut ability
    /// <param name="organ"></param> The organ of the target
    /// <param name="comp"></param> The component of the Gut ability
    private void AttachFuryToOrgans(EntityUid target, EntityUid organ, WerewolfGutComponent comp)
    {
        if (_tag.HasTag(target, comp.VimPilotTag))
            return;

        EnsureComp<GrantsFuryComponent>(organ);
    }
    #endregion
}
