// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared.Destructible.Thresholds.Triggers;

namespace Content.Server.Destructible.Thresholds;

[DataDefinition]
public sealed partial class DamageThreshold
{
    /// <summary>
    /// Whether or not this threshold was triggered in the previous call to
    /// <see cref="Reached"/>.
    /// </summary>
    [ViewVariables] public bool OldTriggered;

    /// <summary>
    /// Whether or not this threshold has already been triggered.
    /// </summary>
    [DataField]
    public bool Triggered;

    /// <summary>
    /// Whether or not this threshold only triggers once.
    /// If false, it will trigger again once the entity is healed
    /// and then damaged to reach this threshold once again.
    /// It will not repeatedly trigger as damage rises beyond that.
    /// </summary>
    [DataField]
    public bool TriggersOnce;

    /// <summary>
    /// The condition that decides if this threshold has been reached.
    /// Gets evaluated each time the entity's damage changes.
    /// </summary>
    [DataField]
    public IThresholdTrigger? Trigger;

    /// <summary>
    /// Behaviors to activate once this threshold is triggered.
    /// TODO: Replace with EntityEffects.
    /// </summary>
    [DataField]
    public List<IThresholdBehavior> Behaviors = new();
}
