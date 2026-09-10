using Content.Goobstation.Shared.PlushieBite;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.PlushieBite;

public sealed class PlushieBiteSystem : EntitySystem
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

        SubscribeLocalEvent<PlushieBiteComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<PlushieBiteComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<PlushieBiteComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        ent.Comp.NextBiteTime = _timing.CurTime + ent.Comp.BiteInterval;
    }

    private void OnUnequipped(Entity<PlushieBiteComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PlushieBiteComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Holder == null || _timing.CurTime < comp.NextBiteTime)
                continue;

            comp.NextBiteTime = _timing.CurTime + comp.BiteInterval;

            if (!_random.Prob(comp.BiteChance))
                continue;

            var holder = comp.Holder.Value;

            if (TryComp<HumanoidAppearanceComponent>(holder, out var appearance)
                && comp.FavoredSpecies.Contains(appearance.Species))
                continue;

            var message = Loc.GetString("plushie-bite-popup", ("plushie", Name(uid)));

            if (comp.BiteSound != null)
                _audio.PlayPvs(comp.BiteSound, uid);
            _popup.PopupEntity(message, uid, holder, PopupType.MediumCaution);
            _damageable.TryChangeDamage(holder, comp.BiteDamage, origin: uid);

            if (comp.BiteReagents != null)
                _bloodstream.TryAddToBloodstream(holder, comp.BiteReagents.Clone());
        }
    }
}
