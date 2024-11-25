using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Components;

namespace Content.Shared.Traits.Assorted;

public sealed class LegsStartParalyzedSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegsStartParalyzedComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LegsStartParalyzedComponent component, ComponentStartup args)
    {
        if (_entMan.TryGetComponent<BodyComponent>(uid, out var body))
        {
            foreach (var legEntity in body.LegEntities)
            {
                var ev = new BodyPartEnableChangedEvent(false);
                RaiseLocalEvent(legEntity, ref ev);
            }
        }
    }
}
