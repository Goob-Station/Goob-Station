# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

examinable-hunger-component-examine-overfed =
    { CAPITALIZE(SUBJECT($entity)) } { CONJUGATE-BASIC($entity, "выглядят", "выглядит") }
    { GENDER($entity) ->
        [male] сытым
        [female] сытой
        [epicene] сытыми
       *[neuter] сытым
    }.
examinable-hunger-component-examine-okay =
    { CAPITALIZE(SUBJECT($entity)) } { CONJUGATE-BASIC($entity, "выглядят", "выглядит") }
    { GENDER($entity) ->
        [male] довольным
        [female] довольной
        [epicene] довольными
       *[neuter] довольным
    }.
examinable-hunger-component-examine-peckish =
    { CAPITALIZE(SUBJECT($entity)) } { CONJUGATE-BASIC($entity, "выглядят", "выглядит") }
    { GENDER($entity) ->
        [male] проголодавшимся
        [female] проголодавшейся
        [epicene] проголодавшимися
       *[neuter] проголодавшимся
    }.
examinable-hunger-component-examine-starving =
    { CAPITALIZE(SUBJECT($entity)) } { CONJUGATE-BASIC($entity, "выглядят", "выглядит") }
    { GENDER($entity) ->
        [male] изголодавшимся
        [female] изголодавшейся
        [epicene] изголодавшимися
       *[neuter] изголодавшимся
    }!
examinable-hunger-component-examine-none = { CAPITALIZE(SUBJECT($entity)) }, похоже, не { CONJUGATE-BASIC($entity, "голодают", "голодает") }.
