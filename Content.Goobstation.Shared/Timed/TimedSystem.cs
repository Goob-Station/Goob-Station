using Content.Goobstation.Common.Magic;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Shared.Timed;

public sealed partial class TimedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimedDespawnComponent, BeforeMindSwappedEvent>(OnBeforeMindSwapped);
    }

    private void OnBeforeMindSwapped(Entity<TimedDespawnComponent> ent, ref BeforeMindSwappedEvent args)
    {
        args.Message = "temporary";
        args.Cancelled = true;
    }
}
