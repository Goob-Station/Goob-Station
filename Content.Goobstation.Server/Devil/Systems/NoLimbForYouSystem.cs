using Content.Goobstation.Shared.Devil.Components;
using Content.Server.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Containers;
using Content.Shared.Body.Systems;
using Robust.Shared.Log;
using System.Linq;
namespace Content.Goobstation.Server.Devil.Systems;
public sealed class NoLimbForYouSystem : EntitySystem
{
    /// <summary>
    /// This system is used to remove any limb you signed away to the Devil. If you try to reattach a limb that you signed off, you will just lose it again, be it a hand or a leg.
    /// </summary>

    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Update(float frameTime)
    {
        var enumerator = EntityQueryEnumerator<NoLimbForYouComponent, BodyComponent>();
        while (enumerator.MoveNext(out var uid, out var comp, out var body))
        {
            comp.Accumulator += frameTime;
            if (comp.Accumulator < comp.CheckInterval) continue;
            comp.Accumulator = 0f;

            bool limbRemoved = false;

            foreach (var limb in _bodySystem.GetBodyChildren(uid, body))
            {
                // FIX: Use _container.TryGetContainingContainer
                if (!_container.TryGetContainingContainer(limb.Id, out var container))
                    continue;

                if (comp.ForbiddenSlots.Contains(container.ID))
                {
                    if (!TryComp<WoundableComponent>(limb.Id, out var woundable) || !woundable.ParentWoundable.HasValue)
                        continue;

                    _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, limb.Id, woundable);
                    QueueDel(limb.Id);

                    limbRemoved = true;
                }
            }

            if (limbRemoved)
                Dirty(uid, body);
        }
    }
}
