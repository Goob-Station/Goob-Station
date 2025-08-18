using Content.Pirate.Server.Mage.Components;
using Content.Server.Electrocution;
using Content.Server.Lightning;
using Content.Server.Magic;
using Content.Shared.Movement.Pulling.Systems;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
// using Content.Server.Pulling;
// using Content.Shared.Pulling.Components;


namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageLightningSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageLightningSpellEvent>(OnLightningSpell);
    }


    private void OnLightningSpell(MageLightningSpellEvent args)
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


        var range = args.MaxElectrocutionRange;
        var i = 0;
        // var xform = Transform(uid);
        foreach (var (ent, component) in _lookup.GetEntitiesInRange<MobStateComponent>(coords, range))
        {
            i++;
            if (ent != args.Performer && i <= 3)
                _lightning.ShootLightning(args.Performer, ent);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MageLightningComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var elec, out var xform))
        {
            if (_timing.CurTime < elec.NextSecond)
                continue;
            elec.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);

            if (!_random.Prob(elec.PassiveElectrocutionChance))
                continue;

            var range = elec.MaxElectrocuteRange;
            var damage = (int) elec.MaxElectrocuteDamage;
            var duration = elec.MaxElectrocuteDuration;
            var i = 0;
            foreach (var (ent, comp) in _lookup.GetEntitiesInRange<StatusEffectsComponent>(xform.MapPosition, range))
            {
                i++;
                if (i <= 3)
                {
                    _electrocution.TryDoElectrocution(ent, uid, damage, duration, true, statusEffects: comp,
                        ignoreInsulation: false);
                }
            }
        }
    }
}
