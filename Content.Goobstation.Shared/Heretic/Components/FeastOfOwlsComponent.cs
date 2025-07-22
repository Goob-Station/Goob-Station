using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Heretic.Components;

[RegisterComponent]
public sealed partial class FeastOfOwlsComponent : Component
{
    [DataField]
    public int Reward = 5;

    [ViewVariables]
    public int CurrentStep;

    [DataField]
    public float Timer = 2f;

    [ViewVariables]
    public float ElapsedTime = 2f;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JitterStutterTime = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier KnowledgeGainSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/eatfood.ogg");

    [DataField]
    public SoundSpecifier BriefingSoundIntense = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain_intense.ogg");
}
