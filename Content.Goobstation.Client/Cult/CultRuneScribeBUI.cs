using Content.Client.UserInterface.Controls;
using Content.Goobstation.Client.UserInterface;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.UserInterface;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultRuneScribeBUI : EntityRadialMenuBUI
{
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    private List<EntProtoId>? _entProtoIDs;

    public CultRuneScribeBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void UpdateMenuState(EntityRadialMenuState state)
    {
        _entProtoIDs = state.IDs;
    }

    protected override IEnumerable<RadialMenuOption> CreateModels(EntityUid owner)
    {
        if (_entProtoIDs == null || _entProtoIDs.Count == 0)
            yield break;

        foreach (var id in _entProtoIDs)
        {
            if (!_prot.TryIndex(id, out var ent))
                continue;

            var color = Color.Red;
            if (ent.TryGetComponent<SpriteComponent>(out var sprite, _compFact))
                color = sprite.Color;

            yield return new RadialMenuActionOption<EntProtoId>(HandleMenuOptionClick, id)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(id),
                BackgroundColor = color,
                ToolTip = ent.EditorSuffix, // works
            };
        }
    }

    protected override void HandleMenuOptionClick(EntProtoId id)
    {
        SendMessage(new BloodCultRuneScribeSelectRuneMessage(id));
    }
}
