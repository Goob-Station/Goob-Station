// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Components;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Foldable;

public sealed class FoldableSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoldableComponent, GetVerbsEvent<AlternativeVerb>>(AddFoldVerb);
        SubscribeLocalEvent<FoldableComponent, AfterAutoHandleStateEvent>(OnHandleState);

        SubscribeLocalEvent<FoldableComponent, ComponentInit>(OnFoldableInit);
        SubscribeLocalEvent<FoldableComponent, ContainerGettingInsertedAttemptEvent>(OnInsertEvent);
        SubscribeLocalEvent<FoldableComponent, InsertIntoEntityStorageAttemptEvent>(OnStoreThisAttempt);
        SubscribeLocalEvent<FoldableComponent, StorageOpenAttemptEvent>(OnFoldableOpenAttempt);

        SubscribeLocalEvent<FoldableComponent, StrapAttemptEvent>(OnStrapAttempt);
    }

    private void OnHandleState(EntityUid uid, FoldableComponent component, ref AfterAutoHandleStateEvent args)
    {
        SetFolded(uid, component, component.IsFolded);
    }

    private void OnFoldableInit(EntityUid uid, FoldableComponent component, ComponentInit args)
    {
        SetFolded(uid, component, component.IsFolded);
    }

    private void OnFoldableOpenAttempt(EntityUid uid, FoldableComponent component, ref StorageOpenAttemptEvent args)
    {
        if (component.IsFolded)
            args.Cancelled = true;
    }

    public void OnStoreThisAttempt(EntityUid uid, FoldableComponent comp, ref InsertIntoEntityStorageAttemptEvent args)
    {
        if (comp.IsFolded)
            args.Cancelled = true;
    }

    public void OnStrapAttempt(EntityUid uid, FoldableComponent comp, ref StrapAttemptEvent args)
    {
        if (comp.IsFolded)
            args.Cancelled = true;
    }

    /// <summary>
    /// Returns false if the entity isn't foldable.
    /// </summary>
    public bool IsFolded(EntityUid uid, FoldableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        return component.IsFolded;
    }

    /// <summary>
    /// Set the folded state of the given <see cref="FoldableComponent"/>
    /// </summary>
    public void SetFolded(EntityUid uid, FoldableComponent component, bool folded)
    {
        component.IsFolded = folded;
        Dirty(uid, component);
        _appearance.SetData(uid, FoldedVisuals.State, folded);
        _buckle.StrapSetEnabled(uid, !component.IsFolded);

        var ev = new FoldedEvent(folded);
        RaiseLocalEvent(uid, ref ev);
    }

    private void OnInsertEvent(EntityUid uid, FoldableComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        if (!component.IsFolded && !component.CanFoldInsideContainer)
            args.Cancel();
    }

    public bool TryToggleFold(EntityUid uid, FoldableComponent comp)
    {
        return TrySetFolded(uid, comp, !comp.IsFolded);
    }

    public bool CanToggleFold(EntityUid uid, FoldableComponent? fold = null)
    {
        if (!Resolve(uid, ref fold))
            return false;

        // Can't un-fold in any container unless enabled (locker, hands, inventory, whatever).
        if (_container.IsEntityInContainer(uid) && !fold.CanFoldInsideContainer)
            return false;

        var ev = new FoldAttemptEvent();
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }

    /// <summary>
    /// Try to fold/unfold
    /// </summary>
    public bool TrySetFolded(EntityUid uid, FoldableComponent comp, bool state)
    {
        if (state == comp.IsFolded)
            return false;

        if (!CanToggleFold(uid, comp))
            return false;

        SetFolded(uid, comp, state);
        return true;
    }

    #region Verb

    private void AddFoldVerb(EntityUid uid, FoldableComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || !CanToggleFold(uid, component))
            return;

        AlternativeVerb verb = new()
        {
            Act = () => TryToggleFold(uid, component),
            Text = component.IsFolded ? Loc.GetString(component.UnfoldVerbText) : Loc.GetString(component.FoldVerbText),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/fold.svg.192dpi.png")),

            // If the object is unfolded and they click it, they want to fold it, if it's folded, they want to pick it up
            Priority = component.IsFolded ? 0 : 2,
        };

        args.Verbs.Add(verb);
    }

    #endregion

    [Serializable, NetSerializable]
    public enum FoldedVisuals : byte
    {
        State
    }
}

/// <summary>
/// Event raised on an entity to determine if it can be folded.
/// </summary>
/// <param name="Cancelled"></param>
[ByRefEvent]
public record struct FoldAttemptEvent(bool Cancelled = false);

/// <summary>
/// Event raised on an entity after it has been folded.
/// </summary>
/// <param name="IsFolded"></param>
[ByRefEvent]
public readonly record struct FoldedEvent(bool IsFolded);