using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Xenobiology;

public sealed partial class SlimeLatchEvent : EntityTargetActionEvent
{
    [DataField]
    public float Damage = 5;
}
