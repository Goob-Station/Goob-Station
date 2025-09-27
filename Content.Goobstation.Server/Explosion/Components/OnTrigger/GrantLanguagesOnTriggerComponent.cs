using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Explosion.Components.OnTrigger;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GrantLanguagesOnTriggerComponent : Component
{
    [DataField]
    public List<ProtoId<LanguagePrototype>> Languages = new();
}
