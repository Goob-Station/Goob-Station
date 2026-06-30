using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Teleportation.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TelesciTeleporterComponent : Component
{
    [DataField]
    public float X = 0; //TODO MAKE INTO VECTOR

    [DataField]
    public float Y = 0;

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
    public EntityUid? Console;

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
public sealed class TelesciBoundUserInterfaceState : BoundUserInterfaceState
{

    public TelesciBoundUserInterfaceState()
    {
    }
}

[Serializable, NetSerializable]
public sealed class TelesciSendMessage : BoundUserInterfaceMessage
{
    public Vector2 Cordinates { get; }

    public TelesciSendMessage(Vector2 cordinates)
    {
        Cordinates = cordinates;
    }
}

[Serializable, NetSerializable]
public sealed class TelesciRetriveMessage : BoundUserInterfaceMessage

{
    public Vector2 Cordinates { get; }

    public TelesciRetriveMessage(Vector2 cordinates)
    {
        Cordinates = cordinates;
    }
}

[Serializable, NetSerializable]
public sealed class TelesciOpenPortaleMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TelesciScanMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TelesciSendEvent : EventArgs
{
    public Vector2 Cordinates { get; }

    public TelesciSendEvent(Vector2 cordinates)
    {
        Cordinates = cordinates;
    }
}

[Serializable, NetSerializable]
public sealed class TelesciRetriveEvent : EventArgs
{
    public Vector2 Cordinates { get; }

    public TelesciRetriveEvent(Vector2 cordinates)
    {
        Cordinates = cordinates;
    }
}

[Serializable, NetSerializable]
public sealed class TelesciCooldowneEvent : EventArgs
{
    public TimeSpan Cooldown { get; }
    public TelesciCooldowneEvent(TimeSpan time)
    {
        Cooldown = time;
    }
}
