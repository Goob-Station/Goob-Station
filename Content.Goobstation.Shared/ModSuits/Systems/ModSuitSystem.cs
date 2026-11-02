using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private HideLayerClothingSystem _hideLayer = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedWiresSystem _wires = default!;
    [Dependency] private ToggleableClothingSystem _toggleableClothing = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    private readonly HashSet<EntityUid> _movingParts = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateDeployQueues();
        UpdateActivation();
        UpdatePowerAlerts();
    }

    [SubscribeLocalEvent]
    private void OnSuitInit(Entity<ModSuitComponent> ent, ref ComponentInit args)
    {
        ent.Comp.PartsContainer = _container.EnsureContainer<Container>(ent, ent.Comp.PartsContainerId);
        ent.Comp.ModulesContainer = _container.EnsureContainer<Container>(ent, ent.Comp.ModulesContainerId);
    }

    [SubscribeLocalEvent]
    private void OnPartInit(Entity<ModPartComponent> ent, ref ComponentInit args)
    {
        ent.Comp.StoredContainer = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.StoredContainerId);
    }

    [SubscribeLocalEvent]
    private void OnSuitMapInit(Entity<ModSuitComponent> ent, ref MapInitEvent args)
    {
        _actionContainer.EnsureAction(ent, ref ent.Comp.ActivateActionEntity, ent.Comp.ActivateAction);
        _actionContainer.EnsureAction(ent, ref ent.Comp.DeployActionEntity, ent.Comp.DeployAction);

        SpawnParts(ent);
        SpawnStartingModules(ent);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnSuitShutdown(Entity<ModSuitComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        foreach (var part in ent.Comp.PartEntities.Values)
        {
            if (!TerminatingOrDeleted(part))
                QueueDel(part);
        }
    }

    [SubscribeLocalEvent]
    private void OnGetActions(Entity<ModSuitComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags is not { } flags || (flags & ent.Comp.RequiredSlot) == 0)
            return;

        args.AddAction(ent.Comp.ActivateActionEntity);
        args.AddAction(ent.Comp.DeployActionEntity);
    }

    [SubscribeLocalEvent]
    private void OnSuitEquipped(Entity<ModSuitComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (_timing.ApplyingState || (args.Clothing.InSlotFlag & ent.Comp.RequiredSlot) == 0)
            return;

        SetWearer(ent, args.Wearer);
    }

    [SubscribeLocalEvent]
    private void OnSuitUnequipped(Entity<ModSuitComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (_timing.ApplyingState || ent.Comp.Wearer != args.Wearer)
            return;

        if (!_toggleableClothing.IsInventoryTransferring(args.Wearer)
            && !TerminatingOrDeleted(ent)
            && !TerminatingOrDeleted(args.Wearer))
        {
            if (ent.Comp.State != ModSuitState.Inactive)
                ForceDeactivate(ent, true);

            RetractAllParts(ent, args.Wearer);
        }

        SetWearer(ent, null);
    }

    [SubscribeLocalEvent]
    private void OnSuitUnequipAttempt(Entity<ModSuitComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (ent.Comp.State == ModSuitState.Inactive)
            return;

        args.Reason = "modsuit-deactivate-first";
        args.Cancel();
    }

    private void SetWearer(Entity<ModSuitComponent> ent, EntityUid? wearer)
    {
        var old = ent.Comp.Wearer;
        if (old == wearer)
            return;

        if (old != null)
        {
            RemoveModuleActions(ent, old.Value);
            if (!_timing.ApplyingState)
                _alerts.ClearAlert(old.Value, ent.Comp.PowerAlert);
        }

        ent.Comp.Wearer = wearer;
        ent.Comp.DeployQueue.Clear();
        Dirty(ent);

        if (wearer != null)
        {
            GrantModuleActions(ent, wearer.Value);
            UpdatePowerAlert(ent);
        }

        var ev = new ModSuitWearerChangedEvent(old, wearer);
        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            RaiseLocalEvent(module, ref ev);
        }
    }

    [SubscribeLocalEvent]
    private void OnActivateAction(Entity<ModSuitComponent> ent, ref ModSuitActivateActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        TryToggleActivation(ent, args.Performer);
    }

    [SubscribeLocalEvent]
    private void OnDeployAction(Entity<ModSuitComponent> ent, ref ModSuitDeployActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ToggleAllParts(ent, args.Performer);
    }

    [SubscribeLocalEvent]
    private void OnGetEquipmentVerbs(Entity<ModSuitComponent> ent, ref GetVerbsEvent<EquipmentVerb> args)
    {
        AddEquipmentVerbs(ent, args.User, args.CanComplexInteract, args.Verbs);
    }

    [SubscribeLocalEvent]
    private void OnGetRelayedEquipmentVerbs(Entity<ModSuitComponent> ent, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        AddEquipmentVerbs(ent, args.Args.User, args.Args.CanComplexInteract, args.Args.Verbs);
    }

    private void AddEquipmentVerbs(Entity<ModSuitComponent> ent, EntityUid user, bool canComplexInteract, SortedSet<EquipmentVerb> verbs)
    {
        if (!canComplexInteract || ent.Comp.Wearer is not { } wearer)
            return;

        if (user == wearer)
        {
            verbs.Add(new EquipmentVerb
            {
                Text = Loc.GetString("modsuit-verb-interface"),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
                Priority = 6,
                Act = () => _ui.OpenUi(ent.Owner, ModSuitUiKey.Key, user),
            });
        }

        if (user != wearer && !_interaction.InRangeUnobstructed(user, wearer))
            return;

        var active = ent.Comp.State != ModSuitState.Inactive;
        verbs.Add(new EquipmentVerb
        {
            Text = Loc.GetString(active ? "modsuit-verb-deactivate" : "modsuit-verb-activate"),
            Icon = new SpriteSpecifier.Texture(new ResPath(active
                ? "/Textures/Interface/VerbIcons/unlock.svg.192dpi.png"
                : "/Textures/Interface/VerbIcons/lock.svg.192dpi.png")),
            Priority = 5,
            Act = () =>
            {
                if (user == wearer)
                    TryToggleActivation(ent, user);
                else
                    StartOtherUserToggle(ent, user, wearer);
            },
        });
    }

    [SubscribeLocalEvent]
    private void OnGetAlternativeVerbs(Entity<ModSuitComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanComplexInteract || ent.Comp.Wearer != null)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("modsuit-verb-interface"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => _ui.OpenUi(ent.Owner, ModSuitUiKey.Key, user),
        });
    }

    private void StartOtherUserToggle(Entity<ModSuitComponent> ent, EntityUid user, EntityUid wearer)
    {
        var doAfter = new DoAfterArgs(EntityManager, user, ent.Comp.OtherUserToggleTime, new ModSuitToggleDoAfterEvent(), ent, wearer, ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Entity(user, EntityManager)), ("item", ent.Owner));
        _popup.PopupEntity(popup, wearer, wearer, PopupType.Large);
    }

    [SubscribeLocalEvent]
    private void OnOtherUserToggleDoAfter(Entity<ModSuitComponent> ent, ref ModSuitToggleDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        TryToggleActivation(ent, args.User);
    }

    [SubscribeLocalEvent]
    private void OnAttemptChangePanel(Entity<ModSuitComponent> ent, ref AttemptChangePanelEvent args)
    {
        if (args.Cancelled || ent.Comp.State == ModSuitState.Inactive)
            return;

        if (args.User is { } user)
            _popup.PopupClient(Loc.GetString("modsuit-panel-sealed"), ent, user);

        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnInteractUsing(Entity<ModSuitComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<ModModuleComponent>(args.Used))
        {
            args.Handled = true;
            TryInstallModule(ent, args.Used, args.User);
            return;
        }

        if (GetCore(ent) is { } core)
            RaiseLocalEvent(core, args);
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<ModSuitComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(ModSuitComponent)))
        {
            var theme = _proto.Index(ent.Comp.Theme);
            args.PushMarkup(Loc.GetString("modsuit-examine-complexity",
                ("current", GetComplexity(ent)),
                ("max", theme.ComplexityMax)));

            if (GetBattery(ent) is { } battery)
            {
                var percent = (int) MathF.Round(_battery.GetChargeLevel(battery.AsNullable()) * 100f);
                args.PushMarkup(Loc.GetString("modsuit-examine-charge", ("percent", percent)));
            }
            else
            {
                args.PushMarkup(Loc.GetString("modsuit-examine-no-core"));
            }

            foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
            {
                var builtin = TryComp<ModModuleComponent>(module, out var moduleComp) && !moduleComp.Removable;
                args.PushMarkup(Loc.GetString(builtin ? "modsuit-examine-module-builtin" : "modsuit-examine-module", ("module", module)));
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnSpeedRefresh(Entity<ModSuitComponent> ent, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (ent.Comp.State != ModSuitState.Active || ent.Comp.Powered)
            return;

        args.Args.ModifySpeed(ent.Comp.UnpoweredSpeedModifier);
    }

    private void RefreshSpeed(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.Wearer is { } wearer)
            _movementSpeed.RefreshMovementSpeedModifiers(wearer);
    }

    private bool CanAccessInternals(EntityUid suit)
    {
        return !HasComp<WiresPanelComponent>(suit) || _wires.IsPanelOpen(suit);
    }

    private bool HasOpenPanel(EntityUid suit)
    {
        return HasComp<WiresPanelComponent>(suit) && _wires.IsPanelOpen(suit);
    }

    private bool ShouldStep(ModSuitComponent comp)
    {
        return _net.IsServer || comp.Wearer != null && comp.Wearer == _player.LocalEntity;
    }

    private void PopupFail(Entity<ModSuitComponent> ent, EntityUid? user, string loc)
    {
        if (user == null)
            return;

        _popup.PopupClient(Loc.GetString(loc), ent, user.Value);
        _audio.PlayPredicted(ent.Comp.FailSound, ent, user.Value);
    }
}
