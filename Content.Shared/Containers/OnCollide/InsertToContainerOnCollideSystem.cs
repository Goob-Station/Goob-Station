using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Content.Shared.Whitelist;
namespace Content.Shared.Containers.OnCollide;

public sealed class InsertToContainerOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InsertToContainerOnCollideComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(EntityUid uid, InsertToContainerOnCollideComponent component, ref StartCollideEvent args)
    {
        var currentVelocity = args.OurBody.LinearVelocity.Length();
        if (currentVelocity < component.RequiredVelocity)
            return;

        if (!_containerSystem.TryGetContainer(uid, component.Container, out var container))
            return;

        if (component.BlacklistedEntities != null && _whitelistSystem.IsValid(component.BlacklistedEntities, args.OtherEntity))
            return;

        if (component.InsertableEntities != null && !_whitelistSystem.IsValid(component.InsertableEntities, args.OtherEntity))
            return;

        //container.Insert(args.OtherEntity, EntityManager, physics: args.OtherBody);
        _containerSystem.Insert(args.OtherEntity, container);
    }
}
