using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Content.Shared.DoAfter;
using Content.Shared.Store;
using Content.Shared.Interaction;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NTR.Scan
{
    [RegisterComponent]
    public sealed partial class BriefcaseScannerComponent : Component
    {
        [DataField("scanDuration")]
        public float ScanDuration = 10f;
    }
}
