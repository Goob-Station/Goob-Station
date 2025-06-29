// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Shitmed.Body.Systems
{
    public sealed class EyesSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly BodySystem _bodySystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EyesComponent, OrganIntegrityChangedEvent>(OnOrganIntegrityChanged);
            SubscribeLocalEvent<EyesComponent, OrganEnabledEvent>(OnOrganEnabled);
            SubscribeLocalEvent<EyesComponent, OrganDisabledEvent>(OnOrganDisabled);
            SubscribeLocalEvent<EyesComponent, EntGotRemovedFromContainerMessage>(OnEyesRemoved);
        }

        private void CheckMissingEyes(EntityUid body, EntityUid eye)
        {
            if (TerminatingOrDeleted(body) || TerminatingOrDeleted(eye))
                return;

            var hasOtherEyes = false;

            if (TryComp<BodyComponent>(body, out var bodyComp))
                if (_bodySystem.TryGetBodyOrganEntityComps<EyesComponent>((body, bodyComp), out var eyes)
                    && eyes.Count > 1)
                    hasOtherEyes = true;

            if (!hasOtherEyes
                && HasComp<EyesComponent>(eye)
                && TryComp(body, out BlindableComponent? blindable))
                _blindableSystem.SetEyeDamage((body, blindable), blindable.MaxDamage);
        }

        // Too much shit would break if I were to nuke blindablecomponent rn. Guess we shitcoding this one.
        private void OnOrganIntegrityChanged(EntityUid uid, EyesComponent component, OrganIntegrityChangedEvent args)
        {
            if (args.NewIntegrity <= 0
                || !TryComp(uid, out OrganComponent? organ)
                || !organ.Body.HasValue
                || !TryComp(organ.Body.Value, out BlindableComponent? blindable)
                || organ.OrganIntegrity <= 0)
                return;

            _blindableSystem.SetEyeDamage((organ.Body.Value, blindable), (int) organ.OrganIntegrity);
        }

        private void OnOrganEnabled(EntityUid uid, EyesComponent component, OrganEnabledEvent args)
        {
            if (TerminatingOrDeleted(uid)
            || args.Organ.Comp.Body is not { Valid: true } body
            || !TryComp(body, out BlindableComponent? blindable))
                return;

            // We add the current eye damage since in any context, the organ being enabled means that it was
            // either removed or disabled, so the BlindableComponent must have some prior damage already.
            var adjustment = (int)(args.Organ.Comp.IntegrityCap - args.Organ.Comp.OrganIntegrity);
            _blindableSystem.SetEyeDamage((body, blindable), adjustment);
        }

        private void OnOrganDisabled(EntityUid uid, EyesComponent component, OrganDisabledEvent args)
        {
            if (TerminatingOrDeleted(uid)
            || args.Organ.Comp.Body is not { Valid: true } body)
                return;

            CheckMissingEyes(body, uid);
        }

        private void OnEyesRemoved(EntityUid uid, EyesComponent component, EntGotRemovedFromContainerMessage args)
        {
            if (TerminatingOrDeleted(uid)
                || !TryComp(args.Entity, out OrganComponent? organ)
                || !organ.Body.HasValue
                || !TryComp(organ.Body.Value, out BlindableComponent? blindable))
                return;

            CheckMissingEyes(organ.Body.Value, uid);
        }
    }
}