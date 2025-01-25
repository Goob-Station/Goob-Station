using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;

namespace Content.Server.Heretic.EntitySystems.PathSpecific;

public sealed partial class ChampionStanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChampionStanceComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<ChampionStanceComponent, TakeStaminaDamageEvent>(OnTakeStaminaDamage);

        // if anyone is reading through and does not have EE newmed you can remove these handlers
        SubscribeLocalEvent<ChampionStanceComponent, BodyPartAttachedEvent>(OnBodyPartAttached);
        SubscribeLocalEvent<ChampionStanceComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
    }

    private bool Condition(Entity<ChampionStanceComponent> ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var dmg)
        || dmg.TotalDamage < 50f) // taken that humanoids have 100 damage before critting
            return false;
        return true;
    }

    private void OnDamageModify(Entity<ChampionStanceComponent> ent, ref DamageModifyEvent args)
    {
        if (!Condition(ent))
            return;

        args.Damage = args.OriginalDamage / 2f;
    }

    private void OnTakeStaminaDamage(Entity<ChampionStanceComponent> ent, ref TakeStaminaDamageEvent args)
    {
        if (!Condition(ent))
            return;

        args.Multiplier /= 2.5f;
    }

    private void OnBodyPartAttached(Entity<ChampionStanceComponent> ent, ref BodyPartAttachedEvent args)
    {
        // can't touch this
        args.Part.Comp.CanSever = false;
    }
    private void OnBodyPartRemoved(Entity<ChampionStanceComponent> ent, ref BodyPartRemovedEvent args)
    {
        // can touch this
        args.Part.Comp.CanSever = true;
    }
}
