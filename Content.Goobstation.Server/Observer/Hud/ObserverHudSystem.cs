// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Observer.Hud;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Ghost;
using Content.Shared.Overlays;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Observer.Hud;

public sealed class ObserverHudSystem : EntitySystem
{
    private static readonly ProtoId<DamageContainerPrototype> SiliconDamageContainer = "Silicon";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ObserverHudUpdateRequestEvent>(OnUpdateRequest);
        SubscribeLocalEvent<GhostComponent, ComponentRemove>(OnGhostRemoved);
    }

    private void OnGhostRemoved(Entity<GhostComponent> ent, ref ComponentRemove args)
    {
        if (!ent.Comp.CanUseObserverHud)
            return;

        RemCompDeferred<ShowJobIconsComponent>(ent.Owner);
        RemCompDeferred<ShowMindShieldIconsComponent>(ent.Owner);
        RemCompDeferred<ShowHealthBarsComponent>(ent.Owner);
        RemCompDeferred<ShowCriminalRecordIconsComponent>(ent.Owner);
    }

    private void OnUpdateRequest(ObserverHudUpdateRequestEvent ev, EntitySessionEventArgs args)
    {
        if (
            // The sender must control a valid entity
            args.SenderSession.AttachedEntity is not { Valid: true } uid ||
            // The controlled entity must be a ghost
            !TryComp<GhostComponent>(uid, out var ghost) ||
            // The ghost's prototype must allow the observer HUD
            !ghost.CanUseObserverHud)
        {
            return;
        }

        SetComponent<ShowJobIconsComponent>(uid, ev.ShowJobMindshield);
        SetComponent<ShowMindShieldIconsComponent>(uid, ev.ShowJobMindshield);
        SetHealth(uid, ev.ShowHealth);
        SetComponent<ShowCriminalRecordIconsComponent>(uid, ev.ShowCriminalRecords);
    }

    private void SetHealth(EntityUid uid, bool enabled)
    {
        if (!enabled)
        {
            RemComp<ShowHealthBarsComponent>(uid);
            return;
        }

        if (EnsureComp(uid, out ShowHealthBarsComponent healthBars))
            return;

        healthBars.DamageContainers.Add(SiliconDamageContainer);
        Dirty(uid, healthBars);
    }

    // Simple HUD overlays are controlled by adding or removing their marker components
    private void SetComponent<T>(EntityUid uid, bool enabled) where T : IComponent, new()
    {
        if (enabled)
            EnsureComp<T>(uid);
        else
            RemComp<T>(uid);
    }
}
