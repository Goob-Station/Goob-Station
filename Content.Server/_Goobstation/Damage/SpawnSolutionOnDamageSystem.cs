
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Climbing.Components;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Vehicles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Content.Shared.DragDrop;
using Content.Shared.Emag.Components;
using Content.Shared.Damage;
using Robust.Shared.Analyzers;
using Robust.Shared.Maths;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Damage;

public sealed partial class SpawnSolutionOnDamageSystem : EntitySystem
{
     public override void Initialize()
    {
        SubscribeLocalEvent<SpawnSolutionOnDamageComponent, BeforeDamageChangedEvent>(OnTakeDamage);
    }
    private void OnTakeDamage(Entity<SpawnSolutionOnDamageComponent> ent, ref BeforeDamageChangedEvent args)
    {

        if (!args.Damage.AnyPositive())
            return;

        if (args.Damage.GetTotal() <= ent.Comp.Threshold)
            return; //dont trigger on low damage

        var random = new Random();
        if (random.NextFloat(0f, 100f) > (ent.Comp.Probability))
            return;
        if (ent.Comp.Solution == "unkown")
            return;

        Spawn(ent.Comp.Solution, Transform(ent).Coordinates);
    }
}
