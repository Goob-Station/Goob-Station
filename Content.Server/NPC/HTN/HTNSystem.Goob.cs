using JetBrains.Annotations;

namespace Content.Server.NPC.HTN;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class HTNSystem
{
    // muh lacking of convenients overloads
    [PublicAPI]
    public void SetHTNEnabled(EntityUid uid, bool state, float planCooldown = 0f, HTNComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        SetHTNEnabled((uid, component), state, planCooldown);
    }
}
