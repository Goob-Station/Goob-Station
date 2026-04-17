# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

bloodstream-component-looks-pale = [color=bisque]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-BASIC($target, "выглядят", "выглядит") } бледно.[/color]
bloodstream-component-slight-bleeding = [color=#893843]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-BE($target) } немного истекает кровью.[/color]
bloodstream-component-bleeding = [color=red]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-BASIC($target, "истекают", "истекает") } кровью.[/color]
bloodstream-component-profusely-bleeding = [color=crimson]{ CAPITALIZE(SUBJECT($target)) } обильно { CONJUGATE-BASIC($target, "истекают", "истекает") } кровью![/color]
bloodstream-component-massive-bleeding = [color=#420000]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-BE($target) } обильное кровотечение.[/color]

bloodstream-component-wounds-cauterized = С болью вы ощущаете, как ваши раны прижигаются!
