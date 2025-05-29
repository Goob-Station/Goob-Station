using Content.Goobstation.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Goobstation.Server.Silicons.Borgs;

public sealed class BorgSwitchableSubtypeSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, BorgSelectSubtypeMessage>(OnSubtypeSelected);
    }

    private void OnSubtypeSelected(Entity<BorgSwitchableSubtypeComponent> ent, ref BorgSelectSubtypeMessage args)
    {
        ent.Comp.BorgSubtype = args.Subtype;
        Dirty(ent);
        UpdateVisuals(ent);
        _userInterface.CloseUi((ent.Owner, null), BorgSwitchableTypeUiKey.SelectBorgType);
    }
}
