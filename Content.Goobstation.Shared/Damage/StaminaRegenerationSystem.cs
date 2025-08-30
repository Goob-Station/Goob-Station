// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Goobstation.Shared.Damage;
public sealed class StaminaRegenerationSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaRegenerationComponent, ComponentStartup>(OnStaminaRegenerationStartup);
        SubscribeLocalEvent<StaminaRegenerationComponent, ComponentShutdown>(OnStaminaRegenerationShutdown);
    }

    private void OnStaminaRegenerationStartup(EntityUid uid, StaminaRegenerationComponent component, ComponentStartup args) =>
      _staminaSystem.ToggleStaminaDrain(uid, component.RegenerationRate, true, false, component.RegenerationKey, uid);

    private void OnStaminaRegenerationShutdown(EntityUid uid, StaminaRegenerationComponent component, ComponentShutdown args) =>
      _staminaSystem.ToggleStaminaDrain(uid, component.RegenerationRate, false, false, component.RegenerationKey, uid);

}
