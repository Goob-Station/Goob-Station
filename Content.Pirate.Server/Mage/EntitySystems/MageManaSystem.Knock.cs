using Content.Server.Doors.Systems;
using Content.Server.Magic;
using Content.Shared.Movement.Pulling.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Doors.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
// using Content.Server.Pulling;
// using Content.Shared.Pulling.Components;


namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageKnockSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DoorSystem _doorSystem = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    // [Dependency] private readonly DoorBoltSystem _boltsSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageKnockSpellEvent>(OnKnockSpell);
    }

    /// <summary>
    ///     Opens all doors within range
    /// </summary>
    /// <param name="args"></param>
    private void OnKnockSpell(MageKnockSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs)) // Inside an entity storage
            return;

        if (args.Handled)
            return;
        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;

        args.Handled = true;
        //_magic.Speak(args);

        // Take power and deal stamina damage
        // _mana.TryAddPowerLevel(comp.Owner, -args.ManaCost);

        //Get the position of the player
        var transform = Transform(args.Performer);
        var coords = transform.Coordinates;

        _audio.PlayPvs(args.KnockSound, args.Performer, AudioParams.Default.WithVolume(args.KnockVolume));

        //Look for doors and don't open them if they're already open.
        foreach (var entity in _lookup.GetEntitiesInRange(coords, args.Range))
        {
            if (TryComp<DoorBoltComponent>(entity, out var bolts))
                _doorSystem.SetBoltsDown((entity, bolts), false);

            if (TryComp<DoorComponent>(entity, out var doorComp) && doorComp.State is not DoorState.Open)
                _doorSystem.StartOpening(entity);
        }
    }
}
