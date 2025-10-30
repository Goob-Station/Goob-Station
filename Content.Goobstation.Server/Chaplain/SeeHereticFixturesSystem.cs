// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Chaplain.Components;
using Content.Shared.Eye;

namespace Content.Goobstation.Server.Chaplain;

public sealed class SeeHereticFixturesSystem : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private const int ChaplainVisFlags = (int) VisibilityFlags.EldritchInfluence;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeeHereticFixturesComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SeeHereticFixturesComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SeeHereticFixturesComponent, GetVisMaskEvent>(OnGetVisMask);
    }

    private void OnStartup(EntityUid uid, SeeHereticFixturesComponent component, ComponentStartup args)
    {
        if (TryComp<EyeComponent>(uid, out var eye))
            _eye.SetVisibilityMask(uid, eye.VisibilityMask | ChaplainVisFlags, eye);
    }

    private void OnShutdown(EntityUid uid, SeeHereticFixturesComponent component, ComponentShutdown args)
    {
        if (TryComp<EyeComponent>(uid, out var eye))
            _eye.SetVisibilityMask(uid, eye.VisibilityMask & ~ChaplainVisFlags, eye);
    }

    private void OnGetVisMask(EntityUid uid, SeeHereticFixturesComponent component, ref GetVisMaskEvent args)
    {
        if (component.SeeShifts)
            args.VisibilityMask |= (int) VisibilityFlags.EldritchInfluence;

        if (component.SeeFractures)
            args.VisibilityMask |= (int) VisibilityFlags.EldritchInfluenceSpent;
    }
}
