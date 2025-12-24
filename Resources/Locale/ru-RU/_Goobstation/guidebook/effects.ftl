entity-effect-guidebook-modify-disgust =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Увеличивает
               *[-1] Уменьшает
            }
       *[other]
            { $deltasign ->
                [1] увеличивает
               *[-1] уменьшает
            }
    } disgust level by { $amount }
