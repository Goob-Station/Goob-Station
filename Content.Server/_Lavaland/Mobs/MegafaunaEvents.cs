namespace Content.Server._Lavaland.Mobs;

/// <summary>
/// Raised when boss is fully defeated.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public sealed partial class MegafaunaKilledEvent : EntityEventArgs;

/// <summary>
/// Raised when boss starts proceeding it's logic.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public sealed partial class MegafaunaStartupEvent : EntityEventArgs;

/// <summary>
/// Raised when boss doesn't die but for any reason deactivates.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public sealed partial class MegafaunaDeinitEvent : EntityEventArgs;
