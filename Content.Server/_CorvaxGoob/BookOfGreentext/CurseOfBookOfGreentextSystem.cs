using Content.Shared._CorvaxGoob.BookOfGreentext;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.BookOfGreentext;

public sealed class CurseOfBookOfGreentextSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<ContainerManagerComponent> _containerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _containerQuery = GetEntityQuery<ContainerManagerComponent>();

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<CurseOfBookOfGreentextComponent>();
        while (enumerator.MoveNext(out var uid, out var component))
        {
            if (component.NextUpdate < _timing.CurTime)
            {
                component.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(5);

                if (!_containerQuery.TryGetComponent(uid, out var currentManager))
                    return;

                var containerStack = new Stack<ContainerManagerComponent>();

                do
                {
                    foreach (var container in currentManager.Containers.Values)
                    {
                        foreach (var entity in container.ContainedEntities)
                        {
                            if (HasComp<BookOfGreentextComponent>(entity))
                            {
                                if (entity == component.Book)
                                {
                                    SetCurseStatus((uid, component), true);
                                    return;
                                }
                            }

                            // if it is a container check its contents
                            if (_containerQuery.TryGetComponent(entity, out var containerManager))
                                containerStack.Push(containerManager);
                        }
                    }
                } while (containerStack.TryPop(out currentManager));

                SetCurseStatus((uid, component), false);
            }
        }
    }

    private void SetCurseStatus(Entity<CurseOfBookOfGreentextComponent> entity, bool status)
    {
        if (entity.Comp.Completed == status)
            return;

        entity.Comp.Completed = status;
        Dirty(entity);
    }
}
