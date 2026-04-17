# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Abilities
changeling-biomass-deficit = Недостаточно биомассы!
changeling-chemicals-deficit = Недостаточно химикатов!

changeling-biomass-warn-first = Мы начинаем голодать...
changeling-biomass-warn-second = Наш голод вызывает боль...
changeling-biomass-warn-third = Наш голод сказывается на нашей форме...
changeling-biomass-warn-death = Наши клетки начинают поедать друг друга. Пути назад нет.

changeling-absorb-start = { CAPITALIZE($user) } начинает поглощать ДНК { CAPITALIZE($target) }!
changeling-absorb-fail-incapacitated = Нельзя поглотить, пока цель не обездвижена.
changeling-absorb-fail-absorbed = Цель уже поглощена.
changeling-absorb-fail-unabsorbable = Цель непоглощаема.
changeling-absorb-end-self = Организм поглощён. Мы эволюционируем.
changeling-absorb-end-self-ling = Другой генокрад поглощён. Тело наполняется силой, клетки стремительно эволюционируют.
changeling-absorb-end-self-ling-incompatible = Другой генокрад поглощён. Однако его текущая форма не позволила нам извлечь его ДНК.
changeling-absorb-end-partial = Организм был поглощён. Нам не удалось извлечь ничего, что помогло бы нашему развитию.
changeling-absorb-onexamine = [color=red]Тело ощущается пустым.[/color]
changeling-absorb-fail-nograb = Мы недостаточно крепко схватили цель.
changeling-absorb-fail-onfire = Цель в огне, сначала потушите её!


changeling-absorbbiomatter-start = { $user } начинает поглощать пищу!
changeling-absorbbiomatter-bad-food = Эта пища непоглощаема.

changeling-transform-cycle = Переключились на ДНК { $target }.
changeling-transform-cycle-empty = У нас нет ДНК для трансформации!
changeling-transform-others = Тело { CAPITALIZE($user) } искажается и принимает облик другого существа!
changeling-transform-fail-self = Нельзя трансформироваться в текущую форму!
changeling-transform-fail-choose = Мы не выбрали форму для трансформации!
changeling-transform-fail-absorbed = Мы не можем трансформироваться в покойника!
changeling-transform-finish = Теперь мы — { $target }.

changeling-sting = Мы тайно ужалили { CAPITALIZE($target) }.
changeling-sting-fail-self = Мы пытались ужалить { CAPITALIZE($target) }, но нам что-то помешало!
changeling-sting-fail-ling = Кто-то пытался тайно ужалить нас!
changeling-sting-fail-fakeweapon = Они не смогут противостоять искусственному оружию.
changeling-sting-fail-hollow = Мы не в состоянии ужалить пустой организм.

changeling-sting-extract-fail-duplicate = Мы уже извлекали эту ДНК ранее.
changeling-sting-extract-fail-lesser = Мы не можем извлечь ДНК из низшего существа!
changeling-sting-extract-max = Сначала нужно избавиться от сохранённой ДНК.

changeling-dartgun-no-stings = У нас нет эволюционированных жал!

changeling-stasis-enter = Мы входим в регенеративный стазис.
changeling-stasis-enter-damaged = Мы вошли в регенеративный стазис. Полученные травмы будет трудно залечить...
changeling-stasis-enter-dead = We enter regenerative stasis. Our catastrophic injuries will take extreme time to heal...
changeling-stasis-exit = Мы выходим из регенеративного стазиса.
changeling-stasis-absorbed = We have no control over our cells. Our body is silent. It is over.
changeling-stasis-defib = A shock pulses through us. Our stasis has been interrupted!

changeling-regenerate = Our body is instantly cleared of any wounds and broken bones.
changeling-regenerate-limbs = Our body emits a loud crack as missing limbs, wounds and broken bones are instantly regenerated!

changeling-fail-hands = Сначала нужно освободить руки.

changeling-muscles-start = Тело стало легче.
changeling-muscles-end = Ноги стали тяжелее.

changeling-equip-armor-fail = Сначала нужно снять верхнюю одежду.

changeling-inject = Мы делаем себе инъекцию.
changeling-inject-fail = Не удалось сделать инъекцию!

changeling-passive-activate = Способность активирована.
changeling-passive-activate-fail = Не удалось активировать способность.
changeling-passive-active = Уже активирована!

changeling-action-fail-onfire = Наши клетки бьются в агонии, не в силах применить эту способность!
changeling-action-fail-lesserform = Нельзя использовать в уменьшенной форме!
changeling-action-fail-absorbed = Нужно поглотить ещё { $number } организмов, чтобы использовать это!
changeling-action-fail-not-changeling = Бро, у тебя не может быть этой способности. Репортни этот баг.

changeling-fleshmend = Начинаем запечатывать раны и восстанавливать мёртвые клетки.
changeling-panacea = Начинаем восстанавливать клеточную структуру и иммунитет.
changeling-adrenaline = Мы вводим в наше тело высокоэффективный адреналин.

changeling-chameleon-start = Мы адаптируем кожу к окружающей среде.
changeling-chameleon-end = Кожа теряет прозрачность.
changeling-chameleon-fire = Our translucency is lost as the flames burn us!

changeling-voidadapt-lowpressure-start = Мы адаптируемся к окружающему низкому давлению.
changeling-voidadapt-lowpressure-end = Окружающее давление больше не низкое. Сбрасываем адаптацию.
changeling-voidadapt-lowtemperature-start = Мы адаптируемся к окружающей холодной температуре.
changeling-voidadapt-lowtemperature-end = Окружающая температура стала теплее. Сбрасываем адаптацию.
changeling-voidadapt-onfire = Наши адаптации становятся слишком болезненными в огне! Мы сбрасываем их!

changeling-hivemind-start = Мы настраиваем разум на частоту колониального сознания.

changeling-lastresort-activate = ТЕКУЩЕЕ ТЕЛО БУДЕТ ПОТЕРЯНО! Используйте снова для подтверждения.

changeling-rejuvenate = Странная энергия пульсирует в вашем теле, вылечивая ваши клетки и восстанавливая химикаты!
