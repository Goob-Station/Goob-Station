using Robust.Shared.Serialization;
using Content.Shared.Eui;
using Content.Shared.Humanoid;
namespace Content.Shared.ERP;

public abstract class SharedERPSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

}


[Serializable, NetSerializable]
public sealed class SetInteractionEuiState : EuiStateBase
{
    public NetEntity TargetNetEntity;
    public Sex UserSex;
    public Sex TargetSex;
    public bool UserHasClothing;
    public bool TargetHasClothing;
    public bool ErpAllowed;
}


[NetSerializable, Serializable]
public sealed class AddLoveMessage : EuiMessageBase
{
    public NetEntity User;
    public NetEntity Target;
    public int Percent;

    public AddLoveMessage(NetEntity user, NetEntity target, int percent)
    {
        User = user;
        Target = target;
        Percent = percent;
    }
}

[NetSerializable, Serializable]
public sealed class RequestInteractionState : EuiMessageBase
{
    private NetEntity _netEntity;
    private NetEntity _value;

    public RequestInteractionState(NetEntity netEntity, NetEntity value)
    {
        _netEntity = netEntity;
        _value = value;
    }
}

[NetSerializable, Serializable]
public sealed class ResponseLoveMessage : EuiMessageBase
{
    public float Percent;

    public ResponseLoveMessage(float percent)
    {
        Percent = percent;
    }
}

[Serializable, NetSerializable]
public sealed class ResponseInteractionState : EuiMessageBase
{
    public Sex UserSex;
    public Sex TargetSex;
    public bool UserHasClothing;
    public bool TargetHasClothing;
    public bool ErpAllowed;
    public HashSet<string> UserTags;
    public HashSet<string> TargetTags;
    public float UserLovePercent;

    public ResponseInteractionState(Sex userSex, Sex targetSex, bool userHasClothing, bool targetHasClothing, bool erp, HashSet<string> userTags, HashSet<string> targetTags, float userLovePercent)
    {
        UserSex = userSex;
        TargetSex = targetSex;
        UserHasClothing = userHasClothing;
        TargetHasClothing = targetHasClothing;
        ErpAllowed = erp;
        UserTags = userTags;
        TargetTags = targetTags;
        UserLovePercent = userLovePercent;
    }
}
