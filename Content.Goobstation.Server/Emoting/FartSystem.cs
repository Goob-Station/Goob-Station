// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 jellygato <aly.jellygato@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Emoting;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Body.Systems;
using Content.Shared.Camera;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class FartSystem : SharedFartSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoilSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly GunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FartComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<FartComponent, PostFartEvent>(OnBibleFart);
    }
    private void OnEmote(Entity<FartComponent> ent, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;

        switch (args.Emote.ID)
        {
            case "Fart":
                args.Handled = true;

                if (ent.Comp.SuperFarted)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                    return;
                }

                if (ent.Comp.FartTimeout)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-out-of-farts"), uid, uid);
                    return;
                }

                if (comp.FartInhale)
                {
                    comp.FartInhale = false;
                    _popup.PopupEntity(Loc.GetString("emote-fart-inhale-disarm-notice"), uid, uid);
                }

                comp.FartTimeout = true;
                _audio.PlayEntity(comp.FartSounds, Filter.Pvs(uid), uid, true);
                break;

            case "FartInhale":
                args.Handled = true;

                if (comp.SuperFarted)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                    return;
                }

                if (comp.FartInhale)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-already-loaded"), uid, uid);
                }

                comp.FartInhale = true;

                //Play fart sound and give notification
                _audio.PlayEntity(comp.FartInhaleSounds, Filter.Pvs(uid), uid, true);
                _popup.PopupEntity(Loc.GetString("emote-fart-inhale-notice"), uid, uid);
                return;

            case "FartSuper":
                args.Handled = true;

                if (comp.SuperFarted)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-ass-off"), uid, uid);
                    return;
                }

                if (!comp.FartInhale)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-not-loaded"), uid, uid);
                    return;
                }

                //Make fart impossible
                comp.FartTimeout = true;
                comp.FartInhale = false;
                comp.SuperFarted = true;

                _audio.PlayEntity(comp.SuperFartSounds, Filter.Pvs(uid), uid, true, AudioParams.Default.WithVolume(0f));

                // Screen shake
                var xformSystem = _entMan.System<SharedTransformSystem>();
                CameraShake(10f, xformSystem.GetMapCoordinates(uid), 0.75f);

                _entMan.SpawnEntity("Butt", xformSystem.GetMapCoordinates(uid));
                _popup.PopupEntity(Loc.GetString("emote-fart-super-fart"), uid, uid);
                break;

            case "AbnormalFart":
                args.Handled = true;

                if (comp.FartTimeout)
                {
                    _popup.PopupEntity(Loc.GetString("emote-fart-out-of-farts"), uid, uid);
                    return;
                }

                comp.FartTimeout = true;
                _audio.PlayEntity(comp.FartSounds, Filter.Pvs(uid), uid, true);

                if (comp.GasAnimation is null)
                    return;

                DoFartAnimation(uid, comp.GasAnimation, comp.FartAnimationSpeed);
                break;

            default:
                return;
        }

        //Just in case
        if (!comp.FartTimeout)
            return;

        //Give random ammonia moles to tile
        var tileMix = _atmos.GetTileMixture(uid, excite: true);
        tileMix?.AdjustMoles(comp.GasToFart, comp.MolesAmmoniaPerFart);

        //Limit the amount of fart spam
        Timer.Spawn(comp.FartTimeoutDuration, () =>
        {
            comp.FartTimeout = false;
        });

        var ev = new PostFartEvent(uid);
        RaiseLocalEvent(uid, ev);
    }

    private void CameraShake(float range, MapCoordinates epicenter, float totalIntensity)
    {
        var players = Filter.Empty();
        players.AddInRange(epicenter, range, _playerManager, EntityManager);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid uid)
                continue;

            var playerPos = _transformSystem.GetWorldPosition(player.AttachedEntity!.Value);
            var delta = epicenter.Position - playerPos;

            if (delta.EqualsApprox(Vector2.Zero))
                delta = new(0.01f, 0);

            var distance = delta.Length();
            var effect = 5 * MathF.Pow(totalIntensity, 0.5f) * (1 - distance / range);
            if (effect > 0.01f)
                _recoilSystem.KickCamera(uid, -delta.Normalized() * effect);
        }
    }

    /// <summary>
    ///     Bible fart
    /// </summary>
    private void OnBibleFart(Entity<FartComponent> ent, ref PostFartEvent args)
    {
        foreach (var near in _lookup.GetEntitiesInRange(ent, 0.4f, LookupFlags.Sundries | LookupFlags.Dynamic)){

            if (!HasComp<BibleComponent>(near))
                continue;

            var ev = new BibleFartSmiteEvent(GetNetEntity(near));
            RaiseNetworkEvent(ev);
            _bodySystem.GibBody(ent, splatModifier: 15);
            _audio.PlayEntity(ent.Comp.BibleSmiteSnd, Filter.Pvs(near), near, true);
            if (!ent.Comp.SuperFarted)
            {
                _audio.PlayEntity(ent.Comp.FartSounds, Filter.Pvs(near), near, true); // Must replay it because gib body makes the original fart sound stop immediately
            }
            else
            {
                _audio.PlayEntity(ent.Comp.SuperFartSounds, Filter.Pvs(near), near, true, AudioParams.Default.WithVolume(0f));
            }
            var xformSystem = _entMan.System<SharedTransformSystem>();
            CameraShake(10f, xformSystem.GetMapCoordinates(near), 1.5f);
            return;
        }
    }
    private void DoFartAnimation(EntityUid uid, string? protoName, float speed)
    {
        var xform = Transform(uid);
        var (pos, rot) = _transform.GetWorldPositionRotation(xform);

        var dir = -rot.ToWorldVec();
        var mapPos = new MapCoordinates(pos + dir * 3f, xform.MapID);

        var plume = _entMan.Spawn(protoName, mapPos);
        _gun.ShootProjectile(plume, dir, Vector2.Zero, uid, uid, speed);
    }
}
