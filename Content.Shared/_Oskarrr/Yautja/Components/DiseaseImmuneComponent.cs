using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.Yautja.Components;

/// <summary>
/// Блокує зараження системою хвороб (віруси/бактерії). Не впливає на лицехватів.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DiseaseImmuneComponent : Component;
