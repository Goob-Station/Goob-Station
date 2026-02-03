using Content.Goobstation.Common.AntagToken;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Goobstation.Server.AntagToken;

/// <summary>
///     Thin bridge system that forwards entity events to the AntagToken IoC manager.
///     Entity events like RoundRestartCleanupEvent can only be subscribed from EntitySystems.
/// </summary>
public sealed class AntagTokenBridgeSystem : EntitySystem
{
    [Dependency] private readonly IAntagTokenManager _tokenManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _tokenManager.ClearActiveTokens());
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New == GameRunLevel.InRound)
            _tokenManager.ClearActiveTokens();
    }
}
