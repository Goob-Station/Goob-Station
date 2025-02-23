using Content.Server.NPC.Components;
using Content.Shared._Goobstation.Weapons.SmartGun;
using Content.Shared.Wieldable.Components;
using Robust.Server.GameStates;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Weapons.Ranged;

public sealed class LaserPointerSystem : SharedLaserPointerSystem
{
    [Dependency] private readonly PvsOverrideSystem _override = default!;

    protected override void PvsOverride(EntityUid entity)
    {
        base.PvsOverride(entity);

        _override.AddGlobalOverride(entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var actorQuery = GetEntityQuery<ActorComponent>();
        var npcCombatQuery = GetEntityQuery<NPCRangedCombatComponent>();
        var query = EntityQueryEnumerator<LaserPointerComponent, WieldableComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var pointer, out var wieldable, out var xform))
        {
            if (!wieldable.Wielded)
                continue;

            if (npcCombatQuery.HasComp(xform.ParentUid) || actorQuery.HasComp(xform.ParentUid))
                continue;

            AddOrRemoveLine(GetNetEntity(uid), pointer, wieldable, xform, null, null);
        }
    }
}
