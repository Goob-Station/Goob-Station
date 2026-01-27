using Content.Client.UserInterface.Controls;
using Content.Goobstation.Client.UserInterface;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Runes;
using Content.Shared.Actions.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultRuneScribeBUI : EntityRadialMenuBUI
{
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public CultRuneScribeBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override IEnumerable<RadialMenuOption> CreateModels(EntityUid owner)
    {
        if (!_ent.TryGetComponent<BloodCultRuneScribeComponent>(owner, out var scribe))
            yield break;

        foreach (var rune in scribe.KnownRunes)
        {
            if (!_prot.TryIndex(rune, out var ent))
                continue;

            var color = Color.Red;
            if (ent.TryGetComponent<SpriteComponent>(out var sprite, _compFact))
                color = sprite.Color;

            yield return new RadialMenuActionOption<EntProtoId>(HandleMenuOptionClick, rune)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(rune),
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
