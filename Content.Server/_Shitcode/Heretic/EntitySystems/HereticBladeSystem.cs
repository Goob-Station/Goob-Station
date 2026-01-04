using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Teleportation;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Goobstation.Shared.Teleportation.Components;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticBladeSystem : SharedHereticBladeSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedRandomTeleportSystem _teleport = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    protected override void ApplyLockBladeEffect(EntityUid target, float probability)
    {
        base.ApplyLockBladeEffect(target, probability);

        if (!_random.Prob(probability))
            return;

        if (!_wound.TryInduceWound(target, "WeepingAvulsion", 25f, out _, damageGroup: "Brute"))
            return;

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/blood3.ogg"), target);

        // Open ribcage for easier ascension if part is fully mangled
        if (TryComp(target, out WoundableComponent? woundable) && woundable.RootWoundable == target &&
            woundable.WoundableSeverity >= WoundableSeverity.Mangled &&
            (!EnsureComp<SkinRetractedComponent>(target, out _) | !EnsureComp<IncisionOpenComponent>(target, out _) |
             !EnsureComp<BonesSawedComponent>(target, out _) | !EnsureComp<BonesOpenComponent>(target, out _)))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/crack2.ogg"),
                target,
                AudioParams.Default.WithVolume(10f));
        }
    }

    protected override void ApplyAshBladeEffect(EntityUid target)
    {
        base.ApplyAshBladeEffect(target);

        _flammable.AdjustFireStacks(target, 2.5f, null, true, 0.5f);
    }

    protected override void ApplyFleshBladeEffect(EntityUid target)
    {
        base.ApplyFleshBladeEffect(target);

        if (!TryComp(target, out BloodstreamComponent? bloodStream))
            return;

        _blood.TryModifyBleedAmount((target, bloodStream), 2f);

        if (!_sol.ResolveSolution(target,
                bloodStream.BloodSolutionName,
                ref bloodStream.BloodSolution,
                out var bloodSolution))
            return;

        _puddle.TrySpillAt(target, bloodSolution.SplitSolution(10), out _);
    }

    protected override void RandomTeleport(EntityUid user, EntityUid blade, RandomTeleportComponent comp)
    {
        base.RandomTeleport(user, blade, comp);

        _teleport.RandomTeleport(user, comp, false);
        QueueDel(blade);
    }
}
