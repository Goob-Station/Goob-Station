using Content.Goobstation.Shared.Slasher;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Systems;
using Content.Goobstation.Shared.Slasher.UI;
using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Station;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherKitSelectSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SlasherIncorporealSystem _incorporeal = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SlasherPrestigeManager _prestige = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherKitSelectComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherKitSelectComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<SlasherKitSelectComponent, SlasherKitSelectedMessage>(OnKitSelected);
    }

    private void OnPlayerAttached(Entity<SlasherKitSelectComponent> ent, ref PlayerAttachedEvent args)
    {
        if (ent.Comp.KitSelected)
            return;

        _movement.ChangeBaseSpeed(ent.Owner, 0f, 0f, MovementSpeedModifierComponent.DefaultAcceleration);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);

        var incorporealComp = EnsureComp<SlasherIncorporealComponent>(ent.Owner);
        _incorporeal.EnterIncorporeal(ent.Owner, (ent.Owner, incorporealComp));
        _actions.RemoveAction(ent.Owner, incorporealComp.IncorporealizeActionEnt);
        incorporealComp.IncorporealizeActionEnt = null;

        _ui.OpenUi(ent.Owner, SlasherKitSelectUiKey.Key, args.Player);
    }

    private void OnUIOpened(Entity<SlasherKitSelectComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!TryComp<ActorComponent>(args.Actor, out var actor))
            return;

        var userId = actor.PlayerSession.UserId;
        var kitInfos = new List<SlasherKitInfo>();
        foreach (var (id, kit) in _proto.Index(ent.Comp.KitList).Kits)
        {
            kitInfos.Add(new SlasherKitInfo(
                id,
                new LocId(id),
                kit.Description,
                kit.Sprite,
                kit.BloodTrailMusic ?? ent.Comp.DefaultThemeSong,
                kit.AscensionId,
                kit.RequiredAscension,
                IsKitUnlocked(ent.Comp, kit, userId),
                kit.Guide));
        }

        _ui.SetUiState(ent.Owner, SlasherKitSelectUiKey.Key, new SlasherKitSelectBoundUserInterfaceState(kitInfos));
    }

    private bool IsKitUnlocked(SlasherKitSelectComponent comp, SlasherKit kit, NetUserId userId)
    {
        return comp.IgnoreAscensionLocks
               || kit.RequiredAscension == null
               || _prestige.HasAscension(userId, kit.RequiredAscension);
    }

    private void OnKitSelected(Entity<SlasherKitSelectComponent> ent, ref SlasherKitSelectedMessage args)
    {
        var kitList = _proto.Index(ent.Comp.KitList);
        if (ent.Comp.KitSelected
            || !kitList.Kits.TryGetValue(args.KitId, out var selectedKit)
            || !TryComp<ActorComponent>(args.Actor, out var actor)
            || !IsKitUnlocked(ent.Comp, selectedKit, actor.PlayerSession.UserId))
            return;

        ent.Comp.KitSelected = true;

        if (TryComp<SlasherIncorporealComponent>(ent.Owner, out var incorporealComp))
        {
            _incorporeal.ExitIncorporeal(ent.Owner, (ent.Owner, incorporealComp));
            _actions.AddAction(ent.Owner, ref incorporealComp.IncorporealizeActionEnt, incorporealComp.IncorporealizeActionId);
        }

        _movement.ChangeBaseSpeed(ent.Owner,
            MovementSpeedModifierComponent.DefaultBaseWalkSpeed,
            MovementSpeedModifierComponent.DefaultBaseSprintSpeed,
            MovementSpeedModifierComponent.DefaultAcceleration);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);

        EntityManager.AddComponents(ent.Owner, kitList.PostSelectionComponents);
        EntityManager.AddComponents(ent.Owner, selectedKit.Components);

        foreach (var compName in selectedKit.RemoveComponents)
            if (Factory.TryGetRegistration(compName, out var registration))
                RemComp(ent.Owner, registration.Type);

        _stationSpawning.EquipStartingGear(ent.Owner, selectedKit.Gear);

        if (TryComp<SlasherSummonMacheteComponent>(ent.Owner, out var summonComp))
        {
            if (selectedKit.MachetePrototype is { } macheteProto)
                summonComp.MachetePrototype = macheteProto;
        }

        if (TryComp<SlasherFearComponent>(ent.Owner, out var fearComp))
        {
            if (selectedKit.FearStyle.Count > 0)
                fearComp.FearStyle = selectedKit.FearStyle;

            if (selectedKit.BloodTrailMusic is { } bloodMusic)
                fearComp.BloodTrailMusic = bloodMusic;

            if (selectedKit.JumpscareSound is { } jumpscareSound)
                fearComp.JumpscareSounds = new() { jumpscareSound };

            if (selectedKit.BloodTrailReagent is { } bloodReagent)
                fearComp.BloodTrailReagent = bloodReagent;

            Dirty(ent.Owner, fearComp);
        }

        if (TryComp<SlasherSummonMeatSpikeComponent>(ent.Owner, out var meatSpikeComp))
        {
            if (selectedKit.MeatSpikePrototype is { } meatSpikeProto)
                meatSpikeComp.MeatSpikePrototype = meatSpikeProto;
        }

        if (TryComp<SlasherSoulStealComponent>(ent.Owner, out var soulSteal))
        {
            soulSteal.AscensionId = selectedKit.AscensionId;

            if (selectedKit.AscensionGear is { } ascensionGear)
                soulSteal.AscensionGear = ascensionGear;

            if (selectedKit.AscendanceAnnouncementKey is { } announcementKey)
                soulSteal.AscendanceAnnouncementKey = announcementKey;

            if (selectedKit.AscendanceSound is { } ascendanceSound)
                soulSteal.AscendanceSound = ascendanceSound;

            if (selectedKit.SoulStealSound is { } soulStealSound)
                soulSteal.SoulStealSound = soulStealSound;
        }

        _ui.CloseUi(ent.Owner, SlasherKitSelectUiKey.Key);
    }
}
