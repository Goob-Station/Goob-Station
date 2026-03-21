// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Kudzu;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Kudzu;

public sealed class KudzuSeeOverSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KudzuSeeOverVisualsComponent, ComponentStartup>(OnVisualsStartup);
        SubscribeLocalEvent<KudzuSeeOverVisualsComponent, ComponentShutdown>(OnVisualsShutdown);
        SubscribeLocalEvent<KudzuSeeOverVisualsComponent, AppearanceChangeEvent>(OnAppearanceChanged);

        SubscribeLocalEvent<SeeOverKudzuComponent, ComponentStartup>(OnViewerMarkerStartup);
        SubscribeLocalEvent<SeeOverKudzuComponent, ComponentShutdown>(OnViewerMarkerShutdown);

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnVisualsStartup(Entity<KudzuSeeOverVisualsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        UpdateDrawDepth((ent.Owner, sprite), ent.Comp, LocalPlayerCanSeeOverKudzu());
    }

    private void OnVisualsShutdown(Entity<KudzuSeeOverVisualsComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _sprite.SetDrawDepth((ent.Owner, sprite), ent.Comp.NormalDrawDepth);
    }

    private void OnAppearanceChanged(Entity<KudzuSeeOverVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        UpdateDrawDepth((ent.Owner, args.Sprite), ent.Comp, LocalPlayerCanSeeOverKudzu());
    }

    private void OnViewerMarkerStartup(Entity<SeeOverKudzuComponent> ent, ref ComponentStartup args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        RefreshAll(LocalPlayerCanSeeOverKudzu());
    }

    private void OnViewerMarkerShutdown(Entity<SeeOverKudzuComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        RefreshAll(LocalPlayerCanSeeOverKudzu());
    }

    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
    {
        RefreshAll(LocalPlayerCanSeeOverKudzu());
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        RefreshAll(false);
    }

    private void RefreshAll(bool canSeeOverKudzu)
    {
        var query = EntityQueryEnumerator<KudzuSeeOverVisualsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var visuals, out var sprite))
        {
            UpdateDrawDepth((uid, sprite), visuals, canSeeOverKudzu);
        }
    }

    private void UpdateDrawDepth(Entity<SpriteComponent?> ent, KudzuSeeOverVisualsComponent visuals, bool canSeeOverKudzu)
    {
        var drawDepth = canSeeOverKudzu
            ? visuals.SeeOverDrawDepth
            : visuals.NormalDrawDepth;

        _sprite.SetDrawDepth(ent, drawDepth);
    }

    private bool LocalPlayerCanSeeOverKudzu()
    {
        return _player.LocalEntity is { } local && HasComp<SeeOverKudzuComponent>(local);
    }
}
