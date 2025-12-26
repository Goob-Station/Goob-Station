namespace Content.Goobstation.Common.Damage;

/// <summary>
/// Raised on an entity before passive damage is dealt.
/// </summary>
[ByRefEvent]
public record struct BeforePassiveDamageEvent()
{
    public bool Cancelled = false;
    public float Multiplier = 1f;
}
