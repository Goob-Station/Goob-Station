// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

/// <summary>
/// System that handles showing a pain level alert to the player
/// </summary>
public sealed class PainAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const string PainAlert = "Pain";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NerveComponent, ComponentInit>(OnNerveSystemMapInit);
        SubscribeLocalEvent<NerveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<NerveComponent, DamageExamineEvent>(OnExamine);
    }

    private void OnNerveSystemMapInit(EntityUid uid, NerveComponent component, ComponentInit args)
    {
        // Ensure the alert is cleared when the component initializes
        _alerts.ClearAlert(uid, _prototypeManager.Index<AlertPrototype>(PainAlert));
    }

    private void OnDamageChanged(EntityUid uid, NerveComponent nerve, ref DamageChangedEvent args)
    {
        // Only update if pain damage changed
        if (args.DamageDelta?.GetTotal() > 0 || args.DamageIncreased)
        {
            UpdatePainAlert(uid, nerve);
        }
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

        // Calculate total pain based on woundable integrity and pain feels
        float totalPain = 0f;

        // Get pain from woundable integrity and pain feels
        if (nerve.PainFeels > 0 && woundable.WoundableIntegrity < 1.0f)
        {
            // Calculate pain based on woundable integrity (lower integrity = more pain)
            var painLevel = (FixedPoint2.New(1) - woundable.WoundableIntegrity) * 100 * nerve.PainFeels;
            totalPain = (float)FixedPoint2.Clamp(painLevel, 0, 100);
        }

        // Show or update the alert only if pain > 1
        if (totalPain > 1f)
        {
            // Calculate severity (0-3) based on pain level
            // 0: >1-25% pain
            // 1: >25-50% pain
            // 2: >50-75% pain
            // 3: >75-100% pain
            var severity = (short)Math.Floor((totalPain - 1f) / 25f);
            severity = Math.Clamp(severity, (short)0, (short)3);
            _alerts.ShowAlert(uid, PainAlert, severity: severity);
        }
        else
        {
            _alerts.ClearAlert(uid, _prototypeManager.Index<AlertPrototype>(PainAlert));
        }
    }
}
