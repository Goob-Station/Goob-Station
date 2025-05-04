using Content.Shared._CorvaxNext.Silicons.Borgs.Components;
using Content.Shared.Actions;
using Content.Shared.Emag.Systems;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Species;
using Content.Shared.StationAi;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxNext.Silicons.Borgs
{
    public abstract partial class SharedAiRemoteControlSystem : EntitySystem
    {

        [Dependency] private readonly SharedStationAiSystem _stationAiSystem = default!;
        [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;

        public override void Initialize()
        {
            base.Initialize();
        }
        public void ReturnMindIntoAi(Entity<AiRemoteControllerComponent> entity, ref ReturnMindIntoAiEvent args)
        {
            if (entity.Comp.AiHolder == null || !_stationAiSystem.TryGetCore(entity.Comp.AiHolder.Value, out var stationAiCore) || stationAiCore.Comp?.RemoteEntity == null)
                return;

            if (entity.Comp.LinkedMind == null)
                return;

            if (!TryComp<StationAiHeldComponent>(entity.Comp.AiHolder, out var stationAiHeldComp))
                return;

            stationAiHeldComp.CurrentConnectedEntity = null;

            _mind.TransferTo(entity.Comp.LinkedMind.Value, entity.Comp.AiHolder);

            _stationAiSystem.SwitchRemoteEntityMode(stationAiCore, true);
            entity.Comp.AiHolder = null;
            entity.Comp.LinkedMind = null;

            _xformSystem.SetCoordinates(stationAiCore.Comp.RemoteEntity.Value, Transform(entity).Coordinates);
        }

        public sealed partial class ReturnMindIntoAiEvent : InstantActionEvent
        {

        }
        public sealed partial class ToggleRemoteDevicesScreenEvent : InstantActionEvent
        {

        }

        [Serializable, NetSerializable]
        public enum RemoteDeviceUiKey : byte
        {
            Key
        }
    }
}
