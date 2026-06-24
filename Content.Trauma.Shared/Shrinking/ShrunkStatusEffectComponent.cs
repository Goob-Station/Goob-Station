// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Shrinking;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShrunkStatusEffectComponent : Component
{
    [DataField]
    public Vector2 OriginalSize;
};
