using Content.Shared.InteractionVerbs;
using Content.Shared.Mood;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Shared.InteractionVerbs.Actions;

[Serializable]
public sealed partial class MoodAction : InteractionAction
{
    [DataField(required: true)]
    public ProtoId<MoodEffectPrototype> Effect;

    [DataField]
    public float Modifier = 1f;

    [DataField]
    public float Offset;

    [DataField]
    public bool Remove;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        return true;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (Remove)
            deps.EntMan.EventBus.RaiseLocalEvent(args.Target, new MoodRemoveEffectEvent(Effect.Id));
        else
            deps.EntMan.EventBus.RaiseLocalEvent(args.Target, new MoodEffectEvent(Effect.Id, Modifier, Offset));

        return true;
    }
}
