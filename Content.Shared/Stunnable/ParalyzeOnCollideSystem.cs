using Content.Shared.Throwing;
using Robust.Shared.Physics.Events;
using Content.Shared.Whitelist;

namespace Content.Shared.Stunnable;

public sealed class ParalyzeOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ParalyzeOnCollideComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ParalyzeOnCollideComponent, LandEvent>(OnLand);
    }

    private void OnStartCollide(EntityUid uid, ParalyzeOnCollideComponent component, ref StartCollideEvent args)
    {
        if (component.CollidableEntities != null && _whitelistSystem.IsValid(component.CollidableEntities, args.OtherEntity));

        if (component.ParalyzeOther)
            _stunSystem.TryParalyze(args.OtherEntity, component.ParalyzeTime, true);
        if (component.ParalyzeSelf)
            _stunSystem.TryParalyze(uid, component.ParalyzeTime, true);

        if (component.RemoveAfterCollide)
        {
            RemComp(uid, component);
        }
    }

    private void OnLand(EntityUid uid, ParalyzeOnCollideComponent component, ref LandEvent args)
    {
        if (component.RemoveOnLand)
        {
            RemComp(uid, component);
        }
    }
}
