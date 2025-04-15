// SPDX-FileCopyrightText: 2023 Phill101 <28949487+Phill101@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Phill101 <holypics4@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CrewManifest;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class CrewManifestUiState : BoundUserInterfaceState
{
    public string StationName;
    public CrewManifestEntries? Entries;

    public CrewManifestUiState(string stationName, CrewManifestEntries? entries)
    {
        StationName = stationName;
        Entries = entries;
    }
}