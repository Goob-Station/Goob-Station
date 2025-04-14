// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CCVar;

namespace Content.Server.CrewManifest;

[RegisterComponent]
public sealed partial class CrewManifestViewerComponent : Component
{
    /// <summary>
    ///     If this manifest viewer is unsecure or not. If it is,
    ///     <see cref="CCVars.CrewManifestUnsecure"/> being false will
    ///     not allow this entity to be processed by CrewManifestSystem.
    /// </summary>
    [DataField("unsecure")] public bool Unsecure;

    /// <summary>
    /// The owner interface of this crew manifest viewer. When it closes, so too will an opened crew manifest.
    /// </summary>
    [DataField(required: true)]
    public Enum OwnerKey { get; private set; } = default!;
}