armor-examine-stamina = - [color=cyan]Stamina[/color] damage reduced by [color=lightblue]{$num}%[/color].

armor-examine-cancel-delayed-knockdown = - [color=green]Completely cancels[/color] stun baton delayed knockdown.

armor-examine-modify-delayed-knockdown-delay =
    - { $deltasign ->
          [1] [color=green]Increases[/color]
          *[-1] [color=red]Decreases[/color]
      } stun baton delayed knockdown delay by [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] second
          *[other] seconds
      }[/color].

armor-examine-modify-delayed-knockdown-time =
    - { $deltasign ->
          [1] [color=red]Increases[/color]
          *[-1] [color=green]Decreases[/color]
      } stun baton delayed knockdown time by [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] second
          *[other] seconds
      }[/color].
