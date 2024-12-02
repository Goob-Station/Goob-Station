using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Changeling.EntitySystems;

public sealed partial class ChangelingAbilitiesSystem : SharedChangelingAbilitiesSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HivemindComponent, GetStatusIconsEvent>(GetChanglingIcon);
    }
    private void GetChanglingIcon(Entity<HivemindComponent> hivemind, ref GetStatusIconsEvent args)
    {
        if (_prototypeManager.TryIndex(hivemind.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
