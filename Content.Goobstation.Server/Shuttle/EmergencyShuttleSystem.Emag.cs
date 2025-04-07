// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emag;
using Content.Server.Administration.Logs;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Charges.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;

namespace Content.Goobstation.Server.Shuttle;

public sealed class GoobEmergencyShuttleSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _logger = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedChargesSystem _charge = default!;
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emerg = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EmergencyShuttleConsoleComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<EmergencyShuttleConsoleComponent, EmergencyShuttleConsoleEmagDoAfterEvent>(OnEmagDoAfter);
    }

    private void OnEmagDoAfter(Entity<EmergencyShuttleConsoleComponent> ent,
        ref EmergencyShuttleConsoleEmagDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!_emerg.EarlyLaunch())
            return;

        _logger.Add(LogType.Emag,
            LogImpact.High,
            $"{ToPrettyString(args.User):player} emagged shuttle console for early launch");

        EnsureComp<EmaggedComponent>(ent);

        if (args.Used != null)
            _charge.UseCharge(args.Used.Value);
    }

    private void OnEmagged(EntityUid uid, EmergencyShuttleConsoleComponent component, ref GotEmaggedEvent args)
    {
        if (_emerg.EarlyLaunchAuthorized || !_emerg.EmergencyShuttleArrived || _emerg.ConsoleAccumulator <= _emerg.AuthorizeTime)
            return;

        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        args.Handled = false;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            args.UserUid,
            component.EmagTime,
            new EmergencyShuttleConsoleEmagDoAfterEvent(),
            uid,
            uid,
            args.EmagUid)
        {
            DistanceThreshold = 1.5f,
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
}