namespace Content.Shared._Goobstation.MisandryBox;

[Flags]
public enum BSOEnforcementPunishmentEnum
{
    Lifeline = 1, // Remove from round
    AccessStrip = 2, // Strip all accesses from user.
    Pacifist = 4, // Apply pacifist
    Clumsy = 8, // Apply clumsy
    DisarmProne = 16, // Apply DisarmProne
    Monkey = 32, // Poly into monkey
    RoleBan = 64, // RoleBan

    Debuff = AccessStrip | Pacifist | Clumsy | DisarmProne,
    Minimod = Lifeline | RoleBan,
}
