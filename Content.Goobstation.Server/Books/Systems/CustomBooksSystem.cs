using Content.Goobstation.Shared.Books;
using Content.Server.Administration.Managers;
using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Robust.Server.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem : SharedCustomBooksSystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    private Dictionary<int, BookData> _pendingBooks = new();
    private int _nextPendingBook = 0;

    public override void Initialize()
    {
        base.Initialize();

        InitializeBinder();
        InitializeScanner();
        InitializePrinter();
        InitializeAdmin();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateScanner(frameTime);
        UpdatePrinter(frameTime);
    }
}
