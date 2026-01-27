using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.UserInterface;
using Content.Shared.Actions.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.UserInterface;

[Virtual]
public abstract class EntityRadialMenuBUI : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;

    public EntityRadialMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        var menu = this.CreateWindow<SimpleRadialMenu>();
        RefreshUI(menu);
        menu.OpenOverMouseScreenPosition();
    }

    public void RefreshUI(SimpleRadialMenu menu)
    {
        menu.Track(Owner);
        menu.SetButtons(CreateModels(Owner));
    }

    protected abstract IEnumerable<RadialMenuOption> CreateModels(EntityUid owner);

    protected virtual IEnumerable<RadialMenuOption> CreateModels(List<EntProtoId> ids)
    {
        if (ids == null || ids.Count == 0)
            yield break;

        foreach (var id in ids)
        {
            if (!_prot.TryIndex(id, out var ent))
                continue;

            var sprite = ent.TryGetComponent<ActionComponent>(out var ac, _compFact) ? ac.Icon : new SpriteSpecifier.EntityPrototype(id);

            yield return new RadialMenuActionOption<EntProtoId>(HandleMenuOptionClick, id)
            {
                Sprite = sprite,
                ToolTip = ent.Name,
            };
        }
    }

    protected virtual void HandleMenuOptionClick(EntProtoId id)
    {
        SendMessage(new EntityRadialMenuSelectMessage(id));
    }
}
