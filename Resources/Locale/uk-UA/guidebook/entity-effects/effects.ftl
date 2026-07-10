# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 mhamster <81412348+mhamsterr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Eris <eris@erisws.com>
# SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Gotimanga <127038462+Gotimanga@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
# SPDX-FileCopyrightText: 2024 Zonespace <41448081+Zonespace27@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 alex-georgeff <54858069+taurie@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

-create-3rd-person = { $chance ->
        [1] створює
        *[other] створює
    }
-cause-3rd-person = { $chance ->
        [1] Причини
        *[other] причина
    }
-satiate-3rd-person = { $chance ->
        [1] насичує
        *[other] насичує
    }
entity-effect-guidebook-spawn-entity = { $chance ->
        [1] Створює
        *[other] створити
    } { $amount ->
        [1] {INDEFINITE($entname)}
        *[other] {$amount} {MAKEPLURAL($entname)}
    }
entity-effect-guidebook-destroy = { $chance ->
        [1] Знищує
        *[other] знищити
    } обєкт
entity-effect-guidebook-break = { $chance ->
        [1] Ламає
        *[other] ламати
    } обєкт
entity-effect-guidebook-explosion = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } вибух
entity-effect-guidebook-emp = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } електромагнітний імпульс
entity-effect-guidebook-flash = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } сліпучий спалах
entity-effect-guidebook-foam-area = { $chance ->
        [1] Створює
        *[other] створити
    } велику кількість піни
entity-effect-guidebook-smoke-area = { $chance ->
        [1] Створює
        *[other] створити
    } велику кількість диму
entity-effect-guidebook-satiate-thirst = { $chance ->
        [1] Втамовує
        *[other] втамувати
    } { $relative ->
        [1] спрагу із середньою швидкістю
        *[other] спрагу зі швидкістю {NATURALFIXED($relative, 3)}x від середньої
    }
entity-effect-guidebook-satiate-hunger = { $chance ->
        [1] Втамовує
        *[other] втамувати
    } { $relative ->
        [1] голод із середньою швидкістю
        *[other] голод зі швидкістю {NATURALFIXED($relative, 3)}x від середньої
    }
entity-effect-guidebook-health-change = { $chance ->
        [1] { $healsordeals ->
                [heals] Лікує
                [deals] Завдає шкоди
                *[both] Змінює здоровя на
             }
        *[other] { $healsordeals ->
                    [heals] лікувати
                    [deals] завдавати шкоди
                    *[both] змінювати здоровя на
                 }
    } { $changes }
entity-effect-guidebook-even-health-change = { $chance ->
        [1] { $healsordeals ->
            [heals] Рівномірно лікує
            [deals] Рівномірно завдає шкоди
            *[both] Рівномірно змінює здоровя на
        }
        *[other] { $healsordeals ->
            [heals] рівномірно лікувати
            [deals] рівномірно завдавати шкоди
            *[both] рівномірно змінювати здоровя на
        }
    } { $changes }
