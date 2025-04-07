// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons.Laws;

public abstract partial class SharedSiliconLawSystem
{
    private void InitializeUpdater()
    {
        SubscribeLocalEvent<SiliconLawUpdaterComponent, EntInsertedIntoContainerMessage>(OnUpdaterInsert);
    }

    protected virtual void OnUpdaterInsert(Entity<SiliconLawUpdaterComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // TODO: Prediction
    }
}