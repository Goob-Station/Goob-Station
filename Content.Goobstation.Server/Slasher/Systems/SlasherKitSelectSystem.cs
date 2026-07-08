using System.Linq;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Systems;
using Content.Goobstation.Shared.Slasher.UI;
using Content.Server.RandomMetadata;
using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Station;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherKitSelectSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SlasherIncorporealSystem _incorporeal = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SlasherPrestigeSystem _prestige = default!;
    [Dependency] private readonly RandomMetadataSystem _randomMetadata = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

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
        _actions.RemoveAction(ent.Owner, incorporealComp.CorporealizeActionEnt);
        incorporealComp.IncorporealizeActionEnt = null;
        incorporealComp.CorporealizeActionEnt = null;

        var uid = ent.Owner;
        _ui.OpenUi(uid, SlasherKitSelectUiKey.Key, args.Player);
    }

    private void OnUIOpened(Entity<SlasherKitSelectComponent> ent, ref BoundUIOpenedEvent args)
    {
        SoundSpecifier? defaultMusic = null;
        foreach (var entry in ent.Comp.PostSelectionComponents.Values)
            if (entry.Component is SlasherFearComponent bloodTrail)
            {
                defaultMusic = bloodTrail.BloodTrailMusic;
                break;
            }

        var userId = CompOrNull<ActorComponent>(args.Actor)?.PlayerSession.UserId;

        var kitInfos = new List<SlasherKitInfo>();
        foreach (var (nameKey, kit) in ent.Comp.Kits)
        {
            kitInfos.Add(new SlasherKitInfo(
                kit.Gear,
                Loc.GetString(nameKey),
                string.IsNullOrEmpty(kit.Description) ? string.Empty : Loc.GetString(kit.Description),
                kit.Sprite,
                kit.BloodTrailMusic ?? defaultMusic,
                kit.AscensionId,
                kit.RequiredAscension,
                IsKitUnlocked(kit, userId),
                kit.Guide));
        }

        _ui.SetUiState(ent.Owner, SlasherKitSelectUiKey.Key, new SlasherKitSelectBoundUserInterfaceState(kitInfos));
    }

    /// <summary>
    /// A kit is unlocked if it has no prestige requirement, or the player has previously
    /// ascended with the required kit.
    /// </summary>
    private bool IsKitUnlocked(SlasherKit kit, NetUserId? userId)
    {
        if (kit.RequiredAscension == null)
            return true;

        return userId != null && _prestige.HasAscension(userId.Value, kit.RequiredAscension);
    }

    private void OnKitSelected(Entity<SlasherKitSelectComponent> ent, ref SlasherKitSelectedMessage args)
    {
        if (ent.Comp.KitSelected
            || args.Index < 0
            || args.Index >= ent.Comp.Kits.Count)
            return;

        var pickedKit = ent.Comp.Kits.Values.ElementAt(args.Index);

        if (!IsKitUnlocked(pickedKit, CompOrNull<ActorComponent>(args.Actor)?.PlayerSession.UserId))
            return;

        ent.Comp.KitSelected = true;

        if (TryComp<SlasherIncorporealComponent>(ent.Owner, out var incorporealComp))
        {
            _incorporeal.ExitIncorporeal(ent.Owner, (ent.Owner, incorporealComp));
            _actions.AddAction(ent.Owner, ref incorporealComp.IncorporealizeActionEnt, incorporealComp.IncorporealizeActionId);
            _actions.AddAction(ent.Owner, ref incorporealComp.CorporealizeActionEnt, incorporealComp.CorporealizeActionId);
            _actions.SetEnabled(incorporealComp.CorporealizeActionEnt, false);
        }

        _movement.ChangeBaseSpeed(ent.Owner,
            MovementSpeedModifierComponent.DefaultBaseWalkSpeed,
            MovementSpeedModifierComponent.DefaultBaseSprintSpeed,
            MovementSpeedModifierComponent.DefaultAcceleration);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);

        var selectedKit = pickedKit;

        EntityManager.AddComponents(ent.Owner, ent.Comp.PostSelectionComponents);

        if (selectedKit.Components.Count > 0)
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
                fearComp.JumpscareSounds = new()
                {
                    jumpscareSound
                };

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

        if (selectedKit.NameSegments is { Count: > 0 } nameSegments)
            _metaData.SetEntityName(ent.Owner, _randomMetadata.GetRandomFromSegments(nameSegments, selectedKit.NameFormat));

        _ui.CloseUi(ent.Owner, SlasherKitSelectUiKey.Key);
    }
}
