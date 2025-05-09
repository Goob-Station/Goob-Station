// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusEffect;

namespace Content.Shared.Speech.EntitySystems;

public abstract class SharedSlurredSystem : EntitySystem
{
    public virtual void DoSlur(EntityUid uid, TimeSpan time, StatusEffectsComponent? status = null) { }
}