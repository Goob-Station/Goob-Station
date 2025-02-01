using Content.Shared.Weapons.Melee.Events;
using Content.Server.Body.Systems;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Weapon;

/// <summary>
///
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
        foreach (var hit in args.HitEntities)
        {
            if (IsInList(uid, Name(hit), component.IcNames))
                _bodySystem.GibBody(hit);

            if (TryComp<ActorComponent>(hit, out var actor) && IsInList(uid, actor.PlayerSession.Name, component.OcNames))
                _bodySystem.GibBody(hit);
        }
    }
    private bool IsInList(EntityUid uid, string target, List<string> names)
    {
        foreach (var name in names)
            if (name == target)
                return true;
        return false;
    }
}
