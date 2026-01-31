using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Content.Shared._Lavaland.Megafauna.Events.Banana;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems.Banana;

/// <summary>
/// This system handles creating multiple entities on top of UID.
/// </summary>
public sealed class RingOfFireSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RingOfFireComponent, RingOfFireEvent>(OnRingOfFire);
    }
    private void OnRingOfFire(EntityUid uid, RingOfFireComponent comp, RingOfFireEvent args)
    {
        var distance = args.RingDistance > 0
            ? args.RingDistance
            : comp.RingDistance;

        var skullProto = args.SkullPrototype != default
            ? args.SkullPrototype
            : comp.SkullPrototype;

        SpawnRing(uid, comp, distance, skullProto);
        args.Handled = true;
    }
    private void SpawnRing(EntityUid owner, RingOfFireComponent comp, float ringDistance, EntProtoId skullProto)
    {
        var coords = Transform(owner).Coordinates;

        for (var i = 0; i < comp.NumberToSpawn; i++)
        {
            var angle = MathF.Tau * i / comp.NumberToSpawn;
            var skull = Spawn(skullProto, coords);

            var orbit = EnsureComp<OrbitingComponent>(skull);
            orbit.Owner = owner;
            orbit.Angle = angle;
            orbit.Radius = 0f;
            orbit.MaxRadius = ringDistance;
            orbit.GrowSpeed = comp.GrowSpeed;
            orbit.RotationSpeed = MathF.Tau;
        }

        if (comp.ActionSound != null)
            _audio.PlayPvs(comp.ActionSound, owner);

        if (comp.ShouldSpeak)
            _chat.TrySendInGameICMessage(owner, Loc.GetString(comp.Speech), InGameICChatType.Speak, false);
    }

}

