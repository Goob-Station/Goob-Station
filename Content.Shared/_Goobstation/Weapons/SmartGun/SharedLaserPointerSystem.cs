using System.Numerics;
using Content.Shared.Wieldable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

public abstract class SharedLaserPointerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LaserPointerComponent, ItemWieldedEvent>(OnWield);
        SubscribeAllEvent<LaserPointerEntityHoveredEvent>(OnHovered);
    }

    private void OnHovered(LaserPointerEntityHoveredEvent ev)
    {
        var pointer = GetEntity(ev.LaserPointerEntity);

        if (!TryComp(pointer, out LaserPointerComponent? laser))
            return;

        var hovered = GetEntity(ev.Hovered);

        laser.TargetedEntity = !EntityManager.EntityExists(hovered) ? null : hovered;
        laser.Direction = ev.Dir;
        Dirty(pointer, laser);
    }

    private void OnWield(Entity<LaserPointerComponent> ent, ref ItemWieldedEvent args)
    {
        _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
    }
}

[Serializable, NetSerializable]
public sealed class LaserPointerEntityHoveredEvent(NetEntity? hovered, Vector2? dir, NetEntity pointer) : EntityEventArgs
{
    public NetEntity? Hovered = hovered;

    public Vector2? Dir = dir;

    public NetEntity LaserPointerEntity = pointer;
}
