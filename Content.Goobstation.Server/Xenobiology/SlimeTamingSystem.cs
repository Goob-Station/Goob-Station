using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles slime taming, likely to be expanded in the future.
/// </summary>
public sealed class SlimeTamingSystem : EntitySystem
{

    private readonly EntProtoId _tameEffects = "EffectHearts";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, InteractionSuccessEvent>(OnTame);
    }

    private void OnTame(Entity<SlimeComponent> ent, ref InteractionSuccessEvent args)
    {
        var (slime, comp) = ent;
        var coords = Transform(slime).Coordinates;
        var user = args.User;
        if (comp.Tamer.HasValue)
            return;

        Spawn(_tameEffects, coords);
        comp.Tamer = user;
        Dirty(ent);
    }
}
