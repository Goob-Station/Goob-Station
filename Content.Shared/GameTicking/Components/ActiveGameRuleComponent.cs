using Robust.Shared.GameStates;

namespace Content.Shared.GameTicking.Components;

/// <summary>
///     Added to game rules before <see cref="GameRuleStartedEvent"/> and removed before <see cref="GameRuleEndedEvent"/>.
///     Mutually exclusive with <seealso cref="EndedGameRuleComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent] // Goob edit
public sealed partial class ActiveGameRuleComponent : Component;
