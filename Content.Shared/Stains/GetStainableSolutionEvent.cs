// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;

namespace Content.Shared.Stains;

public sealed partial class GetStainableSolutionEvent : HandledEntityEventArgs
{
    public EntityUid Stained;
    public Solution? Solution;

    public GetStainableSolutionEvent(EntityUid stained)
    {
        Stained = stained;
    }
}
