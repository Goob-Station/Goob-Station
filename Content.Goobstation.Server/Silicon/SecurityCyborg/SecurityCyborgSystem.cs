using Content.Goobstation.Shared.Silicon.SecurityCyborg;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Silicon.SecurityCyborg;

public sealed partial class SecurityCyborgSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedExplosionSystem _explosion = default!;
    [Dependency] private IRobustRandom _random = default!;

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

        _popup.PopupEntity(Loc.GetString("sec-borg-self-destruct-popup", ("chassis", Name(ent))), ent, PopupType.LargeCaution);

        _explosion.TriggerExplosive(ent);
    }
}
