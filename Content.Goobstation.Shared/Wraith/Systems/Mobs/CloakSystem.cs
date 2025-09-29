using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class CloakSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloakComponent, CloakEvent>(OnCloak);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CloakComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsActive)
                continue;

            if (_timing.CurTime >= comp.EndTime)
            {
                if (TryComp<StealthComponent>(uid, out var stealth))
                    _stealth.SetVisibility(uid, 1f, stealth);

                RemComp<StealthComponent>(uid);
                comp.IsActive = false;
            }
        }
    }
    //TO DO: Cloak should break if the hound leaps.
    public void OnCloak(Entity<CloakComponent> ent, ref CloakEvent args)
    {
        if (args.Handled)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, 0.3f, stealth);

        comp.IsActive = true;
        comp.EndTime = _timing.CurTime + comp.CloakDuration;

        args.Handled = true;
    }
}
