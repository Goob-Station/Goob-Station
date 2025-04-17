// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Sunrise.AssaultOps.Icarus;

[RegisterComponent]
public sealed partial class IcarusBeamComponent : Component
{
    /// <summary>
    ///     Beam moving speed.
    /// </summary>
    [DataField("speed")]
    public float Speed = 8f;

    /// <summary>
    ///     The beam will be automatically cleaned up after this time.
    /// </summary>
    [DataField("lifetime")]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(200);

    /// <summary>
    ///     With this set to true, beam will automatically set the tiles under them to space.
    /// </summary>
    [DataField("destroyTiles")]
    public bool DestroyTiles = true;

    [DataField("destroyRadius")]
    public float DestroyRadius = 4f;

    [DataField("flameRadius")]
    public float FlameRadius = 8f;

    public TimeSpan LifetimeEnd;
}
