using Content.Shared.Trigger;

namespace Content.Shared._ES.Camera.Trigger;

public sealed partial class ESScreenshakeUserOnTriggerSystem : XOnTriggerSystem<ESScreenshakeUserOnTriggerComponent>
{
    [Dependency] private ESScreenshakeSystem _screenShake = default!;

    protected override void OnTrigger(Entity<ESScreenshakeUserOnTriggerComponent> ent, EntityUid target, ref TriggerEvent args)
    {
        if (args.User == null)
            return;

        _screenShake.Screenshake(args.User.Value, ent.Comp.Translation, ent.Comp.Rotation);
        args.Handled = true;
    }
}
