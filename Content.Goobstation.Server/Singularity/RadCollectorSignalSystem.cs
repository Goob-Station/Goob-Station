// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Content.Shared.Singularity.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Singularity;

public sealed class RadCollectorSignalSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly DeviceLinkSystem _device = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public static readonly ProtoId<SourcePortPrototype> EmptyPort = "RadEmpty";
    public static readonly ProtoId<SourcePortPrototype> LowPort = "RadLow";
    public static readonly ProtoId<SourcePortPrototype> FullPort = "RadFull";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RadCollectorSignalComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_automation.IsAutomated(uid))
                continue;

            var ent = (uid, comp);
            _appearance.TryGetData<int>(uid, RadiationCollectorVisuals.PressureState, out var rawState);
            var state = rawState switch
            {
                3 => RadCollectorState.Full,
                2 => RadCollectorState.Low,
                _ => RadCollectorState.Empty
            };

            // nothing changed
            if (comp.LastState == state)
                continue;

            _device.SendSignal(uid, GetPort(comp.LastState), false);
            comp.LastState = state;
            _device.SendSignal(uid, GetPort(state), true);
        }
    }

    private static string GetPort(RadCollectorState state) => state switch
    {
        RadCollectorState.Empty => EmptyPort,
        RadCollectorState.Low => LowPort,
        RadCollectorState.Full => FullPort
    };
}
