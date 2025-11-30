using System.Linq;
using Content.Shared._White.RadialSelector;
using Content.Shared.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SetSelector;

public sealed class RadialItemSelectorSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadialItemSelectorComponent, RadialSelectorSelectedMessage>(OnSelected);
        SubscribeLocalEvent<RadialItemSelectorComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpen);
    }

    private void OnBeforeOpen(Entity<RadialItemSelectorComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        _ui.SetUiState(ent.Owner, RadialSelectorUiKey.Key, new RadialSelectorState(ent.Comp.Entries));
    }

    private void OnSelected(Entity<RadialItemSelectorComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var selected = args.SelectedItem;
        if (!_proto.HasIndex(selected) || ent.Comp.Entries.All(x => x.Prototype != selected))
            return;

        var coords = Transform(ent).Coordinates;
        if (coords.IsValid(EntityManager))
            PredictedSpawnAtPosition(selected, coords);

        _ui.CloseUi(ent.Owner, RadialSelectorUiKey.Key, args.Actor);
        PredictedDel(ent.Owner);
    }
}
