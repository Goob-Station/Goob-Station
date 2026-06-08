// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Baptr0b0t 
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SpeciesAffinity;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SpeciesAffinity;

public sealed class SpeciesAffinitySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeciesAffinityComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<SpeciesAffinityComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<SpeciesAffinityComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        ent.Comp.IsHolderFavored = IsFavored(args.User, ent.Comp);
        ent.Comp.NextBiteTime = _timing.CurTime + ent.Comp.BiteInterval;
    }

    private void OnUnequipped(Entity<SpeciesAffinityComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
        ent.Comp.IsHolderFavored = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpeciesAffinityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Holder == null || comp.IsHolderFavored)
                continue;

            if (_timing.CurTime < comp.NextBiteTime)
                continue;

            comp.NextBiteTime = _timing.CurTime + comp.BiteInterval;

            if (!_random.Prob(comp.BiteChance))
                continue;

            var holder = comp.Holder.Value;
            var message = $"{Name(uid)} doesn't recognize its owner and bites you!";

            if (comp.BiteSound != null)
                _audio.PlayPvs(comp.BiteSound, uid);
            _popup.PopupEntity(message, uid, holder, PopupType.MediumCaution);
            _damageable.TryChangeDamage(holder, comp.BiteDamage, origin: uid);

            if (comp.BiteReagents != null)
                _bloodstream.TryAddToChemicals(holder, comp.BiteReagents.Clone());
        }
    }

    private bool IsFavored(EntityUid holder, SpeciesAffinityComponent comp)
    {
        if (!TryComp<HumanoidAppearanceComponent>(holder, out var appearance))
            return false;

        return comp.FavoredSpecies.Contains(appearance.Species);
    }
}
