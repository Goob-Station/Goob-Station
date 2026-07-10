using Content.Client.UserInterface.Controls;
using Content.Shared._White.RadialSelector;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Pirate.Coffeemaker;

[UsedImplicitly]
public sealed class PirateCoffeemakerRadialMenuBUI : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private SimpleRadialMenu? _menu;
    private RadialSelectorState? _state;

    public PirateCoffeemakerRadialMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.OnClose += Close;
        _menu.Track(Owner);
        _menu.OpenOverMouseScreenPosition();

        if (_state != null)
            UpdateMenu(_state);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not RadialSelectorState radialState)
            return;

        _state = radialState;
        UpdateMenu(radialState);
    }

    private void UpdateMenu(RadialSelectorState state)
    {
        if (_menu == null)
            return;

        var options = ConvertEntries(state.Entries);
        _menu.SetButtons(options);
    }

    private List<RadialMenuOptionBase> ConvertEntries(List<RadialSelectorEntry> entries)
    {
        var result = new List<RadialMenuOptionBase>(entries.Count);

        foreach (var entry in entries)
        {
            if (entry.Category != null)
            {
                var nested = ConvertEntries(entry.Category.Entries);
                result.Add(new RadialMenuNestedLayerOption(nested)
                {
                    IconSpecifier = RadialMenuIconSpecifier.With(entry.Category.Icon),
                    ToolTip = Loc.GetString(entry.Category.Name),
                });
                continue;
            }

            if (entry.Prototype == null)
                continue;

            result.Add(new RadialMenuActionOption<string>(OnSelected, entry.Prototype)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(entry.Icon),
                ToolTip = GetTooltip(entry.Prototype),
            });
        }

        return result;
    }

    private void OnSelected(string selected)
    {
        var msg = new RadialSelectorSelectedMessage(selected);
        SendPredictedMessage(msg);
    }

    private string GetTooltip(string selected)
    {
        if (_prototypeManager.TryIndex<EntityPrototype>(selected, out var proto))
            return Loc.GetString(proto.Name);

        return Loc.GetString(selected);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _menu?.Dispose();
    }
}
