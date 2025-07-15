using Robust.Shared.Serialization;

namespace Content.Shared.Movement.Events;

/// <summary>
///     Raised from the client to the server to require the server to update the client's input CVars.
/// </summary>
[Serializable, NetSerializable]
public sealed class UpdateInputCVarsMessage : EntityEventArgs { }
