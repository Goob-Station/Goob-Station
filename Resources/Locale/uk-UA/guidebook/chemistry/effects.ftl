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

reagent-effect-guidebook-create-entity-reaction-effect = { $chance ->
        *[other] створює
    } { $amount ->
        *[other] {$amount} ($entname)
    }

reagent-effect-guidebook-explosion-reaction-effect = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } вибух

reagent-effect-guidebook-emp-reaction-effect = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } електромагнітний імпульс

reagent-effect-guidebook-foam-area-reaction-effect = { $chance ->
        [1] Створює
        *[other] створюють
    } велику кількість піни

reagent-effect-guidebook-satiate-thirst = { $chance ->
        [1] насичує
        *[other] насичує
    } { $relative ->
        [1] спрагу в середньому
        *[other] втамовує спрагу з {NATURALFIXED($relative, 3)}x середньою швидкістю
    }

reagent-effect-guidebook-satiate-hunger = { $chance ->
        [1] насичує
        *[other] насичує
    } { $relative ->
        [1] голод в середньому
        *[other] голод з {NATURALFIXED($relative, 3)}x середньою швидкістю
    }

reagent-effect-guidebook-health-change = { $chance ->
        [1] { $healsordeals ->
                [heals] Зцілює
                [deals] Шкодить
                *[both] Змінює здоров'я на
             }
        *[other] { $healsordeals ->
                    [heals] зцілють
                    [deals] шкодять
                    *[both] змінюють здоров'я на
                 }
    } { $changes }

reagent-effect-guidebook-status-effect = { $type ->
        [add]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняють
                } {LOC($key)} якнайменше {NATURALFIXED($time, 3)} {MANY("second", $time)} з накопиченням
        *[set]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняють
                } {LOC($key)} якнайменше {NATURALFIXED($time, 3)} {MANY("second", $time)} без накопичення
        [remove]{ $chance ->
                    [1] Прибирає
                    *[other] прибирають
                } {NATURALFIXED($time, 3)} {MANY("second", $time)} {LOC($key)}
    }

reagent-effect-guidebook-activate-artifact = { $chance ->
        [1] Спроб
        *[other] спроба
    } активувати артефакт

reagent-effect-guidebook-set-solution-temperature-effect = { $chance ->
        [1] Встановлює
        *[other] встановлює
    } температуру розчину з точністю {NATURALFIXED($temperature, 2)}k

reagent-effect-guidebook-adjust-solution-temperature-effect = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] прибирає
            }
    } тепло від розчину, поки воно не досягне { $deltasign ->
                [1] не більше {NATURALFIXED($maxtemp, 2)}k
                *[-1] не менше {NATURALFIXED($mintemp, 2)}k
            }

reagent-effect-guidebook-adjust-reagent-reagent = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видаляє
            }
    } {NATURALFIXED($amount, 2)}u з {$reagent} { $deltasign ->
        [1] до
        *[-1] від
    } розчин

reagent-effect-guidebook-adjust-reagent-group = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видалити
            }
    } {NATURALFIXED($amount, 2)}u реагентів у групі {$group} { $deltasign ->
            [1] до
            *[-1] від
        } розв'язок

reagent-effect-guidebook-adjust-temperature = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додає
                *[-1] видалити
            }
    } {POWERJOULES($amount)} тепла { $deltasign ->
            [1] до
            *[-1] від
        }
        від тіла, в якому вона перебуває

reagent-effect-guidebook-chem-cause-disease = { $chance ->
        [1] Причини
        *[other] причина
    } хвороба { $disease }

reagent-effect-guidebook-chem-cause-random-disease = { $chance ->
        [1] Причини
        *[other] причина
    } хвороби { $diseases }

reagent-effect-guidebook-jittering = { $chance ->
        [1] Причини
        *[other] причина
    } тремтіння

reagent-effect-guidebook-chem-clean-bloodstream = { $chance ->
        [1] очищає
        *[other] очищає
    } кровотік від інших хімічних речовин

