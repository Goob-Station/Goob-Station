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
using System.Linq;

namespace Content.Goobstation.Server.Augments;

public sealed class AugmentToolPanelSystem : EntitySystem
{
    [Dependency] private readonly AugmentPowerCellSystem _augmentPowerCell = default!;
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<AugmentToolPanelComponent>(AugmentToolPanelUiKey.Key, subs =>
        {
            subs.Event<AugmentToolPanelSwitchMessage>(OnSwitchTool);
        });
    }

    private void OnSwitchTool(Entity<AugmentToolPanelComponent> augment, ref AugmentToolPanelSwitchMessage args)
    {
        if (_augment.GetBody(augment) is not {} body ||
            !TryComp<HandsComponent>(body, out var hands) ||
            !_augmentPowerCell.TryUseCharge(body, augment.Comp.SwitchCharge))
            return;

        foreach (var part in _body.GetBodyPartChildren(body))
        {
            if (part.Component.PartType != BodyPartType.Hand)
                continue;

            var handLocation = part.Component.Symmetry switch {
                BodyPartSymmetry.None => HandLocation.Middle,
                BodyPartSymmetry.Left => HandLocation.Left,
                BodyPartSymmetry.Right => HandLocation.Right,
                _ => throw new InvalidOperationException(),
            };

            // TODO: need to update this after upstream merge to use nu hands
            if (hands.Hands.Values.FirstOrDefault(hand => hand.Location == handLocation) is not {} desiredHand)
                continue;

            if (desiredHand.HeldEntity is {} item)
            {
                // if we have a tool that's currently out
                if (HasComp<AugmentToolPanelActiveItemComponent>(item))
                {
                    // deposit it back into the storage
                    RemComp<AugmentToolPanelActiveItemComponent>(item);

                    if (!_storage.PlayerInsertEntityInWorld(augment.Owner, body, item))
                    {
                        Log.Error($"Inserting tool {ToPrettyString(item)} back into {ToPrettyString(augment)} failed");
                        EnsureComp<AugmentToolPanelActiveItemComponent>(item);
                        return;
                    }
                }
                else
                {
                    _popup.PopupCursor(Loc.GetString("augment-tool-panel-hand-full"), body);
                    return;
                }

                // no longer holding a tool, stop drawing power
                _toggle.TryDeactivate(augment.Owner, user: body);
            }

            if (GetEntity(args.DesiredTool) is not {} desiredTool)
                return;

            if (!_hands.TryPickup(body, desiredTool, desiredHand))
            {
                _popup.PopupCursor(Loc.GetString("augment-tool-panel-cannot-pick-up"), body);
                return;
            }
            EnsureComp<AugmentToolPanelActiveItemComponent>(desiredTool);
            _toggle.TryActivate(augment.Owner, user: body);
        }
    }
}
