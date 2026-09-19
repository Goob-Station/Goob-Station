using Content.Goobstation.Shared.Hamon.Components;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Hamon;

/// <summary>
/// Your next line is...
/// </summary>
public partial class HamonSystem
{
    private void InitializeNextLine()
    {
        SubscribeLocalEvent<PredictNextLineComponent, HamonNextLineEvent>(OnPredictNextLine);
        SubscribeLocalEvent<PredictNextLineComponent, ListenEvent>(OnPredictListen);
    }

    private void OnPredictNextLine(Entity<PredictNextLineComponent> ent, ref HamonNextLineEvent args)
    {
        args.Handled = true;

        ent.Comp.Target = args.Target;

        var dataset = _proto.Index(ent.Comp.Messages);
        var line = _random.Pick(dataset.Values);

        _chat.TrySendInGameICMessage(ent.Owner, line, InGameICChatType.Speak, false);
        EnsureComp<ActiveListenerComponent>(ent.Owner);
    }

    private void OnPredictListen(Entity<PredictNextLineComponent> ent, ref ListenEvent args)
    {
        if (args.Source != ent.Comp.Target || ent.Comp.Target is null)
            return;

        _chat.TrySendInGameICMessage(ent.Owner, args.Message, InGameICChatType.Speak, false);
        ent.Comp.Target = null;
    }
}
