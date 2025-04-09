// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Cloning.CloningConsole
{
    [Serializable, NetSerializable]
    public sealed class CloningConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string? ScannerBodyInfo;
        public readonly string? ClonerBodyInfo;
        public readonly bool MindPresent;
        public readonly ClonerStatus CloningStatus;
        public readonly bool ScannerConnected;
        public readonly bool ScannerInRange;
        public readonly bool ClonerConnected;
        public readonly bool ClonerInRange;
        public CloningConsoleBoundUserInterfaceState(string? scannerBodyInfo, string? cloningBodyInfo, bool mindPresent, ClonerStatus cloningStatus, bool scannerConnected, bool scannerInRange, bool clonerConnected, bool clonerInRange)
        {
            ScannerBodyInfo = scannerBodyInfo;
            ClonerBodyInfo = cloningBodyInfo;
            MindPresent = mindPresent;
            CloningStatus = cloningStatus;
            ScannerConnected = scannerConnected;
            ScannerInRange = scannerInRange;
            ClonerConnected = clonerConnected;
            ClonerInRange = clonerInRange;
        }
    }

    [Serializable, NetSerializable]
    public enum ClonerStatus : byte
    {
        Ready,
        ScannerEmpty,
        ScannerOccupantAlive,
        OccupantMetaphyiscal,
        ClonerOccupied,
        NoClonerDetected,
        NoMindDetected
    }

    [Serializable, NetSerializable]
    public enum CloningConsoleUiKey : byte
    {
        Key
    }

    [Serializable, NetSerializable]
    public enum UiButton : byte
    {
        Clone,
        Eject
    }

    [Serializable, NetSerializable]
    public sealed class UiButtonPressedMessage : BoundUserInterfaceMessage
    {
        public readonly UiButton Button;

        public UiButtonPressedMessage(UiButton button)
        {
            Button = button;
        }
    }
}