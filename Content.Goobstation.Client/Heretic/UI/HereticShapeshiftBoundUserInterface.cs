using Content.Client.UserInterface.Controls;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Polymorph;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Heretic.UI;

public sealed class HereticShapeshiftBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public HereticShapeshiftBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
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

    private IEnumerable<RadialMenuOption> CreateModels(EntityUid uid)
    {
        if (!_ent.TryGetComponent<ShapeshiftActionComponent>(uid, out var shapeshift))
            yield break;

        foreach (var poly in shapeshift.Polymorphs)
        {
            if (!_prot.TryIndex(poly, out var prototype)
            || prototype.Configuration.Entity == null)
                continue;

            var ent = _prot.Index(prototype.Configuration.Entity.Value);
            yield return new RadialMenuActionOption<PolymorphPrototype>(HandleMenuOptionClick, prototype)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(ent.ID),
                ToolTip = Loc.GetString(ent.Name),
            };
        }
    }

    private void HandleMenuOptionClick(PolymorphPrototype prototype)
    {
        SendMessage(new HereticShapeshiftMessage(prototype));
    }
}
