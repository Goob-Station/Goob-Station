using System.Linq;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Systems;
using Content.Goobstation.Shared.Slasher.UI;
using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Station;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherKitSelectSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SlasherIncorporealSystem _incorporeal = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

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
        var kitInfos = new List<SlasherKitInfo>();
        foreach (var (nameKey, kit) in ent.Comp.Kits)
        {
            kitInfos.Add(new SlasherKitInfo(
                kit.Gear,
                Loc.GetString(nameKey),
                string.IsNullOrEmpty(kit.Description) ? string.Empty : Loc.GetString(kit.Description),
                kit.Sprite));
        }

        _ui.SetUiState(ent.Owner, SlasherKitSelectUiKey.Key, new SlasherKitSelectBoundUserInterfaceState(kitInfos));
    }

    private void OnKitSelected(Entity<SlasherKitSelectComponent> ent, ref SlasherKitSelectedMessage args)
    {
        if (ent.Comp.KitSelected)
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

        if (args.Index < 0 || args.Index >= ent.Comp.Kits.Count)
            return;

        var selectedKit = ent.Comp.Kits.Values.ElementAt(args.Index);

        EntityManager.AddComponents(ent.Owner, ent.Comp.PostSelectionComponents);

        _stationSpawning.EquipStartingGear(ent.Owner, selectedKit.Gear);

        if (TryComp<SlasherSummonMacheteComponent>(ent.Owner, out var summonComp))
        {
            if (selectedKit.MachetePrototype is { } macheteProto)
                summonComp.MachetePrototype = macheteProto;
        }

        if (TryComp<SlasherBloodTrailComponent>(ent.Owner, out var bloodTrail))
        {
            if (selectedKit.BloodTrailMusic is { } bloodMusic)
                bloodTrail.BloodTrailMusic = bloodMusic;

            if (selectedKit.JumpscareSound is { } jumpscareSound)
                bloodTrail.JumpscareSounds = new()
                {
                    jumpscareSound
                };

            if (selectedKit.BloodTrailReagent is { } bloodReagent)
                bloodTrail.BloodTrailReagent = bloodReagent;
        }

        if (TryComp<SlasherSummonMeatSpikeComponent>(ent.Owner, out var meatSpikeComp))
        {
            if (selectedKit.MeatSpikePrototype is { } meatSpikeProto)
                meatSpikeComp.MeatSpikePrototype = meatSpikeProto;
        }

        if (TryComp<SlasherSoulStealComponent>(ent.Owner, out var soulSteal))
        {
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
