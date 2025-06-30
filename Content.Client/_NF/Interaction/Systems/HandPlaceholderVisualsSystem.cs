// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 RatherUncreative <RatherUncreativeName@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Whatstone <whatston3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._NF.Hands.UI;
using Content.Client.Items;
using Content.Client.Items.Systems;
using Content.Shared._NF.Interaction.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that spawn HandPlaceholder items.
/// </summary>
[UsedImplicitly]
public sealed partial class HandPlaceholderVisualsSystem : EntitySystem
{
    [Dependency] ContainerSystem _container = default!;
    [Dependency] ItemSystem _item = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandPlaceholderComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);

        SubscribeLocalEvent<HandPlaceholderVisualsComponent, ComponentRemove>(PlaceholderRemove);

        Subs.ItemStatus<HandPlaceholderVisualsComponent>(_ => new HandPlaceholderStatus());
    }

    private void OnAfterAutoHandleState(Entity<HandPlaceholderComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out HandPlaceholderVisualsComponent? placeholder))
            return;

        if (placeholder.Dummy != EntityUid.Invalid)
            QueueDel(placeholder.Dummy);
        placeholder.Dummy = Spawn(ent.Comp.Prototype);

        if (_container.IsEntityInContainer(ent))
            _item.VisualsChanged(ent);
    }

    private void PlaceholderRemove(Entity<HandPlaceholderVisualsComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.Dummy != EntityUid.Invalid)
            QueueDel(ent.Comp.Dummy);
    }
}