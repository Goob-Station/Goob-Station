using Content.Goobstation.Shared.Heretic.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Heretic.Systems;

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
