// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.StatusEffect
{
    [RegisterComponent]
    [NetworkedComponent]
    [Access(typeof(StatusEffectsSystem))]
    public sealed partial class StatusEffectsComponent : Component
    {
        [ViewVariables]
        public Dictionary<string, StatusEffectState> ActiveEffects = new();

        /// <summary>
        ///     A list of status effect IDs to be allowed
        /// </summary>
        [DataField("allowed", required: true), Access(typeof(StatusEffectsSystem), Other = AccessPermissions.ReadExecute)]
        public List<string> AllowedEffects = default!;
    }

    [RegisterComponent]
    public sealed partial class ActiveStatusEffectsComponent : Component {}

    /// <summary>
    ///     Holds information about an active status effect.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class StatusEffectState
    {
        /// <summary>
        ///     The start and end times of the status effect.
        /// </summary>
        [ViewVariables]
        public (TimeSpan, TimeSpan) Cooldown;

        /// <summary>
        ///     Specifies whether to refresh or accumulate the cooldown of the status effect.
        ///     true - refresh time, false - accumulate time.
        /// </summary>
        [ViewVariables]
        public bool CooldownRefresh = true;

        /// <summary>
        ///     The name of the relevant component that
        ///     was added alongside the effect, if any.
        /// </summary>
        [ViewVariables]
        public string? RelevantComponent;

        public StatusEffectState((TimeSpan, TimeSpan) cooldown, bool refresh, string? relevantComponent=null)
        {
            Cooldown = cooldown;
            CooldownRefresh = refresh;
            RelevantComponent = relevantComponent;
        }

        public StatusEffectState(StatusEffectState toCopy)
        {
            Cooldown = (toCopy.Cooldown.Item1, toCopy.Cooldown.Item2);
            CooldownRefresh = toCopy.CooldownRefresh;
            RelevantComponent = toCopy.RelevantComponent;
        }
    }

    [Serializable, NetSerializable]
    public sealed class StatusEffectsComponentState : ComponentState
    {
        public Dictionary<string, StatusEffectState> ActiveEffects;
        public List<string> AllowedEffects;

        public StatusEffectsComponentState(Dictionary<string, StatusEffectState> activeEffects, List<string> allowedEffects)
        {
            ActiveEffects = activeEffects;
            AllowedEffects = allowedEffects;
        }
    }
}