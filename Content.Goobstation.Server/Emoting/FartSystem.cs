// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.Emoting;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Systems;
using Content.Shared.Camera;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Emoting;

public sealed partial class FartSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private static readonly ProtoId<EmotePrototype> FartEmote = "Fart";
    private static readonly ProtoId<EmotePrototype> InhaleEmote = "FartInhale";
    private static readonly ProtoId<EmotePrototype> SuperFartEmote = "FartSuper";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FartComponent, BeforeEmoteEvent>(OnBeforeEmote);
        SubscribeLocalEvent<FartComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<FartComponent, PostFartEvent>(OnPostFart);
        SubscribeLocalEvent<ButtComponent, OrganAddedToBodyEvent>(OnButtAdded);
        SubscribeLocalEvent<ButtComponent, OrganRemovedFromBodyEvent>(OnButtRemoved);
    }

    private void OnBeforeEmote(Entity<FartComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Cancelled || GetBlockReason(ent, args.Emote.ID) is not { } reason)
            return;

        args.Cancel();
        args.CancelMessage = Loc.GetString(reason);
    }

    private string? GetBlockReason(Entity<FartComponent> ent, ProtoId<EmotePrototype> emote)
    {
        if (emote != FartEmote && emote != InhaleEmote && emote != SuperFartEmote)
            return null;

        if (ent.Comp.Buttless)
            return "emote-fart-ass-off";

        if (emote == FartEmote && _timing.CurTime < ent.Comp.NextFart)
            return "emote-fart-out-of-farts";

        if (emote == InhaleEmote && ent.Comp.Inhaled)
            return "emote-fart-already-loaded";

        if (emote == SuperFartEmote && !ent.Comp.Inhaled)
            return "emote-fart-not-loaded";

        return null;
    }

    private void OnEmote(Entity<FartComponent> ent, ref EmoteEvent args)
    {
        if (args.Handled)
            return;

        if (args.Emote.ID == FartEmote)
            Fart(ent);
        else if (args.Emote.ID == InhaleEmote)
            Inhale(ent);
        else if (args.Emote.ID == SuperFartEmote)
            SuperFart(ent);
        else
            return;

        args.Handled = true;
    }

    private void Fart(Entity<FartComponent> ent)
    {
        if (ent.Comp.Inhaled)
        {
            ent.Comp.Inhaled = false;
            _popup.PopupEntity(Loc.GetString("emote-fart-inhale-disarm-notice"), ent, ent);
        }

        ent.Comp.NextFart = _timing.CurTime + ent.Comp.Cooldown;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.FartSound, ent);
        ReleaseGas(ent, ent.Comp.Moles);

        var ev = new PostFartEvent(false);
        RaiseLocalEvent(ent, ref ev);
    }

    private void Inhale(Entity<FartComponent> ent)
    {
        ent.Comp.Inhaled = true;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.InhaleSound, ent);
        _popup.PopupEntity(Loc.GetString("emote-fart-inhale-notice"), ent, ent);
    }

    private void SuperFart(Entity<FartComponent> ent)
    {
        ent.Comp.Inhaled = false;
        ent.Comp.Buttless = true;
        ent.Comp.NextFart = _timing.CurTime + ent.Comp.Cooldown;
        Dirty(ent);

        var coords = _transform.GetMapCoordinates(ent);

        _audio.PlayPvs(ent.Comp.SuperFartSound, ent);
        CameraShake(coords, ent.Comp.ShakeRange, ent.Comp.SuperFartShakeIntensity);
        ReleaseGas(ent, ent.Comp.Moles * ent.Comp.SuperFartMolesMultiplier);
        LoseButt(ent, coords);
        _popup.PopupEntity(Loc.GetString("emote-fart-super-fart"), ent, ent);

        var ev = new PostFartEvent(true);
        RaiseLocalEvent(ent, ref ev);
    }

    private void LoseButt(Entity<FartComponent> ent, MapCoordinates coords)
    {
        EntityUid butt;
        if (TryComp<BodyComponent>(ent, out var body)
            && _body.TryGetBodyOrganEntityComps<ButtComponent>((ent, body), out var butts)
            && butts.Count > 0)
        {
            butt = butts[0].Owner;
            _body.RemoveOrgan(butt, butts[0].Comp2);
        }
        else
        {
            butt = Spawn(ent.Comp.ButtPrototype, coords);
        }

        _throwing.TryThrow(butt, _random.NextAngle().ToVec() * ent.Comp.ButtThrowDistance);
    }

    private void OnButtAdded(Entity<ButtComponent> ent, ref OrganAddedToBodyEvent args)
    {
        if (!TryComp<FartComponent>(args.Body, out var fart) || !fart.Buttless)
            return;

        fart.Buttless = false;
        Dirty(args.Body, fart);
    }

    private void OnButtRemoved(Entity<ButtComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        if (TerminatingOrDeleted(args.OldBody) || !TryComp<FartComponent>(args.OldBody, out var fart))
            return;

        fart.Buttless = true;
        fart.Inhaled = false;
        Dirty(args.OldBody, fart);
    }

    private void ReleaseGas(Entity<FartComponent> ent, float moles)
    {
        _atmos.GetTileMixture(ent.Owner, excite: true)?.AdjustMoles(ent.Comp.Gas, moles);
    }

    private void OnPostFart(Entity<FartComponent> ent, ref PostFartEvent args)
    {
        foreach (var bible in _lookup.GetEntitiesInRange<BibleComponent>(_transform.GetMapCoordinates(ent),
                     ent.Comp.BibleSmiteRange,
                     LookupFlags.Sundries | LookupFlags.Dynamic))
        {
            RaiseNetworkEvent(new BibleFartSmiteEvent(GetNetEntity(bible)), Filter.Pvs(bible));
            _body.GibBody(ent, splatModifier: 15);

            _audio.PlayPvs(ent.Comp.BibleSmiteSound, bible);
            _audio.PlayPvs(args.SuperFart ? ent.Comp.SuperFartSound : ent.Comp.FartSound, bible);
            CameraShake(_transform.GetMapCoordinates(bible), ent.Comp.ShakeRange, ent.Comp.BibleSmiteShakeIntensity);
            return;
        }
    }

    private void CameraShake(MapCoordinates epicenter, float range, float intensity)
    {
        var filter = Filter.Empty().AddInRange(epicenter, range, entMan: EntityManager);

        foreach (var session in filter.Recipients)
        {
            if (session.AttachedEntity is not { } uid)
                continue;

            var delta = epicenter.Position - _transform.GetWorldPosition(uid);
            if (delta.EqualsApprox(Vector2.Zero))
                delta = new Vector2(0.01f, 0);

            var effect = 5 * MathF.Sqrt(intensity) * (1 - delta.Length() / range);
            if (effect > 0.01f)
                _recoil.KickCamera(uid, -delta.Normalized() * effect);
        }
    }
}
