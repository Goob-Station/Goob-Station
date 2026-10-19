// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Timing;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles the Malf AI camera-upgrade toggle and keeps the Effective flag in sync with core status.
/// </summary>
public sealed class MalfAiCameraUpgradeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Toggle action (bind to the AI while it's in-core to avoid default-generic handler plumbing)
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiToggleCameraUpgradeActionEvent>(OnToggle);

        // Core presence changes (enable when entering core if desired; disable on leaving)
        SubscribeLocalEvent<StationAiHeldComponent, ComponentStartup>(OnHeldStartup);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentShutdown>(OnHeldShutdown);
    }

    private void OnToggle(EntityUid uid, StationAiHeldComponent held, ref MalfAiToggleCameraUpgradeActionEvent ev)
    {
        if (uid == EntityUid.Invalid)
            return;

        var comp = EnsureComp<MalfAiCameraUpgradeComponent>(uid);

        // Flip desired state
        comp.EnabledDesired = !comp.EnabledDesired;

        // Compute effective based on core status (we are in-core due to StationAiHeldComponent)
        comp.EnabledEffective = comp.EnabledDesired;

        Dirty(uid, comp);
    }

    private void OnHeldStartup(EntityUid uid, StationAiHeldComponent held, ref ComponentStartup args)
    {
        // AI has entered/exists in core: if desired, make effective true.
        if (!TryComp(uid, out MalfAiCameraUpgradeComponent? comp))
            return;

        var newEffective = comp.EnabledDesired;
        if (comp.EnabledEffective != newEffective)
        {
            comp.EnabledEffective = newEffective;
            Dirty(uid, comp);
        }
    }

    private void OnHeldShutdown(EntityUid uid, StationAiHeldComponent held, ref ComponentShutdown args)
    {
        // AI has left the core (carded/shunted): immediately disable effective.
        if (!TryComp(uid, out MalfAiCameraUpgradeComponent? comp))
            return;

        if (comp.EnabledEffective)
        {
            comp.EnabledEffective = false;
            Dirty(uid, comp);
        }
    }
}
