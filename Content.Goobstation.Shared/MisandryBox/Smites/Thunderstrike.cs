// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using Content.Shared.Electrocution;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class ThunderstrikeSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedElectrocutionSystem _elect = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const string Sound = "/Audio/_Goobstation/Effects/Smites/Thunderstrike/thunderstrike.ogg";

    // efcc gon get u alaye...
    public void Smite(EntityUid mumu, bool kill = true, TransformComponent? transform = null)
    {
        if (!Resolve(mumu, ref transform))
            return;

        CreateLighting(transform.Coordinates);

        _elect.TryDoElectrocution(mumu, null, 250, TimeSpan.FromSeconds(1), false, ignoreInsulation: true);

        if (!kill)
            return;

        QueueDel(mumu);
        Spawn("Ash", transform.Coordinates);
        _popup.PopupEntity(Loc.GetString("admin-smite-turned-ash-other", ("name", mumu)), mumu, PopupType.LargeCaution);
    }

    public void CreateLighting(EntityCoordinates coordinates, int energy = 125, int radius = 15)
    {
        var ent = Spawn(null, coordinates);
        var comp = _light.EnsureLight(ent);
        _light.SetColor(ent, new Color(255, 255, 255), comp);
        _light.SetEnergy(ent, energy, comp);
        _light.SetRadius(ent, radius, comp);

        var sound = new SoundPathSpecifier(Sound);
        _audio.PlayPvs(sound, coordinates, AudioParams.Default.WithVolume(150f));

        Robust.Shared.Timing.Timer.Spawn(TimeSpan.FromMilliseconds(125), () => Del(ent), CancellationToken.None);
    }
}
