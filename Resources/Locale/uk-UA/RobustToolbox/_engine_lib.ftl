# Used internally by the THE() function.
zzzz-the = { PROPER($ent) ->
    *[false] { $ent }
     [true] { $ent }
    }

# Використовується всередині функції SUBJECT().
zzzz-subject-pronoun = { GENDER($ent) ->
    [male] він
    [female] вона
    [epicene] вони
   *[neuter] воно
   }

# Використовується всередині функції OBJECT().
zzzz-object-pronoun = { GENDER($ent) ->
    [male] його
    [female] її
    [epicene] їх
   *[neuter] його
   }

# Використовується всередині функції DAT-OBJ().
# Не використовується в en-US. Створено для підтримки інших мов.
zzzz-possessive-pronoun = { GENDER($ent) ->
    [male] його
    [female] її
    [epicene] їхній
   *[neuter] його
   }

# Використовується всередині функції POSS-ADJ().
zzzz-possessive-adjective = { GENDER($ent) ->
    [male] його
    [female] її
    [epicene] їхній
   *[neuter] його
   }

# Використовується всередині функції REFLEXIVE().
zzzz-reflexive-pronoun = { GENDER($ent) ->
    [male] себе
    [female] себе
    [epicene] себе
   *[neuter] себе
   }

# Використовується всередині функції CONJUGATE-BE().
zzzz-conjugate-be = { GENDER($ent) ->
    [epicene] є
   *[other] є
   }

# Використовується всередині функції CONJUGATE-HAVE().
zzzz-conjugate-have = { GENDER($ent) ->
    [epicene] мають
   *[other] має
   }

# Використовується всередині функції CONJUGATE-BASIC().
zzzz-conjugate-basic = { GENDER($ent) ->
    [epicene] { $first }
   *[other] { $second }
   }
