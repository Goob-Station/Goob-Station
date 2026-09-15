using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.TemporarilyDisableCollision;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class TemporarilyDisableCollisionComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan EndTime;
}
