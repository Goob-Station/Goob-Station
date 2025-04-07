// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Shared._EinsteinEngines.Silicon.EmitBuzzWhileDamaged;

/// <summary>
/// This is used for controlling the cadence of the buzzing emitted by EmitBuzzOnCritSystem.
/// This component is used by mechanical species that can get to critical health.
/// </summary>
[RegisterComponent]
public sealed partial class EmitBuzzWhileDamagedComponent : Component
{
    [DataField("buzzPopupCooldown")]
    public TimeSpan BuzzPopupCooldown { get; private set; } = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastBuzzPopupTime;

    [DataField("cycleDelay")]
    public float CycleDelay = 2.0f;

    public float AccumulatedFrametime;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("buzzes");
}