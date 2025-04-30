// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;

namespace Content.Server._Shitmed.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<TargetChangeEvent>(OnTargetChange);
        SubscribeLocalEvent<TargetingComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnTargetChange(TargetChangeEvent message, EntitySessionEventArgs args)
    {
        if (!TryComp<TargetingComponent>(GetEntity(message.Uid), out var target))
            return;

        target.Target = message.BodyPart;
        Dirty(GetEntity(message.Uid), target);
    }

    private void OnMobStateChange(EntityUid uid, TargetingComponent component, MobStateChangedEvent args)
    {
        // Revival is handled by the server, so we're keeping all of this here.
        var changed = false;

        if (args.NewMobState == MobState.Dead)
        {
            foreach (var part in GetValidParts())
            {
                component.BodyStatus[part] = TargetIntegrity.Dead;
                changed = true;
            }
            // I love groin shitcode.
            component.BodyStatus[TargetBodyPart.Groin] = TargetIntegrity.Dead;
        }
        else if (args.OldMobState == MobState.Dead && (args.NewMobState == MobState.Alive || args.NewMobState == MobState.Critical))
        {
            component.BodyStatus = _bodySystem.GetBodyPartStatus(uid);
            changed = true;
        }

        if (changed)
        {
            Dirty(uid, component);
            RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(uid)), uid);
        }
    }
}