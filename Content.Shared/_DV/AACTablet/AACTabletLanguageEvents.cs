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
    public List<ProtoId<LanguagePrototype>> SpokenLanguages = [];

    /// <summary>
    ///     The current language the entity uses when speaking.
    ///     Other listeners will hear the entity speak in this language.
    /// </summary>
    [DataField]
    public string CurrentLanguage;

    public AACTabletLanguagesRefreshedEvent(NetEntity tablet, List<ProtoId<LanguagePrototype>> spokenlanguage, string currentLanguage)
    {
        Tablet = tablet;
        SpokenLanguages = [];
        SpokenLanguages.AddRange(spokenlanguage);
        CurrentLanguage = currentLanguage;
    }
}
