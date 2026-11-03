using Content.Goobstation.Shared.CustomFactionIcons;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CustomFactionIcons;

public sealed partial class CustomFactionIconsSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomFactionIconsComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(Entity<CustomFactionIconsComponent> ent, ref GetStatusIconsEvent args)
    {
        foreach (var icon in ent.Comp.FactionIcons)
        {
            if (_prototype.TryIndex(icon, out var iconProto))
                args.StatusIcons.Add(iconProto);
        }
    }
}
