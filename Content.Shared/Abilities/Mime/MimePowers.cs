using Content.Shared.Alert;

namespace Content.Shared.Abilities.Mime;

public sealed partial class BreakVowAlertEvent : BaseAlertEvent
{
    public float PunishChance; // Goobstation - Mime Enforcement

    public BreakVowAlertEvent(float chance = 0.25f)
    {
        PunishChance = chance;
    }
}

public sealed partial class RetakeVowAlertEvent : BaseAlertEvent;
