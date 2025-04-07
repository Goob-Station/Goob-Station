// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Client.Graphics;

namespace Content.Client._EinsteinEngines.Flight.Components;

[RegisterComponent]
public sealed partial class FlightVisualsComponent : Component
{
    /// <summary>
    ///     How long does the animation last
    /// </summary>
    [DataField]
    public float Speed;

    /// <summary>
    ///     How far it goes in any direction.
    /// </summary>
    [DataField]
    public float Multiplier;

    /// <summary>
    ///     How much the limbs (if there are any) rotate.
    /// </summary>
    [DataField]
    public float Offset;

    /// <summary>
    ///     Are we animating layers or the entire sprite?
    /// </summary>
    public bool AnimateLayer = false;
    public int? TargetLayer;

    [DataField]
    public string AnimationKey = "default";

    [ViewVariables(VVAccess.ReadWrite)]
    public ShaderInstance Shader = default!;


}