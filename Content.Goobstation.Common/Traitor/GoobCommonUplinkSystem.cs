namespace Content.Goobstation.Common.Traitor;

public abstract class GoobCommonUplinkSystem : EntitySystem
{
    public abstract EntityUid? FindPenUplinkTarget(EntityUid user);
    public abstract UplinkPreference GetUplinkPreference(EntityUid mindEnt);

    public abstract void SetupPenUplink(EntityUid pen);
    public abstract int[]? GetPenUplinkCode(EntityUid pen);
}

[ByRefEvent]
public record struct GeneratePenSpinCodeEvent
{
    public int[]? Code;
}
