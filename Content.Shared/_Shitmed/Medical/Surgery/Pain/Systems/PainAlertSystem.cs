// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

/// <summary>
/// System that handles showing a pain level alert to the player
/// </summary>
public sealed class PainAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private static readonly string[] PainAlerts = { "Pain0", "Pain1", "Pain2", "Pain3" };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NerveComponent, ComponentInit>(OnNerveSystemMapInit);
        SubscribeLocalEvent<NerveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<NerveComponent, DamageExamineEvent>(OnExamine);

    }

    private void OnNerveSystemMapInit(EntityUid uid, NerveComponent component, ComponentInit args)
    {
        // Clear all pain alerts when the component initializes
        foreach (var alertId in PainAlerts)
        {
            if (_prototypeManager.TryIndex<AlertPrototype>(alertId, out var alert))
                _alerts.ClearAlert(uid, alert);
        }
    }

    private void OnDamageChanged(EntityUid uid, NerveComponent nerve, ref DamageChangedEvent args)
    {
        // Update on both damage and healing
        if (args.DamageDelta != null)  // This will be non-null for both damage and healing
            UpdatePainAlert(uid, nerve);
    }

    private void OnExamine(EntityUid uid, NerveComponent nerve, ref DamageExamineEvent args)
    {
        // Update pain alert when examining damage
        UpdatePainAlert(uid, nerve);
    }

private void UpdatePainAlert(EntityUid uid, NerveComponent? nerve = null)
{
    if (!Resolve(uid, ref nerve, false) || !TryComp<WoundableComponent>(uid, out var woundable))
        return;

    // Find the parent mob that should have the AlertsComponent
    EntityUid? mobUid = null;
    if (TryComp<BodyPartComponent>(uid, out var bodyPart) && bodyPart.Body is { } bodyUid)
        mobUid = bodyUid;
    else
        mobUid = uid;

    if (mobUid == null || !HasComp<AlertsComponent>(mobUid.Value))
        return;

    // Check if the mob is in a critical state
    bool isCritical = false;
    if (TryComp<MobStateComponent>(mobUid, out var mobState))
        isCritical = _mobState.IsCritical(mobUid.Value, mobState);

    float totalPain = 0f;

    if (nerve.PainFeels > 0 && woundable.WoundableIntegrity < woundable.IntegrityCap)
    {
        var normalizedIntegrity = woundable.WoundableIntegrity / woundable.IntegrityCap;
        var painLevel = (FixedPoint2.New(1) - normalizedIntegrity) * 100 * nerve.PainFeels;
        totalPain = (float)FixedPoint2.Clamp(painLevel, 0, 100);
    }

    // Always show alert if in critical state, otherwise follow normal pain rules
    if (isCritical || totalPain > 1f)
    {
        var alertIndex = isCritical
            ? 3  // Max severity in critical state
            : (int)Math.Clamp(Math.Floor((totalPain - 1f) / 25f), 0, 3);

        // Clear all other pain alerts first
        foreach (var alertId in PainAlerts)
        {
            if (_prototypeManager.TryIndex<AlertPrototype>(alertId, out var alert))
                _alerts.ClearAlert(mobUid.Value, alert);
        }

        // Show the appropriate pain alert
        if (_prototypeManager.TryIndex<AlertPrototype>(PainAlerts[alertIndex], out _))
            _alerts.ShowAlert(mobUid.Value, PainAlerts[alertIndex]);
    }
    else
    {
        // Clear all pain alerts
        foreach (var alertId in PainAlerts)
        {
            if (_prototypeManager.TryIndex<AlertPrototype>(alertId, out var alert))
                _alerts.ClearAlert(mobUid.Value, alert);
        }
    }
}
}
