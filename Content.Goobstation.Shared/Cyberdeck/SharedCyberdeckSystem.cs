using System.Linq;
using Content.Goobstation.Common.Access;
using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cyberdeck;

public abstract class SharedCyberdeckSystem : EntitySystem
{
    [Dependency] protected readonly SharedChargesSystem Charges = default!;
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] protected readonly SharedPowerReceiverSystem Power = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    protected EntityQuery<HandsComponent> HandsQuery;
    protected EntityQuery<ContainerManagerComponent> ContainerQuery;
    protected EntityQuery<LimitedChargesComponent> ChargesQuery;

    protected EntityQuery<CyberdeckHackableComponent> HackQuery;
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

        HandsQuery = GetEntityQuery<HandsComponent>();
        ContainerQuery = GetEntityQuery<ContainerManagerComponent>();
        ChargesQuery = GetEntityQuery<LimitedChargesComponent>();

        HackQuery = GetEntityQuery<CyberdeckHackableComponent>();
        UserQuery = GetEntityQuery<CyberdeckUserComponent>();
    }

    protected bool TryHackDevice(EntityUid user, EntityUid device)
    {
        if (!HackQuery.TryComp(device, out var hackable))
            return false;

        return UseCharges(user, hackable.Cost);
    }

    protected bool UseCharges(EntityUid user, FixedPoint2 amount, EntityUid? target = null)
    {
        if (!UserQuery.TryComp(user, out var cyberDeck))
            return false;

        if (cyberDeck.ProviderEntity == null)
            return true; // We don't care if nowhere to take charges from at this point

        if (ChargesQuery.TryComp(cyberDeck.ProviderEntity.Value, out var chargesComp)
            && Charges.HasInsufficientCharges(cyberDeck.ProviderEntity.Value, amount))
        {
            string message;
            var charges = chargesComp.Charges;
            var chargesForm = (amount - charges).Int();

            // SHUT UP C# I HATE BRACES!!!!!!!!!

            // ReSharper disable once EnforceIfStatementBraces
            if (target != null)
                message = Loc.GetString("cyberdeck-insufficient-charges-with-target",
                    ("amount", chargesForm),
                    ("target", Identity.Name(target.Value, EntityManager)));
            // ReSharper disable once EnforceIfStatementBraces
            else
                message = Loc.GetString("cyberdeck-insufficient-charges-with-target",
                    ("amount", chargesForm));

            Popup.PopupEntity(message, user, user, PopupType.MediumCaution);
            return false;
        }

        Charges.UseCharges(cyberDeck.ProviderEntity.Value, amount);
        return true;
    }

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
            // Check all hands for something that can be hacked
            foreach (var item in _hands.EnumerateHeld(args.Target, handsComp))
            {
                if (!HackQuery.HasComp(item))
                    continue;

                target = item;
                break;
            }
        }
        else if (ContainerQuery.TryComp(args.Target, out var containerComp))
        {
            // If it's a container, find anything hackable and hack it.
            // No, I won't stack loops inside an if statement, because birds will start migrating to such Nested code.
            foreach (var container in _container.GetAllContainers(args.Target, containerComp))
            {
                var containerTarget = container.ContainedEntities.FirstOrNull(HackQuery.HasComp);
                if (containerTarget == null)
                    continue;

                target = containerTarget.Value;
            }
        }
        else
            return; // Nothing to hack here

        // To be safe we get the component itself only here.
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
            Hidden = true,
            RequireCanInteract = false,
            ColorOverride = Color.Aquamarine,
        };

        _doAfter.TryStartDoAfter(ev);

        var message = Loc.GetString("cyberdeck-start-hacking", ("target", Identity.Name(target, EntityManager, uid)));
        Popup.PopupEntity(message, uid, uid, PopupType.SmallCaution);

        // Also alert the target if it's a player.
        if (HasComp<ActorComponent>(target))
            Popup.PopupEntity(Loc.GetString("cyberdeck-player-get-hacked"), target, target, PopupType.LargeCaution);
    }

    private void OnUserInit(EntityUid uid, CyberdeckUserComponent component, ComponentStartup args)
    {
        Actions.AddAction(uid, ref component.HackAction, component.HackActionId);
        Actions.AddAction(uid, ref component.VisionAction, component.VisionActionId);
        UpdateAlert((uid, component));

        // Find the cyberdeck source by hand. TODO: Maybe make a BodyOrganRelayEvent and subscribe to it?
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

    protected abstract void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent);

    protected abstract void UpdateAlert(Entity<CyberdeckUserComponent> ent);
}
