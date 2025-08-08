using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.EntityEffects.EffectConditions;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Silicons.Borgs.Components;
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

    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfGutComponent, WerewolfGutEvent>(OnWerewolfGut);
    }

    private void OnWerewolfGut(Entity<WerewolfGutComponent> ent, ref WerewolfGutEvent args) =>
        TryGut(args.Target, ent.Comp);

    private void TryGut(EntityUid target, WerewolfGutComponent comp)
    {
        if (!HasOrgans(target))
            return;

        foreach (var (organId, _) in _body.GetBodyOrgans(target))
        {
            if (!_whitelist.IsValid(comp.Whitelist, organId))
                return;

            _body.RemoveOrgan(organId);
        }

        if (!TryComp<BloodstreamComponent>(target, out var blood))
            return;

        _bloodstream.SpillAllSolutions(target, blood);
    }

    private bool HasOrgans(EntityUid target)
    {
        if (HasComp<BorgChassisComponent>(target) ||
            HasComp<SiliconComponent>(target) ||
            !HasComp<HumanoidAppearanceComponent>(target))
            return false;

        return true;
    }

}
