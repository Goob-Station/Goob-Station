using Content.Shared.Audio;

using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Audio;

public sealed class ConstantSoundSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstantSoundComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(EntityUid uid, ConstantSoundComponent comp, ComponentStartup args)
    {
        if (string.IsNullOrEmpty(comp.Sound))
            return;

        _audio.PlayEntity(comp.Sound, Filter.Pvs(uid), uid, true, AudioParams.Default.WithLoop(true));
    }
}
