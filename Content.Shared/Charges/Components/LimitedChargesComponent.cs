// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Charges.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Charges.Components;

/// <summary>
/// Specifies the attached action has discrete charges, separate to a cooldown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedChargesSystem))]
public sealed partial class LimitedChargesComponent : Component
{
    [DataField, AutoNetworkedField]
    public int LastCharges;

    /// <summary>
    ///     The max charges this action has.
    /// </summary>
    [DataField, AutoNetworkedField] // Goob Change: Stupid fuckin set accessors.
    public int MaxCharges = 3;

    /// <summary>
    /// Last time charges was changed. Used to derive current charges.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan LastUpdate;
}
