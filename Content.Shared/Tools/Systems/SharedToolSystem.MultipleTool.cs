// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Linq;
using Content.Shared.Interaction;
using Content.Shared.Prying.Components;
using Content.Shared.Tools.Components;

namespace Content.Shared.Tools.Systems;

public abstract partial class SharedToolSystem
{
    public void InitializeMultipleTool()
    {
        SubscribeLocalEvent<MultipleToolComponent, ComponentStartup>(OnMultipleToolStartup);
        SubscribeLocalEvent<MultipleToolComponent, ActivateInWorldEvent>(OnMultipleToolActivated);
        SubscribeLocalEvent<MultipleToolComponent, AfterAutoHandleStateEvent>(OnMultipleToolHandleState);
    }

    private void OnMultipleToolHandleState(EntityUid uid, MultipleToolComponent component, ref AfterAutoHandleStateEvent args)
    {
        SetMultipleTool(uid, component);
    }

    private void OnMultipleToolStartup(EntityUid uid, MultipleToolComponent multiple, ComponentStartup args)
    {
        // Only set the multiple tool if we have a tool component.
        if (EntityManager.TryGetComponent(uid, out ToolComponent? tool))
            SetMultipleTool(uid, multiple, tool);
    }

    private void OnMultipleToolActivated(EntityUid uid, MultipleToolComponent multiple, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        args.Handled = CycleMultipleTool(uid, multiple, args.User);
    }

    public bool CycleMultipleTool(EntityUid uid, MultipleToolComponent? multiple = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref multiple))
            return false;

        if (multiple.Entries.Length == 0)
            return false;

        multiple.CurrentEntry = (uint)((multiple.CurrentEntry + 1) % multiple.Entries.Length);
        SetMultipleTool(uid, multiple, playSound: true, user: user);

        return true;
    }

    public virtual void SetMultipleTool(EntityUid uid,
        MultipleToolComponent? multiple = null,
        ToolComponent? tool = null,
        bool playSound = false,
        EntityUid? user = null)
    {
        if (!Resolve(uid, ref multiple, ref tool))
            return;

        Dirty(uid, multiple);

        if (multiple.Entries.Length <= multiple.CurrentEntry)
        {
            multiple.CurrentQualityName = Loc.GetString("multiple-tool-component-no-behavior");
            return;
        }

        var current = multiple.Entries[multiple.CurrentEntry];
        tool.UseSound = current.UseSound;
        tool.Qualities = current.Behavior;

        // TODO: Replace this with a better solution later
        if (TryComp<PryingComponent>(uid, out var pryComp))
        {
            pryComp.Enabled = current.Behavior.Contains("Prying");
        }

        if (playSound && current.ChangeSound != null)
            _audioSystem.PlayPredicted(current.ChangeSound, uid, user);

        if (_protoMan.TryIndex(current.Behavior.First(), out ToolQualityPrototype? quality))
            multiple.CurrentQualityName = Loc.GetString(quality.Name);
    }
}
