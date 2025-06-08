reagent-effect-condition-guidebook-stamina-damage-threshold = { $max ->
        [2147483648] ціль має щонайменше {NATURALFIXED($min, 2)} шкоди витривалості
        *[other] { $min ->
                    [0] ціль має щонайбільше {NATURALFIXED($max, 2)} шкоди витривалості
                    *[other] ціль має від {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} шкоди витривалості
                 }
    }

reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold = { $max ->
        [2147483648] { $min ->
                        [1] є щонайменше {$min} реагент
                        *[other] є щонайменше {$min} реагентів
                     }
        [1] { $min ->
               [0] є щонайбільше {$max} реагент
               *[other] є від {$min} до {$max} реагентів
            }
        *[other] { $min ->
                    [-1] є щонайбільше {$max} реагентів
                    *[other] є від {$min} до {$max} реагентів
                 }
    }

reagent-effect-condition-guidebook-typed-damage-threshold = { $inverse ->
        [true] ціль має щонайбільше
        *[false] ціль має щонайменше
    } { $changes } шкоди
