// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Ghost.ObserverHud;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Ghost;
using Content.Shared.Overlays;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Ghost.ObserverHud;

public sealed class GhostObserverHudSystem : EntitySystem
{
    private static readonly ProtoId<DamageContainerPrototype> SiliconDamageContainer = "Silicon";

    // The server accepts only these observer HUD flags from the client
    private const GhostObserverHudVisuals ValidVisuals =
        GhostObserverHudVisuals.JobMindshield |
        GhostObserverHudVisuals.Health |
        GhostObserverHudVisuals.CriminalRecords;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<GhostObserverHudUpdateRequestEvent>(OnUpdateRequest);
        SubscribeLocalEvent<GhostObserverHudComponent, ComponentShutdown>(OnHudShutdown);
        SubscribeLocalEvent<GhostComponent, ComponentRemove>(OnGhostRemoved);
    }

    private void OnGhostRemoved(Entity<GhostComponent> ent, ref ComponentRemove args) =>
        RemCompDeferred<GhostObserverHudComponent>(ent.Owner);

    private void OnUpdateRequest(GhostObserverHudUpdateRequestEvent ev, EntitySessionEventArgs args)
    {
        // Server-side validation: the sender must be a regular ghost with this feature enabled
        if (args.SenderSession.AttachedEntity is not { Valid: true } uid ||
            !TryComp<GhostComponent>(uid, out var ghost) ||
            !HasComp<GhostObserverHudComponent>(uid) ||
            ghost.CanGhostInteract)
        {
            return;
        }

        // Keep only supported flags before updating the ghost's HUD
        Apply(uid, ev.Visuals & ValidVisuals);
    }

    private void OnHudShutdown(Entity<GhostObserverHudComponent> ent, ref ComponentShutdown args)
    {
        RemCompDeferred<ShowJobIconsComponent>(ent.Owner);
        RemCompDeferred<ShowMindShieldIconsComponent>(ent.Owner);
        RemCompDeferred<ShowHealthBarsComponent>(ent.Owner);
        RemCompDeferred<ShowCriminalRecordIconsComponent>(ent.Owner);
    }

    private void Apply(EntityUid uid, GhostObserverHudVisuals visuals)
    {
        SetComponent<ShowJobIconsComponent>(uid, visuals.HasFlag(GhostObserverHudVisuals.JobMindshield));
        SetComponent<ShowMindShieldIconsComponent>(uid, visuals.HasFlag(GhostObserverHudVisuals.JobMindshield));

        SetHealth(uid, visuals.HasFlag(GhostObserverHudVisuals.Health));

        SetComponent<ShowCriminalRecordIconsComponent>(uid, visuals.HasFlag(GhostObserverHudVisuals.CriminalRecords));
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

/// <summary>
/// Marks regular ghosts whose prototypes grant the observer HUD action.
/// </summary>
[RegisterComponent]
public sealed partial class GhostObserverHudComponent : Component { }
