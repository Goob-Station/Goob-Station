// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CCVar;

namespace Content.Server.Instruments;

public sealed partial class InstrumentSystem
{
    public int MaxMidiEventsPerSecond { get; private set; }
    public int MaxMidiEventsPerBatch { get; private set; }
    public int MaxMidiBatchesDropped { get; private set; }
    public int MaxMidiLaggedBatches { get; private set; }

    private void InitializeCVars()
    {
        Subs.CVar(_cfg, CCVars.MaxMidiEventsPerSecond, obj => MaxMidiEventsPerSecond = obj, true);
        Subs.CVar(_cfg, CCVars.MaxMidiEventsPerBatch, obj => MaxMidiEventsPerBatch = obj, true);
        Subs.CVar(_cfg, CCVars.MaxMidiBatchesDropped, obj => MaxMidiBatchesDropped = obj, true);
        Subs.CVar(_cfg, CCVars.MaxMidiLaggedBatches, obj => MaxMidiLaggedBatches = obj, true);
    }
}