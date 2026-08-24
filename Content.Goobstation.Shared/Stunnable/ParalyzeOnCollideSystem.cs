// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Stunnable;

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
        if (component.CollidableEntities != null &&
            _whitelistSystem.IsValid(component.CollidableEntities, args.OtherEntity))
            return;

        if (component.ParalyzeOther && args.OtherEntity != null)
            _stunSystem.TryUpdateParalyzeDuration(args.OtherEntity, component.ParalyzeTime);
        if (component.ParalyzeSelf && uid != null)
            _stunSystem.TryUpdateParalyzeDuration(uid, component.ParalyzeTime);

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
