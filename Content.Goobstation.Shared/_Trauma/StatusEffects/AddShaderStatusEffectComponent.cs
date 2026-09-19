// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared._Trauma.StatusEffects;

[RegisterComponent]
public sealed partial class AddShaderStatusEffectComponent : Component
{
    [DataField(required: true)]
    public string Shader;
}
