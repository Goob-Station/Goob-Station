// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Sound;

public abstract partial class SharedSpamEmitSoundRequirePowerSystem : EntitySystem
{
    [Dependency] protected readonly SharedEmitSoundSystem EmitSound = default!;
}