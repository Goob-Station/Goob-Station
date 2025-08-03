using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;

namespace Content.Server._White.Inventory.Components;

[RegisterComponent]
public sealed partial class EffectsOnEquipComponent : Component
{
    [DataField(required: true)]
    public string Slot;

    [DataField]
    public List<EntityEffect> Effects = new ();

    [DataField]
    public EntityWhitelist? Blacklist;
}
