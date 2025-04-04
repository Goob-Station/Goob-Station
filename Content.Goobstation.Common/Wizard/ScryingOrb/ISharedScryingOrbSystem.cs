namespace Content.Goobstation.Common.Wizard.ScryingOrb;

public interface ISharedScryingOrbSystem : IEntitySystem
{
    bool IsScryingOrbEquipped(EntityUid uid);
}

