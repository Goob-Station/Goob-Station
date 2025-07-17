// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 krusti <43324723+Topicranger@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 krusti <krusti@fluffytech.xyz>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<int> MaxMidiEventsPerSecond =
        CVarDef.Create("midi.max_events_per_second", 1000, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> MaxMidiEventsPerBatch =
        CVarDef.Create("midi.max_events_per_batch", 60, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> MaxMidiBatchesDropped =
        CVarDef.Create("midi.max_batches_dropped", 1, CVar.SERVERONLY);

    public static readonly CVarDef<int> MaxMidiLaggedBatches =
        CVarDef.Create("midi.max_lagged_batches", 8, CVar.SERVERONLY);

    /// <summary>
    /// Defines the max amount of characters to allow in the "Midi channel selector".
    /// </summary>
    public static readonly CVarDef<int> MidiMaxChannelNameLength =
        CVarDef.Create("midi.max_channel_name_length", 64, CVar.SERVERONLY);
}
