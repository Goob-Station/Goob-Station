using Content.Goobstation.Shared.Wraith.Curses;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wraith.Curses;

public sealed class CurseHolderSystem : SharedCurseHolderSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseHolderComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<CurseHolderComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ent.Comp.CurseStatusIcons.Count == 0)
            return;

        foreach (var curseIcon in ent.Comp.CurseStatusIcons)
        {
            var icon = _proto.Index(curseIcon);
            args.StatusIcons.Add(icon);
        }
    }
}
