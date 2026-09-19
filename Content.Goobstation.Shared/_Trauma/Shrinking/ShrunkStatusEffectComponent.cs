// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using System.Numerics;
namespace Content.Goobstation.Shared._Trauma.Shrinking;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShrunkStatusEffectComponent : Component
{
    [DataField]
    public Vector2 OriginalSize;
};
