// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Coordinates;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.Nutrition;
using Content.Goobstation.Shared._Trauma.Ranching.Components;

namespace Content.Goobstation.Shared._Trauma.Ranching.Systems;

/// <summary>
/// Used for those stupid chickens that make me do more work.
/// </summary>
public sealed partial class SpecialEggsSystem : EntitySystem
{
    [Dependency] private SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlateableChickenComponent, InteractUsingEvent>(OnInteract);

        SubscribeLocalEvent<ChickenChestComponent, FullyAteEvent>(OnChanged);
    }

    private void OnChanged(Entity<ChickenChestComponent> ent, ref FullyAteEvent args)
    {
        var foodproto = Prototype(args.Food);

        if (foodproto is null)
            return;

        var coords = Transform(ent).Coordinates;

        // Minecraft crazy craft chicken chest, if you know you know.
        PredictedSpawnAtPosition(foodproto.ID, coords);
        PredictedSpawnAtPosition(foodproto.ID, coords);
    }

    private void OnInteract(Entity<PlateableChickenComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<PlateableChickenOreComponent>(args.Used, out var ore))
            return;

        EntityManager.AddComponents(ent.Owner, ore.Components);
        RemComp(ent, ent.Comp);

        if (_stack.GetCount(args.Used) > 0)
        {
            _stack.ReduceCount(args.Used, 1);
            return;
        }

        Del(args.Used);
    }
}
