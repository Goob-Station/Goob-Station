using Content.Shared.InteractionVerbs;
using Content.Shared.Jittering;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class JitterAction : InteractionAction
{
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(1);

    [DataField]
    public float Amplitude = 10f, Frequency = 4f;

    [DataField]
    public bool Refresh = false;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        return true;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        deps.EntMan.System<SharedJitteringSystem>().DoJitter(args.Target, Time, Refresh, Amplitude, Frequency);
        return true;
    }
}
