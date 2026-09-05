using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Forensics;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerVeilSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerVeilEvent>(OnVeil);
        SubscribeLocalEvent<BloodsuckerVeilComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<BloodsuckerVeilComponent>();
        while (query.MoveNext(out var uid, out var veil))
        {
            if (!veil.Active)
                continue;

            if (_timing.CurTime < veil.UpdateTimer)
                continue;

            veil.UpdateTimer = _timing.CurTime + veil.UpdateDelay;
            Dirty(uid, veil);

            DrainBlood(uid, veil);
        }
    }

    private void OnVeil(Entity<BloodsuckerComponent> ent, ref BloodsuckerVeilEvent args)
    {
        if (!TryComp(ent, out BloodsuckerVeilComponent? comp))
            return;

        if (comp.Active)
        {
            Deactivate(ent.Owner, comp);
        }
        else
        {
            if (!TryUseCosts(ent, comp))
                return;
            Activate(ent.Owner, comp);
        }

        args.Handled = true;
    }

    private void OnShutdown(Entity<BloodsuckerVeilComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Active)
            Deactivate(ent.Owner, ent.Comp);
    }

    private void Activate(EntityUid uid, BloodsuckerVeilComponent comp)
    {
        if (!TryComp(uid, out HumanoidAppearanceComponent? humanoid))
            return;

        // Snapshot current appearance
        comp.SavedName = MetaData(uid).EntityName;
        comp.SavedSex = humanoid.Sex;
        comp.SavedGender = humanoid.Gender;
        comp.SavedAge = humanoid.Age;
        comp.SavedSkinColor = humanoid.SkinColor;
        comp.SavedEyeColor = humanoid.EyeColor;
        comp.SavedMarkings = humanoid.MarkingSet.GetForwardEnumerator()
            .Select(m => new Marking(m.MarkingId, m.MarkingColors.ToList()))
            .ToList();
        comp.SavedHeight = humanoid.Height;
        comp.SavedWidth = humanoid.Width;

        // Randomize
        var newProfile = HumanoidCharacterProfile.RandomWithSpecies(humanoid.Species);
        _humanoid.LoadProfile(uid, newProfile, humanoid);
        _metaData.SetEntityName(uid, newProfile.Name);

        if (TryComp(uid, out DnaComponent? dna))
        {
            dna.DNA = _forensics.GenerateDNA();
            var ev = new GenerateDnaEvent { Owner = uid, DNA = dna.DNA };
            RaiseLocalEvent(uid, ref ev);
        }

        if (TryComp(uid, out FingerprintComponent? fingerprint))
            fingerprint.Fingerprint = _forensics.GenerateFingerprint();

        _identity.QueueIdentityUpdate(uid);

        comp.Active = true;
        comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;
        Dirty(uid, comp);

        _audio.PlayPvs(comp.Sound, uid);
        _popup.PopupEntity(Loc.GetString("bloodsucker-veil-on"), uid, uid, PopupType.Small);
    }

    private void Deactivate(EntityUid uid, BloodsuckerVeilComponent comp)
    {
        if (!TryComp(uid, out HumanoidAppearanceComponent? humanoid))
            return;

        if (comp.SavedMarkings != null)
        {
            var restoredProfile = new HumanoidCharacterProfile()
            {
                Name = comp.SavedName ?? MetaData(uid).EntityName,
            }
            .WithSex(comp.SavedSex)
            .WithGender(comp.SavedGender)
            .WithAge(comp.SavedAge)
            .WithHeight(comp.SavedHeight)
            .WithWidth(comp.SavedWidth)
            .WithSpecies(humanoid.Species);

            _humanoid.LoadProfile(uid, restoredProfile, humanoid);

            // Dear god...
            humanoid.SkinColor = comp.SavedSkinColor;
            humanoid.EyeColor = comp.SavedEyeColor;
            var newMarkingSet = new MarkingSet();
            foreach (var marking in comp.SavedMarkings)
            {
                if (_markingManager.TryGetMarking(marking, out var prototype))
                    newMarkingSet.AddBack(prototype.MarkingCategory, marking);
            }
            humanoid.MarkingSet = newMarkingSet;
            humanoid.Sex = comp.SavedSex;
            Dirty(uid, humanoid);

            _identity.QueueIdentityUpdate(uid);
        }

        if (comp.SavedName != null)
            _metaData.SetEntityName(uid, comp.SavedName);

        comp.SavedName = null;
        comp.Active = false;
        Dirty(uid, comp);

        _audio.PlayPvs(comp.Sound, uid);
        _popup.PopupEntity(Loc.GetString("bloodsucker-veil-off"), uid, uid, PopupType.Small);
    }

    private void DrainBlood(EntityUid uid, BloodsuckerVeilComponent comp)
    {
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(uid, bloodstream),
            -FixedPoint2.New(comp.BloodDrainPerSecond));
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerVeilComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (!TryComp(ent.Owner, out BloodstreamComponent? bloodstream))
            return false;

        var currentBlood = bloodstream.BloodSolution is { } sol
            ? (float) sol.Comp.Solution.Volume
            : 0f;

        if (currentBlood < comp.BloodCost)
        {
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-veil-no-blood"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            return false;
        }

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(ent.Owner, bloodstream),
            -FixedPoint2.New(comp.BloodCost));

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        return true;
    }
}
