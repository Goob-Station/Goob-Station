using Content.Shared.Slippery;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Goobstation.Wizard.SlipOnCollide;

public sealed class SlipOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SlipperySystem _slippery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipOnCollideComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<SlipOnCollideComponent> ent, ref StartCollideEvent args)
    {
        var (uid, comp) = ent;

        if (!_slippery.CanSlip(uid, args.OtherEntity))
            return;

        if (!TryComp(uid, out SlipperyComponent? slippery))
            return;

        _slippery.TrySlip(uid, slippery, args.OtherEntity, force: comp.Force, predicted: false);
    }
}
