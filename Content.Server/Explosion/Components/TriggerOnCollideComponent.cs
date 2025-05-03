// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Explosion.Components;

/// <summary>
///     Triggers when colliding with another entity.
/// </summary>
[RegisterComponent]
public sealed partial class TriggerOnCollideComponent : Component
{
    /// <summary>
    ///     The fixture with which to collide.
    /// </summary>
    [DataField(required: true)]
    public string FixtureID = string.Empty;

    /// <summary>
    ///     Doesn't trigger if the other colliding fixture is nonhard.
    /// </summary>
    [DataField]
    public bool IgnoreOtherNonHard = true;
}
