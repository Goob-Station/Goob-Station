## Survivor

roles-antag-survivor-name = Вцілілий
roles-antag-survivor-objective = Поточна ціль: вижити
survivor-role-greeting = Ви Вцілілий. Понад усе вам потрібно повернутися до Центрального Командування живим.
    Зберіть стільки вогневої потужності, скільки потрібно, щоб гарантувати своє виживання.
    Не довіряйте нікому.
survivor-round-end-dead-count = {
    $deadCount ->
        [one] [color=red]{$deadCount}[/color] вцілілий загинув.
        *[other] [color=red]{$deadCount}[/color] вцілілих загинуло.
}
survivor-round-end-alive-count = {
    $aliveCount ->
        [one] [color=yellow]{$aliveCount}[/color] вцілілий лишився покинутим на станції.
        *[other] [color=yellow]{$aliveCount}[/color] вцілілих лишилися покинутими на станції.
}
survivor-round-end-alive-on-shuttle-count = {
    $aliveCount ->
        [one] [color=green]{$aliveCount}[/color] вцілілий вибрався живим.
        *[other] [color=green]{$aliveCount}[/color] вцілілих вибралися живими.
}