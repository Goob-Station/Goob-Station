using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed partial class RalliedSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RalliedComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        //TO DO: Buff attack and attack speed of mob so long as they have this component.

        var query = EntityQueryEnumerator<RalliedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Not time yet
            if (curTime < comp.NextTick)
                continue;

            //TO DO: Revert mob values back to original values.
            _popup.PopupPredicted("The rally effect wears off.", uid, uid);
            RemComp<RalliedComponent>(uid);
        }
    }

    private void OnMapInit(Entity<RalliedComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.RalliedDuration;
        Dirty(ent);
    }

}
