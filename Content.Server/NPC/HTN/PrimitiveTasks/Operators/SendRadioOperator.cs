using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class SendRadioOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private RadioSystem _radio = default!;
    private NavMapSystem _navMap = default!;

    [DataField(required:true)]
    public LocId Message;

    [DataField(required: true)]
    public ProtoId<RadioChannelPrototype> RadioChannel = default!;

    [DataField]
    public string Key = string.Empty;

    [DataField]
    public bool KeyIsEntity = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);

        _radio = sysManager.GetEntitySystem<RadioSystem>();
        _navMap = sysManager.GetEntitySystem<NavMapSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var message = string.Empty;

        if (KeyIsEntity)
        {
            if (!blackboard.TryGetValue<EntityUid>(Key, out var value, _entManager) || _entManager.Deleted(value))
                return HTNOperatorStatus.Failed;

            var xform = _entManager.GetComponent<TransformComponent>(value);
            var location = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((value, xform)));

            message = Loc.GetString(Message, ("entity", Identity.Entity(value, _entManager)), ("coords", $"X: {MathF.Round(xform.Coordinates.X)}, Y: {MathF.Round(xform.Coordinates.Y)}"), ("location", location));
        }
        else
        {
            if (!blackboard.TryGetValue<object>(Key, out var value, _entManager))
                return HTNOperatorStatus.Failed;

            message = Loc.GetString(Message, ("key", value));
        }

        var speaker = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        _radio.SendRadioMessage(speaker, message, RadioChannel, speaker, escapeMarkup: false);
        return HTNOperatorStatus.Finished;
    }
}
