// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Doors.Systems;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Doors.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicIngressSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicIngress>(OnCosmicIngress);
    }

    private void OnCosmicIngress(Entity<CosmicCultComponent> uid, ref EventCosmicIngress args)
    {
        var target = args.Target;

        if (args.Handled)
            return;

        args.Handled = true;

        if (uid.Comp.CosmicEmpowered
            && TryComp<DoorBoltComponent>(target, out var doorBolt))
            _door.SetBoltsDown((target, doorBolt), false);

        _door.StartOpening(target);
        _audio.PlayPvs(uid.Comp.IngressSFX, uid);
        Spawn(uid.Comp.AbsorbVFX, Transform(target).Coordinates);
        _cult.MalignEcho(uid);
    }
}
