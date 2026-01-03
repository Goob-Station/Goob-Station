using System.Linq;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Coordinates;
using Content.Shared.Doors.Components;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedEldritchIdCardSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;

    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly SharedAccessSystem _access = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchIdCardComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<EldritchIdCardComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EldritchIdCardComponent, ActivatableUIOpenAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<EldritchIdCardComponent, EldritchIdMessage>(OnMessage);
        SubscribeLocalEvent<EldritchIdCardComponent, GetVerbsEvent<AlternativeVerb>>(OnAltVerb);
        SubscribeLocalEvent<EldritchIdCardComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnBeforeInteract(Entity<EldritchIdCardComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!args.CanReach || args.Target == null || !HereticOrGhoul(args.User))
            return;

        var target = args.Target.Value;

        if (TryComp(target, out IdCardComponent? idCard))
        {
            args.Handled = true;
            EatCard(ent, (target, idCard), args.User);
            return;
        }

        if (HasComp<LockPortalComponent>(target) && (ent.Comp.PortalOne == target || ent.Comp.PortalTwo == target))
        {
            args.Handled = true;
            ClearPortals(ent);
            return;
        }

        if (!HasComp<DoorComponent>(target))
            return;

        var coords = Transform(target).Coordinates;

        if (_lookup.GetEntitiesInRange<LockPortalComponent>(coords, 0.4f).Count > 0)
        {
            _popup.PopupClient(Loc.GetString("heretic-ability-fail-tile-occupied"), args.User, args.User);
            return;
        }

        args.Handled = true;

        if (_net.IsClient)
            return;

        var portalOneResolved = Exists(ent.Comp.PortalOne);
        var portalTwoResolved = Exists(ent.Comp.PortalTwo);

        if (portalOneResolved && portalTwoResolved)
        {
            QueueDel(ent.Comp.PortalOne);
            var newPortal = SpawnAttachedTo(ent.Comp.Portal, target.ToCoordinates());
            var newPortalComp = EnsureComp<LockPortalComponent>(newPortal);
            var portalTwoComp = EnsureComp<LockPortalComponent>(ent.Comp.PortalTwo!.Value);
            newPortalComp.Inverted = ent.Comp.Inverted;
            portalTwoComp.Inverted = ent.Comp.Inverted;
            newPortalComp.LinkedPortal = ent.Comp.PortalTwo.Value;
            portalTwoComp.LinkedPortal = newPortal;
            Dirty(newPortal, newPortalComp);
            Dirty(ent.Comp.PortalTwo.Value, portalTwoComp);
            ent.Comp.PortalOne = ent.Comp.PortalTwo.Value;
            ent.Comp.PortalTwo = newPortal;
            _popup.PopupEntity(Loc.GetString("eldritch-id-card-component-link-two"), args.User, args.User);
            return;
        }

        if (!portalOneResolved)
        {
            var newPortal = SpawnAttachedTo(ent.Comp.Portal, target.ToCoordinates());
            ent.Comp.PortalOne = newPortal;
            var newPortalComp = EnsureComp<LockPortalComponent>(newPortal);
            newPortalComp.Inverted = ent.Comp.Inverted;
            Dirty(newPortal, newPortalComp);

            if (!portalTwoResolved)
            {
                _popup.PopupEntity(Loc.GetString("eldritch-id-card-component-link-one"), args.User, args.User);
                return;
            }

            _popup.PopupEntity(Loc.GetString("eldritch-id-card-component-link-two"), args.User, args.User);

            var portalTwoComp = EnsureComp<LockPortalComponent>(ent.Comp.PortalTwo!.Value);
            portalTwoComp.Inverted = ent.Comp.Inverted;
            Dirty(ent.Comp.PortalTwo.Value, portalTwoComp);
            newPortalComp.LinkedPortal = ent.Comp.PortalTwo.Value;
            portalTwoComp.LinkedPortal = newPortal;
            return;
        }


        if (!portalTwoResolved)
        {
            var newPortal = SpawnAttachedTo(ent.Comp.Portal, target.ToCoordinates());
            ent.Comp.PortalTwo = newPortal;
            var newPortalComp = EnsureComp<LockPortalComponent>(newPortal);
            newPortalComp.Inverted = ent.Comp.Inverted;
            Dirty(newPortal, newPortalComp);

            if (!portalOneResolved)
            {
                _popup.PopupEntity(Loc.GetString("eldritch-id-card-component-link-one"), args.User, args.User);
                return;
            }

            _popup.PopupEntity(Loc.GetString("eldritch-id-card-component-link-two"), args.User, args.User);

            var portalOneComp = EnsureComp<LockPortalComponent>(ent.Comp.PortalOne!.Value);
            portalOneComp.Inverted = ent.Comp.Inverted;
            Dirty(ent.Comp.PortalOne.Value, portalOneComp);
            newPortalComp.LinkedPortal = ent.Comp.PortalOne.Value;
            portalOneComp.LinkedPortal = newPortal;
        }
    }

    private void OnAltVerb(Entity<EldritchIdCardComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!HereticOrGhoul(args.User))
            return;

        var user = args.User;

        args.Verbs.Add(new()
        {
            Act = () => Invert(ent, user),
            Text = Loc.GetString("eldritch-id-card-component-invert"),
            Message = Loc.GetString("eldritch-id-card-component-invert-message"),
            Icon = new SpriteSpecifier.Rsi(new("Objects/Misc/id_cards.rsi"), "gold"),
            Priority = 2
        });
    }

    private void OnMessage(Entity<EldritchIdCardComponent> ent, ref EldritchIdMessage args)
    {
        if (!HereticOrGhoul(args.Actor))
            return;

        if (!ent.Comp.Configs.Remove(args.Config))
            return;

        if (GetConfig(ent.Owner) is { } config)
            ent.Comp.Configs.Add(config);

        DirtyField(ent.Owner, ent.Comp, nameof(EldritchIdCardComponent.Configs));

        Shapeshift(ent.Owner, args.Config, args.Actor);
    }

    private void OnAttempt(Entity<EldritchIdCardComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HereticOrGhoul(args.User))
            args.Cancel();
    }

    private void OnMapInit(Entity<EldritchIdCardComponent> ent, ref MapInitEvent args)
    {
        InitializeEldritchId(ent);
    }

    private void OnExamine(Entity<EldritchIdCardComponent> ent, ref ExaminedEvent args)
    {
        if (!HereticOrGhoul(args.Examiner))
            return;

        if (ent.Comp.Inverted)
            args.PushMarkup(Loc.GetString("eldritch-id-card-component-examine-inverted"));

        args.PushMarkup(Loc.GetString("eldritch-id-card-component-examine-message"));
    }

    private bool HereticOrGhoul(EntityUid uid)
    {
        return HasComp<HereticComponent>(uid) || HasComp<GhoulComponent>(uid);
    }

    private void Invert(Entity<EldritchIdCardComponent> ent, EntityUid user)
    {
        ent.Comp.Inverted = !ent.Comp.Inverted;
        DirtyField(ent.Owner, ent.Comp, nameof(EldritchIdCardComponent.Inverted));

        _popup.PopupClient(Loc.GetString("eldritch-id-card-component-on-invert", ("inverted", ent.Comp.Inverted)),
            user,
            user);
    }

    private void Shapeshift(Entity<EldritchIdCardComponent?, IdCardComponent?, AccessComponent?> ent,
        EldritchIdConfiguration config,
        EntityUid user)
    {
        if (!Resolve(ent, ref ent.Comp1))
            return;

        _idCard.TryChangeFullName(ent, config.FullName, ent.Comp2, user);
        _idCard.TryChangeJobDepartment(ent, config.Departments, ent.Comp2);
        _idCard.TryChangeJobTitle(ent, config.JobTitle, ent.Comp2, user);
        _idCard.TryChangeJobIcon(ent, _proto.Index(config.JobIcon), ent.Comp2, user);
        _access.TrySetTags(ent, config.AccessTags, ent.Comp3);

        ent.Comp1.CurrentProto = config.CardPrototype;
        DirtyField(ent.Owner, ent.Comp1, nameof(EldritchIdCardComponent.CurrentProto));

        UpdateSprite((ent.Owner, ent.Comp1));
    }

    private EldritchIdConfiguration? GetConfig(Entity<IdCardComponent?, AccessComponent?, EldritchIdCardComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, false))
            return null;

        EntProtoId? proto = null;

        if (Resolve(ent, ref ent.Comp3, false))
            proto = ent.Comp3.CurrentProto;

        if (proto == null)
        {
            proto = Prototype(ent)?.ID;
            if (proto == null)
                return null;
        }

        return new EldritchIdConfiguration(ent.Comp1.FullName,
            ent.Comp1.LocalizedJobTitle,
            ent.Comp1.JobIcon,
            ent.Comp1.JobDepartments.ToList(),
            ent.Comp2.Tags.ToHashSet(),
            proto.Value);
    }

    private void ClearPortals(Entity<EldritchIdCardComponent> ent)
    {
        PredictedQueueDel(ent.Comp.PortalOne);
        PredictedQueueDel(ent.Comp.PortalTwo);

        ent.Comp.PortalOne = null;
        ent.Comp.PortalTwo = null;
        DirtyFields(ent.Owner,
            ent.Comp,
            null,
            nameof(EldritchIdCardComponent.PortalOne),
            nameof(EldritchIdCardComponent.PortalTwo));
    }

    private void EatCard(Entity<EldritchIdCardComponent> ent, Entity<IdCardComponent> idCard, EntityUid user)
    {
        if (ent.Owner == idCard.Owner)
            return;

        var config = GetConfig(idCard.AsNullable());

        if (config == null)
            return;

        ent.Comp.Configs.Add(config);
        DirtyField(ent.Owner, ent.Comp, nameof(EldritchIdCardComponent.Configs));

        _audio.PlayPredicted(ent.Comp.EatSound, Transform(idCard.Owner).Coordinates, user);
        PredictedDel(idCard.Owner);
    }

    protected virtual void UpdateSprite(Entity<EldritchIdCardComponent> ent) { }

    protected virtual bool InitializeEldritchId(Entity<EldritchIdCardComponent> ent)
    {
        if (!TryComp(ent, out IdCardComponent? id))
            return false;

        id.BypassLogging = true;
        id.CanMicrowave = false;

        ent.Comp.CurrentProto = Prototype(ent)?.ID;

        Entity<EldritchIdCardComponent, IdCardComponent> idCard = (ent.Owner, ent.Comp, id);
        Dirty(idCard);

        var ui = _compFact.GetComponent<ActivatableUIComponent>();
        ui.InHandsOnly = true;
        ui.SingleUser = true;
        ui.Key = EldritchIdUiKey.Key;

        AddComp(ent.Owner, ui, true);

        _ui.SetUi(ent.Owner, EldritchIdUiKey.Key, new InterfaceData("EldritchIdBoundUserInterface"));

        RemCompDeferred<ChameleonClothingComponent>(ent);
        return true;
    }
}
