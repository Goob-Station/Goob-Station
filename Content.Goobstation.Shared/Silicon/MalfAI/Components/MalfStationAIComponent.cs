using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Store;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class MalfStationAIComponent : Component
{
    /// <summary>
    /// The amount of processing power the player gets after rolling the antag.
    /// </summary>
    [DataField]
    public FixedPoint2 StartingProcessingPower = 50;

    /// <summary>
    /// The prototype or ID of the currency entity for use with the store system.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<CurrencyPrototype>))]
    public string ProcessingPowerPrototype = "ProcessingPower";

    /// <summary>
    /// The reward for hacking an APC.
    /// </summary>
    [DataField]
    public FixedPoint2 HackAPCReward = 10;

    /// <summary>
    /// Used to prevent running multiple hacking doafters at the same time.
    /// </summary>
    public DoAfterId? HackDoAfterID;

    [DataField]
    public SoundCollectionSpecifier BuzzingSound = new("sparks", AudioParams.Default);

    #region Actions

    [DataField, ValidatePrototypeId<EntityPrototype>]
    public string MalfAIToggleShopAction = "ActionMalfAIShop";


    // Machine overload

    [DataField]
    public int MachineOverloadCost = 10;

    /// <summary>
    /// This specifies how long an overloaded machine will take before it explodes.
    /// </summary>
    [DataField]
    public float SecondsToOverload = 5.0f;

    #endregion
}