using Content.Goobstation.Shared.Blinking;
using Content.Goobstation.Shared.Breathing;
using Content.Server.Body.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Evil;

public sealed class EvilSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        var uid = args.Entity;

        EnsureComp<BlinkingComponent>(uid);

        if (HasComp<RespiratorComponent>(uid))
            EnsureComp<ManualBreathingComponent>(uid);
    }
}
