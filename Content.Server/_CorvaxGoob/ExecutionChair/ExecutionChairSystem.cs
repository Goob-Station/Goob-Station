using Content.Shared.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Electrocution;
using Content.Server.Power.EntitySystems;
using Content.Shared.Buckle.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxNext.ExecutionChair
{
    public sealed partial class ExecutionChairSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTimer = default!;
        [Dependency] private readonly IRobustRandom _randomGen = default!;
        [Dependency] private readonly DeviceLinkSystem _deviceSystem = default!;
        [Dependency] private readonly ElectrocutionSystem _shockSystem = default!;
        [Dependency] private readonly SharedAudioSystem _soundSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        private ISawmill _sawmill = default!;

        private const float VolumeVariationMin = 0.8f;
        private const float VolumeVariationMax = 1.2f;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ExecutionChairComponent, MapInitEvent>(OnChairSpawned);
            SubscribeLocalEvent<ExecutionChairComponent, SignalReceivedEvent>(OnSignalReceived);

            _sawmill = Logger.GetSawmill("execution_chair");
        }

        private void OnChairSpawned(EntityUid uid, ExecutionChairComponent component, ref MapInitEvent args)
        {
            _deviceSystem.EnsureSinkPorts(uid, component.TogglePort, component.OnPort, component.OffPort);
        }

        private void OnSignalReceived(EntityUid uid, ExecutionChairComponent component, ref SignalReceivedEvent args)
        {
            // default case for switch below
            bool DefaultCase(EntityUid uid, string port, ExecutionChairComponent component)
            {
                _sawmill.Debug($"Receieved unexpected port signal: {port} on chair {ToPrettyString(uid)}");
                return component.Enabled;
            }

            var newState = args.Port switch
            {
                var p when p == component.TogglePort => !component.Enabled,
                var p when p == component.OnPort => true,
                var p when p == component.OffPort => false,
                _ => DefaultCase(uid, args.Port, component)
            };

            UpdateChairState(uid, newState, component);
        }

        private void UpdateChairState(EntityUid uid, bool activated, ExecutionChairComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.Enabled = activated;
            Dirty(uid, component);
            var message = activated
                ? Loc.GetString("execution-chair-turn-on")
                : Loc.GetString("execution-chair-chair-turn-off");

            _popup.PopupEntity(message, uid, PopupType.Medium);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<ExecutionChairComponent>();

            while (query.MoveNext(out var uid, out var chair))
            {
                if (!ValidateChairOperation(uid, chair))
                    continue;

                if (!TryComp<StrapComponent>(uid, out var restraint) || restraint.BuckledEntities.Count == 0)
                    continue;

                ApplyShockEffect(uid, chair, restraint);
            }
        }

        /// <summary>
        /// Ensures that the chair is in a valid state to operate:
        ///  - The chair is anchored in the world (not picked up or moved).
        ///  - The chair is powered.
        ///  - The chair is currently enabled/turned on.
        ///  - The current game time has passed beyond the next scheduled damage tick.
        /// </summary>
        private bool ValidateChairOperation(EntityUid uid, ExecutionChairComponent chair)
        {
            var transformComponent = Transform(uid);
            return transformComponent.Anchored &&
                   this.IsPowered(uid, EntityManager) &&
                   chair.Enabled &&
                   _gameTimer.CurTime >= chair.NextDamageTick;
        }

        private void ApplyShockEffect(EntityUid uid, ExecutionChairComponent chair, StrapComponent restraint)
        {
            var shockDuration = TimeSpan.FromSeconds(chair.DamageTime);

            foreach (var target in restraint.BuckledEntities)
            {
                var volumeModifier = _randomGen.NextFloat(VolumeVariationMin, VolumeVariationMax);

                var shockSuccess = _shockSystem.TryDoElectrocution(
                    target,
                    uid,
                    chair.DamagePerTick,
                    shockDuration,
                    true,
                    volumeModifier,
                    ignoreInsulation: true
                );

                if (shockSuccess && chair.PlaySoundOnShock && chair.ShockNoises != null)
                {
                    var audioParams = AudioParams.Default.WithVolume(chair.ShockVolume);
                    _soundSystem.PlayPvs(chair.ShockNoises, target, audioParams);
                }
            }

            chair.NextDamageTick = _gameTimer.CurTime + TimeSpan.FromSeconds(1);
        }
    }
}
