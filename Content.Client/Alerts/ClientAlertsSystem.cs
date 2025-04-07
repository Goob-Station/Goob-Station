// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Linq;
using Content.Shared.Alert;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.Alerts;

[UsedImplicitly]
public sealed class ClientAlertsSystem : AlertsSystem
{
    public AlertOrderPrototype? AlertOrder { get; set; }

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public event EventHandler? ClearAlerts;
    public event EventHandler<IReadOnlyDictionary<AlertKey, AlertState>>? SyncAlerts;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlertsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<AlertsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<AlertsComponent, ComponentHandleState>(OnHandleState);
    }
    protected override void LoadPrototypes()
    {
        base.LoadPrototypes();

        AlertOrder = _prototypeManager.EnumeratePrototypes<AlertOrderPrototype>().FirstOrDefault();
        if (AlertOrder == null)
            Log.Error("No alertOrder prototype found, alerts will be in random order");
    }

    public IReadOnlyDictionary<AlertKey, AlertState>? ActiveAlerts
    {
        get
        {
            var ent = _playerManager.LocalEntity;
            return ent is not null
                ? GetActiveAlerts(ent.Value)
                : null;
        }
    }

    private void OnHandleState(Entity<AlertsComponent> alerts, ref ComponentHandleState args)
    {
        if (args.Current is not AlertComponentState cast)
            return;

        alerts.Comp.Alerts = cast.Alerts;

        UpdateHud(alerts);
    }

    protected override void AfterShowAlert(Entity<AlertsComponent> alerts)
    {
        UpdateHud(alerts);
    }

    protected override void AfterClearAlert(Entity<AlertsComponent> alerts)
    {
        UpdateHud(alerts);
    }

    private void UpdateHud(Entity<AlertsComponent> entity)
    {
        if (_playerManager.LocalEntity == entity.Owner)
            SyncAlerts?.Invoke(this, entity.Comp.Alerts);
    }

    private void OnPlayerAttached(EntityUid uid, AlertsComponent component, LocalPlayerAttachedEvent args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        SyncAlerts?.Invoke(this, component.Alerts);
    }

    protected override void HandleComponentShutdown(EntityUid uid, AlertsComponent component, ComponentShutdown args)
    {
        base.HandleComponentShutdown(uid, component, args);

        if (_playerManager.LocalEntity != uid)
            return;

        ClearAlerts?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayerDetached(EntityUid uid, AlertsComponent component, LocalPlayerDetachedEvent args)
    {
        ClearAlerts?.Invoke(this, EventArgs.Empty);
    }

    public void AlertClicked(ProtoId<AlertPrototype> alertType)
    {
        RaisePredictiveEvent(new ClickAlertEvent(alertType));
    }
}
