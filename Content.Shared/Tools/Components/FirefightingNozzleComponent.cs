// Assmos - Extinguisher Nozzle

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory; 
using Content.Shared.Whitelist; 

namespace Content.Shared.Tools.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(SharedToolSystem))]
public sealed partial class FirefightingNozzleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    ///     Name of solution/>.
    /// </summary>
    [DataField]
    public const string SolutionName = "tank";

    /// <summary>
    ///     Reagent that will be used as fuel for welding.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> TankReagent = "Water";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public SlotFlags TargetSlot;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public EntityWhitelist? ProviderWhitelist;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ExternalContainer = false;

    /// <summary>
    ///     Sound played when refilling the welder.
    /// </summary>
    [DataField]
    public SoundSpecifier FirefightingNozzleRefill = new SoundPathSpecifier("/Audio/Effects/refill.ogg");
}
