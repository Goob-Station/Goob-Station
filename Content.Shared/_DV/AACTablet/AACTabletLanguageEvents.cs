using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.AACTablet;

[Serializable, NetSerializable]
public sealed class AACTabletLanguagesRefreshedEvent : EntityEventArgs
{
    /// <summary>
    /// The tablet entity,
    /// </summary>
    public NetEntity Tablet;

    /// <summary>
    /// The known language prototypes.
    /// </summary>
    public List<ProtoId<LanguagePrototype>> SpokenLanguages;

    public AACTabletLanguagesRefreshedEvent(NetEntity tablet, List<ProtoId<LanguagePrototype>> spokenlanguage)
    {
        Tablet = tablet;
        SpokenLanguages = [];
        SpokenLanguages.AddRange(spokenlanguage);
    }
}
