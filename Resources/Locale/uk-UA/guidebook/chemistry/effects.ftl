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
        [1] втамовує
        *[other] втамовують
    } { $relative ->
        [1] спрагу із середньою швидкістю
        *[other] спрагу зі швидкістю {NATURALFIXED($relative, 3)}x від середньої
    }
reagent-effect-guidebook-satiate-hunger = { $chance ->
        [1] втамовує
        *[other] втамовують
    } { $relative ->
        [1] голод із середньою швидкістю
        *[other] голод зі швидкістю {NATURALFIXED($relative, 3)}x від середньої
    }
reagent-effect-guidebook-health-change = { $chance ->
        [1] { $healsordeals ->
                [heals] Зцілює
                [deals] Завдає шкоди
                *[both] Змінює здоров'я на
             }
        *[other] { $healsordeals ->
                    [heals] зцілюють
                    [deals] завдають шкоди
                    *[both] змінюють здоров'я на
                 }
    } { $changes }
reagent-effect-guidebook-status-effect = { $type ->
        [add]   { $chance ->
                    [1] Спричиняє
                    *[other] спричиняють
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} {MANY("second", $time)} з накопиченням
        *[set]  { $chance ->
                    [1] Спричиняє
                    *[other] спричиняють
                } {LOC($key)} щонайменше на {NATURALFIXED($time, 3)} {MANY("second", $time)} без накопичення
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
        *[other] встановлюють
    } температуру розчину рівно на {NATURALFIXED($temperature, 2)}k
reagent-effect-guidebook-adjust-solution-temperature-effect = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Забирає
            }
        *[other]
            { $deltasign ->
                [1] додають
                *[-1] забирають
            }
    } тепло розчину, доки він не досягне { $deltasign ->
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
                [1] додають
                *[-1] видаляють
            }
    } {NATURALFIXED($amount, 2)}u {$reagent} { $deltasign ->
        [1] до
        *[-1] з
    } розчину
reagent-effect-guidebook-adjust-reagent-group = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Видаляє
            }
        *[other]
            { $deltasign ->
                [1] додають
                *[-1] видаляють
            }
    } {NATURALFIXED($amount, 2)}u реагентів з групи {$group} { $deltasign ->
            [1] до
            *[-1] з
        } розчину
reagent-effect-guidebook-adjust-temperature = { $chance ->
        [1] { $deltasign ->
                [1] Додає
                *[-1] Забирає
            }
        *[other]
            { $deltasign ->
                [1] додають
                *[-1] забирають
            }
    } {POWERJOULES($amount)} тепла { $deltasign ->
            [1] до
            *[-1] від
        }
        тіла, в якому перебуває
reagent-effect-guidebook-chem-cause-disease = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } хворобу { $disease }
reagent-effect-guidebook-chem-cause-random-disease = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } одну з хвороб: { $diseases }
reagent-effect-guidebook-jittering = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } тремтіння
reagent-effect-guidebook-chem-clean-bloodstream = { $chance ->
        [1] очищає
        *[other] очищають
    } кровотік від інших хімічних речовин
reagent-effect-guidebook-cure-disease = { $chance ->
        [1] Лікує
        *[other] лікують
    } хвороби
reagent-effect-guidebook-cure-eye-damage = { $chance ->
        [1] { $deltasign ->
                [1] Пошкоджує
                *[-1] Зцілює
            }
        *[other]
            { $deltasign ->
                [1] пошкоджують
                *[-1] зцілюють
            }
    } очі
reagent-effect-guidebook-chem-vomit = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } блювання
reagent-effect-guidebook-create-gas = { $chance ->
        [1] створює
        *[other] створюють
    } { $moles } { $moles ->
        [1] моль
        *[other] молів
    } газу { $gas }
reagent-effect-guidebook-drunk = { $chance ->
        [1] Спричиняє
        *[other] спричиняють
    } сп'яніння
reagent-effect-guidebook-electrocute = { $chance ->
        [1] Б'є струмом
        *[other] б'ють струмом
    } метаболізатора протягом {NATURALFIXED($time, 3)} {MANY("second", $time)}
reagent-effect-guidebook-extinguish-reaction = { $chance ->
        [1] гасить
        *[other] гасять
    } вогонь
