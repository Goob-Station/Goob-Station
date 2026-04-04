using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.ChameleonStamp;

[RegisterComponent, AutoGenerateComponentState(true), NetworkedComponent]
public sealed partial class ChameleonStampComponent : Component
{
    [DataField]
    public EntProtoId DefaultStampPrototype = "RubberStampApproved";

    [DataField, AutoNetworkedField]
    public EntProtoId SelectedStampColorPrototype = "RubberStampApproved";

    [DataField, AutoNetworkedField]
    public EntProtoId SelectedStampSpritePrototype = "RubberStampApproved";

    [DataField, AutoNetworkedField]
    public EntProtoId SelectedStampStatePrototype = "RubberStampApproved";

    [DataField, AutoNetworkedField]
    public Color? CustomStampColor = null;

    [DataField, AutoNetworkedField]
    public string? CustomName;

    [DataField, AutoNetworkedField]
    public string? CustomDescription;

    [DataField, AutoNetworkedField]
    public string? StampedName;
}

[Serializable, NetSerializable]
public enum ChameleonStampUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ChameleonStampApplySettingsMessage : BoundUserInterfaceMessage
{
    public readonly EntProtoId SelectedStampColorPrototype;
    public readonly Color CustomStampColor;

    public readonly string? CustomName;
    public readonly string? CustomDescription;

    public readonly string? StampedName;

    public readonly EntProtoId SelectedStampSpritePrototype;
    public readonly EntProtoId SelectedStampStatePrototype;

    public ChameleonStampApplySettingsMessage(
        EntProtoId selectedStampColorPrototype,
        Color customStampColor,
        string? customName,
        string? customDescription,
        string? stampedName,
        EntProtoId selectedStampSpritePrototype,
        EntProtoId selectedStampStatePrototype)
    {
        SelectedStampColorPrototype = selectedStampColorPrototype;
        CustomStampColor = customStampColor;

        CustomName = customName;
        CustomDescription = customDescription;
        StampedName = stampedName;

        SelectedStampSpritePrototype = selectedStampSpritePrototype;
        SelectedStampStatePrototype = selectedStampStatePrototype;
    }
}
