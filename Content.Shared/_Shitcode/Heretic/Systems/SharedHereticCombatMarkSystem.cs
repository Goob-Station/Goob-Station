using Content.Shared.Heretic;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedHereticCombatMarkSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public virtual bool ApplyMarkEffect(EntityUid target,
        HereticCombatMarkComponent mark,
        string? path,
        EntityUid user)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        _audio.PlayPredicted(mark.TriggerSound, target, user);
        RemCompDeferred(target, mark);
        return true;
    }
}
