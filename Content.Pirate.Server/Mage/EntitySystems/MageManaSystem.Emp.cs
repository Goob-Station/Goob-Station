using Content.Server.Emp;
using Content.Server.Magic;
using Content.Shared.Movement.Pulling.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
// using Content.Server.Pulling;
// using Content.Shared.Pulling.Components;


namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageEmpSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
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
        // SubscribeLocalEvent<MageTeleportManaComponent, ComponentStartup>(Startup);
        // SubscribeLocalEvent<MageTeleportManaComponent, ComponentShutdown>(Shutdown);

        // SubscribeLocalEvent<MageTeleportManaComponent, MageTeleportSpellEvent>(OnTeleportSpell);

        SubscribeLocalEvent<MageEmpSpellEvent>(OnEmp);
    }

    /// <summary>
    ///     Teleports the user to the clicked location
    /// </summary>
    /// <param name="args"></param>
    private void OnEmp(MageEmpSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(args.Performer)) // Inside an entity storage
            return;

        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;


        var coords = Transform(args.Performer).MapPosition;
        _emp.EmpPulse(coords, args.EmpRange, args.EmpConsumption, args.EmpDuration);

        // _mana.TryAddPowerLevel(comp.Owner, -args.ManaCost);

        args.Handled = true;
    }
}
