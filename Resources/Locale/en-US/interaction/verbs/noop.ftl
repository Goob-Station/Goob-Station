interaction-LookAt-name = Look at
interaction-LookAt-description = Stare into the void and see it stare back.
interaction-LookAt-success-self-popup = You look at {THE($target)}.
interaction-LookAt-success-target-popup = You feel {THE($user)} looking at you...
interaction-LookAt-success-others-popup = {THE($user)} looks at {THE($target)}.

interaction-Hug-name = Hug
interaction-Hug-description = A hug a day keeps the psychological horrors beyond your comprehension away.
interaction-Hug-success-self-popup = You hug {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} hugs you.
interaction-Hug-success-others-popup = {THE($user)} hugs {THE($target)}.

interaction-Pet-name = Pet
interaction-Pet-description = Pet your co-worker to ease their stress.
interaction-Pet-success-self-popup = You pet {THE($target)} on {POSS-ADJ($target)} head.
interaction-Pet-success-target-popup = {THE($user)} pets you on {POSS-ADJ($target)} head.
interaction-Pet-success-others-popup = {THE($user)} pets {THE($target)}.

interaction-KnockOn-name = Knock
interaction-KnockOn-description = Knock on the target to attract attention.
interaction-KnockOn-success-self-popup = You knock on {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} knocks on you.
interaction-KnockOn-success-others-popup = {THE($user)} knocks on {THE($target)}.

interaction-Rattle-name = Rattle
interaction-Rattle-success-self-popup = You rattle {THE($target)}.
interaction-Rattle-success-target-popup = {THE($user)} rattles you.
interaction-Rattle-success-others-popup = {THE($user)} rattles {THE($target)}.

# The below includes conditionals for if the user is holding an item
interaction-WaveAt-name = Wave at
interaction-WaveAt-description = Wave at the target. If you are holding an item, you will wave it.
interaction-WaveAt-success-self-popup = You wave {$hasUsed ->
    [false] at {THE($target)}.
    *[true] your {$used} at {THE($target)}.
}
interaction-WaveAt-success-target-popup = {THE($user)} waves {$hasUsed ->
    [false] at you.
    *[true] {POSS-PRONOUN($user)} {$used} at you.
}
interaction-WaveAt-success-others-popup = {THE($user)} waves {$hasUsed ->
    [false] at {THE($target)}.
    *[true] {POSS-PRONOUN($user)} {$used} at {THE($target)}.
}
