using Content.Shared.Mindcontrol;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Mindcontrol;

public sealed class MindcontrolSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindcontrolComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }
    private void OnGetStatusIconsEvent(Entity<MindcontrolComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.MindcontrolIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
