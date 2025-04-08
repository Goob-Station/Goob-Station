// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared.Preferences;
using Content.Shared.UserInterface;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.MagicMirror;

public abstract class SharedWizardMirrorSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UISystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WizardMirrorComponent, AfterInteractEvent>(OnMagicMirrorInteract);
        SubscribeLocalEvent<WizardMirrorComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<WizardMirrorComponent, ActivatableUIOpenAttemptEvent>(OnAttemptOpenUI);
        SubscribeLocalEvent<WizardMirrorComponent, BoundUserInterfaceCheckRangeEvent>(OnMirrorRangeCheck);
    }

    private void OnMagicMirrorInteract(Entity<WizardMirrorComponent> mirror, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        UpdateInterface(mirror, args.Target.Value, mirror);
        UISystem.TryOpenUi(mirror.Owner, WizardMirrorUiKey.Key, args.User);
    }

    private void OnMirrorRangeCheck(EntityUid uid,
        WizardMirrorComponent component,
        ref BoundUserInterfaceCheckRangeEvent args)
    {
        if (args.Result == BoundUserInterfaceRangeResult.Fail)
            return;

        if (component.Target == null || !Exists(component.Target))
        {
            component.Target = null;
            args.Result = BoundUserInterfaceRangeResult.Fail;
            return;
        }

        if (!_interaction.InRangeUnobstructed(component.Target.Value, uid))
            args.Result = BoundUserInterfaceRangeResult.Fail;
    }

    private void OnAttemptOpenUI(EntityUid uid, WizardMirrorComponent component, ref ActivatableUIOpenAttemptEvent args)
    {
        var user = component.Target ?? args.User;

        if (!HasComp<HumanoidAppearanceComponent>(user))
            args.Cancel();
    }

    private void OnBeforeUIOpen(Entity<WizardMirrorComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateInterface(ent, args.User, ent);
    }

    protected void UpdateInterface(EntityUid mirrorUid, EntityUid targetUid, WizardMirrorComponent component)
    {
        if (!TryComp<HumanoidAppearanceComponent>(targetUid, out var humanoid))
            return;

        component.Target ??= targetUid;

        var hair = (HairStyles.DefaultHairStyle, humanoid.CachedHairColor ?? Color.Black);
        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Hair, out var hairMarkings) && hairMarkings.Count > 0)
        {
            var hairMarking = hairMarkings[0];
            hair = (hairMarking.MarkingId, hairMarking.MarkingColors.FirstOrNull() ?? Color.Black);
        }

        var facialHair = (HairStyles.DefaultFacialHairStyle, humanoid.CachedFacialHairColor ?? Color.Black);
        if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.FacialHair, out var facialHairMarkings) &&
            facialHairMarkings.Count > 0)
        {
            var facialHairMarking = facialHairMarkings[0];
            facialHair = (facialHairMarking.MarkingId, facialHairMarking.MarkingColors.FirstOrNull() ?? Color.Black);
        }

        var appearance = new HumanoidCharacterAppearance(hair.Item1,
            hair.Item2,
            facialHair.Item1,
            facialHair.Item2,
            humanoid.EyeColor,
            humanoid.SkinColor,
            humanoid.MarkingSet.GetForwardEnumerator().ToList());

        var profile = new HumanoidCharacterProfile().WithGender(humanoid.Gender)
            .WithSex(humanoid.Sex)
            .WithSpecies(humanoid.Species)
            .WithName(MetaData(targetUid).EntityName)
            .WithAge(humanoid.Age)
            .WithCharacterAppearance(appearance);

        var state = new WizardMirrorUiState(profile);

        component.Target = targetUid;
        UISystem.SetUiState(mirrorUid, WizardMirrorUiKey.Key, state);
        Dirty(mirrorUid, component);
    }
}

[Serializable, NetSerializable]
public enum WizardMirrorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class WizardMirrorUiState(HumanoidCharacterProfile profile) : BoundUserInterfaceState
{
    public HumanoidCharacterProfile Profile = profile;
}

[Serializable, NetSerializable]
public sealed class WizardMirrorMessage(HumanoidCharacterProfile profile) : BoundUserInterfaceMessage
{
    public HumanoidCharacterProfile Profile = profile;
}