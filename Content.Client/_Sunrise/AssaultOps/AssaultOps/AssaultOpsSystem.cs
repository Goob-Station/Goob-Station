using Content.Shared._Sunrise.AssaultOps;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunrise.AssaultOps.AssaultOps;

public sealed class AssaultOpsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AssaultOperativeComponent, GetStatusIconsEvent>(GetVampireIcon);
    }

    private void GetVampireIcon(EntityUid uid, AssaultOperativeComponent component, ref GetStatusIconsEvent args)
    {
        var iconPrototype = _prototype.Index(component.StatusIcon);
        args.StatusIcons.Add(iconPrototype);
    }
}
