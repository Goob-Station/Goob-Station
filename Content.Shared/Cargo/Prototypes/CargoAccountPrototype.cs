using Content.Shared.Access;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo.Prototypes;

/// <summary>
/// This is a prototype for a single account that stores money on StationBankAccountComponent
/// </summary>
[Prototype]
public sealed partial class CargoAccountPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Full IC name of the account.
    /// </summary>
    [DataField]
    public LocId Name;

    /// <summary>
    /// A shortened code used to refer to the account in UIs
    /// </summary>
    [DataField]
    public LocId Code;

    /// <summary>
    /// Color corresponding to the account.
    /// </summary>
    [DataField]
    public Color Color;

    /// <summary>
    /// Channel used for announcing transactions.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel;

    /// <summary>
    /// Paper prototype used for acquisition slips.
    /// </summary>
    [DataField]
    public EntProtoId AcquisitionSlip;

    // CorvaxGoob-CargoFeatures-Start
    /// <summary>
    /// Доступ, который будет проверяться на возможность установки на ящик, и который будет устанавливаться в случае заказа такого.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> SecureCrateOrderAccess = new();

    /// <summary>
    /// Название для отдела, куда будет указывать доставка по умолчанию.
    /// </summary>
    [DataField]
    public LocId? DepartmentDestinationName;

    /// <summary>
    /// Прототип для ящика, который будет спавнится при одобрении заказа с пометкой о защите заказа.
    /// </summary>
    [DataField]
    public EntProtoId? SecureCratePrototype;
    // CorvaxGoob-CargoFeatures-End
}
