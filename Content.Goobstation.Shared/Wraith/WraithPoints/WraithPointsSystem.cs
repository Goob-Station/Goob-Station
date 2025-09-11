using Content.Shared.Actions.Events;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

/// <summary>
/// This handles the Wraith Points.
/// </summary>
public sealed class WraithPointsSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithPointsComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<ActionWraithPointsComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    #region Events

    private void OnMapInit(Entity<WraithPointsComponent> ent, ref MapInitEvent args) =>
        ChangeWraithPoints(ent.Comp.StartingWraithPoints, ent.AsNullable());

    #endregion

    #region Action Handlers

    private void OnActionPerformed(Entity<ActionWraithPointsComponent> ent, ref ActionPerformedEvent args)
    {
        var performer = args.Performer;

        AdjustWraithPoints(ent.Comp.WpConsume, performer);
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Changes the Wraith Points of an entity to a set amount
    /// </summary>
    /// <param name="wraithPoints"></param> The wraith points to replace the existing ones with.
    /// <param name="ent"></param> The entity
    public void ChangeWraithPoints(int wraithPoints, Entity<WraithPointsComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.WraithPoints = wraithPoints;

        if (ent.Comp.WraithPoints < 0)
            ent.Comp.WraithPoints = 0;

        Dirty(ent);
    }

    /// <summary>
    /// Increments or decrements wraith points
    /// </summary>
    /// <param name="wraithPoints"></param> The wraith points to add to the existing ones.
    /// <param name="ent"></param> The entity
    public void AdjustWraithPoints(int wraithPoints, Entity<WraithPointsComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.WraithPoints += wraithPoints;

        if (ent.Comp.WraithPoints < 0)
            ent.Comp.WraithPoints = 0;

        Dirty(ent);
    }
    #endregion
}
