// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// This handles removing accents when using the accentless trait.
/// </summary>
public sealed class AccentlessSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AccentlessComponent, ComponentStartup>(RemoveAccents);
    }

    private void RemoveAccents(EntityUid uid, AccentlessComponent component, ComponentStartup args)
    {
        foreach (var accent in component.RemovedAccents.Values)
        {
            var accentComponent = accent.Component;
            RemComp(uid, accentComponent.GetType());
        }
    }
}