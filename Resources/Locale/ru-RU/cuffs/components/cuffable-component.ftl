# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cuffable-component-cannot-interact-message = Вы не можете этого сделать!
cuffable-component-cannot-remove-cuffs-too-far-message = Вы слишком далеко, чтобы снять наручники.

cuffable-component-start-uncuffing-self = Вы начинаете мучительно выкручиваться из наручников.
cuffable-component-start-uncuffing-observer = { $user } начинает расковывать { $target }!
cuffable-component-start-uncuffing-self-observer = { $user } начинает расковывать { REFLEXIVE($target) } себя!
cuffable-component-start-uncuffing-target-message = Вы начинаете расковывать { $targetName }.
cuffable-component-start-uncuffing-by-other-message = { $otherName } начинает расковывать вас!

cuffable-component-remove-cuffs-success-message = Вы успешно снимаете наручники.
cuffable-component-remove-cuffs-push-success-message = Вы успешно снимаете наручники и толкаете { $otherName } на пол.
cuffable-component-remove-cuffs-by-other-success-message = { $otherName } снимает с вас наручники.
cuffable-component-remove-cuffs-to-other-partial-success-message =
    Вы успешно снимаете наручники. { $cuffedHandCount } { $cuffedHandCount ->
        [one] рука осталась
        [few] руки остались
       *[other] рук остались
    } у { $otherName } в наручниках.
cuffable-component-remove-cuffs-by-other-partial-success-message =
    { $otherName } успешно снимает с вас наручники. { $cuffedHandCount } { $cuffedHandCount ->
        [one] ваша рука осталась
        [few] ваших руки остались
       *[other] ваших рук остались
    } в наручниках.
cuffable-component-remove-cuffs-partial-success-message =
    Вы успешно снимаете наручники. { $cuffedHandCount } { $cuffedHandCount ->
        [one] ваша рука осталась
        [few] ваших руки остались
       *[other] ваших рук остались
    } в наручниках.
cuffable-component-remove-cuffs-fail-message = Вам не удалось снять наручники.

# UnrestrainVerb
uncuff-verb-get-data-text = Освободить
