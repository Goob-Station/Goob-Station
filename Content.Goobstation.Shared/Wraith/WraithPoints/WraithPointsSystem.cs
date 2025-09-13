using Content.Shared.Actions.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using MvcContrib;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

/// <summary>
/// This handles the Wraith Points.
/// </summary>
public sealed class WraithPointsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithPointsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PassiveWraithPointsComponent, MapInitEvent>(PassiveOnMapInit);

        SubscribeLocalEvent<ActionWraithPointsComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<PassiveWraithPointsComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.WpGenerationAccumulator)
                continue;

            AdjustWraithPoints(comp.BaseWpGeneration, uid);
            comp.WpGenerationAccumulator = _timing.CurTime + comp.NextWpUpdate;

            DirtyField<PassiveWraithPointsComponent>(
                (uid, comp),
                nameof(PassiveWraithPointsComponent.WpGenerationAccumulator));
        }
    }

    #region Events

    private void OnMapInit(Entity<WraithPointsComponent> ent, ref MapInitEvent args) =>
        ChangeWraithPoints(ent.Comp.StartingWraithPoints, ent.AsNullable());

    private void PassiveOnMapInit(Entity<PassiveWraithPointsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.WpGenerationAccumulator = ent.Comp.NextWpUpdate + _timing.CurTime;
        ent.Comp.BaseWpResetter = ent.Comp.BaseWpGeneration;
        Dirty(ent);
    }


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

        ent.Comp.WraithPoints = Math.Clamp(wraithPoints, 0, int.MaxValue);
        Dirty(ent);
    }

    /// <summary>
    /// Increments/Decrements wraith points
    /// </summary>
    /// <param name="wraithPoints"></param> The wraith points to add to the existing ones.
    /// <param name="ent"></param> The entity
    public void AdjustWraithPoints(int wraithPoints, Entity<WraithPointsComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.WraithPoints = Math.Clamp(ent.Comp.WraithPoints + wraithPoints, 0, int.MaxValue);
        Dirty(ent);

        // debug
        //_popupSystem.PopupPredicted(
        //    Loc.GetString("wraith-points-added", ("wp", wraithPoints)),
        //    ent.Owner,
        //    ent.Owner);
    }

    /// <summary>
    /// Increments/Decrements the generation rate of Wraith Points
    /// </summary>
    /// <param name="rate"></param> The rate to add to the existing rate
    /// <param name="ent"></param>
    public void AdjustWpGenerationRate(int rate, Entity<PassiveWraithPointsComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.WpGeneration += rate;
        Dirty(ent);

        AdjustWpGeneration((ent.Owner, ent.Comp));
    }

    /// <summary>
    /// Resets everything related to Wraith Points.
    /// </summary>
    /// <param name="ent"></param> The entity
    /// <param name="passiveWp"></param> For resetting the passive WP generation
    public void ResetEverything(Entity<WraithPointsComponent?> ent, PassiveWraithPointsComponent? passiveWp = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.WraithPoints = 0;

        if (passiveWp != null)
        {
            passiveWp.WpGeneration = 1;
            passiveWp.BaseWpGeneration = passiveWp.BaseWpResetter;
        }
    }
    #endregion

    private void AdjustWpGeneration(Entity<PassiveWraithPointsComponent> ent)
    {
        ent.Comp.BaseWpGeneration *= ent.Comp.WpGeneration;
        Dirty(ent);
    }
}
