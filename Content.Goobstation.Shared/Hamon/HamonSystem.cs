using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Shared.Hamon;

public sealed class HamonSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PredictNextLineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PredictNextLineComponent, HamonNextLineEvent>(OnPredictNextLine);
        SubscribeLocalEvent<PredictNextLineComponent, ListenEvent>(OnListen);
    }

    private void OnMapInit(Entity<PredictNextLineComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ent.Comp.Action);
    }

    private void OnPredictNextLine(Entity<PredictNextLineComponent> ent, ref HamonNextLineEvent args)
    {
        ent.Comp.Target = args.Target;

        _chat.TrySendInGameICMessage(ent.Owner, ent.Comp.Message, InGameICChatType.Speak, false);
        EnsureComp<ActiveListenerComponent>(ent.Owner);
    }

    private void OnListen(Entity<PredictNextLineComponent> ent, ref ListenEvent args)
    {
        if (args.Source != ent.Comp.Target || ent.Comp.Target is null)
            return;

        _chat.TrySendInGameICMessage(ent.Owner, args.Message, InGameICChatType.Speak, false);
        ent.Comp.Target = null;
    }
}
