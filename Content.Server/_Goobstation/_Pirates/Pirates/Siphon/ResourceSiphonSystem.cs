using Content.Server._Goobstation._Pirates.GameTicking.Rules;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Interaction;

namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

public sealed partial class ResourceSiphonSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationAnchorSystem _anchor = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceSiphonComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ResourceSiphonComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<ResourceSiphonComponent, AfterInteractUsingEvent>(OnInteractUsing);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<ResourceSiphonComponent>();
        while (eqe.MoveNext(out var siphon))
        {
            siphon.ActivationRewindClock -= frameTime;
            if (siphon.ActivationRewindClock <= 0)
                siphon.ActivationPhase = 0; // reset
        }
    }

    private void OnInit(Entity<ResourceSiphonComponent> ent, ref ComponentInit args)
    {
        if (!TryBindRule(ent)) return;
    }

    private void OnInteract(Entity<ResourceSiphonComponent> ent, ref AfterInteractEvent args)
    {
        ent.Comp.ActivationPhase += 1;
        if (ent.Comp.ActivationPhase < 3)
        {
            var loc = Loc.GetString($"pirate-siphon-activate-{ent.Comp.ActivationPhase}");
            _popup.PopupEntity(loc, ent, Shared.Popups.PopupType.LargeCaution);
        }
        else ActivateSiphon(ent);
    }

    private void OnInteractUsing(Entity<ResourceSiphonComponent> ent, ref AfterInteractUsingEvent args)
    {
        // todo add money
    }

    public void ActivateSiphon(Entity<ResourceSiphonComponent> ent)
    {
        ent.Comp.Active = true;

        if (TryComp<StationAnchorComponent>(ent, out var anchor))
            _anchor.SetStatus((ent, anchor), true);

        _chat.DispatchGlobalAnnouncement(Loc.GetString("data-siphon-activated"), "Priority", colorOverride: Color.Red);
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
}
