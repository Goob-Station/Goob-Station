// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

/// <summary>
/// System that handles showing a pain level alert to the player
/// </summary>
public sealed class PainAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const string PainAlert = "Pain";
    private const float UpdateRate = 10f; // Update every 10 seconds
    private float _accumulatedTime;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NerveComponent, ComponentInit>(OnNerveSystemMapInit);
    }

    private void OnNerveSystemMapInit(EntityUid uid, NerveComponent component, ComponentInit args)
    {
        // Ensure the alert is cleared when the component initializes
        _alerts.ClearAlert(uid, _prototypeManager.Index<AlertPrototype>(PainAlert));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        _accumulatedTime += frameTime;
        if (_accumulatedTime < UpdateRate)
            return;

        _accumulatedTime -= UpdateRate;

        var query = EntityQueryEnumerator<NerveComponent>();
        while (query.MoveNext(out var uid, out var nerve))
        {
            UpdatePainAlert(uid, nerve);
        }
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

        // Calculate severity (0-5) based on pain level
        var severity = (short)Math.Floor(totalPain / 20f);
        severity = Math.Clamp(severity, (short)0, (short)5);

        // Show or update the alert
        if (totalPain > 0)
        {
            _alerts.ShowAlert(uid, PainAlert, severity: severity);
        }
        else
        {
            _alerts.ClearAlert(uid, _prototypeManager.Index<AlertPrototype>(PainAlert));
        }
    }
}
