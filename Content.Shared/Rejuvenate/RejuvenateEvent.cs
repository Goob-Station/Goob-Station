// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Rejuvenate;

public sealed class RejuvenateEvent(bool uncuff = true, bool resetActions = true) : EntityEventArgs // Goob edit
{
    // Goobstation start
    public bool Uncuff = uncuff;

    public bool ResetActions = resetActions;
    // Goobstation end
}