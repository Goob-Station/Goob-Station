using System.Globalization;
using Content.Goobstation.Shared.AlmanacBlade;
using Content.Shared.Hands.EntitySystems;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.AlmanacBlade;

public sealed class AlmanacBladeSystem : SharedAlmanacBladeSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private bool _inRussia;

    public override void Initialize()
    {
        base.Initialize();

        _inRussia = CultureInfo.CurrentCulture.Name.EndsWith("-RU", StringComparison.OrdinalIgnoreCase);

        SubscribeLocalEvent<AlmanacBladeComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_inRussia)
            return;

        var local = _player.LocalEntity;
        var query = EntityQueryEnumerator<AlmanacBladeComponent>();
        while (query.MoveNext(out var uid, out var blade))
        {
            var playing = blade.HardBassDay && local != null && _hands.IsHolding(local.Value, uid);

            if (playing && blade.HardBassStream == null)
                blade.HardBassStream = _audio.PlayGlobal(blade.HardBass, Filter.Local(), false, AudioParams.Default.WithLoop(true))?.Entity;
            else if (!playing && blade.HardBassStream != null)
                blade.HardBassStream = _audio.Stop(blade.HardBassStream);
        }
    }

    private void OnShutdown(Entity<AlmanacBladeComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.HardBassStream = _audio.Stop(ent.Comp.HardBassStream);
    }
}
