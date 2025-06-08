interaction-LookAt-name = Поглянь на це
interaction-LookAt-description = Вдивіться в порожнечу і побачите, як вона дивиться у відповідь.
interaction-LookAt-success-self-popup = Ви дивитеся на {THE($target)}.
interaction-LookAt-success-target-popup = Ви відчуваєте, що {THE($user)} дивиться на вас...
interaction-LookAt-success-others-popup = {THE($user)} дивиться на {THE($target)}.

interaction-Hug-name = Обійми
interaction-Hug-description = Обійми на день запобігають психологічним жахіттям, які ви не можете собі уявити.
interaction-Hug-success-self-popup = Ви обіймаєте {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} обіймає вас.
interaction-Hug-success-others-popup = {THE($user)} обіймає {THE($target)}.

interaction-Pet-name = Погладити
interaction-Pet-description = Погладьте свого колегу, щоб зняти стрес.
interaction-Pet-success-self-popup = Ви гладите {THE($target)} по {POSS-ADJ($target)} голові.
interaction-Pet-success-target-popup = {THE($user)} гладить вас по {POSS-ADJ($target)} голові.
interaction-Pet-success-others-popup = {THE($user)} гладить {THE($target)}.

interaction-KnockOn-name = Постукай
interaction-KnockOn-description = Постукайте по мішені, щоб привернути увагу.
interaction-KnockOn-success-self-popup = Ви стукаєте по {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} стукає до вас.
interaction-KnockOn-success-others-popup = {THE($user)} стукає в {THE($target)}.

interaction-Rattle-name = Брязкальце
interaction-Rattle-success-self-popup = Ти брязкаєш {THE($target)}.
interaction-Rattle-success-target-popup = {THE($user)} лякає тебе.
interaction-Rattle-success-others-popup = {THE($user)} брязкає {THE($target)}.

# Нижче наведено умови для випадку, коли користувач тримає предмет
interaction-WaveAt-name = Помахайте рукою
interaction-WaveAt-description = Помахайте на ціль. Якщо ви тримаєте в руках якийсь предмет, ви будете махати ним.
interaction-WaveAt-success-self-popup = Ти махаєш {$hasUsed ->
    [false] на {THE($target)}.
    *[true] ваш {$used} на {THE($target)}.
}
interaction-WaveAt-success-target-popup = {THE($user)} махає {$hasUsed ->
    [false] на тебе.
    *[true] {OBJECT($user)} {$used} на вас.
}
interaction-WaveAt-success-others-popup = {THE($user)} махає {$hasUsed ->
    [false] на {THE($target)}.
    *[true] {OBJECT($user)} {$used} у {THE($target)}.
}

interaction-PetAnimal-name = {interaction-Pet-name}
interaction-PetAnimal-description = Погладь тварину.
interaction-PetAnimal-success-self-popup = {interaction-Pet-success-self-popup}
interaction-PetAnimal-success-target-popup = {interaction-Pet-success-target-popup}
interaction-PetAnimal-success-others-popup = {interaction-Pet-success-others-popup}