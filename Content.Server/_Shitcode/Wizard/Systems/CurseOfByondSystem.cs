// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Components;
using Content.Shared.Alert;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class CurseOfByondSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CurseOfByondComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CurseOfByondComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, CurseOfByondComponent component, ComponentStartup args)
    {
        _alertsSystem.ShowAlert(uid, component.CurseOfByondAlertKey);
    }

    private void OnShutdown(EntityUid uid, CurseOfByondComponent component, ComponentShutdown args)
    {
        _alertsSystem.ClearAlert(uid, component.CurseOfByondAlertKey);
    }
}