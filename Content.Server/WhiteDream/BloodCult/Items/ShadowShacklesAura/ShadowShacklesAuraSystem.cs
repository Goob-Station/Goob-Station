using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Cuffs;
using Content.Server.Stunnable;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.GameObjects;


namespace Content.Server.WhiteDream.BloodCult.Items.ShadowShacklesAura;

public sealed class ShadowShacklesAuraSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowShacklesAuraComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, ShadowShacklesAuraComponent component, MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        var target = args.HitEntities.First();
        if (uid == target
            || !HasComp<StunnedComponent>(target)
            || HasComp<BloodCultistComponent>(target))
            return;

        if (component.Speech != null)
            _chat.TrySendInGameICMessage(args.User, component.Speech, component.ChatType, false);

        var shuckles = Spawn(component.ShacklesProto, _transform.GetMapCoordinates(args.User));
        if (!_cuffable.TryAddNewCuffs(target, args.User, shuckles))
        {
            QueueDel(shuckles);
            return;
        }

        _stun.TryKnockdown(target, component.KnockdownDuration, true);
        _statusEffects.TryAddStatusEffect<MutedComponent>(target, "Muted", component.MuteDuration, true);
        QueueDel(uid);
    }
}
