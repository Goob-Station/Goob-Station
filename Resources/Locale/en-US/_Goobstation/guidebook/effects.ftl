entity-effect-guidebook-modify-disgust =
    { $chance ->
        [1] { $deltasign ->
                [1] Increases
                *[-1] Decreases
            }
        *[other]
            { $deltasign ->
                [1] increase
                *[-1] decrease
            }
    } disgust level by { $amount }
