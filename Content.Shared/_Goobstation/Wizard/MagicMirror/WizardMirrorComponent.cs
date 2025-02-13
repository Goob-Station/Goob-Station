using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.MagicMirror;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WizardMirrorComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    [DataField(required: true)]
    public HashSet<ProtoId<SpeciesPrototype>> AllowedSpecies = new();
}
