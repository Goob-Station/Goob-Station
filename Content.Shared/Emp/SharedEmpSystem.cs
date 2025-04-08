// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Timing;

namespace Content.Shared.Emp;

public abstract class SharedEmpSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    protected const string EmpDisabledEffectPrototype = "EffectEmpDisabled";
}