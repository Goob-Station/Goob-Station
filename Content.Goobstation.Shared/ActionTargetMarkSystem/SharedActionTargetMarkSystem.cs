using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ActionTargetMarkSystem;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedActionTargetMarkSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public abstract void SetMark(EntityUid user,
        EntityUid? target,
        ActionTargetMarkComponent? actionTargetMarkComponent = null);
}
