using Content.Shared.Mindshield.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowMindShieldIconsSystem : EquipmentHudSystem<ShowMindShieldIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindShieldComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, MindShieldComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        var statusIcon = component.MindShieldStatusIcon; // Goobstation - check if mindshield is broken

        if (component.Broken)
            statusIcon = component.MindShieldBrokenStatusIcon; // Goobstation - check if mindshield is broken

        if (_prototype.TryIndex(statusIcon, out var iconPrototype)) // Goobstation
            ev.StatusIcons.Add(iconPrototype);
    }
}
