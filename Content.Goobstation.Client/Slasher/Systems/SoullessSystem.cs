using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Adds a faction icon for soulless entities.
/// </summary>
public sealed class SoullessSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoullessComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<SoullessComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.FactionIcon, out var iconProto))
            args.StatusIcons.Add(iconProto);
    }
}
