using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.Prototypes;

namespace Content.Server._EinsteinEngines.Language;

[RegisterComponent]
public sealed partial class TowerOfBabelComponent : Component
{
    [DataField]
    public ProtoId<LanguagePrototype> DefaultLanguage = "TauCetiBasic";
}
