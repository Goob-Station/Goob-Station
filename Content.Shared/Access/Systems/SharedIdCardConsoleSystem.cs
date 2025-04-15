// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared.Access.Systems
{
    [UsedImplicitly]
    public abstract class SharedIdCardConsoleSystem : EntitySystem
    {
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly ILogManager _log = default!;

        public const string Sawmill = "idconsole";
        protected ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();
            _sawmill = _log.GetSawmill(Sawmill);

            SubscribeLocalEvent<IdCardConsoleComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<IdCardConsoleComponent, ComponentRemove>(OnComponentRemove);
        }

        private void OnComponentInit(EntityUid uid, IdCardConsoleComponent component, ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(uid, IdCardConsoleComponent.PrivilegedIdCardSlotId, component.PrivilegedIdSlot);
            _itemSlotsSystem.AddItemSlot(uid, IdCardConsoleComponent.TargetIdCardSlotId, component.TargetIdSlot);
        }

        private void OnComponentRemove(EntityUid uid, IdCardConsoleComponent component, ComponentRemove args)
        {
            _itemSlotsSystem.RemoveItemSlot(uid, component.PrivilegedIdSlot);
            _itemSlotsSystem.RemoveItemSlot(uid, component.TargetIdSlot);
        }

        [Serializable, NetSerializable]
        private sealed class IdCardConsoleComponentState : ComponentState
        {
            public List<string> AccessLevels;

            public IdCardConsoleComponentState(List<string> accessLevels)
            {
                AccessLevels = accessLevels;
            }
        }
    }
}