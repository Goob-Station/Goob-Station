using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._BloodCult.Touchspell;

public sealed partial class TouchspellSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, BloodCultStunEvent>(OnStunEquip);
        SubscribeLocalEvent<FuuMajinComponent, AfterInteractEvent>(OnStun);
    }

    private void OnStunEquip(Entity<BloodCultistComponent> ent, ref BloodCultStunEvent args)
    {
        var st = Spawn("TouchspellCultStun", Transform(ent).Coordinates);

        if (!_hands.TryForcePickupAnyHand(ent, st))
        {
            QueueDel(st);
            return;
        }

        args.Handled = true;
    }

    private void OnStun(Entity<FuuMajinComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null
        || HasComp<BloodCultistComponent>(args.Target))
            return;

        var target = (EntityUid) args.Target;

        if (HasComp<StatusEffectsComponent>(target))
        {
            _chat.TrySendInGameICMessage(args.User, "Fuu ma'jin!", InGameICChatType.Whisper, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(ent.Comp.StunDuration), true);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(ent.Comp.SpeechDuration), true);
            _statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(ent.Comp.MuteDuration), false);
        }

        QueueDel(ent);
        args.Handled = true;
    }
}
