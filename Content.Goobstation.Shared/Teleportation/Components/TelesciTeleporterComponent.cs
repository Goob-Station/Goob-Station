using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Teleportation.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TelesciTeleporterComponent : Component
{
    [DataField]
    public Vector2 Position;

    [DataField]
    public float TeleportSize = 0.5f; // area teleported  1 tile

    [DataField]
    public TimeSpan Cooldown = TimeSpan.Zero;

    [DataField]
    public TimeSpan CooldownInterval = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The corresponding console entity.
    /// Can be null if not linked.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Computer;

    [DataField]
    public SoundSpecifier SoundSucess = new SoundCollectionSpecifier("sparks"); //TODO FIND BETTER SOUNDS

    [DataField]
    public SoundSpecifier SoundFaliure = new SoundCollectionSpecifier("sparks"); //TODO FIND BETTER SOUNDS

    [DataField]
    public float TeleportFaliureMultiplyer = 5f;

    [DataField]
    public float TeleportMaxDistance= 500f;
}

// Events

[Serializable, NetSerializable]
public enum TelesciUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class TelesciBoundUserInterfaceState : BoundUserInterfaceState;

[Serializable, NetSerializable]
public sealed class TelesciSendMessage(Vector2 coordinates) : BoundUserInterfaceMessage
{
    public Vector2 Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class TelesciRetrieveMessage(Vector2 coordinates) : BoundUserInterfaceMessage
{
    public Vector2 Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class TelesciOpenPortaleMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TelesciScanMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TelesciSendEvent(Vector2 coordinates) : EventArgs
{
    public Vector2 Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class TelesciRetrieveEvent(Vector2 coordinates) : EventArgs
{
    public Vector2 Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class TelesciCooldowneEvent(TimeSpan time) : EventArgs
{
    public TimeSpan Cooldown = time;
}
