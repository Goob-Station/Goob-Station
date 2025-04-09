// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Doors.Electronics;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.Doors.Electronics;

public sealed class DoorElectronicsBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private DoorElectronicsConfigurationMenu? _window;

    public DoorElectronicsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<DoorElectronicsConfigurationMenu>();
        _window.OnAccessChanged += UpdateConfiguration;
        Reset();
    }

    public override void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        base.OnProtoReload(args);

        if (!args.WasModified<AccessLevelPrototype>())
            return;

        Reset();
    }

    private void Reset()
    {
        List<ProtoId<AccessLevelPrototype>> accessLevels = new();

        foreach (var accessLevel in _prototypeManager.EnumeratePrototypes<AccessLevelPrototype>())
        {
            if (accessLevel.Name != null)
            {
                accessLevels.Add(accessLevel.ID);
            }
        }

        accessLevels.Sort();
        _window?.Reset(_prototypeManager, accessLevels);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        var castState = (DoorElectronicsConfigurationState) state;

        _window?.UpdateState(castState);
    }

    public void UpdateConfiguration(List<ProtoId<AccessLevelPrototype>> newAccessList)
    {
        SendMessage(new DoorElectronicsUpdateConfigurationMessage(newAccessList));
    }
}