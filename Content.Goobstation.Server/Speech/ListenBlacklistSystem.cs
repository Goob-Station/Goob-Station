using Content.Goobstation.Shared.Speech;
using Content.Server.Speech;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Speech;

/// <summary>
/// Handles <see cref="ListenAttemptEvent"/> for <see cref="ListenBlacklistComponent"/>.
/// </summary>
public sealed class ListenBlacklistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ListenBlacklistComponent, ListenAttemptEvent>(OnListenAttempt);
    }

    // TODO: if chat ever gets refactored move this system to shared
    private void OnListenAttempt(Entity<ListenBlacklistComponent> ent, ref ListenAttemptEvent args)
    {
        if (_whitelist.IsBlacklistPass(ent.Comp.Blacklist, args.Source))
            args.Cancel();
    }
}
