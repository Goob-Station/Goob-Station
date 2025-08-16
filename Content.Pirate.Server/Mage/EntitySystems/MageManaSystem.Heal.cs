using Content.Server.Body.Systems;
using Content.Server.Magic;
using Content.Shared.Movement.Pulling.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageHealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damaging = default!;
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

        SubscribeLocalEvent<MageHealSpellEvent>(OnSwapSpell);
    }

    /// <summary>
    ///     Teleports the user to the clicked location
    /// </summary>
    /// <param name="args"></param>
    private void OnSwapSpell(MageHealSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(args.Performer)) // Inside an entity storage
            return;

        if (args.Handled)
            return;
        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;

        var healBlunt = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Blunt"), -30);
        var healSlash = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Slash"), -30);
        var healPierce = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Piercing"), -30);
        var healHeat = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Heat"), -30);
        var healCold = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Cold"), -30);
        var healPoison = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Poison"), -30);
        var healRadiation = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Radiation"), -30);
        var healAsphyxiation = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Asphyxiation"), -30);
        var healBloodloss = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Bloodloss"), -30);
        var healCell = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Cellular"), -30);
        _damaging.TryChangeDamage(args.Target, healBlunt);
        _damaging.TryChangeDamage(args.Target, healSlash);
        _damaging.TryChangeDamage(args.Target, healPierce);
        _damaging.TryChangeDamage(args.Target, healHeat);
        _damaging.TryChangeDamage(args.Target, healCold);
        _damaging.TryChangeDamage(args.Target, healPoison);
        _damaging.TryChangeDamage(args.Target, healRadiation);
        _damaging.TryChangeDamage(args.Target, healBloodloss);
        _damaging.TryChangeDamage(args.Target, healAsphyxiation);
        _damaging.TryChangeDamage(args.Target, healCell);
        _bloodstream.TryModifyBleedAmount(args.Target, -100);
        //_magic.Speak(args);

        args.Handled = true;
    }
}
