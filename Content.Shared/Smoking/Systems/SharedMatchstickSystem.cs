// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Shitmed Change Start
using Content.Shared.Smoking.Components;

namespace Content.Shared.Smoking.Systems;

public abstract class SharedMatchstickSystem : EntitySystem
{
    public virtual bool SetState(Entity<MatchstickComponent> ent, SmokableState state)
    {
        if (ent.Comp.CurrentState == state)
            return false;

        ent.Comp.CurrentState = state;
        Dirty(ent);
        return true;
    }
} 
// Shitmed Change End