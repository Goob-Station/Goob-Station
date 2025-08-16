using Content.Server.Magic;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
// using Content.Server.Pulling;
// using Content.Shared.Pulling.Components;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageDashSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageDashSpellEvent>(OnDashSpell);
    }

    /// <summary>
    ///     Teleports the user to the clicked location
    /// </summary>
    /// <param name="args"></param>
    private void OnDashSpell(MageDashSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(args.Performer)) // Inside an entity storage
            return;

        var transform = Transform(args.Performer);
        // Must be on the same map
        if (transform.MapID != args.Target.GetMapId(EntityManager))
            return;

        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;

        PullableComponent? pullable = null; // To avoid "might not be initialized when accessed" warning
        if (_entity.TryGetComponent<PullerComponent>(args.Performer, out var puller) &&
            puller.Pulling != null &&
            _entity.TryGetComponent(puller.Pulling, out pullable) &&
            pullable.BeingPulled)

            // Temporarily stop pulling to avoid not teleporting fully to the target
        {
            if (puller.Pulling != null)
            {
                // Assuming you have the PullableComponent from earlier checks or retrieval
                if (_entity.TryGetComponent(puller.Pulling.Value, out PullableComponent? pullableComponent))
                {
                    _pulling.TryStopPull(puller.Pulling.Value, pullableComponent, args.Performer);
                }
            }
        }

        // Teleport the performer to the target
        _transform.SetCoordinates(args.Performer, args.Target);
        _transform.AttachToGridOrMap(args.Performer, transform);

        if (pullable != null && puller != null && pullable.Puller != null && puller.Pulling != null)
        {
            // Get transform of the pulled entity
            var pulledTransform = Transform(pullable.Owner);

            // Teleport the pulled entity to the target
            // TODO: Relative position to the performer
            _transform.SetCoordinates(pullable.Owner, args.Target);
            _transform.AttachToGridOrMap(args.Performer, transform);

            // Resume pulling
            // TODO: This does nothing? // This does things sometimes, but the client never knows
            _pulling.TryStartPull(puller.Pulling.Value, pullable.Puller.Value);
        }

        // Play the teleport sound
        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        // Take power and deal stamina damage
        // _mana.TryAddPowerLevel(comp.Owner, -args.ManaCost);
        // _stamina.TakeStaminaDamage(args.Performer, args.StaminaCost);
        // _mana.TryUseAbility(args.Performer, comp, args.ManaCost);

        // Speak
        //_magic.Speak(args);

        args.Handled = true;
    }
}
