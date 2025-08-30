using Content.Goobstation.Shared.Augments;
using Content.Server.Power.EntitySystems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Augments;

public sealed class AugmentToolPanelSystem : SharedAugmentToolPanelSystem
{
    [Dependency] private readonly AugmentPowerCellSystem _augmentPowerCell = default!;
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<BodyPartComponent> _partQuery;

    public override void Initialize()
    {
        base.Initialize();

        _handsQuery = GetEntityQuery<HandsComponent>();
        _partQuery = GetEntityQuery<BodyPartComponent>();

        SubscribeLocalEvent<AugmentToolPanelComponent, AugmentLostPowerEvent>(OnLostPower);
        Subs.BuiEvents<AugmentToolPanelComponent>(AugmentToolPanelUiKey.Key, subs =>
        {
            subs.Event<AugmentToolPanelSwitchMessage>(OnSwitchTool);
        });
    }

    private void OnLostPower(Entity<AugmentToolPanelComponent> augment, ref AugmentLostPowerEvent args)
    {
        // items automatically retract if you lose power
        SwitchTool(augment, null, args.Body);
    }

    private void OnSwitchTool(Entity<AugmentToolPanelComponent> augment, ref AugmentToolPanelSwitchMessage args)
    {
        if (_augment.GetBody(augment) is not {} body ||
            !_augmentPowerCell.TryUseChargeBody(body, augment.Comp.SwitchCharge))
            return;

        SwitchTool(augment, GetEntity(args.DesiredTool), body);
    }

    /// <summary>
    /// Switches to a tool using a hand derived from the augment's arm.
    /// </summary>
    public void SwitchTool(Entity<AugmentToolPanelComponent> augment, EntityUid? tool, EntityUid body)
    {
        if (!_handsQuery.TryComp(body, out var hands))
            return;

        // organs get parented to the body part
        // the arm's symmetry is the same as the hand
        var partUid = Transform(augment).ParentUid;
        var part = _partQuery.Comp(partUid);
        var location = part.Symmetry switch {
            BodyPartSymmetry.None => HandLocation.Middle,
            BodyPartSymmetry.Left => HandLocation.Left,
            BodyPartSymmetry.Right => HandLocation.Right
        };

        // TODO: need to update this after upstream merge to use nu hands
        foreach (var hand in hands.Hands.Values)
        {
            if (hand.Location == location)
            {
                SwitchTool(augment, tool, body, hand);
                return;
            }
        }

        // no hand found rip bozo
        _popup.PopupEntity(Loc.GetString("augment-tool-panel-no-hand"), body, body, PopupType.LargeCaution);
    }

    /// <summary>
    /// Switches to a tool using the specified hand.
    /// </summary>
    public void SwitchTool(Entity<AugmentToolPanelComponent> augment, EntityUid? desiredTool, EntityUid body, Hand hand)
    {
        if (hand.HeldEntity is {} item)
        {
            // if we have a tool that's currently out, deposit it back into the storage
            if (RemComp<AugmentToolPanelActiveItemComponent>(item))
            {
                if (!_storage.PlayerInsertEntityInWorld(augment.Owner, body, item))
                {
                    Log.Error($"Inserting tool {ToPrettyString(item)} back into {ToPrettyString(augment)} failed");
                    EnsureComp<AugmentToolPanelActiveItemComponent>(item); // prevent exploits
                    return;
                }

                if (desiredTool == null) // don't double popup, only show it when deselecting
                    _popup.PopupEntity(Loc.GetString("augment-tool-panel-retracted", ("item", item)), body, body);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("augment-tool-panel-hand-full"), body, body, PopupType.SmallCaution);
                return;
            }

            // no longer holding a tool, stop drawing power
            _toggle.TryDeactivate(augment.Owner, user: body);
        }

        if (desiredTool is not {} tool)
            return;

        if (!_hands.TryPickup(body, tool, hand))
        {
            _popup.PopupEntity(Loc.GetString("augment-tool-panel-cannot-pick-up"), body, body, PopupType.SmallCaution);
            return;
        }

        EnsureComp<AugmentToolPanelActiveItemComponent>(tool);
        _toggle.TryActivate(augment.Owner, user: body);
        _popup.PopupEntity(Loc.GetString("augment-tool-panel-selected", ("item", tool)), body, body);
    }
}
