using Robust.Shared.Audio;

namespace Content.Shared.Damage.Components;

[RegisterComponent]
public sealed partial class StaminaDamageOnHitComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("damage")]
    public float Damage = 20f; // goob edit

    // goob edit
    [DataField]
    public float Overtime = 0f;

    [DataField("sound")]
    public SoundSpecifier? Sound;
}
