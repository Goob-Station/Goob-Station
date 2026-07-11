using Content.Shared._MACRO.StrangeMoods;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MACRO.StrangeMoods;

[UsedImplicitly]
public sealed class StrangeMoodsBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private StrangeMoodsMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<StrangeMoodsMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not StrangeMoodsBuiState msg)
            return;

        _menu?.Update(msg);
    }
}