reagent-effect-guidebook-cure-disease = { $chance ->
        [1] Ліки
        *[other] лікування
    } хвороби

reagent-effect-guidebook-cure-eye-damage = { $chance ->
        [1] { $deltasign ->
                [1] Укладає угоди
                *[-1] Зцілює
            }
        *[other]
            { $deltasign ->
                [1] угода
                *[-1] зцілює
            }
    } пошкодження очей

reagent-effect-guidebook-chem-vomit = { $chance ->
        [1] Причини
        *[other] причина
    } блювота

reagent-effect-guidebook-create-gas = { $chance ->
        [1] створює
        *[other] створює
    } { $moles } { $moles ->
        [1] моль
        *[other] молі
    } of { $gas }

reagent-effect-guidebook-drunk = { $chance ->
        [1] Причини
        *[other] причина
    } пияцтво

reagent-effect-guidebook-electrocute = { $chance ->
        [1] Електрошок
        *[other] удар струмом
    } метаболізатор для {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-extinguish-reaction = { $chance ->
        [1] гасить
        *[other] гасить
    } вогонь

reagent-effect-guidebook-flammable-reaction = { $chance ->
        [1] Збільшує
        *[other] збільшення
    } займистість

reagent-effect-guidebook-ignite = { $chance ->
        [1] запалює
        *[other] підпалює
    } метаболізатор

reagent-effect-guidebook-make-sentient = { $chance ->
        [1] робить
        *[other] робить
    } метаболізатор розумним

reagent-effect-guidebook-make-polymorph = { $chance ->
        [1] Polymorphs
        *[other] polymorph
    } the metabolizer into a { $entityname }

reagent-effect-guidebook-modify-bleed-amount = { $chance ->
        [1] { $deltasign ->
                [1] Індукує
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] індукує
                    *[-1] зменшує
                 }
    } кровотеча

reagent-effect-guidebook-modify-blood-level = { $chance ->
        [1] { $deltasign ->
                [1] Збільшує
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] збільшує
                    *[-1] зменшує
                 }
    } рівень крові

