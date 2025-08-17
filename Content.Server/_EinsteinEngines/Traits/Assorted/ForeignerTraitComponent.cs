using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._EinsteinEngines.Traits.Assorted;

/// <summary>
///     When applied to a not-yet-spawned player entity, removes <see cref="BaseLanguage"/> from the lists of their languages
///     and gives them a translator instead.
/// </summary>
[RegisterComponent]
public sealed partial class ForeignerTraitComponent : Component
{
    /// <summary>
    ///     The "base" language that is to be removed and substituted with a translator.
    ///     By default, equals to the fallback language, which is TauCetiBasic.
    /// </summary>
    [DataField]
    public ProtoId<LanguagePrototype> BaseLanguage = SharedLanguageSystem.FallbackLanguagePrototype;

    /// <summary>
    ///     Whether this trait prevents the entity from understanding the base language.
    /// </summary>
    [DataField]
    public bool CantUnderstand = true;

    /// <summary>
    ///     Whether this trait prevents the entity from speaking the base language.
    /// </summary>
    [DataField]
    public bool CantSpeak = true;

    /// <summary>
    ///     The base translator prototype to use when creating a translator for the entity.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId BaseTranslator = default!;
}
