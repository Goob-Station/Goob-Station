using Content.Client.UserInterface.Controls;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic.Messages;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic.UI;

[UsedImplicitly]
public sealed class MawedCrucibleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent(Owner, out MawedCrucibleComponent? crucible))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(crucible.Potions);
        _menu.SetButtons(buttonModels);

        _menu.Open();
    }

    private IEnumerable<RadialMenuActionOption> ConvertToButtons(IReadOnlyList<EntProtoId> entProtoIds)
    {
        var models = new RadialMenuActionOption[entProtoIds.Count];
        for (var i = 0; i < entProtoIds.Count; i++)
        {
            var protoId = entProtoIds[i];
            var proto = _proto.Index(protoId);
            models[i] = new RadialMenuActionOption<EntProtoId>(HandleRadialMenuClick, protoId)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(protoId),
                ToolTip = proto.Name,
            };
        }

        return models;
    }

    private void HandleRadialMenuClick(EntProtoId proto)
    {
        SendPredictedMessage(new MawedCrucibleMessage(proto));
    }
}
