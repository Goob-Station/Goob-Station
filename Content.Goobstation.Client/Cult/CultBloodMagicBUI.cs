using Content.Client.UserInterface.Controls;
using Content.Goobstation.Client.UserInterface;
using Content.Goobstation.Shared.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultBloodMagicBUI : EntityRadialMenuBUI
{
    private List<EntProtoId>? _entProtoIDs;

    public CultBloodMagicBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void UpdateMenuState(EntityRadialMenuState state)
    {
        _entProtoIDs = state.IDs;
        RefreshUI(ExistingMenu);
    }

    protected override IEnumerable<RadialMenuOption> CreateModels(EntityUid owner)
    {
        if (_entProtoIDs == null || _entProtoIDs.Count == 0)
            yield break;

        var models = base.CreateModels(_entProtoIDs);
        foreach (var model in models)
            yield return model;
    }
}
