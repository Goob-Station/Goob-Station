using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Polymorph.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerTrespassSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerTrespassEvent>(OnTrespass);
    }

    private void OnTrespass(Entity<BloodsuckerComponent> ent, ref BloodsuckerTrespassEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerTrespassComponent? comp))
            return;

        if (!TryUseCosts(ent, comp))
            return;

        var mist = _polymorph.PolymorphEntity(ent.Owner, comp.MistPolymorph);
        if (mist == null)
            return;

        _audio.PlayPvs(comp.Sound, mist.Value);
        args.Handled = true;
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerTrespassComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        return true;
    }
}
