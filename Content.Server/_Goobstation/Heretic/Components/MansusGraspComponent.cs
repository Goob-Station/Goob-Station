using Content.Server.Heretic.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.Heretic.Components;

[RegisterComponent, Access(typeof(MansusGraspSystem))]
public sealed partial class MansusGraspComponent : Component
{
    [DataField]
    public string? Path;

    [DataField]
    public TimeSpan CooldownAfterUse = TimeSpan.FromSeconds(10);

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5f);

    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public TimeSpan SpeechTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public TimeSpan AffectedTime = TimeSpan.FromMinutes(5);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    [DataField]
    public LocId Invocation = "heretic-speech-mansusgrasp";
}
