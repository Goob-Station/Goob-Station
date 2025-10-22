interaction-LookAt-name = Смотреть
interaction-LookAt-description = Посмотрите в пустоту, и почувствуйте, как она смотрит на вас.
interaction-LookAt-success-self-popup = Вы смотрите на { THE($target) }.
interaction-LookAt-success-target-popup = Вы чувствуете, что { THE($user) } смотрит на вас...
interaction-LookAt-success-others-popup = { THE($user) } смотрит на { THE($target) }.
interaction-Hug-name = Обнять
interaction-Hug-description = Обнимашки помогают справиться с экзистенциальными страхами.
interaction-Hug-success-self-popup = Вы обнимаете { THE($target) }.
interaction-Hug-success-target-popup = { THE($user) } обнимает вас.
interaction-Hug-success-others-popup = { THE($user) } обнимает { THE($target) }.
interaction-Pet-name = Погладить
interaction-Pet-description = Погладьте коллегу, чтобы избавить его от стресса.
interaction-Pet-success-self-popup = Вы гладите { THE($target) } по { POSS-ADJ($target) } голове.
interaction-Pet-success-target-popup = { THE($user) } гладит вас по голове.
interaction-Pet-success-others-popup = { THE($user) } гладит { THE($target) }.
interaction-KnockOn-name = Постучать
interaction-KnockOn-description = Постучите по существу, чтобы привлечь внимание.
interaction-KnockOn-success-self-popup = Вы стучите по { THE($target) }.
interaction-KnockOn-success-target-popup = { THE($user) } стучит по вам.
interaction-KnockOn-success-others-popup = { THE($user) } стучит по { THE($target) }.
interaction-Rattle-name = Потрясти
interaction-Rattle-success-self-popup = Вы трясёте { THE($target) }.
interaction-Rattle-success-target-popup = { THE($user) } трясёт вас.
interaction-Rattle-success-others-popup = { THE($user) } трясёт { THE($target) }.
# The below includes conditionals for if the user is holding an item
interaction-WaveAt-name = Помахать
interaction-WaveAt-description = Помашите существу. Если вы держите предмет, то помашете им.
interaction-WaveAt-success-self-popup =
    Вы машете { $hasUsed ->
        [false] на { THE($target) }.
       *[true] вашим { $used } на { THE($target) }.
    }
interaction-WaveAt-success-target-popup =
    { THE($user) } машет { $hasUsed ->
        [false] на вас.
       *[true] { POSS-PRONOUN($user) } { $used } на вас.
    }
interaction-WaveAt-success-others-popup =
    { THE($user) } машет { $hasUsed ->
        [false] на { THE($target) }.
       *[true] { POSS-PRONOUN($user) } { $used } на { THE($target) }.
    }
