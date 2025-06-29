using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Binary.Components
{
    public sealed record GasHeatPumpData(float LastMolesTransferred);

    [Serializable, NetSerializable]
    public enum GasHeatPumpUiKey
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string PumpLabel { get; }
        public float TransferRate { get; }
        public bool Enabled { get; }

        public GasHeatPumpBoundUserInterfaceState(string pumpLabel, float transferRate, bool enabled)
        {
            PumpLabel = pumpLabel;
            TransferRate = transferRate;
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpToggleStatusMessage : BoundUserInterfaceMessage
    {
        public bool Enabled { get; }

        public GasHeatPumpToggleStatusMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasHeatPumpChangeTransferRateMessage : BoundUserInterfaceMessage
    {
        public float TransferRate { get; }

        public GasHeatPumpChangeTransferRateMessage(float transferRate)
        {
            TransferRate = transferRate;
        }
    }
}
