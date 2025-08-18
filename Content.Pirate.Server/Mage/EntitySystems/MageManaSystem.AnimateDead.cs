using Content.Server.Body.Systems;
using Content.Server.Magic;
using Content.Server.NPC.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageAnimateDeadSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactionSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageAnimateDeadSpellEvent>(OnAnimateDead);
    }

    /// <summary>
    ///     Teleports the user to the clicked location
    /// </summary>
    /// <param name="args"></param>
    private void OnAnimateDead(MageAnimateDeadSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(args.Performer)) // Inside an entity storage
            return;

        if (args.Handled)
            return;
        // Speak(args);

        var transform = Transform(args.Performer);
        var coords = transform.Coordinates;

        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;

        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));
        foreach (var entity in _lookup.GetEntitiesInRange(coords, args.Range))
        {
            if (!TryComp<MobStateComponent>(entity, out var mobState))
                continue;
            if (_mobStateSystem.IsDead(entity))
            {
                if (!TryComp<BodyComponent>(entity, out var body))
                    return;
                _bodySystem.GibBody(entity, true, body);
                var mobtransform = Transform(entity);
                var mobcoords = mobtransform.MapPosition;
                var mob = Spawn("MobSkeletonAngry", mobcoords);
                _metaData.SetEntityName(mob, "angry skeleton");
                _metaData.SetEntityDescription(mob, "This skeleton is out for blood");
                // _npcFactionSystem.AddFaction(mob, "SimpleHostile");
                _npcFactionSystem.RemoveFaction(mob, "NanoTrasen", false);
                _npcFactionSystem.AddFaction(mob, "Wizard");
                _npcFactionSystem.IgnoreEntity(entity, args.Performer);
                // var comps = _entity.AddComponent<HTNComponent>(mob);
                // comps.RootTask = new HTNCompoundTask()
                // {
                //     Task = "SimpleHostileCompound"
                // };
            }
        }


        // _transformSystem.SetCoordinates(args.Performer, args.Target);
        // transform.AttachToGridOrMap();
        // _audio.PlayPvs(args.BlinkSound, args.Performer, AudioParams.Default.WithVolume(args.BlinkVolume));
        //_magic.Speak(args);

        // _mana.TryAddPowerLevel(comp.Owner, -args.ManaCost);

        args.Handled = true;
    }
}
