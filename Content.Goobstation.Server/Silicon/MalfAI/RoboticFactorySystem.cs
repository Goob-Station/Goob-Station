using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Server.Physics.Controllers;
using Content.Server.Standing;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Power;
using Content.Shared.Standing;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed class RoboticFactorySystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoboticFactoryComponent, StartCollideEvent>(OnCollision);
        SubscribeLocalEvent<RoboticFactoryComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<RoboticFactoryComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<RoboticFactoryComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ConversionContainer = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerID);
    }

    public override void Update(float frameTime)
    {
        foreach (var comp in EntityQuery<RoboticFactoryComponent>())
        {
            if (!comp.FinishTime.HasValue || _timing.CurTime < comp.FinishTime)
                continue;

            FinishConverting(comp);

            comp.FinishTime = null;
        }
    }

    private void OnPowerChanged(Entity<RoboticFactoryComponent> ent, ref PowerChangedEvent args)
    {
        ent.Comp.Powered = args.Powered;
    }

    private void OnCollision(Entity<RoboticFactoryComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.Powered || !IsEntityValid(args.OtherEntity, ent.Comp))
            return;

        StartConvert(args.OtherEntity, ent.Comp);
    }

    public EntityUid ConvertToBorg(EntityUid entity, RoboticFactoryComponent comp)
    {
        var borgEntity = Spawn(comp.BorgProtoId, Transform(entity).Coordinates);

        QueueDel(entity);

        // If the entity doesn't have a mind (they've left the game for example)
        // then a borg is spawned anyway to open up a ghost role
        if (_mind.TryGetMind(entity, out var mindId, out var _))
            _mind.TransferTo(mindId, borgEntity);

        return borgEntity;
    }

    private void StartConvert(EntityUid entity, RoboticFactoryComponent comp)
    {
        if (comp.ConversionContainer.Count > 0)
            return;

        comp.FinishTime = _timing.CurTime + comp.Duration;

        _container.Insert(entity, comp.ConversionContainer);
    }

    private void FinishConverting(RoboticFactoryComponent comp)
    {
        if (comp.ConversionContainer.ContainedEntities.Count == 0)
            return;

        var entity = comp.ConversionContainer.ContainedEntities.FirstOrNull();

        if (!entity.HasValue)
            return;

        _container.Remove(entity.Value, comp.ConversionContainer);

        ConvertToBorg(entity.Value, comp);
    }

    private bool IsEntityValid(EntityUid uid, RoboticFactoryComponent comp)
    {
        if (!_entityWhitelist.CheckBoth(uid, comp.Blacklist, comp.Whitelist))
            return false;

        if (!TryComp<StandingStateComponent>(uid, out var standing) || standing.CurrentState != StandingState.Lying)
            return false;

        return TryComp<MobStateComponent>(uid, out var mobComp) && mobComp.CurrentState != MobState.Dead;
    }
}