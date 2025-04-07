// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Stacks;
using Content.Shared.Storage.Components;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared.Storage.EntitySystems
{
    [UsedImplicitly]
    public abstract class SharedItemCounterSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ItemCounterComponent, EntInsertedIntoContainerMessage>(CounterEntityInserted);
            SubscribeLocalEvent<ItemCounterComponent, EntRemovedFromContainerMessage>(CounterEntityRemoved);
        }

        private void CounterEntityInserted(EntityUid uid, ItemCounterComponent itemCounter,
            EntInsertedIntoContainerMessage args)
        {
            if (!EntityManager.TryGetComponent(uid, out AppearanceComponent? appearanceComponent))
                return;

            var count = GetCount(args, itemCounter);
            if (count == null)
                return;

            _appearance.SetData(uid, StackVisuals.Actual, count, appearanceComponent);

            if (itemCounter.MaxAmount != null)
                _appearance.SetData(uid, StackVisuals.MaxCount, itemCounter.MaxAmount, appearanceComponent);
        }

        private void CounterEntityRemoved(EntityUid uid, ItemCounterComponent itemCounter,
            EntRemovedFromContainerMessage args)
        {
            if (!EntityManager.TryGetComponent(uid, out AppearanceComponent? appearanceComponent))
                return;

            var count = GetCount(args, itemCounter);
            if (count == null)
                return;

            _appearance.SetData(uid, StackVisuals.Actual, count, appearanceComponent);
            if (itemCounter.MaxAmount != null)
                _appearance.SetData(uid, StackVisuals.MaxCount, itemCounter.MaxAmount, appearanceComponent);
        }

        protected abstract int? GetCount(ContainerModifiedMessage msg, ItemCounterComponent itemCounter);
    }
}