using Content.Shared.Body.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModInjectModuleSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    [SubscribeLocalEvent]
    private void OnUsed(Entity<ModInjectModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        args.Handled = true;
        _audio.PlayPredicted(ent.Comp.Sound, args.User, args.User);

        if (_net.IsClient)
            return;

        if (ent.Comp.ClearStuns)
        {
            _stun.TryUnstun(args.User);
            _stun.TryStanding(args.User);
        }

        _bloodstream.TryAddToBloodstream(args.User, ent.Comp.Solution.Clone());
    }
}
