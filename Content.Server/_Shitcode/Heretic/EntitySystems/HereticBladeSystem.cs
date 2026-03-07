using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Teleportation;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Goobstation.Shared.Teleportation.Components;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Heretic.EntitySystems;

public sealed class HereticBladeSystem : SharedHereticBladeSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedRandomTeleportSystem _teleport = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    protected override void ApplyLockBladeEffect(EntityUid target, EntityUid targetPart, float probability)
    {
        base.ApplyLockBladeEffect(target, targetPart, probability);

        if (!_random.Prob(probability))
            return;

        if (!_wound.TryInduceWound(targetPart, "WeepingAvulsion", 25f, out _, damageGroup: "Brute"))
            return;

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/blood3.ogg"), target);
        var effectAmount = _random.Next(3, 6);

        // Open ribcage for easier ascension if chest is fully mangled
        if (TryComp(targetPart, out WoundableComponent? woundable) && woundable.RootWoundable == target &&
            woundable.WoundableSeverity >= WoundableSeverity.Mangled &&
            (!EnsureComp<SkinRetractedComponent>(targetPart, out _) | !EnsureComp<IncisionOpenComponent>(targetPart, out _) |
             !EnsureComp<BonesSawedComponent>(targetPart, out _) | !EnsureComp<BonesOpenComponent>(targetPart, out _)))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/crack2.ogg"),
                target,
                AudioParams.Default.WithVolume(10f));
            effectAmount *= 2;
        }

        if (!TryComp(target, out BloodstreamComponent? bloodStream))
            return;

        if (!_sol.ResolveSolution(target,
                bloodStream.BloodSolutionName,
                ref bloodStream.BloodSolution,
                out var bloodSolution) || bloodSolution.Volume < 3)
            return;

        var coords = _transform.GetMapCoordinates(target);

        for (var i = 0; i < effectAmount; i++)
        {
            var sol = bloodSolution.SplitSolution(3);
            var color = sol.GetColor(_proto);
            var dir = _random.NextAngle().ToVec();
            var chunk = Spawn("BloodChunkEffect", coords);
            if (!_sol.TryGetSolution(chunk, "print", out var solEnt, true) || !_sol.TryAddSolution(solEnt.Value, sol))
            {
                Del(chunk);
                break;
            }

            if (TryComp(chunk, out TrailComponent? trail))
            {
                trail.Color = color;
                Dirty(chunk, trail);
            }

            _throw.TryThrow(chunk,
                dir * _random.NextVector2(0f, 2f),
                _random.NextFloat(0.5f, 1.5f),
                null,
                0f,
                2f,
                false,
                false,
                true,
                false);

            if (bloodSolution.Volume < 3)
                break;
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
