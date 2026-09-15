// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Step;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Stacks;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._Oskarrr.Surgery;

public sealed class BoneHarvestSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryBonesPresentConditionComponent, SurgeryValidEvent>(OnBonesPresentValid);
        SubscribeLocalEvent<SurgeryStepExtractBonesEffectComponent, SurgeryStepEvent>(OnExtractStep);
        SubscribeLocalEvent<SurgeryStepExtractBonesEffectComponent, SurgeryStepCompleteCheckEvent>(OnExtractCheck);
        SubscribeLocalEvent<SurgeryStepRegrowBonesEffectComponent, SurgeryStepEvent>(OnRegrowStep);
        SubscribeLocalEvent<SurgeryStepRegrowBonesEffectComponent, SurgeryStepCompleteCheckEvent>(OnRegrowCheck);
    }

    private void OnBonesPresentValid(Entity<SurgeryBonesPresentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (args.Cancelled)
            return;

        var hasBones = TryComp<WoundableComponent>(args.Part, out var woundable)
                       && woundable.Bone.ContainedEntities.Count > 0
                       && !HasComp<BonelessComponent>(args.Part);

        if (ent.Comp.Inverse ? hasBones : !hasBones)
            args.Cancelled = true;
    }

    private void OnExtractStep(Entity<SurgeryStepExtractBonesEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<WoundableComponent>(args.Part, out var woundable))
            return;

        var bones = woundable.Bone.ContainedEntities.ToList();
        foreach (var bone in bones)
        {
            _container.Remove(bone, woundable.Bone);
            QueueDel(bone);
        }

        var amount = _random.Next(ent.Comp.MinAmount, ent.Comp.MaxAmount + 1);
        if (amount <= 0)
            return;

        var coords = _transform.GetMoverCoordinates(args.Body);
        var stack = Spawn(ent.Comp.Prototype, coords);
        if (TryComp<StackComponent>(stack, out _))
            _stack.SetCount(stack, amount);
    }

    private void OnExtractCheck(Entity<SurgeryStepExtractBonesEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<WoundableComponent>(args.Part, out var woundable))
            return;

        if (woundable.Bone.ContainedEntities.Count > 0
            || !HasComp<BonelessComponent>(args.Part)
            || !HasComp<BonesHarvestedComponent>(args.Part))
            args.Cancelled = true;
    }

    private void OnRegrowStep(Entity<SurgeryStepRegrowBonesEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (_net.IsClient)
            return;

        RegrowBonesOnPart(args.Part);
    }

    private void OnRegrowCheck(Entity<SurgeryStepRegrowBonesEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<WoundableComponent>(args.Part, out var woundable))
        {
            args.Cancelled = true;
            return;
        }

        if (HasComp<BonelessComponent>(args.Part)
            || HasComp<BonesHarvestedComponent>(args.Part)
            || woundable.Bone.ContainedEntities.Count == 0)
            args.Cancelled = true;
    }

    /// <summary>
    /// Removes harvest markers and ensures an anatomical bone exists in the part.
    /// Only works on surgically harvested parts, not naturally boneless species.
    /// </summary>
    public bool RegrowBonesOnPart(EntityUid part, WoundableComponent? woundable = null)
    {
        if (_net.IsClient)
            return false;

        if (!Resolve(part, ref woundable))
            return false;

        // Only surgically harvested parts — not naturally boneless species.
        if (!HasComp<BonesHarvestedComponent>(part))
            return false;

        RemComp<BonelessComponent>(part);
        RemComp<BonesHarvestedComponent>(part);

        if (woundable.Bone.ContainedEntities.Count > 0)
            return true;

        var bone = Spawn(woundable.BoneEntity);
        if (!TryComp<BoneComponent>(bone, out var boneComp))
        {
            QueueDel(bone);
            return false;
        }

        _transform.SetParent(bone, part);
        _container.Insert(bone, woundable.Bone);
        boneComp.BoneWoundable = part;
        Dirty(bone, boneComp);
        Dirty(part, woundable);
        return true;
    }
}
