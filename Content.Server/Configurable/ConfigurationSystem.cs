// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Configurable;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using static Content.Shared.Configurable.ConfigurationComponent;

namespace Content.Server.Configurable;

public sealed class ConfigurationSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConfigurationComponent, ConfigurationUpdatedMessage>(OnUpdate);
        SubscribeLocalEvent<ConfigurationComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ConfigurationComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ConfigurationComponent, ContainerIsInsertingAttemptEvent>(OnInsert);
    }

    private void OnInteractUsing(EntityUid uid, ConfigurationComponent component, InteractUsingEvent args)
    {
        // TODO use activatable ui system
        if (args.Handled)
            return;

        if (!_toolSystem.HasQuality(args.Used, component.QualityNeeded))
            return;

        args.Handled = _uiSystem.TryOpenUi(uid, ConfigurationUiKey.Key, args.User);
    }

    private void OnStartup(EntityUid uid, ConfigurationComponent component, ComponentStartup args)
    {
        UpdateUi(uid, component);
    }

    private void UpdateUi(EntityUid uid, ConfigurationComponent component)
    {
        if (_uiSystem.HasUi(uid, ConfigurationUiKey.Key))
            _uiSystem.SetUiState(uid, ConfigurationUiKey.Key, new ConfigurationBoundUserInterfaceState(component.Config));
    }

    private void OnUpdate(EntityUid uid, ConfigurationComponent component, ConfigurationUpdatedMessage args)
    {
        foreach (var key in component.Config.Keys)
        {
            var value = args.Config.GetValueOrDefault(key);

            if (string.IsNullOrWhiteSpace(value) || component.Validation != null && !component.Validation.IsMatch(value))
                continue;

            component.Config[key] = value;
        }

        UpdateUi(uid, component);

        var updatedEvent = new ConfigurationUpdatedEvent(component);
        RaiseLocalEvent(uid, updatedEvent, false);

        // TODO support float (spinbox) and enum (drop-down) configurations
        // TODO support verbs.
    }

    private void OnInsert(EntityUid uid, ConfigurationComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!_toolSystem.HasQuality(args.EntityUid, component.QualityNeeded))
            return;

        args.Cancel();
    }

    /// <summary>
    /// Sent when configuration values got changes
    /// </summary>
    public sealed class ConfigurationUpdatedEvent : EntityEventArgs
    {
        public ConfigurationComponent Configuration;

        public ConfigurationUpdatedEvent(ConfigurationComponent configuration)
        {
            Configuration = configuration;
        }
    }
}