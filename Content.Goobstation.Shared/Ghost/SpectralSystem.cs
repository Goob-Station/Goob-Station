using Content.Goobstation.Common.Magic;
using Content.Shared.Ghost;

namespace Content.Goobstation.Shared.Ghost;

public sealed partial class SpectralSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpectralComponent, BeforeMindSwappedEvent>(OnBeforeMindSwapped);
    }

    private void OnBeforeMindSwapped(Entity<SpectralComponent> ent, ref BeforeMindSwappedEvent args)
    {
        args.Message = ent.Comp.MindswapText;
        args.Cancelled = true;
    }
}
