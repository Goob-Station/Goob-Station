using Content.Goobstation.Common.AntagToken;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Goobstation.Server.AntagToken;

public sealed class AntagTokenSystem : EntitySystem
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
