using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Server.GameObjects;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Null Charge ability.
/// Null Charge is an ability that disables an APC until it gets fixed.
/// </summary>
public sealed class ShadowlingNullChargeSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingNullChargeComponent, NullChargeEvent>(OnNullCharge);
        SubscribeLocalEvent<ShadowlingNullChargeComponent, NullChargeDoAfterEvent>(OnNullChargeAfter);
    }

    private void OnNullCharge(EntityUid uid, ShadowlingNullChargeComponent component, NullChargeEvent args)
    {
        if (args.Handled)
            return;

        if (!IsApcInRange(uid, component.Range))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            component.NullChargeToComplete,
            new NullChargeDoAfterEvent(),
            uid);

        _popupSystem.PopupEntity(Loc.GetString("shadowling-null-charge-start"), uid, uid, PopupType.MediumCaution);
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnNullChargeAfter(EntityUid uid, ShadowlingNullChargeComponent component, NullChargeDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        bool apcAffected = false;
        foreach (var apc in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (apcAffected)
                break;

            if (!TryComp<ApcComponent>(apc, out var apcComponent))
                continue;
            if (!TryComp<PowerNetworkBatteryComponent>(apc, out var battery))
                continue;

            if (apcComponent.MainBreakerEnabled)
            {
                apcComponent.MainBreakerEnabled = false;
                battery.CanDischarge = false;
                apcAffected = true;
            }
        }

        if (apcAffected)
            _popupSystem.PopupEntity(Loc.GetString("shadowling-null-charge-success"), uid, uid, PopupType.Medium);

        var effectEnt = Spawn(component.NullChargeEffect, _transformSystem.GetMapCoordinates(uid));
        _transformSystem.SetParent(effectEnt, uid);
    }

    private bool IsApcInRange(EntityUid uid, float range)
    {
        foreach (var target in _lookup.GetEntitiesInRange(uid, range))
        {
            if (HasComp<ApcComponent>(target))
                return true;
        }
        return false;
    }

}
