// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

namespace Content.Server._Mono.FireControl;

[RegisterComponent]
public sealed partial class FireControlServerComponent : Component
{
    [ViewVariables]
    public EntityUid? ConnectedGrid = null;

    [ViewVariables]
    public HashSet<EntityUid> Controlled = new();

    [ViewVariables]
    public HashSet<EntityUid> Consoles = new();

    [ViewVariables, DataField]
    public int ProcessingPower;

    [ViewVariables]
    public int UsedProcessingPower;
}
