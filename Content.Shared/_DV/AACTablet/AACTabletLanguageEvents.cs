using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.AACTablet;

/// <summary>
///     Raised on the AAC tablet its opened to request known languages from server.
/// </summary>
public sealed class AACTabletLanguagesRefreshRequestEvent(EntityUid uid) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The tablet itself
    /// </summary>
    public EntityUid Uid = uid;
}
/// <summary>
///     Raised on the AAC tablet from server to update the list of languages.
/// </summary>
public sealed class AACTabletLanguagesRefreshedEvent(EntityUid uid) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The tablet itself
    /// </summary>
    public EntityUid Uid = uid;

    /// <summary>
    /// The known language protos
    /// </summary>
    public List<ProtoId<LanguagePrototype>> SpokenLanguages = [];
}



