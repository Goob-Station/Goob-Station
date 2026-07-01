// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Guidebook;
using Robust.Shared.Network;

namespace Content.Client.Guidebook;

/// <summary>
/// Client system for storing and retrieving values extracted from entity prototypes
/// for display in the guidebook (<see cref="RichText.ProtodataTag"/>).
/// Requests data from the server on <see cref="Initialize"/>.
/// Can also be pushed new data when the server reloads prototypes.
/// </summary>
public sealed class GuidebookDataSystem : EntitySystem
{
    [Dependency] private readonly IClientNetManager _net = default!;

    private GuidebookData? _data;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<UpdateGuidebookDataEvent>(OnServerUpdated);

        // Request data from the server
        // Pirate: Replay startup has no server connection to request guidebook data from.
        if (_net.IsConnected)
            RaiseNetworkEvent(new RequestGuidebookDataEvent());
    }

    private void OnServerUpdated(UpdateGuidebookDataEvent args)
    {
        // Got new data from the server, either in response to our request, or because prototypes reloaded on the server
        _data = args.Data;
        _data.Freeze();
    }

    /// <summary>
    /// Attempts to retrieve a value using the given identifiers.
    /// See <see cref="GuidebookData.TryGetValue"/> for more information.
    /// </summary>
    public bool TryGetValue(string prototype, string component, string field, out object? value)
    {
        if (_data == null)
        {
            value = null;
            return false;
        }
        return _data.TryGetValue(prototype, component, field, out value);
    }
}
