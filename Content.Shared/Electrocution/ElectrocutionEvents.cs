using Content.Shared.Inventory;

namespace Content.Shared.Electrocution
{
    public sealed class ElectrocutionAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
    {
        public SlotFlags TargetSlots { get; }

        public readonly EntityUid TargetUid;
        public readonly EntityUid? SourceUid;
        public float SiemensCoefficient = 1f;

        public ElectrocutionAttemptEvent(EntityUid targetUid, EntityUid? sourceUid, float siemensCoefficient, SlotFlags targetSlots)
        {
            TargetUid = targetUid;
            TargetSlots = targetSlots;
            SourceUid = sourceUid;
            SiemensCoefficient = siemensCoefficient;
        }
    }

    public sealed class ElectrocutedEvent : EntityEventArgs
    {
        public readonly EntityUid TargetUid;
        public readonly EntityUid? SourceUid;
        public readonly float SiemensCoefficient;
        public readonly float? ShockDamage = null; // Goobstation

        public ElectrocutedEvent(EntityUid targetUid, EntityUid? sourceUid, float siemensCoefficient, float shockDamage) // Goobstation
        {
            TargetUid = targetUid;
            SourceUid = sourceUid;
            SiemensCoefficient = siemensCoefficient;
            ShockDamage = shockDamage; // Goobstation
        }
    }
}
