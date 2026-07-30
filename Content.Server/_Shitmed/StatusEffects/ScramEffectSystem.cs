// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.StatusEffects;
using Content.Shared._Trauma.Teleportation;
using Content.Shared.Teleportation;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ScrambleLocationEffectSystem : EntitySystem
{
    [Dependency] private readonly RandomTeleportSystem _teleportSys = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ScrambleLocationEffectComponent, ComponentInit>(OnInit);
    }
    private void OnInit(EntityUid uid, ScrambleLocationEffectComponent component, ComponentInit args)
    {
        // TODO: Add the teleport component via onAdd:
        var teleport = EnsureComp<RandomTeleportComponent>(uid);
        _teleportSys.RandomTeleport(uid, teleport);
    }


}
