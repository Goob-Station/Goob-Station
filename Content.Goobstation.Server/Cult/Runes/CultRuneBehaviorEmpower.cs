using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Goobstation.Shared.UserInterface;
using Robust.Server.GameObjects;
using System.Linq;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class CultRuneBehaviorEmpower : CultRuneBehavior
{
    private UserInterfaceSystem _ui = default!;

    public override void Initialize(IEntityManager ent)
    {
        base.Initialize(ent);

        _ui = ent.System<UserInterfaceSystem>();
    }

    public override bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!base.IsValid(ent, invokers, targets, out invalidReason))
            return false;

        return true;
    }

    public override void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null)
    {
        var invoker = invokers.First();

        if (!owner.HasValue
        || !ent.TryGetComponent<BloodMagicProviderComponent>(invoker, out var magic))
            return;

        _ui.TryOpenUi(owner.Value, EntityRadialMenuKey.Key, invoker);
    }
}
