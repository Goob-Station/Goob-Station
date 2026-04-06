using Content.Goobstation.Shared.Leash;
using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Terror;

/// <summary>
/// Handles the terror-specific consequences of breaking a proximity leash:
/// shows a warning popup on each tick and gibs the entity when the leash breaks.
/// </summary>
public sealed class TerrorProximitySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PurpleTerrorComponent, ProximityLeashTickEvent>(OnTick);
        SubscribeLocalEvent<PurpleTerrorComponent, ProximityLeashBreakEvent>(OnBreak);
    }

    private void OnTick(EntityUid uid, PurpleTerrorComponent _, ref ProximityLeashTickEvent ev)
    {
        _popup.PopupEntity(Loc.GetString("terror-far-from-queen"), uid, uid, PopupType.MediumCaution);
    }

    private void OnBreak(EntityUid uid, PurpleTerrorComponent _, ref ProximityLeashBreakEvent ev)
    {
        _body.GibBody(uid);
    }
}
