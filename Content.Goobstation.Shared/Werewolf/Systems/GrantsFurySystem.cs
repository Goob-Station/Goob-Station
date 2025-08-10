using Content.Goobstation.Shared.Werewolf.Components;
using Content.Shared.Nutrition;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the food that has fury in it.
/// </summary>
public sealed class GrantsFurySystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GrantsFuryComponent, AfterFullyEatenEvent>(OnEaten);
    }

    private void OnEaten(Entity<GrantsFuryComponent> ent, ref AfterFullyEatenEvent args)
    {
        var ev = new EatenFuryEvent(ent.Comp.Fury);
        RaiseLocalEvent(args.User, ref ev);
    }
}
