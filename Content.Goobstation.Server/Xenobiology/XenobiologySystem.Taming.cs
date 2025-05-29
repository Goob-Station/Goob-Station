using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles slime taming, likely to be expanded in the future.
/// </summary>
public partial class XenobiologySystem
{
    private readonly EntProtoId _tameEffects = "EffectHearts"; // get this out of here

    private void InitializeTaming()
    {
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
