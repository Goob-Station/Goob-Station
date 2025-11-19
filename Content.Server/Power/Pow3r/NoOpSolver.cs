// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Threading;

namespace Content.Server.Power.Pow3r
{
    public sealed class NoOpSolver : IPowerSolver
    {
        public void Tick(float frameTime, PowerState state, IParallelManager parallel)
        {
            // Literally nothing.
        }

        public void Validate(PowerState state)
        {
            // Literally nothing.
        }
    }
}
