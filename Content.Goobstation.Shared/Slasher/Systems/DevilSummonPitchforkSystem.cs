using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class DevilSummonPitchforkSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilSummonPitchforkComponent, DevilSummonPitchforkEvent>(OnSummon);
    }

    private void OnSummon(Entity<DevilSummonPitchforkComponent> ent, ref DevilSummonPitchforkEvent args)
    {
        // Fail if the user has no hands
        if (!TryComp<HandsComponent>(ent.Owner, out var hands) || hands.Hands.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("wieldable-component-no-hands"), ent.Owner, ent.Owner);
            args.Handled = true;
            return;
        }

        // Ensure prototype exists
        if (!_protos.TryIndex(ent.Comp.Prototype, out EntityPrototype? _))
        {
            args.Handled = true;
            return;
        }

        // Spawn the item at the user
        var item = Spawn(ent.Comp.Prototype, _xform.GetMoverCoordinates(ent.Owner));

        // Try to place in hand
        _hands.TryPickupAnyHand(ent.Owner, item);

        args.Handled = true;
    }
}
