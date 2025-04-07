using Content.Server.Body.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Weapons;

/// <summary>
/// Gib this Person
/// </summary>
public sealed class GibThisGuySystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GibThisGuyComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public void OnMeleeHit(EntityUid uid, GibThisGuyComponent component, MeleeHitEvent args)
    {
        if (component.RequireBoth)
        {
            foreach (var hit in args.HitEntities)
                if (component.IcNames.Contains(Name(hit)) &&
                    TryComp<ActorComponent>(hit, out var actor) &&
                    component.OcNames.Contains(actor.PlayerSession.Name))
                    _bodySystem.GibBody(hit);
            return;
        }
        foreach (var hit in args.HitEntities)
        {
            if (component.IcNames.Contains(Name(hit)))
                _bodySystem.GibBody(hit);

            if (TryComp<ActorComponent>(hit, out var actor) &&
                component.OcNames.Contains(actor.PlayerSession.Name))
                _bodySystem.GibBody(hit);
        }
    }
}
