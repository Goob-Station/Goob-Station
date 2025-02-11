using Content.Shared.DoAfter;
using Content.Shared.GridPreloader.Systems;
using Content.Shared.Interaction.Events;

namespace Content.Shared._Lavaland.Salvage;

public abstract class SharedShelterCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, UseInHandEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, ShelterCapsuleComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.DeployTime, new ShelterCapsuleDeployDoAfterEvent(), uid, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }
}
