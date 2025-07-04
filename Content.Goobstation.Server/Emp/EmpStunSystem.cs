// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Emp;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Server.Emp;

public sealed class EmpStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, EmpPulseEvent>(OnEmpParalyze);
        SubscribeLocalEvent<BorgChassisComponent, EmpPulseEvent>(OnEmpParalyze);
    }

    private void OnEmpParalyze(EntityUid uid, Component component, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        var duration = args.Duration;
        if (duration > TimeSpan.FromSeconds(15))
            duration = TimeSpan.FromSeconds(15);
        _stun.TryParalyze(uid, duration, true);
    }
}