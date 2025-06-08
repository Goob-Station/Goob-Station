reagent-effect-guidebook-deal-stamina-damage = { $chance ->
        [1] { $deltasign ->
                [1] Завдає
                *[-1] Відновлює
            }
        *[other]
            { $deltasign ->
                [1] завдає
                *[-1] відновлює
            }
    } { $amount } { $immediate ->
                    [true] миттєво
                    *[false] поступово
                  } шкоди витривалості
