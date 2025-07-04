# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 FoLoKe <36813380+FoLoKe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Remie Richards <remierichards@gmail.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 Eris <erisfiregamer1@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### Interaction Messages

# When trying to eat food without the required utensil... but you gotta hold it
food-you-need-to-hold-utensil = You need to be holding a {$utensil} to eat that!

food-nom = Nom. {$flavors}
food-swallow = You swallow the {$food}. {$flavors}

food-has-used-storage = You cannot eat the {$food} with an item stored inside.

food-system-remove-mask = You need to take off the {$entity} first.

## System

food-system-you-cannot-eat-any-more = You can't eat any more!
food-system-you-cannot-eat-any-more-other = {CAPITALIZE(SUBJECT($target))} can't eat any more!
food-system-try-use-food-is-empty = {CAPITALIZE(THE($entity))} is empty!
food-system-wrong-utensil = You can't eat {THE($food)} with {INDEFINITE($utensil)} {$utensil}.
food-system-cant-digest = You can't digest {THE($entity)}!
food-system-cant-digest-other = {CAPITALIZE(SUBJECT($target))} can't digest {THE($entity)}!

food-system-verb-eat = Eat

## Force feeding

food-system-force-feed = {CAPITALIZE(THE($user))} is trying to feed you something!
food-system-force-feed-success = {CAPITALIZE(THE($user))} forced you to eat something! {$flavors}
food-system-force-feed-success-user = You successfully feed {THE($target)}
