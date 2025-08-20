reagent-effect-condition-guidebook-stamina-damage-threshold =
    { $max ->
        [2147483648] цель имеет как минимум { NATURALFIXED($min, 2) } урона выносливости
       *[other]
            { $min ->
                [0] цель имеет не более { NATURALFIXED($max, 2) } урона выносливости
               *[other] цель имеет между { NATURALFIXED($min, 2) } и { NATURALFIXED($max, 2) } урона выносливости
            }
    }
reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold =
    { $max ->
        [2147483648]
            { $min ->
                [1] есть по крайней мере { $min } реагента
               *[other] есть по крайней мере { $min } реагентов
            }
        [1]
            { $min ->
                [0] есть не более { $max } реагента
               *[other] есть от { $min } до { $max } реагентов
            }
       *[other]
            { $min ->
                [-1] есть не более { $max } реагента
               *[other] есть от { $min } до { $max } реагентов
            }
    }
reagent-effect-condition-guidebook-typed-damage-threshold =
    { $inverse ->
        [true] цель имеет не более
       *[false] цель имеет как минимум
    } { $changes } повреждений
