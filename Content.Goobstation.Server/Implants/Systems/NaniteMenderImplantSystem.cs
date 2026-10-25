// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Implants;
using Content.Server.Administration.Systems;
using Content.Server.Jittering;
using Content.Server.Popups;
using Content.Shared.Administration.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed partial class NaniteMenderImplantSystem : EntitySystem
{
    [Dependency] private RejuvenateSystem _rejuvenate = default!;
    [Dependency] private JitteringSystem _jittering = default!;
    [Dependency] private PopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NaniteMendEvent>(OnNaniteMend);
    }
    private void OnNaniteMend(NaniteMendEvent args)
    {
        var popup = Loc.GetString("nanite-mend-popup");
        _popup.PopupEntity(popup, args.Target, args.Target, PopupType.Medium);

        _jittering.AddJitter(args.Target);
        _rejuvenate.PerformRejuvenate(args.Target);
    }

}
