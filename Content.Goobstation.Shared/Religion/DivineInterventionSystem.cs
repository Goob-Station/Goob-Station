// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;
using Content.Goobstation.Shared.Bible;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Religion
{

    public sealed partial class DivineInterventionSystem : EntitySystem
    {

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DivineInterventionComponent, EntityTargetActionEvent>(OnEntityTargetAction);
        }

        private void OnEntityTargetAction(EntityUid uid, DivineInterventionComponent component, EntityTargetActionEvent args)
        {
            if (args.Target == null)
                return;

            var targetEntity = args.Target;

            // Use reflection to get the Type from the string.
            var type = _reflectionManager.LooseGetType(component.BlockedComponent);

            if (type == null)
            {
                Log.Error($"Invalid component type specified in DivineInterventionComponent: {component.BlockedComponent}");
                return;
            }

            if (_entityManager.HasComponent(targetEntity, type))
            {
                args.Handled = true; //Marked as handled so other systems don't process it.
            }

        }
    }
}
