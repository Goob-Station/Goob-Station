using System.Linq;
using Content.Client._Goobstation.Research.UI;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.Research.UI;

[UsedImplicitly]
public sealed class ResearchConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private FancyResearchConsoleMenu? _consoleMenu;  // Goobstation R&D Console rework - ResearchConsoleMenu -> FancyResearchConsoleMenu

    public ResearchConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        var owner = Owner;

        _consoleMenu = this.CreateWindow<FancyResearchConsoleMenu>();   // Goobstation R&D Console rework - ResearchConsoleMenu -> FancyResearchConsoleMenu
        _consoleMenu.SetEntity(owner);

        _consoleMenu.OnTechnologyCardPressed += id =>
        {
            SendMessage(new ConsoleUnlockTechnologyMessage(id));
        };

        _consoleMenu.OnServerButtonPressed += () =>
        {
            SendMessage(new ConsoleServerSelectionMessage());
        };
    }

    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if (!args.WasModified<TechnologyPrototype>())
            return;

        if (State is not ResearchConsoleBoundInterfaceState rState)
            return;

        _consoleMenu?.UpdatePanels(rState);
        _consoleMenu?.UpdateInformationPanel(rState);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ResearchConsoleBoundInterfaceState castState)
            return;

        // Goobstation checks added
        // Thats for avoiding refresh spam when only points are updated
        if (_consoleMenu == null)
            return;
        if (!_consoleMenu.LocalState.Researches.SequenceEqual(castState.Researches))
            _consoleMenu.UpdatePanels(castState);
        if (_consoleMenu.LocalState.Points != castState.Points)
            _consoleMenu.UpdateInformationPanel(castState);
    }
}