entity-effect-guidebook-status-effect-old = { $type ->
        [update]{ $chance ->
                    [1] Спричиняє
                     *[other] спричинити
                 } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. без накопичення
        [add]   { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. з накопиченням
        [set]  { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} на {NATURALFIXED($time, 3)} сек. без накопичення
        *[remove]{ $chance ->
                    [1] Видаляє
                    *[other] видалити
                } {NATURALFIXED($time, 3)} сек. {LOC($key)}
    }
entity-effect-guidebook-status-effect = { $type ->
    [update]{ $chance ->
    [1] Спричиняє
    *[other] спричинити
                     } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. без накопичення
    [add]   { $chance ->
    [1] Спричиняє
    *[other] спричинити
                    } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. з накопиченням
    [set]  { $chance ->
    [1] Спричиняє
    *[other] спричинити
                    } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. без накопичення
    *[remove]{ $chance ->
    [1] Видаляє
    *[other] видалити
                    } {NATURALFIXED($time, 3)} сек. {LOC($key)}
        } { $delay ->
    [0] негайно
    *[other] після затримки {NATURALFIXED($delay, 3)} секунди
        }
reagent-effect-guidebook-status-effect-delay = { $type ->
        [add]   { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. з накопиченням
        *[set]  { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. без накопичення
        [remove]{ $chance ->
                    [1] Видаляє
                    *[other] видалити
                } {NATURALFIXED($time, 3)} сек. {LOC($key)}
    } після затримки {NATURALFIXED($delay, 3)} секунди
entity-effect-guidebook-status-effect-indef = { $type ->
    [update]{ $chance ->
    [1] Спричиняє
    *[other] спричинити
                     } постійний {LOC($key)}
    [add]   { $chance ->
    [1] Спричиняє
    *[other] спричинити
                    } постійний {LOC($key)}
    [set]  { $chance ->
    [1] Спричиняє
    *[other] спричинити
                    } постійний {LOC($key)}
    *[remove]{ $chance ->
    [1] Видаляє
    *[other] видалити
                    } {LOC($key)}
        } { $delay ->
    [0] негайно
    *[other] після затримки {NATURALFIXED($delay, 3)} секунди
        }
reagent-effect-guidebook-knockdown = { $type ->
        [add]   { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. з накопиченням
        *[set]  { $chance ->
                    [1] Спричиняє
                    *[other] спричинити
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} сек. без накопичення
        [remove]{ $chance ->
                    [1] Видаляє
                    *[other] видалити
                } {NATURALFIXED($time, 3)} сек. {LOC($key)}
    } після затримки {NATURALFIXED($delay, 3)} секунди
entity-effect-guidebook-set-solution-temperature-effect = { $chance ->
        [1] Встановлює
        *[other] встановити
    } температуру розчину рівно на {NATURALFIXED($temperature, 2)}k
entity-effect-guidebook-adjust-solution-temperature-effect = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Забирає
            }
        *[other]
            { $deltasign ->
                [1] додати
                *[-1] забрати
            }
    } тепло { $deltasign ->
                [1] до
                *[-1] з
           } розчину, доки він не досягне { $deltasign ->
                [1] щонайбільше {NATURALFIXED($maxtemp, 2)}k
                *[-1] щонайменше {NATURALFIXED($mintemp, 2)}k
            }
entity-effect-guidebook-adjust-reagent-reagent = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додати
                *[-1] видалити
            }
    } {NATURALFIXED($amount, 2)}u {$reagent} { $deltasign ->
        [1] до
        *[-1] з
    } розчину
entity-effect-guidebook-adjust-reagent-group = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додати
                *[-1] видалити
            }
    } {NATURALFIXED($amount, 2)}u реагентів з групи {$group} { $deltasign ->
            [1] до
            *[-1] з
        } розчину
entity-effect-guidebook-adjust-temperature = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Забирає
            }
        *[other]
            { $deltasign ->
                [1] додати
                *[-1] забрати
            }
    } {POWERJOULES($amount)} тепла { $deltasign ->
            [1] до
            *[-1] з
        } тіла, в якому перебуває
entity-effect-guidebook-chem-cause-disease = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } хворобу { $disease }
entity-effect-guidebook-chem-cause-random-disease = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } хвороби { $diseases }
entity-effect-guidebook-jittering = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } тремтіння
entity-effect-guidebook-clean-bloodstream = { $chance ->
        [1] Очищує
        *[other] очистити
    } кровотік від інших хімікатів
entity-effect-guidebook-cure-disease = { $chance ->
        [1] Лікує
        *[other] вилікувати
    } хвороби
entity-effect-guidebook-eye-damage = { $chance ->
        [1] { $deltasign ->
                [1] Завдає
                *[-1] Лікує
            }
        *[other]
            { $deltasign ->
                [1] завдати
                *[-1] вилікувати
            }
    } пошкодження очей
entity-effect-guidebook-vomit = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } блювання
entity-effect-guidebook-create-gas = { $chance ->
        [1] Створює
        *[other] створити
    } { $moles } { $moles ->
        [1] моль
        *[other] молів
    } { $gas }
entity-effect-guidebook-drunk = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } спяніння
entity-effect-guidebook-electrocute = { $chance ->
        [1] Бє струмом
        *[other] бити струмом
    } метаболізатор протягом {NATURALFIXED($time, 3)} сек.
entity-effect-guidebook-emote = { $chance ->
        [1] Змусить
        *[other] змусити
    } метаболізатор виконати [bold][color=white]{$emote}[/color][/bold]
entity-effect-guidebook-extinguish-reaction = { $chance ->
        [1] Гасить
        *[other] загасити
    } вогонь
entity-effect-guidebook-flammable-reaction = { $chance ->
        [1] Збільшує
        *[other] збільшити
    } займистість
entity-effect-guidebook-ignite = { $chance ->
        [1] Підпалює
        *[other] підпалити
    } метаболізатор
entity-effect-guidebook-make-sentient = { $chance ->
        [1] Робить
        *[other] зробити
    } метаболізатор розумним
