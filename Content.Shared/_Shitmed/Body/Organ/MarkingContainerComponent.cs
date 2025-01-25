// This is a uh, very shitty copout to not wanting to modify the prototypes for felinids, and entities at large so they have ears.
// I will do that at some point, for now I just want the funny surgery to work lol.
using Robust.Shared.GameStates;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Shitmed.Body.Organ;

[RegisterComponent, NetworkedComponent]
public sealed partial class MarkingContainerComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<MarkingPrototype>))]
    public string Marking = default!;

}
