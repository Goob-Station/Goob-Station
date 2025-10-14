// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.SupermatterHalberd;

public sealed class SupermatterHalberdSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RaysSystem _rays = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SupermatterHalberdComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SupermatterHalberdComponent, SmHalberdExecuteDoAfterEvent>(OnExecute);
    }

    private void OnExecute(Entity<SupermatterHalberdComponent> ent, ref SmHalberdExecuteDoAfterEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        var (uid, comp) = ent;

        var ray = GetEntity(args.RayEffect);
        if (ray != null)
            PredictedQueueDel(ray);

        if (args.Cancelled)
            return;

        args.Handled = true;

        _admin.Add(HasComp<MobStateComponent>(args.Target.Value) ? LogType.Gib : LogType.InteractUsing,
            LogImpact.Medium,
            $"{ToPrettyString(args.User):user} ashed {ToPrettyString(args.Target.Value):target} using {ToPrettyString(uid):used}");

        var coords = Transform(args.Target.Value).Coordinates;
        _audio.PlayPredicted(comp.ExecuteSound, uid, args.User);
        PredictedDel(args.Target);
        PredictedSpawnAtPosition(comp.AshProto, coords);
        PredictedSpawnAtPosition(comp.ExecuteEffect, coords);
    }

    private void OnAfterInteract(Entity<SupermatterHalberdComponent> ent, ref AfterInteractEvent args)
    {
        var (uid, comp) = ent;

        if (args.Target == null)
            return;

        if (!_whitelist.IsValid(comp.ObliterateWhitelist, args.Target.Value) &&
            (!TryComp(args.Target.Value, out MobStateComponent? mobState) || mobState.CurrentState != MobState.Dead))
            return;

        var rayEffect = _rays.DoRays(_transform.GetMapCoordinates(args.Target.Value),
            Color.Yellow,
            Color.Orange,
            minMaxRadius: new Vector2(3f, 6f));

        var doArgs = new DoAfterArgs(EntityManager,
            args.User,
            comp.ExecuteDelay,
            new SmHalberdExecuteDoAfterEvent(GetNetEntity(rayEffect)),
            uid,
            args.Target,
            uid)
        {
            NeedHand = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = false,
            MultiplyDelay = false,
        };

        if (_doAfter.TryStartDoAfter(doArgs))
            return;

        if (rayEffect != null)
            PredictedQueueDel(rayEffect.Value);
    }
}

[Serializable, NetSerializable]
public sealed partial class SmHalberdExecuteDoAfterEvent(NetEntity? rayEffect) : DoAfterEvent
{
    public NetEntity? RayEffect = rayEffect;

    public SmHalberdExecuteDoAfterEvent() : this(null) { }

    public override DoAfterEvent Clone() => this;
}
