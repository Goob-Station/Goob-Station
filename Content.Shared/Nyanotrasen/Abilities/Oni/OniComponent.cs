// SPDX-FileCopyrightText: 2024 ScyronX <166930367+ScyronX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Shared.Abilities.Oni
{
    [RegisterComponent]
    public sealed partial class OniComponent : Component
    {
        [DataField("modifiers", required: true)]
        public DamageModifierSet MeleeModifiers = default!;

        [DataField("stamDamageBonus")]
        public float StamDamageMultiplier = 1.25f;
    }
}