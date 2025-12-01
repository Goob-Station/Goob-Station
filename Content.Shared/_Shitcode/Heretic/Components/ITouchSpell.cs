using Robust.Shared.Audio;

namespace Content.Shared._Shitcode.Heretic.Components;

public interface ITouchSpell
{
    public EntityUid? Action { get; set; }

    public TimeSpan Cooldown { get; set; }

    public LocId Speech { get; set; }

    public SoundSpecifier? Sound { get; set; }
}
