using Content.Server.Chat.Systems;
using Content.Shared.Speech.Components;
using Content.Shared.Speech;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Speech.Muting;
using Content.Shared.Actions.Events;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Magic.Components;
using Content.Shared.Damage;
using Content.Shared.Chat;
using Content.Shared._Shitmed.Damage;


namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// As soon as the chat refactor moves to Shared
/// the logic here can move to the shared <see cref="SharedSpeakOnActionSystem"/>
/// </summary>
public sealed class SpeakOnActionSystem : SharedSpeakOnActionSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeakOnActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnActionPerformed(Entity<SpeakOnActionComponent> ent, ref ActionPerformedEvent args)
    {
        var user = args.Performer;

        // If we can't speak, we can't speak
        if (!HasComp<SpeechComponent>(user) || HasComp<MutedComponent>(user))
            return;

        // Goob. TODO: Remove Aviu from this plane of existence for whatever has occured here.
        var speech = ent.Comp.Sentence;

        if (TryComp(ent, out MagicComponent? magic))
        {
            var invocationEv = new GetSpellInvocationEvent(magic.School, args.Performer);
            RaiseLocalEvent(args.Performer, invocationEv);
            if (invocationEv.Invocation.HasValue)
                speech = invocationEv.Invocation;
            if (invocationEv.ToHeal.GetTotal() > FixedPoint2.Zero)
            {
                _damageable.TryChangeDamage(args.Performer,
                    -invocationEv.ToHeal,
                    true,
                    false,
                    targetPart: TargetBodyPart.All,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll); // Shitmed Change
            }
        }

        if (string.IsNullOrWhiteSpace(speech))
            return;

        _chat.TrySendInGameICMessage(user, Loc.GetString(speech), InGameICChatType.Speak, false);
    }
}
