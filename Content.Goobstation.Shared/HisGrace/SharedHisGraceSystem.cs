// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.HisGrace;

public abstract partial class SharedHisGraceSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] protected readonly SharedPopupSystem Popup = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HisGraceComponent, ContainerGettingInsertedAttemptEvent>(OnHandPickUpAttempt);
        SubscribeLocalEvent<HisGraceComponent, PullAttemptEvent>(OnPullAttempt);

    }

    private void OnHandPickUpAttempt(Entity<HisGraceComponent> hisGrace, ref ContainerGettingInsertedAttemptEvent args)
    {
        var user = args.Container.Owner;

        if (hisGrace.Comp.User != null && hisGrace.Comp.User != user)
        {
            args.Cancel();
            Popup.PopupEntity(Loc.GetString("hisgrace-pickup-denied"), user ,user);
        }
    }

    private void OnPullAttempt(Entity<HisGraceComponent> hisGrace, ref PullAttemptEvent args)
    {
        if (hisGrace.Comp.User != null && hisGrace.Comp.User != args.PullerUid)
        {
            args.Cancelled = true;
            Popup.PopupEntity(  Loc.GetString("hisgrace-pickup-denied"), args.PullerUid ,args.PullerUid);
        }
    }

    protected virtual void VisualsChanged(Entity<HisGraceComponent> ent, string key)
    {

    }

    protected void DoAscensionVisuals(Entity<HisGraceComponent> ent, string key)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, ToggleableVisuals.Enabled, true, appearance);
        _item.SetHeldPrefix(ent, key);

        VisualsChanged(ent, key);
    }
}
