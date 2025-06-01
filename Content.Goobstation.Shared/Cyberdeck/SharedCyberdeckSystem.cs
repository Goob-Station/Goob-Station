using System.Linq;
using Content.Goobstation.Common.Access;
using Content.Goobstation.Shared.Cyberdeck.Components;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract class SharedCyberdeckSystem : EntitySystem
{
    [Dependency] protected readonly SharedChargesSystem Charges = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    protected EntityQuery<CyberdeckHackableComponent> HackQuery;
    protected EntityQuery<HandsComponent> HandsQuery;
    protected EntityQuery<CyberdeckUserComponent> UserQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AirlockComponent, CyberdeckHackDoAfterEvent>(OnAirlockHacked);
        SubscribeLocalEvent<AccessComponent, CyberdeckHackDoAfterEvent>(OnAccessHacked);
        SubscribeLocalEvent<BorgChassisComponent, CyberdeckHackDoAfterEvent>(OnBorgHacked);

        SubscribeLocalEvent<CyberdeckUserComponent, ComponentStartup>(OnUserInit);
        SubscribeLocalEvent<CyberdeckUserComponent, ComponentShutdown>(OnUserShutdown);

        SubscribeLocalEvent<CyberdeckProjectionComponent, ComponentStartup>(OnProjectionInit);
        SubscribeLocalEvent<CyberdeckProjectionComponent, ComponentShutdown>(OnProjectionShutdown);
        SubscribeLocalEvent<CyberdeckProjectionComponent, GetVerbsEvent<Verb>>(OnProjectionVerbs);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckHackActionEvent>(OnStartHacking);

        HackQuery = GetEntityQuery<CyberdeckHackableComponent>();
        HandsQuery = GetEntityQuery<HandsComponent>();
        UserQuery = GetEntityQuery<CyberdeckUserComponent>();
    }

    protected bool UseCharges(EntityUid device, EntityUid user)
    {
        if (!HackQuery.TryComp(device, out var hackable)
            || !UserQuery.TryComp(user, out var cyberDeck)
            || cyberDeck.ProviderEntity == null
            || Charges.IsEmpty(cyberDeck.ProviderEntity.Value))
            return false;

        Charges.UseCharges(cyberDeck.ProviderEntity.Value, hackable.Cost);
        return true;
    }

    protected abstract void ShutdownProjection(Entity<CyberdeckProjectionComponent> ent);

    private void OnProjectionVerbs(Entity<CyberdeckProjectionComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!HasComp<StationAiHeldComponent>(args.User))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cyberdeck-station-ai-smite-verb"),
            Act = () => { ShutdownProjection(ent); },
            Impact = LogImpact.High,
        });
    }

    private void OnStartHacking(EntityUid uid, CyberdeckUserComponent component, CyberdeckHackActionEvent args)
    {
        if (args.Handled || args.Target == uid)
            return;

        args.Handled = true;

        EntityUid target;

        if (HackQuery.HasComp(args.Target))
            target = args.Target;
        else if (HandsQuery.TryComp(args.Target, out var handsComp)
            && _hands.TryGetActiveItem((args.Target, handsComp), out var item)
            && HackQuery.HasComp(item.Value))
            target = item.Value; // Hacked from entity's active hand
        else
            return; // Nothing to hack here

        // To be safe we get the component only here.
        if (!HackQuery.TryComp(target, out var hackable))
            return;

        var ev = new DoAfterArgs(
            EntityManager,
            uid,
            hackable.HackingTime,
            new CyberdeckHackDoAfterEvent(),
            target,
            target,
            component.ProviderEntity,
            uid)
        {
            BlockDuplicate = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            Broadcast = false,
        };

        _doAfter.TryStartDoAfter(ev);

        // Also alert the target if it's a player.
        if (HasComp<ActorComponent>(args.Target))
            _popup.PopupEntity(Loc.GetString("cyberdeck-player-hacking"), target, target, PopupType.LargeCaution);
    }

    private void OnUserInit(EntityUid uid, CyberdeckUserComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        _actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);

        // Find the cyberdeck source by hand. (evil LINQ moment)
        var organ = _body.GetBodyOrgans(uid).Where(x => HasComp<CyberdeckSourceComponent>(x.Id)).FirstOrNull();

        if (organ == null)
            return;

        component.ProviderEntity = organ.Value.Id;
    }

    private void OnUserShutdown(EntityUid uid, CyberdeckUserComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.HackAction);
        _actions.RemoveAction(uid, component.VisionAction);

        // Return to body if player was visiting out projection
        if (TryComp<CyberdeckProjectionComponent>(component.ProjectionEntity, out var projectionComp))
            ShutdownProjection((component.ProjectionEntity.Value, projectionComp));
    }

    private void OnProjectionInit(EntityUid uid, CyberdeckProjectionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ReturnAction, component.ReturnActionId);
    }

    private void OnProjectionShutdown(EntityUid uid, CyberdeckProjectionComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ReturnAction);
    }

    private void OnAirlockHacked(Entity<AirlockComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(ent.Owner, args.User))
            return;

        _door.StartOpening(ent.Owner, user: args.User);
    }

    private void OnAccessHacked(Entity<AccessComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(ent.Owner, args.User))
            return;

        var ignore = EnsureComp<IgnoreAccessComponent>(ent);
        ignore.Ignore.Add(args.User);
    }

    private void OnBorgHacked(Entity<BorgChassisComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(ent.Owner, args.User))
            return;

        // TODO this probably shouldn't be hardcoded, but I don't know where to put this.
        _stun.TryParalyze(ent.Owner, TimeSpan.FromSeconds(8), true);
        _jitter.DoJitter(ent.Owner, TimeSpan.FromSeconds(8), true);
    }
}
