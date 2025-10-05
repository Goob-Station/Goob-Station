using Content.Server.Atmos.EntitySystems;
using Content.Shared._CorvaxGoob.CCCVars;
using Content.Shared._CorvaxGoob.DynamicAudio;
using Content.Shared._CorvaxGoob.DynamicAudio.Effects;
using Content.Shared._CorvaxGoob.TTS;
using Robust.Shared.Audio.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.DynamicSound;

public sealed class DynamicAudioSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private float _soundBarotraumaMoles = 10;

    private TimeSpan _nextPlayersBarotraumaCheck = new TimeSpan();
    private TimeSpan _cooldownPlayersCheck = TimeSpan.FromMilliseconds(100);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, MoveEvent>(OnMove);

        Subs.CVar(_cfg, CCCVars.SoundBarotraumaMoles, value => _soundBarotraumaMoles = value);
    }

    // requires to once process sound physics like tile moles
    private void OnMove(Entity<AudioComponent> entity, ref MoveEvent ev)
    {
        if (HasComp<DynamicAudioComponent>(entity))
            return;

        DoDynamicChecks(entity);

        EnsureComp<DynamicAudioComponent>(entity);
    }

    /// <summary>
    /// Marks TTS sound if it in barotrauma. Should by used instant after sound played.
    /// </summary>
    public void DoDynamicTTSChecks(Entity<TTSComponent> entity)
    {
        var mixture = _atmos.GetTileMixture((entity, Transform(entity)));

        entity.Comp.InBarotrauma = mixture is null || mixture.TotalMoles < _soundBarotraumaMoles;

        Dirty(entity);
    }

    /// <summary>
    /// Marks sound if it in barotrauma. Should by used instant after sound played.
    /// </summary>
    public void DoDynamicChecks(EntityUid entityUid)
    {
        if (TerminatingOrDeleted(entityUid) || Paused(entityUid))
            return;

        var mixture = _atmos.GetTileMixture((entityUid, Transform(entityUid)));

        if (mixture is null || mixture.TotalMoles < _soundBarotraumaMoles)
            Dirty(entityUid, EnsureComp<InBarotraumaAudioEffectComponent>(entityUid));
        else if (HasComp<InBarotraumaAudioEffectComponent>(entityUid))
            RemComp<InBarotraumaAudioEffectComponent>(entityUid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextPlayersBarotraumaCheck)
            return;

        _nextPlayersBarotraumaCheck = _timing.CurTime + _cooldownPlayersCheck;

        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity is null)
                continue;

            DoDynamicChecks(session.AttachedEntity.Value);
        }
    }
}
