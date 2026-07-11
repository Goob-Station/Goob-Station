using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Fluids;

[Serializable, NetSerializable]
public sealed class LiquidSplashEffectEvent(NetEntity target, Color color) : EntityEventArgs
{
    public readonly NetEntity Target = target;
    public readonly Color Color = color;
}
