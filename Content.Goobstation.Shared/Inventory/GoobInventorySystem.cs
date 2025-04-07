// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Shared.Inventory;

public sealed partial class GoobInventorySystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        InitializeRelays();
    }
}