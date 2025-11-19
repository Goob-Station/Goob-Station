// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI camera-upgrade toggle and keeps the Effective flag in sync with core status.
/// </summary>
public sealed class MalfAiCameraUpgradeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Core presence changes (enable when entering core if desired; disable on leaving)
        SubscribeLocalEvent<StationAiHeldComponent, ComponentStartup>(OnHeldStartup);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentShutdown>(OnHeldShutdown);

        // Grant-on-purchase event
        SubscribeLocalEvent<MalfunctioningAiComponent, MalfAiCameraUpgradeUnlockedEvent>(OnCameraUpgradeUnlocked);
    }

    private void OnCameraUpgradeUnlocked(Entity<MalfunctioningAiComponent> ent, ref MalfAiCameraUpgradeUnlockedEvent ev)
    {
        var comp = EnsureComp<MalfAiCameraUpgradeComponent>(ent);
        comp.EnabledDesired = true;

        // Effective only while the AI is in its core (StationAiHeldComponent present).
        comp.EnabledEffective = HasComp<StationAiHeldComponent>(ent);
        Dirty(ent, comp);
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
