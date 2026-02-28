using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Messages;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.UserInterface;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class MawedCrucibleSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AnchorableSystem _anchor = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MawedCrucibleComponent, InteractUsingEvent>(OnInteract);
        SubscribeLocalEvent<MawedCrucibleComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<MawedCrucibleComponent, ActivatableUIOpenAttemptEvent>(OnUiAttempt);
        SubscribeLocalEvent<MawedCrucibleComponent, MawedCrucibleMessage>(OnMessage);
    }

    private void OnMessage(Entity<MawedCrucibleComponent> ent, ref MawedCrucibleMessage args)
    {
        if (!ent.Comp.Potions.Contains(args.Proto) || ent.Comp.CurrentMass < ent.Comp.MaxMass)
            return;

        UpdateMass(ent, 0);
        _audio.PlayPredicted(ent.Comp.BrewSound, ent, args.Actor);
        PredictedSpawnAtPosition(args.Proto, Transform(ent).Coordinates);
    }

    private void OnUiAttempt(Entity<MawedCrucibleComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!Transform(ent).Anchored || ent.Comp.CurrentMass < ent.Comp.MaxMass)
            args.Cancel();
    }

    private void OnExamine(Entity<MawedCrucibleComponent> ent, ref ExaminedEvent args)
    {
        if (!HereticOrGhoul(args.Examiner))
            return;

        if (ent.Comp.CurrentMass > 0)
            args.PushMarkup(Loc.GetString("mawed-crucible-examine-can-refill-flask"));

        if (ent.Comp.CurrentMass < ent.Comp.MaxMass)
        {
            args.PushMarkup(Loc.GetString("mawed-crucible-examine-not-full",
                ("to-fill", ent.Comp.MaxMass - ent.Comp.CurrentMass)));
        }
        else
            args.PushMarkup(Loc.GetString("mawed-crucible-examine-full"));

        var isAnchored = Comp<TransformComponent>(ent).Anchored;
        var messageId = isAnchored ? "mawed-crucible-examine-anchored" : "mawed-crucible-examine-unanchored";
        args.PushMarkup(Loc.GetString(messageId));
    }

    private void OnInteract(Entity<MawedCrucibleComponent> ent, ref InteractUsingEvent args)
    {
        if (!HereticOrGhoul(args.User))
            return;

        var xform = Transform(ent);

        if (_tag.HasTag(args.Used, ent.Comp.AnchorTag))
        {
            ToggleAnchor((ent, xform), ref args);
            return;
        }

        if (!xform.Anchored)
            return;

        if (_tag.HasTag(args.Used, ent.Comp.MeatTag) && _whitelist.IsValid(ent.Comp.FuelWhitelist, args.Used))
        {
            RefuelCrucible(ent, ref args);
            return;
        }

        if (!_tag.HasTag(args.Used, ent.Comp.EldritchFlaskTag))
            return;

        RefillFlask(ent, ref args);
    }

    private void RefillFlask(Entity<MawedCrucibleComponent> ent, ref InteractUsingEvent args)
    {
        if (ent.Comp.CurrentMass <= 0)
        {
            _popup.PopupClient(Loc.GetString("mawed-crucible-not-enough-fuel-message"), ent, args.User);
            return;
        }

        if (!TryComp(args.Used, out SolutionContainerManagerComponent? container))
            return;

        if (!_sol.TryGetSolution((args.Used, container), "drink", out var sol))
            return;

        if (sol.Value.Comp.Solution.AvailableVolume == FixedPoint2.Zero)
        {
            _popup.PopupClient(Loc.GetString("mawed-crucible-flask-full-message"), ent, args.User);
            return;
        }

        if (!_sol.TryAddReagent(sol.Value, ent.Comp.EldritchEssence, ent.Comp.EldritchEssencePerMass, out _))
            return;

        _audio.PlayPredicted(ent.Comp.BrewSound, ent, args.User);
        UpdateMass(ent, ent.Comp.CurrentMass - 1);
        args.Handled = true;
    }

    private void RefuelCrucible(Entity<MawedCrucibleComponent> ent, ref InteractUsingEvent args)
    {
        if (ent.Comp.CurrentMass >= ent.Comp.MaxMass)
        {
            _popup.PopupClient(Loc.GetString("mawed-crucible-full-message"), ent, args.User);
            return;
        }

        args.Handled = true;
        UpdateMass(ent, ent.Comp.CurrentMass + 1);
        _audio.PlayPredicted(ent.Comp.MassGainSound, ent, args.User);
        PredictedQueueDel(args.Used);
    }

    private void ToggleAnchor(Entity<TransformComponent> ent, ref InteractUsingEvent args)
    {
        if (ent.Comp.Anchored)
        {
            _transform.Unanchor(ent);
            _popup.PopupClient(Loc.GetString("anchorable-unanchored"), ent, args.User);
            args.Handled = true;
            return;
        }

        if (!TryComp<PhysicsComponent>(ent, out var anchorBody) ||
            _anchor.TileFree(ent.Comp.Coordinates, anchorBody))
        {
            if (!_transform.AnchorEntity(ent))
                return;

            _popup.PopupClient(Loc.GetString("anchorable-anchored"), ent, args.User);
            args.Handled = true;
        }
        else
            _popup.PopupClient(Loc.GetString("anchorable-occupied"), ent, args.User);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<MawedCrucibleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (!xform.Anchored || comp.CurrentMass >= comp.MaxMass)
            {
                comp.Accumulator = 0f;
                continue;
            }

            comp.Accumulator += frameTime;

            if (comp.Accumulator < comp.MassGainTime)
                continue;

            comp.Accumulator = 0f;
            UpdateMass((uid, comp), comp.CurrentMass + 1);
            _audio.PlayPvs(comp.MassGainSound, uid);
        }
    }

    private void UpdateMass(Entity<MawedCrucibleComponent> ent, int newMass)
    {
        var empty = newMass == 0;
        _appearance.SetData(ent, CrucibleVisuals.Empty, empty);
        _light.SetEnabled(ent, !empty);

        ent.Comp.CurrentMass = newMass;
        Dirty(ent);
    }

    private bool HereticOrGhoul(EntityUid uid)
    {
        return HasComp<HereticComponent>(uid) || HasComp<GhoulComponent>(uid);
    }
}
