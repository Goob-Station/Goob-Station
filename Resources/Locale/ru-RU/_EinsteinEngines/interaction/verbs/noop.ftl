interaction-LookAt-name = Смотреть
interaction-LookAt-description = Посмотрите в пустоту, и почувствуйте, как она смотрит на вас.
interaction-LookAt-success-self-popup = Вы смотрите на { $target }.
interaction-LookAt-success-target-popup = Вы чувствуете, что { $user } смотрит на вас...
interaction-LookAt-success-others-popup = { $user } смотрит на { $target }.
interaction-Hug-name = Обнять
interaction-Hug-description = Обнимашки помогают справиться с экзистенциальными страхами.
interaction-Hug-success-self-popup = Вы обнимаете { $target }.
interaction-Hug-success-target-popup = { $user } обнимает вас.
interaction-Hug-success-others-popup = { $user } обнимает { $target }.
interaction-Pet-name = Погладить
interaction-Pet-description = Погладьте коллегу, чтобы избавить его от стресса.
interaction-Pet-success-self-popup = Вы гладите { $target } по { POSS-ADJ($target) } голове.
interaction-Pet-success-target-popup = { $user } гладит вас по голове.
interaction-Pet-success-others-popup = { $user } гладит { $target }.
interaction-KnockOn-name = Постучать
interaction-KnockOn-description = Постучите по существу, чтобы привлечь внимание.
interaction-KnockOn-success-self-popup = Вы стучите по { $target }.
interaction-KnockOn-success-target-popup = { $user } стучит по вам.
interaction-KnockOn-success-others-popup = { $user } стучит по { $target }.
interaction-Rattle-name = Потрясти
interaction-Rattle-success-self-popup = Вы трясёте { $target }.
interaction-Rattle-success-target-popup = { $user } трясёт вас.
interaction-Rattle-success-others-popup = { $user } трясёт { $target }.
# The below includes conditionals for if the user is holding an item
interaction-WaveAt-name = Помахать
interaction-WaveAt-description = Помашите существу. Если вы держите предмет, то помашете им.
interaction-WaveAt-success-self-popup =
    Вы машете { $hasUsed ->
        [false] на { $target }.
       *[true] вашим { $used } на { $target }.
    }
interaction-WaveAt-success-target-popup =
    { $user } машет { $hasUsed ->
        [false] на вас.
       *[true] { POSS-PRONOUN($user) } { $used } на вас.
    }
interaction-WaveAt-success-others-popup =
    { $user } машет { $hasUsed ->
        [false] на { $target }.
       *[true] { POSS-PRONOUN($user) } { $used } на { $target }.
    }
