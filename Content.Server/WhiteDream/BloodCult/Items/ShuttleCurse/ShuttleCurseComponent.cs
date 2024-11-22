using Robust.Shared.Audio;

namespace Content.Server.WhiteDream.BloodCult.Items.ShuttleCurse;

[RegisterComponent]
public sealed partial class ShuttleCurseComponent : Component
{
    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(3);

    [DataField]
    public SoundSpecifier ScatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public List<string> CurseMessages = new()
    {
        "shuttle-curse-message-1",
        "shuttle-curse-message-2",
        "shuttle-curse-message-3",
        "shuttle-curse-message-4",
        "shuttle-curse-message-5",
        "shuttle-curse-message-6",
        "shuttle-curse-message-7",
        "shuttle-curse-message-8",
        "shuttle-curse-message-9",
        "shuttle-curse-message-10",
        "shuttle-curse-message-11",
    };
}
