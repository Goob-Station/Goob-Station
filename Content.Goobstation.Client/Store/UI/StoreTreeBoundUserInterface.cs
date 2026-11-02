using Content.Shared.Store;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Store.UI;

[UsedImplicitly]
public sealed class StoreTreeBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private StoreTreeMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<StoreTreeMenu>();
        _menu.OnBuy += id => SendMessage(new StoreBuyListingMessage(id));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is StoreTreeUpdateState treeState)
            _menu?.UpdateState(treeState);
    }

    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if ((args.WasModified<StoreTreePrototype>() || args.WasModified<ListingPrototype>())
            && State is StoreTreeUpdateState treeState)
            _menu?.UpdateState(treeState, true);
    }
}
