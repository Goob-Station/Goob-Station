// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alex Pavlenko <diraven@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Boaz1111 <149967078+Boaz1111@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ghagliiarghii <68826635+Ghagliiarghii@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Redfire1331 <125223432+Redfire1331@users.noreply.github.com>
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
// SPDX-FileCopyrightText: 2024 neutrino <67447925+neutrino-laser@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 redfire1331 <Redfire1331@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.DoAfter;

/// <summary>
///     Base type for events that get raised when a do-after is canceled or finished.
/// </summary>
[Serializable, NetSerializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class DoAfterEvent : HandledEntityEventArgs
{
    /// <summary>
    ///     The do after that triggered this event. This will be set by the do after system before the event is raised.
    /// </summary>
    [NonSerialized]
    public DoAfter DoAfter = default!;

    //TODO: User pref to toggle repeat on specific doafters
    /// <summary>
    ///     If set to true while handling this event, then the DoAfter will automatically be repeated.
    /// </summary>
    public bool Repeat = false;

    /// <summary>
    ///     Duplicate the current event. This is used by state handling, and should copy by value unless the reference
    ///     types are immutable.
    /// </summary>
    public abstract DoAfterEvent Clone();

    #region Convenience properties
    public bool Cancelled => DoAfter.Cancelled;
    public EntityUid User => DoAfter.Args.User;
    public EntityUid? Target => DoAfter.Args.Target;
    public EntityUid? Used => DoAfter.Args.Used;
    public DoAfterArgs Args => DoAfter.Args;
    #endregion

    /// <summary>
    /// Check whether this event is "the same" as another event for duplicate checking.
    /// </summary>
    public virtual bool IsDuplicate(DoAfterEvent other)
    {
        return GetType() == other.GetType();
    }
}

/// <summary>
///     Blank / empty event for simple do afters that carry no information.
/// </summary>
/// <remarks>
///     This just exists as a convenience to avoid having to re-implement Clone() for every simply DoAfterEvent.
///     If an event actually contains data, it should actually override Clone().
/// </remarks>
[Serializable, NetSerializable]
public abstract partial class SimpleDoAfterEvent : DoAfterEvent
{
    // TODO: Find some way to enforce that inheritors don't store data?
    // Alternatively, I just need to allow generics to be networked.
    // E.g., then a SimpleDoAfter<TEvent> would just raise a TEvent event.
    // But afaik generic event types currently can't be serialized for networking or YAML.

    public override DoAfterEvent Clone() => this;
}

// Placeholder for obsolete async do afters
[Serializable, NetSerializable]
[Obsolete("Dont use async DoAfters")]
public sealed partial class AwaitedDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     This event will optionally get raised every tick while a do-after is in progress to check whether the do-after
///     should be canceled.
/// </summary>
public sealed partial class DoAfterAttemptEvent<TEvent> : CancellableEntityEventArgs where TEvent : DoAfterEvent
{
    /// <summary>
    ///     The do after that triggered this event.
    /// </summary>
    public readonly DoAfter DoAfter;

    /// <summary>
    ///     The event that the DoAfter will raise after successfully finishing. Given that this event has the data
    ///     required to perform the interaction, it should also contain the data required to validate/attempt the
    ///     interaction.
    /// </summary>
    public readonly TEvent Event;

    public DoAfterAttemptEvent(DoAfter doAfter, TEvent @event)
    {
        DoAfter = doAfter;
        Event = @event;
    }
}