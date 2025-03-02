using System.Runtime.InteropServices;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args, WeakToHolyComponent comp)
    {
        var multiplier = comp.DamageMultiplier;
    }
}

// Okay you see what I'm getting at here.
// Not finished an a little supercoded but alas.
