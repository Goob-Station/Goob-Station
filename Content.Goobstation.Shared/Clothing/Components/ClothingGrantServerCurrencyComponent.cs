using System.ComponentModel.DataAnnotations;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Components;

/// <summary>
/// This is used for clothing that grants server currency if worn on end of round
/// </summary>
[RegisterComponent]
public sealed partial class ClothingGrantServerCurrencyComponent : Component
{
    [DataField]
    public int Amount = 5;

    [DataField]
    public SlotFlags Slot = SlotFlags.NECK;

}


