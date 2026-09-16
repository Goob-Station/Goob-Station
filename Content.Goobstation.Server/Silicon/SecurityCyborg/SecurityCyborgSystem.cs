using Content.Goobstation.Shared.Silicon.SecurityCyborg;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Silicon.SecurityCyborg;

public sealed class SecurityCyborgSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SecurityCyborgComponent, SECBorgSelfDestructEvent>(OnSelfDestruct);
    }

    private void OnSelfDestruct(Entity<SecurityCyborgComponent> ent, ref SECBorgSelfDestructEvent args)
    {
        args.Handled = true;

        if (TryComp<BorgChassisComponent>(ent, out var chassis) &&
            chassis.BrainContainer.ContainedEntity is { } brain)
        {
            _container.Remove(brain, chassis.BrainContainer);
            _throwing.TryThrow(brain, _random.NextVector2() * 5, 5f);
        }

        var comp = ent.Comp;
        _explosion.QueueExplosion(
            ent,
            comp.ExplosionType,
            comp.TotalIntensity,
            comp.Slope,
            comp.MaxTileIntensity,
            canCreateVacuum: false);

        _popup.PopupEntity(Loc.GetString("sec-borg-self-destruct-popup", ("chassis", Name(ent))), ent, PopupType.LargeCaution);

        QueueDel(ent);
    }
}