using Robust.Shared.ContentPack;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.ContributorName;

public sealed class ContributorNameSystem : EntitySystem
{
    [Dependency] private readonly IResourceManager _resourceManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    private string[] _names = default!;

    public override void Initialize()
    {
        base.Initialize();
        _names = LoadNames();

        SubscribeLocalEvent<ContributorNameComponent, MapInitEvent>(OnMapInit);
    }

    private string[] LoadNames()
    {
        // Hardcoded file path :(
        var fullText = _resourceManager.ContentFileReadAllText("/Credits/GitHub.txt");
        return fullText.Trim().Split(", ");
    }

    private void OnMapInit(EntityUid uid, ContributorNameComponent comp, MapInitEvent args)
    {
        var entMetaData = MetaData(uid);

        _metaData.SetEntityName(uid, _random.Pick(_names), entMetaData);
    }
}
