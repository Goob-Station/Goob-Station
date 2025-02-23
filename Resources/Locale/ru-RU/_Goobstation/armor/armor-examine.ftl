armor-examine-stamina = - [color=cyan]Выносливость[/color] урон снижен на [color=lightblue]{ $num }%[/color].
armor-examine-cancel-delayed-knockdown = - [color=green]Полностью отменяет[/color] оглушение дубинкой с задержкой нокдауна.
armor-examine-modify-delayed-knockdown-delay =
    - { $deltasign ->
        [1] [color=green]Увеличивает[/color]
       *[-1] [color=red]Уменьшает[/color]
    } оглушение дубинкой на [color=lightblue]{ NATURALFIXED($amount, 2) } { $amount ->
        [1] секунду
       *[other] секунд
    }[/color].
armor-examine-modify-delayed-knockdown-time =
    - { $deltasign ->
        [1] [color=red]Увеличивает[/color]
       *[-1] [color=green]Уменьшает[/color]
    } оглушение дубинкой на [color=lightblue]{ NATURALFIXED($amount, 2) } { $amount ->
        [1] секунду
       *[other] секунд
    }[/color].
