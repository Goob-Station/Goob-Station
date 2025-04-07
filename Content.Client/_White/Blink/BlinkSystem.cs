// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Items;
using Content.Shared._White.Blink;

namespace Content.Client._White.Blink;

public sealed class BlinkSystem : SharedBlinkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<BlinkComponent>(ent => new BlinkStatusControl(ent));
    }
}