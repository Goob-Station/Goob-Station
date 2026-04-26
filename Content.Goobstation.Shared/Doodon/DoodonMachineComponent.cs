using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Generic Doodon production machine.
/// </summary>
[RegisterComponent]
public sealed partial class DoodonMachineComponent : Component
{
    // What the machine makes 
    [DataField(required: true)]
    public string OutputPrototype = default!;

    // What material it needs
    [DataField(required: true)]
    public string ResinStack = default!;

    // The cost of the material
    [DataField]
    public int ResinCost = 1;

    // Time it takes to make the item
    [DataField]
    public float ProcessTime = 5f;

    // Does what it make need required housing? (Doodon workers/warriors)
    // If so, then this should be true.
    // If not (swords, gems, shields), then it should be false
    [DataField]
    public bool RespectPopulationCap = true;

    // Does the machine spawn what it makes when it is built??
    // If so, (Doodon workers/warriors), then it should be true
    // If not, (Items) then it should be false
    [DataField]
    public bool SpawnOnMapInit = false;

    // Does the machine serve as housing for a doodon type? (Dwellings, Warrior huts, pastures)
    [DataField]
    public DoodonHousingType ProducedHousing = DoodonHousingType.None;

    // fields for time tracking
    public bool Processing = false;
    public float Accumulator = 0f;

    // What sound it makes when material is inserted
    // Default is splat because it's funny
    [DataField]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/_Goobstation/Doodon/lancer_splat.ogg");

    [DataField] public bool InitialSpawnDone = false;
}
