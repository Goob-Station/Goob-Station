using Content.Server._Goobstation._Pirates.GameTicking.Rules;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Station.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

public sealed partial class ResourceSiphonSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationAnchorSystem _anchor = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    private float TickTimer = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceSiphonComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ResourceSiphonComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<ResourceSiphonComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ResourceSiphonComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<ResourceSiphonComponent>();
        while (eqe.MoveNext(out var uid, out var siphon))
        {
            siphon.ActivationRewindClock -= frameTime;
            if (siphon.ActivationRewindClock <= 0)
                siphon.ActivationPhase = 0; // reset
        }

        TickTimer -= frameTime;
        if (TickTimer <= 0)
        {
            TickTimer = 1;
            eqe = EntityQueryEnumerator<ResourceSiphonComponent>(); // reset it ig
            while (eqe.MoveNext(out var uid, out var siphon))
                if (siphon.Active)
                    Tick((uid, siphon));
        }
    }

    private void Tick(Entity<ResourceSiphonComponent> ent)
    {
        AllEntityQuery<BecomesStationComponent, StationMemberComponent>().MoveNext(out var eqData, out _, out _);
        var station = _station.GetOwningStation(eqData);
        if (station == null) return;

        if (!TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        var funds = bank.Balance - ent.Comp.DrainRate;
        if (funds > 0)
        {
            _cargo.DeductFunds(bank, (int) ent.Comp.DrainRate);
            UpdateCredits(ent, ent.Comp.DrainRate);
        }
    }

    #region Event Handlers
    private void OnInit(Entity<ResourceSiphonComponent> ent, ref ComponentInit args)
    {
        if (!TryBindRule(ent)) return;
    }

    private void OnInteract(Entity<ResourceSiphonComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Active) return;

        AllEntityQuery<BecomesStationComponent, StationMemberComponent>().MoveNext(out var eqData, out _, out _);
        var station = _station.GetOwningStation(eqData);
        if (station == null
        || Transform((EntityUid) station).MapID != Transform(ent).MapID)
        {
            _popup.PopupEntity(Loc.GetString("pirate-siphon-activate-fail"), ent, args.User, Shared.Popups.PopupType.Medium);
            return;
        }

        ent.Comp.ActivationPhase += 1;
        if (ent.Comp.ActivationPhase < 3)
        {
            var loc = Loc.GetString($"pirate-siphon-activate-{ent.Comp.ActivationPhase}");
            _popup.PopupEntity(loc, ent, args.User, Shared.Popups.PopupType.LargeCaution);
        }
        else ActivateSiphon(ent);
    }

    private void OnInteractUsing(Entity<ResourceSiphonComponent> ent, ref InteractUsingEvent args)
    {
        if (HasComp<CashComponent>(args.Used))
        {
            var price = _pricing.GetPrice(args.Used);
            if (price == 0) return;

            UpdateCredits(ent, (float) price);
            QueueDel(args.Used);
        }

        // add more stuff here if needed
    }

    private void OnExamine(Entity<ResourceSiphonComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("pirate-siphon-examine", ("num", ent.Comp.Credits)));
    }
    #endregion

    public void ActivateSiphon(Entity<ResourceSiphonComponent> ent)
    {
        ent.Comp.Active = true;

        if (TryComp<StationAnchorComponent>(ent, out var anchor))
            _anchor.SetStatus((ent, anchor), true);

        var coords = _xform.GetWorldPosition(Transform(ent));
        _popup.PopupCoordinates(Loc.GetString("data-siphon-activated"), Transform(ent).Coordinates, Shared.Popups.PopupType.Medium);

        var anloc = Loc.GetString("data-siphon-activated-announcement", ("pos", $"X: {coords.X}; Y: {coords.Y}"));
        _chat.DispatchGlobalAnnouncement(anloc, "Priority", colorOverride: Color.Red);
    }

    public bool TryBindRule(Entity<ResourceSiphonComponent> ent)
    {
        var eqe = EntityQueryEnumerator<ActivePirateRuleComponent>();
        while (eqe.MoveNext(out var ruid, out var rule))
        {
            if (rule.BoundSiphon == null)
            {
                rule.BoundSiphon = ent;
                ent.Comp.BoundGamerule = ruid;
                return true;
            }
        }
        return false;
    }
    public EntityUid? GetRule(Entity<ResourceSiphonComponent> ent)
    {
        if (ent.Comp.BoundGamerule == null)
            TryBindRule(ent);

        return ent.Comp.BoundGamerule;
    }

    public bool SyncWithGamerule(Entity<ResourceSiphonComponent> ent)
    {
        if (GetRule(ent) == null
        || !TryComp<ActivePirateRuleComponent>(ent.Comp.BoundGamerule, out var prule))
            return false;

        prule.Credits = ent.Comp.Credits;

        return true;
    }

    public void UpdateCredits(Entity<ResourceSiphonComponent> ent, float amount)
    {
        var newAmount = ent.Comp.Credits + amount;
        ent.Comp.Credits = Math.Min(ent.Comp.CreditsThreshold, newAmount);

        if (newAmount > ent.Comp.CreditsThreshold)
        {
            if (ent.Comp.Active)
                ent.Comp.Active = false; // stop siphoning
        }
    }
}
