using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Shared.Physics;

public sealed class SharedPreventCollideSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventCollideComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(EntityUid uid, PreventCollideComponent component, ref PreventCollideEvent args)
    {
        if (component.Uid == args.OtherEntity)
            args.Cancelled = true;

        if (component.Whitelist != null && _whitelist.IsValid(component.Whitelist, args.OtherEntity)) // Goobstation
            args.Cancelled = true;
    }
}
