# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 freeman2651 <104049107+freeman2651@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DadeKuma <mattafix68@gmail.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

handcuff-component-target-self = Вы начинаете заковывать себя.
handcuff-component-cuffs-broken-error = Наручники сломаны!
handcuff-component-target-has-no-hands-error = { $targetName } не имеет рук!
handcuff-component-target-has-no-free-hands-error = { $targetName } не имеет свободных рук!
handcuff-component-too-far-away-error = Вы слишком далеко, чтобы использовать наручники!
handcuff-component-start-cuffing-observer = { $user } начинает заковывать { $target }!
handcuff-component-start-cuffing-self-observer = { $user } начинает заковывать { REFLEXIVE($target) } себя.
handcuff-component-start-cuffing-target-message = Вы начинаете заковывать { $targetName }.
handcuff-component-start-cuffing-by-other-message = { $otherName } начинает заковывать вас!
handcuff-component-cuff-observer-success-message =
    { $user } { GENDER($user) ->
        [male] заковал
        [female] заковала
        [epicene] заковали
       *[neuter] заковало
    } { $target }.
handcuff-component-cuff-self-observer-success-message =
    { $user } { GENDER($user) ->
        [male] заковал
        [female] заковала
        [epicene] заковали
       *[neuter] заковало
    } { REFLEXIVE($target) } себя.
handcuff-component-cuff-other-success-message = Вы успешно заковали { $otherName }.
handcuff-component-cuff-by-other-success-message =
    { $otherName } { GENDER($otherName) ->
        [male] заковал
        [female] заковала
        [epicene] заковали
       *[neuter] заковало
    } вас!
handcuff-component-cuff-self-success-message = Вы заковали себя.
handcuff-component-cuff-interrupt-message = Вам помешали заковать { $targetName }!
handcuff-component-cuff-interrupt-other-message = Вы помешали { $otherName } заковать вас!
handcuff-component-cuff-interrupt-self-message = Вам помешали заковать себя.
handcuff-component-cuff-interrupt-buckled-message = Вы не можете пристегнуться в наручниках!
handcuff-component-cuff-interrupt-unbuckled-message = Вы не можете отстегнуться в наручниках!
handcuff-component-cannot-drop-cuffs = Вы не можете надеть наручники на { $target }
