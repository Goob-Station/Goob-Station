// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
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

using Content.Shared.Ghost;
using Content.Shared.IdentityManagement.Components;
using Robust.Shared.Utility; // Goob

namespace Content.Shared.IdentityManagement;

/// <summary>
///     Static content API for getting the identity entities/names for a given entity.
///     This should almost always be used in favor of metadata name, if the entity in question is a human player that
///     can have identity.
/// </summary>
public static class Identity
{
    /// <summary>
    ///     Returns the name that should be used for this entity for identity purposes.
    /// </summary>
    /// <remarks>
    /// This will return the true identity of the entity if called before the
    /// identity component has been initialized — this may occur for example if
    /// the client raises an event in response to an entity entering PVS for
    /// the first time.
    /// </remarks>
    public static string Name(EntityUid uid, IEntityManager ent, EntityUid? viewer = null)
    {
        if (!uid.IsValid())
            return string.Empty;

        var meta = ent.GetComponent<MetaDataComponent>(uid);
        if (meta.EntityLifeStage <= EntityLifeStage.Initializing)
            return FormattedMessage.EscapeText(meta.EntityName); // Goob Sanitize Text

        var uidName = meta.EntityName;

        if (!ent.TryGetComponent<IdentityComponent>(uid, out var identity))
            return FormattedMessage.EscapeText(uidName); // Goob Sanitize Text

        var ident = identity.IdentityEntitySlot?.ContainedEntity;
        if (ident is null)
            return FormattedMessage.EscapeText(uidName); // Goob Sanitize Text

        var identName = ent.GetComponent<MetaDataComponent>(ident.Value).EntityName;
        if (viewer == null || !CanSeeThroughIdentity(uid, viewer.Value, ent))
        {
            return FormattedMessage.EscapeText(identName); // Goob Sanitize Text
        }
        if (uidName == identName)
        {
            return FormattedMessage.EscapeText(uidName); // Goob Sanitize Text
        }

        return $"{FormattedMessage.EscapeText(uidName)} ({FormattedMessage.EscapeText(identName)})"; // Goob Sanitize Text
    }

    /// <summary>
    ///     Returns the entity that should be used for identity purposes, for example to pass into localization.
    ///     This is an extension method because of its simplicity, and if it was any harder to call it might not
    ///     be used enough for loc.
    /// </summary>
    /// <param name="viewer">
    ///     If this entity can see through identities, this method will always return the actual target entity.
    /// </param>
    /// <inheritdoc cref="Name" path="remarks" />
    public static EntityUid Entity(EntityUid uid, IEntityManager ent, EntityUid? viewer = null)
    {
        if (!ent.TryGetComponent<IdentityComponent>(uid, out var identity))
            return uid;

        if (viewer != null && CanSeeThroughIdentity(uid, viewer.Value, ent))
            return uid;

        return identity.IdentityEntitySlot?.ContainedEntity ?? uid;
    }

    public static bool CanSeeThroughIdentity(EntityUid uid, EntityUid viewer, IEntityManager ent)
    {
        // Would check for uid == viewer here but I think it's better for you to see yourself
        // how everyone else will see you, otherwise people will probably get confused and think they aren't disguised
        return ent.HasComponent<GhostComponent>(viewer);
    }

}
