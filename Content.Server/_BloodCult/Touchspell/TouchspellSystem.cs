using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.Interaction;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.Player;
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
    [Dependency] private readonly BloodCultRuleSystem _cultRule = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

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

        var stunDuration = ent.Comp.StunDuration;
        var speechDuration = ent.Comp.SpeechDuration;
        var muteDuration = ent.Comp.MuteDuration;

        var rule = _cultRule.GetRule();
        if (rule != null)
        {
            switch (rule.Stage)
            {
                case CultStage.RedEyes:
                    stunDuration = 8f;
                    muteDuration = 6f;
                    speechDuration = 16f;
                    break;
                case CultStage.Pentagram:
                    stunDuration = 1.6f;
                    muteDuration = 1.2f;
                    speechDuration = 3f;
                    break;
            }
        }

        if (HasComp<StatusEffectsComponent>(target))
        {
            _chat.TrySendInGameICMessage(args.User, "Fuu ma'jin!", InGameICChatType.Whisper, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Items/welder.ogg"), target);
            _stun.TryParalyze(target, TimeSpan.FromSeconds(stunDuration), true);
            _language.DoRatvarian(target, TimeSpan.FromSeconds(speechDuration), true);
            _statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(muteDuration), false);
        }

        QueueDel(ent);
        args.Handled = true;
    }
}
