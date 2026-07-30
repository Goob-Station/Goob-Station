namespace Content.Trauma.Shared.Mobs;

/// <summary>
/// Component for mice and other spammed mobs to despawn 5 minutes after dying.
/// </summary>
/// 
[RegisterComponent]
public sealed partial class AntiMobSpamComponent : Component
{
    [DataField]
    public TimeSpan Test = TimeSpan.FromMinutes(5);
}
