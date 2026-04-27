using Content.Goobstation.Shared.Communications;
using Content.Server.Power.Components;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Communications;

public sealed class TelecomTransmitterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecomConnectionOverrideEvent>(OnOverride);
    }

    private void OnOverride(ref TelecomConnectionOverrideEvent ev)
    {
        ev.Cancelled = !HasTransmitterConnection(ev.SourceMap, ev.TargetMap);
    }

    public bool HasTransmitterConnection(MapId firstMapId, MapId secondMapId)
    {
        bool hasFirstMap = false;
        bool hasSecondMap = false;
        var query = EntityQueryEnumerator<TelecomTransmitterComponent, ApcPowerReceiverComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var power, out var xform))
        {
            // TODO add something with device networks
            if (xform.MapID == firstMapId && power.Powered)
                hasFirstMap = true;

            if (xform.MapID == secondMapId && power.Powered)
                hasSecondMap = true;
        }

        return hasFirstMap && hasSecondMap;
    }
}
