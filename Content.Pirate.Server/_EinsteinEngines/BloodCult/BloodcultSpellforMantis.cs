using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Content.Shared.Popups;
using Content.Pirate.Shared.BloodCult;

namespace Content.Pirate.Server.BloodCult
{
    public sealed class BloodCultSpellsSystem : EntitySystem
    {

        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SummonEquipmentEvent>(OnSummonEquipment);
        }
        private void OnSummonEquipment(SummonEquipmentEvent ev)
        {
            if (ev.Handled)
                return;

            foreach (var (slot, protoId) in ev.Prototypes)
            {
                var entity = Spawn(protoId, _transform.GetMapCoordinates(ev.Performer));
                if (!_hands.TryPickupAnyHand(ev.Performer, entity))
                {
                    _popup.PopupEntity(Loc.GetString("cult-magic-no-empty-hand"), ev.Performer, ev.Performer);
                    _actions.SetCooldown(ev.Action, TimeSpan.FromSeconds(1));
                    QueueDel(entity);
                    ev.Handled = true;
                    return;
                }
            }

            ev.Handled = true;
        }
    }
}
