// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Oskarrr.EngineerJockey;

/// <summary>
/// Occupied ancient cryopod / engineer pod that can awaken an Engineer Jockey.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EngineerCryopodComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Occupied = true;

    [DataField, AutoNetworkedField]
    public bool Awakened;

    [DataField]
    public EntProtoId SpawnPrototype = "MobEngineerJockey";

    /// <summary>
    /// Optional entity to spawn in place of this pod after awakening (e.g. broken pod).
    /// If null, only the sprite state is swapped.
    /// </summary>
    [DataField]
    public EntProtoId? BrokenPrototype;

    /// <summary>
    /// Sprite RSI state to switch to when emptied without replacing the entity.
    /// </summary>
    [DataField]
    public string? EmptySpriteState;

    [DataField]
    public TimeSpan AwakenDelay = TimeSpan.FromSeconds(8);

    [DataField]
    public float ConsoleRange = 25f;
}

[Serializable, NetSerializable]
public enum EngineerConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class EngineerConsoleBuiState : BoundUserInterfaceState
{
    public List<EngineerCryopodEntry> Pods;

    public EngineerConsoleBuiState(List<EngineerCryopodEntry> pods)
    {
        Pods = pods;
    }
}

[Serializable, NetSerializable]
public sealed class EngineerCryopodEntry
{
    public NetEntity Pod;
    public string Name;
    public bool Occupied;

    public EngineerCryopodEntry(NetEntity pod, string name, bool occupied)
    {
        Pod = pod;
        Name = name;
        Occupied = occupied;
    }
}

[Serializable, NetSerializable]
public sealed class EngineerConsoleAwakenMessage : BoundUserInterfaceMessage
{
    public NetEntity Pod;

    public EngineerConsoleAwakenMessage(NetEntity pod)
    {
        Pod = pod;
    }
}
