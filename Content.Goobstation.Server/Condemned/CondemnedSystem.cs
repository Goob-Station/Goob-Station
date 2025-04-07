using Content.Goobstation.Shared.CheatDeath;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Server.Condemned;

public sealed partial class CondemnedSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private readonly EntProtoId _pentagramEffectProto = "Pentagram";
    private readonly EntProtoId _hellgraspEffectProto = "HellHand";
    private readonly SoundPathSpecifier _earthquakeSoundPath = new("/Audio/_Goobstation/Effects/earth_quake.ogg");

    public enum CondemnedPhase : byte
    {
        Waiting,
        PentagramActive,
        HandActive,
        Complete
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CondemnedComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CondemnedComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CondemnedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            switch (comp)
            {
                case { CurrentPhase: CondemnedPhase.PentagramActive }:
                    UpdatePentagramPhase(uid, comp, frameTime);
                    break;

                case { CurrentPhase: CondemnedPhase.HandActive }:
                    UpdateHandPhase(uid, comp, frameTime);
                    break;
            }
        }
    }

    private void UpdatePentagramPhase(EntityUid uid, CondemnedComponent comp, float frameTime)
    {
        comp.PhaseTimer += frameTime;

        if (!(comp.PhaseTimer >= 3f))
            return;

        var coords = Transform(uid).Coordinates;
        comp.HandEntity = Spawn(_hellgraspEffectProto, coords);

        comp.HandDuration = TryComp<TimedDespawnComponent>(comp.HandEntity, out var timedDespawn) ? timedDespawn.Lifetime : 1f;

        comp.CurrentPhase = CondemnedPhase.HandActive;
        comp.PhaseTimer = 0f;
    }

    private void UpdateHandPhase(EntityUid uid, CondemnedComponent comp, float frameTime)
    {
        comp.PhaseTimer += frameTime;

        // Wait for hand animation duration before deleting
        if (!(comp.PhaseTimer >= comp.HandDuration))
            return;

        QueueDel(uid);
        comp.CurrentPhase = CondemnedPhase.Complete;
    }

    private void TryObliterateEntity(EntityUid uid, CondemnedComponent comp)
    {
        // Initial effect setup
        var coords = Transform(uid).Coordinates;
        Spawn(_pentagramEffectProto, coords);
        _audio.PlayPvs(_earthquakeSoundPath, uid);

        // Start update sequence
        comp.CurrentPhase = CondemnedPhase.PentagramActive;
        comp.PhaseTimer = 0f;
    }

    private void OnExamined(EntityUid uid, CondemnedComponent comp, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && !comp.IsCorporateOwned)
            args.PushMarkup(Loc.GetString("condemned-component-examined", ("target", uid)));
    }

    private void OnMobStateChanged(EntityUid uid, CondemnedComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || comp.IsCorporateOwned)
            return;

        if (TryComp<CheatDeathComponent>(uid, out var cheatDeath) && cheatDeath.ReviveAmount > 0)
            return;

        TryObliterateEntity(uid, comp);
    }
}
