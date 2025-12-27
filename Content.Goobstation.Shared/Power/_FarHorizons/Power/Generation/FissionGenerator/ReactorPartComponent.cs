using Content.Shared._FarHorizons.Materials;
using Content.Shared.Atmos;
using Content.Shared.Materials;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/reactorcomponents.dm

/// <summary>
/// A reactor part for the reactor grid.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorPartComponent : Component
{
    /// <summary>
    /// The entity prototype name this component results from.
    /// </summary>
    [DataField]
    public EntProtoId ProtoId = "BaseReactorPart";

    /// <summary>
    /// Icon of this component as it shows in the UIs.
    /// </summary>
    [DataField]
    public string IconStateInserted = "base";

    /// <summary>
    /// Icon of this component as it shows in the world.
    /// </summary>
    [DataField]
    public string IconStateCap = "rod_cap";

    /// <summary>
    /// Byte indicating what type of rod this reactor part is
    /// </summary>
    [DataField]
    public int RodType = 0;

    public enum RodTypes
    {
        None = 0,
        FuelRod = 1 << 0,    // 1 Can be processed by the nuclear centrifuge
        ControlRod = 1 << 1, // 2 Can change its NeutronCrossSection according to control rod setting
        GasChannel = 1 << 2, // 4 Can process gas
    }

    #region Variables
    /// <summary>
    /// Temperature of this component, starts at room temp Kelvin by default.
    /// </summary>
    [DataField]
    public float Temperature = Atmospherics.T20C;

    /// <summary>
    /// How much does this component share heat with surrounding components? Basically surface area in contact (m2).
    /// </summary>
    [DataField]
    public float ThermalCrossSection = 10;

    /// <summary>
    /// How adept is this component at interacting with neutrons - fuel rods are set up to capture them, heat exchangers are set up not to.
    /// </summary>
    [DataField]
    public float NeutronCrossSection = 0.5f;

    /// <summary>
    /// Control rods don't moderate neutrons, they absorb them.
    /// </summary>
    [DataField]
    public bool IsControlRod = false;

    /// <summary>
    /// Max health to set <see cref="MeltHealth"/> to on init.
    /// </summary>
    [DataField]
    public float MaxHealth = 100;

    /// <summary>
    /// Essentially indicates how long this component can be at a dangerous temperature before it melts.
    /// </summary>
    [DataField]
    public float MeltHealth = 100;

    /// <summary>
    /// If this component is melted, you can't take it out of the reactor and it might do some weird stuff.
    /// </summary>
    [DataField]
    public bool Melted = false;

    /// <summary>
    /// The dangerous temperature above which this component starts to melt. 1700K is the melting point of steel.
    /// </summary>
    [DataField]
    public float MeltingPoint = 1700;

    /// <summary>
    /// How much gas this component can hold, and will be processed per tick.
    /// </summary>
    [DataField]
    public float GasVolume = 0;

    /// <summary>
    /// Thermal mass. Basically how much energy it takes to heat this up 1Kelvin.
    /// </summary>
    [DataField]
    public float ThermalMass = 420 * 250; //specific heat capacity of steel (420 J/KgK) * mass of component (Kg)
    #endregion

    [DataField("material")]
    public ProtoId<MaterialPrototype> Material = "Steel";

    [DataField]
    public MaterialProperties? Properties;

    #region Type specific
    /// <summary>
    /// The target insertion level of the control rod.
    /// </summary>
    [DataField]
    public float ConfiguredInsertionLevel = 1;

    /// <summary>
    /// How adept the gas channel is at transfering heat to/from gasses.
    /// </summary>
    [DataField]
    public float GasThermalCrossSection = 15; //was 15

    /// <summary>
    /// The gas mixture inside the gas channel.
    /// </summary>
    public GasMixture? AirContents;
    #endregion

    /// <summary>
    /// Creates a new <see cref="ReactorPartComponent"> with information from an existing one.
    /// </summary>
    /// <param name="source"></param>
    public ReactorPartComponent(ReactorPartComponent source)
    {
        ProtoId = source.ProtoId;
        IconStateInserted = source.IconStateInserted;
        IconStateCap = source.IconStateCap;
        RodType = source.RodType;

        Temperature = source.Temperature;
        ThermalCrossSection = source.ThermalCrossSection;
        NeutronCrossSection = source.NeutronCrossSection;
        IsControlRod = source.IsControlRod;
        MaxHealth = source.MaxHealth;
        MeltHealth = source.MeltHealth;
        Melted = source.Melted;
        MeltingPoint = source.MeltingPoint;
        GasVolume = source.GasVolume;
        ThermalMass = source.ThermalMass;

        Material = source.Material;
        Properties = source.Properties != null ? new MaterialProperties(source.Properties) : null;

        ConfiguredInsertionLevel = source.ConfiguredInsertionLevel;
        GasThermalCrossSection = source.GasThermalCrossSection;
        AirContents = source.AirContents;
    }

    public bool HasRodType(RodTypes type) => (RodType & (int)type) == (int)type;
}

/// <summary>
/// A virtual neutron that flies around within the reactor.
/// </summary>
[NetworkedComponent]
public sealed class ReactorNeutron
{
    public Direction dir = Direction.North;
    public float velocity = 1;
}