using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Generic Doodon production machine.
/// </summary>
[RegisterComponent]
public sealed partial class DoodonMachineComponent : Component
{
    [DataField(required: true)]
    public string OutputPrototype = default!;

    [DataField(required: true)]
    public string ResinStack = default!;

    [DataField]
    public int ResinCost = 1;

    [DataField]
    public float ProcessTime = 5f;

    [DataField]
    public bool RespectPopulationCap = true;

    [DataField]
    public bool SpawnOnMapInit = false;

    [DataField]
    public DoodonHousingType ProducedHousing = DoodonHousingType.None;

    public bool Processing;
    public float Accumulator;

    [DataField]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/_Goobstation/Doodon/lancer_splat.ogg");

    [DataField] public bool InitialSpawnDone = false;
}
