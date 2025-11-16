using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Interaction;

namespace Content.Goobstation.Shared.Augments;

public sealed class AugmentSystem : EntitySystem
{
    private EntityQuery<InstalledAugmentsComponent> _installedQuery;
    private EntityQuery<OrganComponent> _organQuery;

    public override void Initialize()
    {
        base.Initialize();

        _installedQuery = GetEntityQuery<InstalledAugmentsComponent>();
        _organQuery = GetEntityQuery<OrganComponent>();

        SubscribeLocalEvent<AugmentComponent, OrganAddedToBodyEvent>(OnOrganOrganAddedToBody);
        SubscribeLocalEvent<AugmentComponent, OrganRemovedFromBodyEvent>(OnOrganOrganRemovedFromBody);
        SubscribeLocalEvent<InstalledAugmentsComponent, AccessibleOverrideEvent>(OnAccessibleOverride);
    }

    private void OnOrganOrganAddedToBody(Entity<AugmentComponent> augment, ref OrganAddedToBodyEvent args)
    {
        var installed = EnsureComp<InstalledAugmentsComponent>(args.Body);
        installed.InstalledAugments.Add(GetNetEntity(augment));
    }

    private void OnOrganOrganRemovedFromBody(Entity<AugmentComponent> augment, ref OrganRemovedFromBodyEvent args)
    {
        if (!TryComp<InstalledAugmentsComponent>(args.OldBody, out var installed))
            return;

        installed.InstalledAugments.Remove(GetNetEntity(augment));
        if (installed.InstalledAugments.Count == 0)
            RemComp<InstalledAugmentsComponent>(args.OldBody);
    }

    private void OnAccessibleOverride(Entity<InstalledAugmentsComponent> ent, ref AccessibleOverrideEvent args)
    {
        if (GetBody(args.Target) is not {} body || body != args.User)
            return;

        // let the user interact with their installed augments
        args.Handled = true;
        args.Accessible = true;
    }

    #region Public API

    /// <summary>
    /// Get the body linked to an augment's organ.
    /// Returns null if not installed into a body.
    /// </summary>
    public EntityUid? GetBody(EntityUid uid) => _organQuery.CompOrNull(uid)?.Body;

    /// <summary>
    /// Relays an event to all installed augments.
    /// </summary>
    public void RelayEvent<T>(EntityUid body, ref T ev) where T: notnull
    {
        if (_installedQuery.TryComp(body, out var comp))
            RelayEvent((body, comp), ref ev);
    }

    /// <summary>
    /// Relay an event in the form usable for a subscription.
    /// </summary>
    public void RelayEvent<T>(Entity<InstalledAugmentsComponent> ent, ref T ev) where T: notnull
    {
        foreach (var netEnt in ent.Comp.InstalledAugments)
        {
            var aug = GetEntity(netEnt);
            RaiseLocalEvent(aug, ref ev);
        }
    }

    #endregion
}