reagent-effect-guidebook-paralyze = { $chance ->
        [1] паралізує
        *[other] паралізує
    } метаболізатор принаймні на {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-movespeed-modifier = { $chance ->
        [1] Змінює
        *[other] змінює
    } швидкість руху на {NATURALFIXED($walkspeed, 3)}x принаймні на {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-reset-narcolepsy = { $chance ->
        [1] Тимчасово зберігається
        *[other] тимчасово зупиняє
    } від нарколепсії

reagent-effect-guidebook-wash-cream-pie-reaction = { $chance ->
        [1] змиває
        *[other] змиває
    } кремовий пиріг з обличчя

reagent-effect-guidebook-cure-zombie-infection = { $chance ->
        [1] Лікує
        *[other] лікує
    } поточну зомбі-інфекцію

reagent-effect-guidebook-cause-zombie-infection = { $chance ->
        [1] дає
        *[other] дає
    } особі зомбі-інфекцію

reagent-effect-guidebook-innoculate-zombie-infection = { $chance ->
        [1] Ліки
        *[other] лікування
    } поточну зомбі-інфекцію та забезпечує імунітет до майбутніх інфекцій

reagent-effect-guidebook-reduce-rotting = { $chance ->
        [1] Регенерує
        *[other] регенерує
    } {NATURALFIXED($time, 3)} {MANY("second", $time)} гниття

reagent-effect-guidebook-missing = { $chance ->
        [1] Причини
        *[other] причина
    } невідомий ефект, оскільки його ще ніхто не описав

reagent-effect-guidebook-change-glimmer-reaction-effect = { $chance ->
        [1] Змінює
        *[other] Змінює
    } кількість мерехтінь на {$count} пунктів
reagent-effect-guidebook-chem-remove-psionic = { $chance ->
        [1] Видаляє
        *[other] видаляє
    } псіонічні здібності
reagent-effect-guidebook-chem-reroll-psionic = { $chance ->
        [1] Дозволяє
        *[other] дозволяє
    } шанс отримати іншу псионічну силу

reagent-effect-guidebook-add-moodlet = змінює настрій на {$amount}
    { $timeout ->
        [0] на невизначений час
        *[other] на {$timeout} секунд
    }

reagent-effect-guidebook-smoke-area-reaction-effect = { $chance ->
        [1] створює
        *[other] створює
    } велику кількість диму
reagent-effect-guidebook-purify-evil = Очищає від злих сил
reagent-effect-guidebook-plant-diethylamine = { $chance ->
      [1] Збільшує
      *[other] збільшує
    } тривалість життя та/або базового здоров'я рослини з імовірністю 10% для кожного
reagent-effect-guidebook-plant-robust-harvest = { $chance ->
        [1] Збільшує
        *[other] збільшує
    } потужність рослини на {$increase} до максимальної величини {$limit}. При досягненні потужності {$seedlesstreshold} рослина втрачає насіння. Спроба додати потужність понад {$limit} може спричинити зниження врожаю з ймовірністю 10%
reagent-effect-guidebook-plant-seeds-add = { $chance ->
        [1] Відновлює
        *[other] відновити
    } насіння рослини
reagent-effect-guidebook-plant-seeds-remove = { $chance ->
        [1] Видаляє
        *[other] видаляє
    } насіння рослини

reagent-effect-guidebook-stamina-change = { $chance ->
        [1] { $deltasign ->
                [-1] Збільшує
                *[1] Зменшує
            }
        *[other] { $deltasign ->
                    [-1] збільшити
                    *[1] зменшити
                 }
    } витривалість на {$amount} балів
reagent-effect-guidebook-chem-restorereroll-psionic = { $chance ->
        [1] Відновлює
        *[other] відновити
    } здатність отримувати користь від реагентів, що відкривають розум
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
reagent-effect-guidebook-remove-moodlet = Видаляє мудлет {$name}.
reagent-effect-guidebook-purge-moodlets = Видаляє всі активні непостійні мудлети.
reagent-effect-guidebook-blind-non-sling = { $chance ->
        [1] Засліплює будь-якого
        *[other] засліпити будь-якого
    } не-тіневика
reagent-effect-guidebook-heal-sling = { $chance ->
        [1] Лікує будь-якого
        *[other] лікувати будь-якого
    } тіневика та раба
reagent-effect-guidebook-flash-reaction-effect = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } сліпучий спалах
reagent-effect-guidebook-area-reaction = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } реакцію диму або піни на {NATURALFIXED($duration, 3)} {MANY("секунду", "секунди", "секунд", $duration)}
reagent-effect-guidebook-add-to-solution-reaction = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } додавання хімікатів, застосованих до об'єкта, до його внутрішнього контейнера з розчином
reagent-effect-guidebook-plant-attribute = { $chance ->
        [1] Регулює
        *[other] регулювати
    } {$attribute} на [color={$colorName}]{$amount}[/color]
reagent-effect-guidebook-plant-cryoxadone = { $chance ->
        [1] Омолоджує
        *[other] омолодити
    } рослину, залежно від її віку та часу росту
reagent-effect-guidebook-plant-phalanximine = { $chance ->
        [1] Відновлює
        *[other] відновити
    } життєздатність рослини, яка стала нежиттєздатною через мутацію
reagent-effect-guidebook-artifact-unlock = { $chance ->
        [1] Допомагає
        *[other] допомогти
        } розблокувати інопланетний артефакт.
reagent-effect-guidebook-even-health-change = { $chance ->
        [1] { $healsordeals ->
            [heals] Рівномірно лікує
            [deals] Рівномірно завдає шкоди
            *[both] Рівномірно змінює здоров'я на
        }
        *[other] { $healsordeals ->
            [heals] рівномірно лікує
            [deals] рівномірно завдає шкоди
            *[both] рівномірно змінює здоров'я на
        }
    } { $changes }
reagent-effect-guidebook-emote = { $chance ->
        [1] Змусить
        *[other] змусить
    } метаболізатор до [bold][color=white]{$emote}[/color][/bold]