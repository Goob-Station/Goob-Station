namespace Content.Shared.Turrets;

// Goob-MalfAi: DeployableTurretComponent is access-restricted to this system,
// so the Malfunction AI turret upgrade goes through this helper.
public abstract partial class SharedDeployableTurretSystem
{
    /// <summary>
    /// Removes the deployed-state fragility: the turret takes damage as if retracted even
    /// while its cover is open. The new modifier set applies from the next deployment cycle.
    /// </summary>
    public void SetResilientWhenDeployed(Entity<DeployableTurretComponent> entity)
    {
        entity.Comp.DeployedDamageModifierSetId = entity.Comp.RetractedDamageModifierSetId;
        Dirty(entity);
    }
}
