using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared._Goobstation.Traits.Assorted;

public sealed class LegsStartParalyzedSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegsStartParalyzedComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, LegsStartParalyzedComponent component, MapInitEvent args)
    {
        if (!_entMan.TryGetComponent<BodyComponent>(uid, out var body))
            return;

        foreach (var legEntity in body.LegEntities)
        {
            if (TryComp(legEntity, out BodyPartComponent? part))
            {
                part.CanEnable = false;
                Dirty(legEntity, part);
            }

            var ev = new BodyPartEnableChangedEvent(false);
            RaiseLocalEvent(legEntity, ref ev);
        }
    }
}
