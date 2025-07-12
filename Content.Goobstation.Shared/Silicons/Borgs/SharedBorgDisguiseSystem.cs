// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Silicons.Borgs;

/// <summary>
/// Manages Borg disguises, such as the Syndicate Saboteur's chameleon projector.
/// </summary>
public abstract class SharedBorgDisguiseSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Goobstation.Shared.Silicons.Borgs.BorgDisguiseComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<Goobstation.Shared.Silicons.Borgs.BorgDisguiseComponent, ComponentShutdown>(OnCompRemove);
    }

    /// <summary>
    /// Swaps the shared parts of the entity's components based on the disguise state.
    /// </summary>
    /// <param name="uid">The entity to swap</param>
    /// <param name="comp">The component to use for getting the disguise state and description.</param>
    protected void UpdateSharedAppearance(EntityUid uid, Goobstation.Shared.Silicons.Borgs.BorgDisguiseComponent comp)
    {
        if (TryPrototype(uid, out var entityPrototype))
        {
            _meta.SetEntityDescription(uid, comp.Disguised ? comp.Description : entityPrototype.Description);
        }
    }

    #region ActionManagement

    /// <summary>
    /// Gives the action to disguise
    /// </summary>
    private void OnMapInit(EntityUid uid, Goobstation.Shared.Silicons.Borgs.BorgDisguiseComponent comp, MapInitEvent args)
    {
        _actionsSystem.AddAction(uid, ref comp.ActionEntity, comp.Action);
    }

    /// <summary>
    /// Takes away the action to disguise from the entity.
    /// </summary>
    private void OnCompRemove(EntityUid uid, Goobstation.Shared.Silicons.Borgs.BorgDisguiseComponent comp, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, comp.ActionEntity);
    }

    #endregion
}

/// <summary>
/// Should be relayed upon using the action.
/// </summary>
public sealed partial class BorgDisguiseToggleActionEvent : InstantActionEvent
{
}