entity-effect-guidebook-make-polymorph = { $chance ->
        [1] Поліморфує
        *[other] поліморфувати
    } метаболізатор у { $entityname }
entity-effect-guidebook-modify-bleed-amount = { $chance ->
        [1] { $deltasign ->
                [1] Спричиняє
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] спричинити
                    *[-1] зменшити
                 }
    } кровотечу
entity-effect-guidebook-modify-blood-level = { $chance ->
        [1] { $deltasign ->
                [1] Збільшує
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] збільшувати
                    *[-1] зменшувати
                 }
    } рівень крові
entity-effect-guidebook-paralyze = { $chance ->
        [1] Паралізує
        *[other] паралізувати
    } метаболізатор щонайменше на {NATURALFIXED($time, 3)} сек.
entity-effect-guidebook-movespeed-modifier = { $chance ->
        [1] Змінює
        *[other] змінити
    } швидкість руху на {NATURALFIXED($sprintspeed, 3)}x щонайменше на {NATURALFIXED($time, 3)} сек.
entity-effect-guidebook-reset-narcolepsy = { $chance ->
        [1] Тимчасово стримує
        *[other] тимчасово стримати
    } нарколепсію
entity-effect-guidebook-wash-cream-pie-reaction = { $chance ->
        [1] Змиває
        *[other] змити
    } кремовий пиріг з обличчя
entity-effect-guidebook-cure-zombie-infection = { $chance ->
        [1] Лікує
        *[other] вилікувати
    } поточну зомбі-інфекцію
entity-effect-guidebook-cause-zombie-infection = { $chance ->
        [1] Дає
        *[other] дати
    } особі зомбі-інфекцію
entity-effect-guidebook-innoculate-zombie-infection = { $chance ->
        [1] Лікує
        *[other] вилікувати
    } поточну зомбі-інфекцію і надає імунітет до майбутніх інфекцій
entity-effect-guidebook-reduce-rotting = { $chance ->
        [1] Відновлює
        *[other] відновити
    } {NATURALFIXED($time, 3)} сек. гниття
entity-effect-guidebook-area-reaction = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } реакцію диму або піни на {NATURALFIXED($duration, 3)} сек.
entity-effect-guidebook-add-to-solution-reaction = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } додавання {$reagent} до його внутрішнього контейнера з розчином
entity-effect-guidebook-artifact-unlock = { $chance ->
        [1] Допомагає
        *[other] допомогти
        } розблокувати інопланетний артефакт.
entity-effect-guidebook-artifact-durability-restore = Відновлює {$restored} міцності в активних вузлах інопланетного артефакта.
entity-effect-guidebook-plant-attribute = { $chance ->
        [1] Регулює
        *[other] регулювати
    } {$attribute} на {$positive ->
    [true] [color=red]{$amount}[/color]
    *[false] [color=green]{$amount}[/color]
    }
entity-effect-guidebook-plant-cryoxadone = { $chance ->
        [1] Омолоджує
        *[other] омолодити
    } рослину, залежно від її віку та часу росту
entity-effect-guidebook-plant-phalanximine = { $chance ->
        [1] Відновлює
        *[other] відновити
    } життєздатність рослини, яка стала нежиттєздатною через мутацію
entity-effect-guidebook-plant-diethylamine = { $chance ->
        [1] Збільшує
        *[other] збільшити
    } тривалість життя рослини та/або базове здоровя з 10% шансом для кожного
entity-effect-guidebook-plant-robust-harvest = { $chance ->
        [1] Збільшує
        *[other] збільшити
    } силу рослини на {$increase} до максимуму {$limit}. Рослина втрачає насіння, коли сила досягає {$seedlesstreshold}. Спроба додати силу понад {$limit} може з 10% шансом зменшити врожайність
entity-effect-guidebook-plant-seeds-add = { $chance ->
        [1] Відновлює
        *[other] відновити
    } насіння рослини
entity-effect-guidebook-plant-seeds-remove = { $chance ->
        [1] Видаляє
        *[other] видалити
    } насіння рослини
entity-effect-guidebook-plant-mutate-chemicals = { $chance ->
        [1] Мутує
        *[other] мутувати
    } рослину, щоб вона виробляла {$name}
reagent-effect-guidebook-add-to-chemicals = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додати
                *[-1] видалити
            }
    } {NATURALFIXED($amount, 2)}u {$reagent} { $deltasign ->
        [1] до
        *[-1] з
    } розчину