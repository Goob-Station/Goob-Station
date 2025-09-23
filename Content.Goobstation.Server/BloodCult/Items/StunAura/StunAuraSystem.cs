using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Stunnable;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.BloodCult.Items.StunAura;

public sealed class StunAuraSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StunAuraComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, StunAuraComponent component, MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        var target = args.HitEntities.First();
        if (uid == target
            || HasComp<Goobstation.Shared.BloodCult.BloodCultist.BloodCultistComponent>(target)
            || HasComp<Goobstation.Shared.BloodCult.Constructs.ConstructComponent>(target))
            return;

        if (component.Speech != null)
            _chat.TrySendInGameICMessage(args.User, component.Speech, component.ChatType, false);

        _statusEffects.TryAddStatusEffect<MutedComponent>(target, "Muted", component.MuteDuration, true);
        _stun.TryParalyze(target, component.ParalyzeDuration, true);
        QueueDel(uid);
    }
}
