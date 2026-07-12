using Content.Shared.Actions;

namespace Content.Pirate.Shared.Avali.EntitySystems;

/// <summary>
/// Shared base for nanite-induced stasis.
/// </summary>
public abstract class SharedStasisSystem : EntitySystem;

/// <summary>
/// Raised when the entity begins preparing to enter stasis.
/// </summary>
public sealed partial class PrepareStasisActionEvent : InstantActionEvent;

/// <summary>
/// Raised once preparation to enter stasis completes.
/// </summary>
public sealed partial class EnterStasisActionEvent : InstantActionEvent;

/// <summary>
/// Raised when the entity exits stasis.
/// </summary>
public sealed partial class ExitStasisActionEvent : InstantActionEvent;
