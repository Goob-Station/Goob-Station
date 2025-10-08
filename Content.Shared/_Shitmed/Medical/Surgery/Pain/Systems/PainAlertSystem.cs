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
using System.Linq;

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
        {
            Logger.Debug($"[PainAlert] Missing required components for entity {uid}");
            return;
        }

        // Log the current state of the components
        if (nerve == null || woundable == null)
        {
            Logger.Error($"[PainAlert] NerveComponent or WoundableComponent is null for entity {uid}");
            return;
        }

        Logger.Debug($"[PainAlert] Updating pain alert for entity {uid}");
        Logger.Debug($"[PainAlert] NerveComponent found: {nerve != null}, WoundableComponent found: {woundable != null}");

        // Find the parent mob that should have the AlertsComponent
        EntityUid? mobUid = null;
        if (TryComp<BodyPartComponent>(uid, out var bodyPart) && bodyPart.Body is { } bodyUid)
        {
            // This is a body part, get the parent mob
            mobUid = bodyUid;
            Logger.Debug($"[PainAlert] Found parent mob {mobUid} for body part {uid}");
        }
        else
        {
            // This might already be the mob
            mobUid = uid;
        }

        // Ensure we have a valid mob UID and it has AlertsComponent
        if (mobUid == null || !HasComp<AlertsComponent>(mobUid.Value))
        {
            Logger.Error($"[PainAlert] Could not find parent mob with AlertsComponent for entity {uid}");
            return;
        }

        // Calculate total pain based on woundable integrity and pain feels
        float totalPain = 0f;

        // Log initial values
        if (woundable == null)
        {
            Logger.Error($"[PainAlert] WoundableComponent is null for entity {uid}");
            return;
        }

        // Get pain from woundable integrity and pain feels
        if (nerve == null)
        {
            Logger.Error($"[PainAlert] NerveComponent is null for entity {uid}");
            return;
        }

        if (nerve.PainFeels > 0 && woundable.WoundableIntegrity < woundable.IntegrityCap)
        {
            // Calculate pain based on woundable integrity (lower integrity = more pain)
            // Normalize the integrity to a 0-1 range based on IntegrityCap
            var normalizedIntegrity = woundable.WoundableIntegrity / woundable.IntegrityCap;
            var painLevel = (FixedPoint2.New(1) - normalizedIntegrity) * 100 * nerve.PainFeels;
            totalPain = (float)FixedPoint2.Clamp(painLevel, 0, 100);
            Logger.Debug($"[PainAlert] Calculated pain: {totalPain} (from painLevel: {painLevel}, integrity: {woundable.WoundableIntegrity}/{woundable.IntegrityCap})");
        }
        else
        {
            Logger.Debug($"[PainAlert] No pain calculated - PainFeels: {nerve.PainFeels}, WoundableIntegrity: {woundable.WoundableIntegrity}/{woundable.IntegrityCap}");
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

            Logger.Debug($"[PainAlert] Showing alert with severity {severity} for pain {totalPain} on mob {mobUid}");
            
            // Verify the alert prototype exists
            if (!_prototypeManager.HasIndex<AlertPrototype>(PainAlert))
            {
                Logger.Error($"[PainAlert] Alert prototype {PainAlert} not found in prototype manager!");
                return;
            }
            
            // Try to get the alert prototype
            if (!_prototypeManager.TryIndex<AlertPrototype>(PainAlert, out var alertProto))
            {
                Logger.Error($"[PainAlert] Failed to get alert prototype {PainAlert}");
                return;
            }
            
            Logger.Debug($"[PainAlert] Found alert prototype: {alertProto.ID}, ClientHandled: {alertProto.ClientHandled}");
            
            // Show the alert
            _alerts.ShowAlert(mobUid.Value, PainAlert, severity: severity);
            Logger.Debug($"[PainAlert] Alert shown for {mobUid}");
        }
        else
        {
            Logger.Debug($"[PainAlert] Clearing alert - pain level {totalPain} is <= 1");
            _alerts.ClearAlert(mobUid.Value, _prototypeManager.Index<AlertPrototype>(PainAlert));
        }
    }
}
