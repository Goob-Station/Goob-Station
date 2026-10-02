using Content.Shared.Stealth.Components;

namespace Content.Shared.Stealth;

public abstract partial class SharedStealthSystem
{
    public void SetRevealOnDamage(EntityUid uid, bool value, StealthComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        comp.RevealOnDamage = value;
    }
}
