using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Cult.Events;
using Content.Goobstation.Shared.Cult.Runes;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cult;

public sealed class BloodCultRuneScribeBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public BloodCultRuneScribeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        var menu = this.CreateWindow<SimpleRadialMenu>();
        menu.Track(Owner);
        menu.SetButtons(CreateModels(Owner));

        menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOption> CreateModels(EntityUid owner)
    {
        if (!_ent.TryGetComponent<BloodCultRuneScribeComponent>(owner, out var scribe))
            yield break;

        foreach (var rune in scribe.Runes)
        {
            if (!_prot.TryIndex(rune, out var prot))
                continue;

            yield return new RadialMenuActionOption<EntProtoId>(HandleMenuOptionClick, rune)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(rune),
                ToolTip = Loc.GetString(rune),
            };
        }
    }

    private void HandleMenuOptionClick(EntProtoId id)
    {
        SendMessage(new BloodCultRuneScribeSelectRuneMessage(id));
    }
}