reagent-effect-guidebook-flammable-reaction = { $chance ->
        [1] Збільшує
        *[other] збільшують
    } займистість
reagent-effect-guidebook-ignite = { $chance ->
        [1] підпалює
        *[other] підпалюють
    } метаболізатора
reagent-effect-guidebook-make-sentient = { $chance ->
        [1] робить
        *[other] роблять
    } метаболізатора розумним
reagent-effect-guidebook-make-polymorph = { $chance ->
        [1] Перетворює
        *[other] перетворюють
    } метаболізатора на { $entityname }
reagent-effect-guidebook-modify-bleed-amount = { $chance ->
        [1] { $deltasign ->
                [1] Спричиняє
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] спричиняють
                    *[-1] зменшують
                 }
    } кровотечу
reagent-effect-guidebook-modify-blood-level = { $chance ->
        [1] { $deltasign ->
                [1] Збільшує
                *[-1] Зменшує
            }
        *[other] { $deltasign ->
                    [1] збільшують
                    *[-1] зменшують
                 }
    } рівень крові
reagent-effect-guidebook-paralyze = { $chance ->
        [1] паралізує
        *[other] паралізують
    } метаболізатора щонайменше на {NATURALFIXED($time, 3)} {MANY("second", $time)}
reagent-effect-guidebook-movespeed-modifier = { $chance ->
        [1] Змінює
        *[other] змінюють
    } швидкість руху на {NATURALFIXED($walkspeed, 3)}x щонайменше на {NATURALFIXED($time, 3)} {MANY("second", $time)}
reagent-effect-guidebook-reset-narcolepsy = { $chance ->
        [1] Тимчасово зупиняє
        *[other] тимчасово зупиняють
    } нарколепсію
reagent-effect-guidebook-wash-cream-pie-reaction = { $chance ->
        [1] змиває
        *[other] змивають
    } кремовий пиріг з обличчя
reagent-effect-guidebook-cure-zombie-infection = { $chance ->
        [1] Лікує
        *[other] лікують
    } поточну зомбі-інфекцію
reagent-effect-guidebook-cause-zombie-infection = { $chance ->
        [1] заражає
        *[other] заражають
    } особу зомбі-інфекцією
reagent-effect-guidebook-innoculate-zombie-infection = { $chance ->
        [1] Лікує
        *[other] лікують
    } поточну зомбі-інфекцію та забезпечує імунітет до майбутніх інфекцій
reagent-effect-guidebook-reduce-rotting = { $chance ->
        [1] Зменшує
        *[other] зменшують
    } гниття на {NATURALFIXED($time, 3)} {MANY("second", $time)}
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
        *[other] створюють
    } велику кількість диму
reagent-effect-guidebook-purify-evil = Очищає від злих сил
reagent-effect-guidebook-plant-diethylamine = { $chance ->
      [1] Збільшує
      *[other] збільшують
    } тривалість життя та/або базове здоров'я рослини з шансом 10% для кожного
reagent-effect-guidebook-plant-robust-harvest = { $chance ->
        [1] Збільшує
        *[other] збільшують
    } потужність рослини на {$increase} до максимуму {$limit}. Коли потужність досягає {$seedlesstreshold}, рослина втрачає насіння. Спроба додати потужність понад {$limit} може спричинити зниження врожаю з шансом 10%
reagent-effect-guidebook-plant-seeds-add = { $chance ->
        [1] Відновлює
        *[other] відновлюють
    } насіння рослини
reagent-effect-guidebook-plant-seeds-remove = { $chance ->
        [1] Видаляє
        *[other] видаляють
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
reagent-effect-guidebook-remove-moodlet = Видаляє мудлет {$name}.
reagent-effect-guidebook-purge-moodlets = Видаляє всі активні непостійні мудлети.
reagent-effect-guidebook-flash-reaction-effect = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } сліпучий спалах
reagent-effect-guidebook-area-reaction = { $chance ->
        [1] Спричиняє
        *[other] спричинити
    } реакцію диму або піни на {NATURALFIXED($duration, 3)} сек.
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
        *[other] змусити
    } метаболізатор виконати [bold][color=white]{$emote}[/color][/bold]
reagent-effect-guidebook-artifact-durability-restore = Відновлює {$restored} міцності в активних вузлах інопланетних артефактів.