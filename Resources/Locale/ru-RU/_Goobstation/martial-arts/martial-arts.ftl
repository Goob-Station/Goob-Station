# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cqc-fail-notself = Вы не можете никого обучить при помощи { CAPITALIZE($manual) }.

cqc-fail-changeling = Мы вышли за пределы использования искусства.
cqc-fail-knowanother = Вы уже знаете другое боевое искусство.
cqc-fail-already = Вы уже знаете всё о боевом искусстве.
cqc-success-unblocked = Ваши навыки рукопашного боя больше не привязаны к кухне.
cqc-success-learned = Вы изучили рукопашный бой.
capoeira-success-learned = Вы освоили капоэйру. Учебник сгорает у вас в руках...
dragon-success-learned = Вы освоили стиль Дракона (Кунг-фу). Учебник сгорает у вас в руках...
ninjutsu-success-learned = Вы освоили ниндзюцу. Свиток сгорает у вас в руках...
hellrip-success-learned = Вы освоили Адский разрыв. Свиток сгорает у вас в руках...

carp-scroll-waiting = Путь в тысячу миль начинается с одного шага, а путь мудрости проходит медленно, урок за уроком.
carp-scroll-advance = Вы сделали ещё один шаг к мастерству Пути Спящего Карпа.
carp-scroll-complete = Теперь вы мастер Пути Спящего Карпа.

carp-saying-huah = ХУА!
carv-vaying-hya = ХИЯ!
carp-saying-choo = ЧУ!
carp-saying-wuo = ВУО!
carp-saying-kya = КЯ!
carp-saying-huh = ХА!
carp-saying-hiyoh = ХИЙО!
carp-saying-strike = УДАР КАРПА!
carp-saying-bite = КУСЬ КАРПА!

carp-saying-banzai = БАНЗАЙ!!
carp-saying-kiya = КИЯААА!
carp-saying-omae = ОМАЕ ВА МОУ СИНДЭИРУ!
carp-saying-see = ТЫ МЕНЯ НЕ ВИДИШЬ!
carp-saying-time = МОЁ ВРЕМЯ ПРИШЛО!!
carp-saying-cowabunga = КАВАБАНГА!

krav-maga-ready =
    { GENDER($user) ->
        [male] Вы готовите
        [female] Вы готовите
        [epicene] Вы готовите
       *[neuter] Вы готовите
    } { $action }.

martial-arts-action-sender =
    { GENDER($user) ->
        [male] Вы ударили
        [female] Вы ударили
        [epicene] Вы ударили
       *[neuter] Вы ударили
    } { $name } { $move }.
martial-arts-action-receiver =
    { $name } { GENDER($name) ->
        [male] ударил
        [female] ударила
        [epicene] ударили
       *[neuter] ударило
    } тебя { $move }.

martial-arts-fail-prone = Нельзя использовать этот приём в лежачем положении!
martial-arts-fail-target-down = Нельзя использовать этот приём на лежащей цели!
martial-arts-fail-target-standing = Нельзя использовать этот приём на стоящей цели!
capoeira-fail-low-velocity = Вы слишком медленны для этого приёма!
ninjutsu-fail-loss-of-surprise = Ваши намерения раскрыты! Этот приём сейчас невозможен!

alerts-dragon-power-name = Сила Дракона
alerts-dragon-power-desc = Вы размышляете о прошлых и будущих битвах. Это озарение защитит вас от будущих атак.

alerts-sneak-attack-name = Скрытая атака
alerts-sneak-attack-desc = Для истинного синоби первая и последняя атака — одно и то же.

alerts-loss-of-surprise-name = Потеря элемента неожиданности
alerts-loss-of-surprise-desc = Ваши намерения раскрыты! Потребуется время, чтобы снова скрытно атаковать.
