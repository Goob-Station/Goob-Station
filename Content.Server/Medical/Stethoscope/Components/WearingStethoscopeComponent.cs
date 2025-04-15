// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Threading;

namespace Content.Server.Medical.Components
{
    /// <summary>
    /// Used to let doctors use the stethoscope on people.
    /// </summary>
    [RegisterComponent]
    public sealed partial class WearingStethoscopeComponent : Component
    {
        public CancellationTokenSource? CancelToken;

        [DataField("delay")]
        public float Delay = 2.5f;

        public EntityUid Stethoscope = default!;
    }
}