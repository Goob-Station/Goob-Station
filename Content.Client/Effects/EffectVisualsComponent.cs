// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Client.Effects;

/// <summary>
/// Deletes the attached entity whenever any animation completes. Used for temporary client-side entities.
/// </summary>
[RegisterComponent]
public sealed partial class EffectVisualsComponent : Component {}