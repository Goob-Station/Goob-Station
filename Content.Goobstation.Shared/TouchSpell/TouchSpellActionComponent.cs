using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.TouchSpell;

/// <summary>
///     Indicates that it should spawn a touch spell when used.
/// </summary>
[RegisterComponent]
public sealed partial class TouchSpellActionComponent : Component
{
    [DataField(required: true)] public EntProtoId TouchSpell;
    [DataField] public EntityTargetActionEvent? Event;
}
