// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Timing; // Goobstation

namespace Content.Client.SurveillanceCamera;

public sealed class SurveillanceCameraMonitorSystem : EntitySystem
{
    [Dependency] private readonly IClientGameTiming _gameTiming = default!; // Goobstation

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ActiveSurveillanceCameraMonitorVisualsComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var curTime = _gameTiming.CurTime; // Goobstation
            comp.TimeLeft -= (float) (curTime - comp.PreviousCurTime).TotalSeconds; // Goobstation

            if (comp.TimeLeft <= 0)
            {
                comp.OnFinish?.Invoke();

                RemCompDeferred<ActiveSurveillanceCameraMonitorVisualsComponent>(uid);
            }

            comp.PreviousCurTime = curTime; // Goobstation
        }
    }

    public void AddTimer(EntityUid uid, Action onFinish)
    {
        var comp = EnsureComp<ActiveSurveillanceCameraMonitorVisualsComponent>(uid);
        comp.OnFinish = onFinish;
        comp.PreviousCurTime = _gameTiming.CurTime; // Goobstation
    }

    public void RemoveTimer(EntityUid uid)
    {
        RemCompDeferred<ActiveSurveillanceCameraMonitorVisualsComponent>(uid);
    }
}
