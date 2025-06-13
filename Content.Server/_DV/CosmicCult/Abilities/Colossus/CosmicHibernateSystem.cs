using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicHibernateSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusHibernate>(OnColossusHibernate);
    }

    private void OnColossusHibernate(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusHibernate args)
    {
        if (ent.Comp.Attacking || ent.Comp.Hibernating || !_transform.AnchorEntity(ent))
            return;
        args.Handled = true;
        var comp = ent.Comp;

        comp.Hibernating = true;
        comp.HibernationTimer = comp.HibernationWait + _timing.CurTime;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Action);
        _appearance.SetData(ent, ColossusVisuals.Hibernation, ColossusAction.Running);
        _stun.TryStun(ent, comp.HibernationWait, true);
        _popup.PopupCoordinates(
            Loc.GetString("ghost-role-colossus-hibernate"),
            Transform(ent).Coordinates,
            PopupType.LargeCaution);
    }
}
