using Content.Goobstation.Shared.Supermatter.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Administration.Logs;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Database;
using Content.Shared.Magic;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard;

public sealed partial class SummonSupermatterSpellSystem : EntitySystem
{
    [Dependency] private ISharedAdminLogManager _adminLog = default!;
    [Dependency] private SharedMagicSystem _magic = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedTransformSystem _transform = default!;


    [SubscribeLocalEvent]
    private void OnSummonSupermatter(SummonSupermatterEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var mapCoords = _transform.GetMapCoordinates(ev.Performer);
        if (!_map.TryFindGridAt(mapCoords, out var gridUid, out var grid))
        {
            _popup.PopupClient(Loc.GetString("spell-fail-no-grid"), ev.Performer, ev.Performer, PopupType.Medium);
            return;
        }

        var coords = _map.MapToGrid(gridUid, mapCoords).SnapToGrid(grid);

        EnsureComp<SupermatterImmuneComponent>(ev.Performer);
        GrantRadiationResistance(ev.Performer, ev.RadiationModifier);

        var sm = PredictedSpawnAtPosition(ev.Proto, coords);

        if (ev.Activated && TryComp<SupermatterComponent>(sm, out var supermatter))
            supermatter.Activated = true;

        _adminLog.Add(LogType.Supermatter,
            LogImpact.Extreme,
            $"{ToPrettyString(ev.Performer):actor} summoned an active Supermatter {ToPrettyString(sm):subject} at {coords}");

        ev.Handled = true;
    }

    private void GrantRadiationResistance(EntityUid uid, ProtoId<DamageModifierSetPrototype> modifierId)
    {
        if (!_proto.Resolve(modifierId, out var modifier))
            return;

        var buff = EnsureComp<DamageProtectionBuffComponent>(uid);
        buff.Modifiers.TryAdd(modifierId, modifier);
    }
}
