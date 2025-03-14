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
        if (!TryComp(args.User, out ActorComponent? actor) || component.IP == string.Empty)
            return;

        _redial.Redial(actor.PlayerSession.Channel, component.IP);

        args.Handled = true;
    }
}
