using Content.Server.Atmos.Piping.EntitySystems;
using Content.Shared.Throwing;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly AtmosDeviceSystem _atmosDeviceSys = default!;
}