using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Redial;

public sealed class RedialUserOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly RedialManager _redial = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RedialUserOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(EntityUid uid, RedialUserOnTriggerComponent component, TriggerEvent args)
    {
        if (!TryComp(args.User, out ActorComponent? actor) || component.Address == string.Empty)
            return;

        _redial.Redial(actor.PlayerSession.Channel, component.Address);

        args.Handled = true;
    }
}
