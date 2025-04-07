// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Worldgen.Systems;

namespace Content.Server.Worldgen.Components;

/// <summary>
///     This is used for sending a signal to the entity it's on to load contents whenever a loader gets close enough.
///     Does not support unloading.
/// </summary>
[RegisterComponent]
[Access(typeof(LocalityLoaderSystem))]
public sealed partial class LocalityLoaderComponent : Component
{
    /// <summary>
    ///     The maximum distance an entity can be from the loader for it to not load.
    ///     Once a loader is closer than this, the event is fired and this component removed.
    /// </summary>
    [DataField("loadingDistance")] public int LoadingDistance = 32;
}
