using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Grab;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabModifierComponent : Component, IGrabCooldownComponent
{
    [DataField]
    public GrabStage StartingGrabStage = GrabStage.Soft;

    [DataField]
    public float GrabEscapeModifier;

    [DataField]
    public float GrabEscapeMultiplier = 1f;

    [DataField]
    public float GrabMoveSpeedMultiplier = 1f;

    [DataField, AutoNetworkedField]
    public TimeSpan GrabCooldownEnd { get; set; } = TimeSpan.Zero;

    [DataField]
    public string GrabCooldownVerb { get; set; } = "grabbing-item-cooldown-verb";

    [DataField]
    public TimeSpan GrabCooldownDuration { get; set; } = TimeSpan.FromSeconds(0);

    public bool IsCooldownActive(TimeSpan now)
    {
        return GrabCooldownEnd > now;
    }

    public void StartCooldown(TimeSpan now)
    {
        GrabCooldownEnd = now + GrabCooldownDuration;
    }
}
