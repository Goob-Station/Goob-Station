// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Server.Polymorph.Components;

[RegisterComponent]
[Access(typeof(PolymorphSystem))]
public sealed partial class PolymorphedEntityComponent : Component
{
    /// <summary>
    /// The polymorph prototype, used to track various information
    /// about the polymorph
    /// </summary>
    [DataField(required: true)]
    public PolymorphConfiguration Configuration = new();

    /// <summary>
    /// The original entity that the player will revert back into
    /// </summary>
    [DataField(required: true)]
    public EntityUid Parent;

    /// <summary>
    /// The amount of time that has passed since the entity was created
    /// used for tracking the duration
    /// </summary>
    [DataField]
    public float Time;

    [DataField]
    public EntityUid? Action;
}