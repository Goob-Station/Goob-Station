using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.EntitySystems;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.DynamicAudio;
using Content.Goobstation.Shared.DynamicAudio.Effects;
using Robust.Shared.Audio.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.DynamicSound;

public sealed class DynamicAudioSystem : SharedDynamicAudioSystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    private float _soundBarotraumaMoles = 10;

    private TimeSpan _nextPlayersBarotraumaCheck = new TimeSpan();
    private TimeSpan _cooldownPlayersCheck = TimeSpan.FromMilliseconds(100);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, MoveEvent>(OnMove);
        Subs.CVar(_cfg, GoobCVars.SoundBarotraumaMoles, value => _soundBarotraumaMoles = value);
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
    /// Marks sound if it in barotrauma. Should by used instant after sound played.
    /// </summary>
    public void DoDynamicChecks(EntityUid entityUid)
    {
        if (TerminatingOrDeleted(entityUid)
            || Paused(entityUid))
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
