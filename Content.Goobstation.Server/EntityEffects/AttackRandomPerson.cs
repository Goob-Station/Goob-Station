using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Weapons.Melee;
using Content.Shared.EntityEffects;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.EntityEffects;

[UsedImplicitly]
public sealed partial class AttackRandomPerson : EntityEffect
{
    [DataField]
    public bool AttackIfNone = false;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-random-attack", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var lookup = args.EntityManager.System<EntityLookupSystem>();
        var hands = args.EntityManager.System<HandsSystem>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var uid = args.TargetEntity;
        var entMan = args.EntityManager;

        // First we ensure that we actually have a weapon or can attack
        MeleeWeaponComponent? melee = null;

        if (hands.TryGetActiveItem(uid, out var item) && !entMan.TryGetComponent(item, out melee))
            return;

        if (item == null && !entMan.TryGetComponent(uid, out melee))
            return;

        if (melee == null)
            return;

        // Getting target
        var xform = entMan.GetComponent<TransformComponent>(uid);
        NetEntity? target = null;
        NetCoordinates targetCoords = new();
        var ents = lookup.GetEntitiesInRange<MobStateComponent>(xform.Coordinates, 1f).ToList();

        if (ents.Count > 0)
        {
            var targetEnt = random.Pick(ents);
            target = entMan.GetNetEntity(targetEnt);
            targetCoords = entMan.GetNetCoordinates(entMan.GetComponent<TransformComponent>(targetEnt).Coordinates);
        }
        else if (AttackIfNone)
        {
            var coords = new EntityCoordinates(xform.Coordinates.EntityId, xform.Coordinates.Position + random.NextVector2(-1, 1));
            targetCoords = entMan.GetNetCoordinates(coords);
        }
        else
        {
            return;
        }

        // Event setup and attacking
        LightAttackEvent ev = new(target, entMan.GetNetEntity(item ?? uid), targetCoords);
        args.EntityManager.System<MeleeWeaponSystem>().DoLightAttack(uid, ev, item ?? uid, melee, null, false);
    }
}
