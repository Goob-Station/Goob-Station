// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicImpositionSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicImposingComponent, BeforeDamageChangedEvent>(OnImpositionDamaged);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicImposition>(OnCosmicImposition);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CosmicImposingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Expiry)
                continue;

            RemComp(uid, comp);
        }
    }

    private void OnCosmicImposition(Entity<CosmicCultComponent> uid, ref EventCosmicImposition args)
    {
        EnsureComp<CosmicImposingComponent>(uid, out var comp);
        comp.Expiry = _timing.CurTime + uid.Comp.CosmicImpositionDuration;
        Spawn(uid.Comp.ImpositionVFX, Transform(uid).Coordinates);
        args.Handled = true;
        _audio.PlayPvs(uid.Comp.ImpositionSFX, uid, AudioParams.Default.WithVariation(0.05f));
        _cult.MalignEcho(uid);
    }

    private void OnImpositionDamaged(Entity<CosmicImposingComponent> uid, ref BeforeDamageChangedEvent args) =>
        args.Cancelled = true;
}
