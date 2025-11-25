// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Standing;
using Content.Goobstation.Shared.Sprinting;
using Content.Server.Stunnable;
using Content.Shared.CombatMode;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Server.Sprinting;

public sealed class SprintingSystem : SharedSprintingSystem
{

    [Dependency] private readonly StunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SprinterComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, SprinterComponent sprinter, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (uid.Id < otherUid.Id)
            return;

        if (!sprinter.IsSprinting)
        {
            return;
        }

        if (!TryComp(otherUid, out SprinterComponent? otherSprinter) || !otherSprinter.IsSprinting)
        {
            return;
        }

        _stunSystem.TryKnockdown(uid, sprinter.KnockdownDurationOnInterrupt, false, DropHeldItemsBehavior.NoDrop);
        _stunSystem.TryKnockdown(otherUid,
            otherSprinter.KnockdownDurationOnInterrupt,
            false,
            DropHeldItemsBehavior.NoDrop);
    }
}
