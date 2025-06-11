using System.Linq;
using Content.Goobstation.Common.Access;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Maths.FixedPoint;
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
using Content.Shared.Power.EntitySystems;
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
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

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

        SubscribeLocalEvent<CyberdeckProjectionComponent, GetVerbsEvent<Verb>>(OnProjectionVerbs);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckHackActionEvent>(OnStartHacking);

        HackQuery = GetEntityQuery<CyberdeckHackableComponent>();
        HandsQuery = GetEntityQuery<HandsComponent>();
        UserQuery = GetEntityQuery<CyberdeckUserComponent>();
    }

    protected bool TryHackDevice(EntityUid user, EntityUid device)
    {
        if (!HackQuery.TryComp(device, out var hackable)
            || !UserQuery.TryComp(user, out var cyberDeck)
            || cyberDeck.ProviderEntity == null
            || Charges.HasInsufficientCharges(cyberDeck.ProviderEntity.Value, hackable.Cost)
            || !_power.IsPowered(device))
            return false;

        Charges.UseCharges(cyberDeck.ProviderEntity.Value, hackable.Cost);
        return true;
    }

    protected bool UseCharges(EntityUid user, FixedPoint2 amount)
    {
        if (!UserQuery.TryComp(user, out var cyberDeck)
            || cyberDeck.ProviderEntity == null
            || Charges.HasInsufficientCharges(cyberDeck.ProviderEntity.Value, amount))
            return false;

        Charges.UseCharges(cyberDeck.ProviderEntity.Value, amount);
        return true;
    }

    protected abstract void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent);

    private void OnProjectionVerbs(Entity<CyberdeckProjectionComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!HasComp<StationAiHeldComponent>(args.User))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cyberdeck-station-ai-smite-verb"),
            Act = () => { ShutdownProjection(ent.Owner); },
            Impact = LogImpact.High,
        });
    }

    private void OnStartHacking(EntityUid uid, CyberdeckUserComponent component, CyberdeckHackActionEvent args)
    {
        if (args.Handled || args.Target == uid)
            return;

        args.Handled = true;
        EntityUid target = default;

        if (HackQuery.HasComp(args.Target))
            target = args.Target;

        else if (HandsQuery.TryComp(args.Target, out var handsComp))
        {
            // Chech all hands for something that can be hacked
            foreach (var item in _hands.EnumerateHeld(args.Target, handsComp))
            {
                if (!HackQuery.HasComp(item))
                    continue;

                target = item;
                break;
            }
        }
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
        if (HasComp<ActorComponent>(target))
            _popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"), target, target, PopupType.LargeCaution);
    }

    private void OnUserInit(EntityUid uid, CyberdeckUserComponent component, ComponentStartup args)
    {
        Actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        Actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);

        // Find the cyberdeck source by hand. TODO: Maybe make a BodyOrganRelayEvent?
        var evil = _body.GetBodyOrgans(uid).Where(x => HasComp<CyberdeckSourceComponent>(x.Id)).FirstOrNull();
        if (evil == null)
            return;

        component.ProviderEntity = evil.Value.Id;
    }

    private void OnUserShutdown(EntityUid uid, CyberdeckUserComponent component, ComponentShutdown args)
    {
        Actions.RemoveAction(uid, component.HackAction);
        Actions.RemoveAction(uid, component.VisionAction);
        Actions.RemoveAction(uid, component.ReturnAction);

        // Return to body if player was visiting projection
        ShutdownProjection(component.ProjectionEntity);
    }

    private void OnAirlockHacked(Entity<AirlockComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!TryHackDevice(args.User, ent.Owner))
            return;

        _door.StartOpening(ent.Owner, user: args.User);
    }

    private void OnAccessHacked(Entity<AccessComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!TryHackDevice(args.User, ent.Owner))
            return;

        var ignore = EnsureComp<IgnoreAccessComponent>(ent);
        ignore.Ignore.Add(args.User);
    }

    private void OnBorgHacked(Entity<BorgChassisComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!TryHackDevice(args.User, ent.Owner))
            return;

        // TODO this probably shouldn't be hardcoded, but I don't know where to put this.
        _stun.TryParalyze(ent.Owner, TimeSpan.FromSeconds(8), true);
        _jitter.DoJitter(ent.Owner, TimeSpan.FromSeconds(8), true);
    }
}
