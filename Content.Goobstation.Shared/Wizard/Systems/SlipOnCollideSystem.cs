// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared.Slippery;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wizard.Systems;

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

        _slippery.TrySlip(uid, slippery, args.OtherEntity, predicted: false);
    }
}