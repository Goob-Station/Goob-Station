// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Speech.Muting;

namespace Content.Server._Shitmed.Body.Systems;

public sealed class TongueSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TongueComponent, OrganEnabledEvent>(OnOrganEnabled);
        SubscribeLocalEvent<TongueComponent, OrganDisabledEvent>(OnOrganDisabled);
        SubscribeLocalEvent<TongueComponent, OrganAddedToBodyEvent>(OnAddedToBody);
        SubscribeLocalEvent<TongueComponent, OrganRemovedFromBodyEvent>(OnRemovedFromBody);
    }

    private void OnOrganEnabled(Entity<TongueComponent> ent, ref OrganEnabledEvent args)
    {
        UpdateTongueless(ent, args.Organ.Comp.Body, true);
    }

    private void OnOrganDisabled(Entity<TongueComponent> ent, ref OrganDisabledEvent args)
    {
        UpdateTongueless(ent, args.Organ.Comp.Body, false);
    }

    private void OnAddedToBody(Entity<TongueComponent> ent, ref OrganAddedToBodyEvent args)
    {
        if (TryComp<OrganComponent>(ent, out var organ))
            UpdateTongueless(ent, args.Body, organ.Enabled);
    }

    private void OnRemovedFromBody(Entity<TongueComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        UpdateTongueless(ent, args.OldBody, false);
    }

    private void UpdateTongueless(EntityUid tongue, EntityUid? bodyUid, bool enabled)
    {
        if (TerminatingOrDeleted(tongue)
            || bodyUid is not { Valid: true } body
            || TerminatingOrDeleted(body))
            return;

        if (enabled)
        {
            RemComp<TonguelessComponent>(body);
            return;
        }

        if (TryComp<BodyComponent>(body, out var bodyComp)
            && _body.TryGetBodyOrganEntityComps<TongueComponent>((body, bodyComp), out var tongues)
            && tongues.Count > 1)
            return;

        EnsureComp<TonguelessComponent>(body);
    }
}
