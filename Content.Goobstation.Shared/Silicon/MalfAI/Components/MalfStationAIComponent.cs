using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class MalfStationAIComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<CurrencyPrototype>))]
    public string ProcessingPowerPrototype = "ProcessingPower";

    /// <summary>
    /// The reward for hacking an APC.
    /// </summary>
    [DataField]
    public FixedPoint2 HackAPCReward = 10;

    [DataField, ValidatePrototypeId<EntityPrototype>]
    public string MalfAIToggleShopAction = "ActionMalfAIShop";

    public DoAfterId? HackDoAfterID;
}

public sealed partial class MachineOverloadActionEvent : EntityTargetActionEvent;