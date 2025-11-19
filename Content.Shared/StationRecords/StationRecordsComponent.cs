// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.StationRecords;

[Access(typeof(SharedStationRecordsSystem))]
[RegisterComponent]
public sealed partial class StationRecordsComponent : Component
{
    // Every single record in this station, by key.
    // Essentially a columnar database, but I really suck
    // at implementing that so
    [IncludeDataField]
    public StationRecordSet Records = new();
}
