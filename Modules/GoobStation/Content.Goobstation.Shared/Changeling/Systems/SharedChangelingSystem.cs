// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cloning;
using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Body.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedChangelingSystem : EntitySystem
{
    [Dependency] protected readonly SharedBodySystem Body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, SwitchableOverlayToggledEvent>(OnVisionToggle);
        SubscribeLocalEvent<ChangelingIdentityComponent, TransferredToCloneEvent>(OnTransferredToClone);
    }

    private void OnVisionToggle(Entity<ChangelingIdentityComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        if (args.User != ent.Owner)
            return;

        if (TryComp(ent, out EyeProtectionComponent? eyeProtection))
            eyeProtection.ProtectionTime = args.Activated ? TimeSpan.Zero : TimeSpan.FromSeconds(10);

        UpdateFlashImmunity(ent, !args.Activated);
    }

    private void OnTransferredToClone(Entity<ChangelingIdentityComponent> ent, ref TransferredToCloneEvent args)
    {
        // no ling duping
        RemComp<ChangelingComponent>(ent);
        RemComp<ChangelingIdentityComponent>(ent);
        RemComp<StoreComponent>(ent);

        // old ling cells are very confused by the new friend
        Body.GibBody(ent);
    }

    protected virtual void UpdateFlashImmunity(EntityUid uid, bool active) { }
}
