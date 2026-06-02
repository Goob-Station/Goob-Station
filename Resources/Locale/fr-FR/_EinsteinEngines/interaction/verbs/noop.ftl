interaction-LookAt-name = Fixer
interaction-LookAt-description = Regardez dans le vide et voyez-le vous regarder en retour.
interaction-LookAt-success-self-popup = Vous fixez {THE($target)}.
interaction-LookAt-success-target-popup = Vous sentez {THE($user)} vous fixer...
interaction-LookAt-success-others-popup = {THE($user)} fixe {THE($target)}.

interaction-Hug-name = Câliner
interaction-Hug-description = Un câlin par jour éloigne les horreurs psychologiques au-delà de votre compréhension.
interaction-Hug-success-self-popup = Vous câlinez {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} vous câline.
interaction-Hug-success-others-popup = {THE($user)} câline {THE($target)}.

interaction-KnockOn-name = Frapper
interaction-KnockOn-description = Frapper sur la cible pour attirer l'attention.
interaction-KnockOn-success-self-popup = Vous frappez sur {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} frappe sur vous.
interaction-KnockOn-success-others-popup = {THE($user)} frappe sur {THE($target)}.

# The below includes conditionals for if the user is holding an item
interaction-WaveAt-name = Saluer
interaction-WaveAt-description = Saluer la cible. Si vous tenez un objet, vous l'agitiez.
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
