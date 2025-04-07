// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Atmos.Monitor;
using Content.Shared.Atmos.Monitor.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Atmos.Monitor.Components;

[RegisterComponent]
public sealed partial class AirAlarmComponent : Component
{
    [DataField] public AirAlarmMode CurrentMode { get; set; } = AirAlarmMode.Filtering;
    [DataField] public bool AutoMode { get; set; } = true;

    // Remember to null this afterwards.
    [ViewVariables] public IAirAlarmModeUpdate? CurrentModeUpdater { get; set; }

    public readonly HashSet<string> KnownDevices = new();
    public readonly Dictionary<string, GasVentPumpData> VentData = new();
    public readonly Dictionary<string, GasVentScrubberData> ScrubberData = new();
    public readonly Dictionary<string, AtmosSensorData> SensorData = new();

    public bool CanSync = true;

    /// <summary>
    /// Previous alarm state for use with output ports.
    /// </summary>
    [DataField("state")]
    public AtmosAlarmType State = AtmosAlarmType.Normal;

    /// <summary>
    /// The port that gets set to high while the alarm is in the danger state, and low when not.
    /// </summary>
    [DataField("dangerPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string DangerPort = "AirDanger";

    /// <summary>
    /// The port that gets set to high while the alarm is in the warning state, and low when not.
    /// </summary>
    [DataField("warningPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string WarningPort = "AirWarning";

    /// <summary>
    /// The port that gets set to high while the alarm is in the normal state, and low when not.
    /// </summary>
    [DataField("normalPort", customTypeSerializer: typeof(PrototypeIdSerializer<SourcePortPrototype>))]
    public string NormalPort = "AirNormal";
}