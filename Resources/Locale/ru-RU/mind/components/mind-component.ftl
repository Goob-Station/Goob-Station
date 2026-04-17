# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
# SPDX-FileCopyrightText: 2022 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

# MindContainerComponent localization

comp-mind-ghosting-prevented = Вы не можете стать призраком в данный момент.

## Messages displayed when a body is examined and in a certain state

comp-mind-examined-catatonic = { CAPITALIZE(SUBJECT($ent)) } в кататоническом ступоре. Стрессы жизни в глубоком космосе, должно быть, оказались слишком тяжелы для { OBJECT($ent) }. Любое восстановление невозможно.
comp-mind-examined-dead =
    { CAPITALIZE(SUBJECT($ent)) } { GENDER($ent) ->
        [male] мёртв
        [female] мертва
        [epicene] мертвы
       *[neuter] мертво
    }
comp-mind-examined-ssd = { CAPITALIZE(SUBJECT($ent)) } рассеяно смотрит в пустоту и ни на что не реагирует. { CAPITALIZE(SUBJECT($ent)) } может скоро придти в себя.
comp-mind-examined-dead-and-ssd = { CAPITALIZE(POSS-ADJ($ent)) } душа бездействует и может скоро вернуться.
comp-mind-examined-dead-and-irrecoverable = { CAPITALIZE(POSS-ADJ($ent)) } душа покинула тело и пропала. Любое восстановление невозможно.
