// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Research.UI;

[UsedImplicitly]
public sealed class FancyResearchConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private FancyResearchConsoleMenu? _consoleMenu;

    protected override void Open()
    {
        base.Open();

        _consoleMenu = this.CreateWindow<FancyResearchConsoleMenu>();
        _consoleMenu.SetEntity(Owner);
        _consoleMenu.OnTechnologyCardPressed += id => SendMessage(new ConsoleUnlockTechnologyMessage(id));
        _consoleMenu.OnServerButtonPressed += () => SendMessage(new ConsoleServerSelectionMessage());
    }

    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if (args.WasModified<TechnologyPrototype>() && State is ResearchConsoleBoundInterfaceState state)
            _consoleMenu?.UpdateState(state.Points, state.Researches, true);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ResearchConsoleBoundInterfaceState castState)
            _consoleMenu?.UpdateState(castState.Points, castState.Researches);
    }
}
