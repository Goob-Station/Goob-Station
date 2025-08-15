using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Overhaul;

[RegisterComponent]
public sealed partial class OverhaulComponent : Component
{
    [DataField("disassembleDamage")]
    public float DisassembleDamage = 50f;  // Damage on disassemble

    [DataField("reassembleHeal")]
    public float ReassembleHeal = 30f;  // Healing on reassemble

    [DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(5);  // Cooldown time

    [DataField("mode")]  // Toggle mode
    public string Mode = "disassemble";
}
