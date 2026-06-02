### Interaction Messages

# System

## When trying to ingest without the required utensil... but you gotta hold it
ingestion-you-need-to-hold-utensil = Vous devez tenir {INDEFINITE($utensil)} {$utensil} pour manger ça !

ingestion-try-use-is-empty = {CAPITALIZE(THE($entity))} est vide !
ingestion-try-use-wrong-utensil = Vous ne pouvez pas {$verb} {THE($food)} avec {INDEFINITE($utensil)} {$utensil}.

ingestion-remove-mask = Vous devez d'abord retirer {$entity}.

## Failed Ingestion

ingestion-you-cannot-ingest-any-more = Vous ne pouvez plus {$verb} !
ingestion-other-cannot-ingest-any-more = {CAPITALIZE(SUBJECT($target))} ne peut plus {$verb} !

ingestion-cant-digest = Vous ne pouvez pas digérer {THE($entity)} !
ingestion-cant-digest-other = {CAPITALIZE(SUBJECT($target))} ne peut pas digérer {THE($entity)} !

## Action Verbs, not to be confused with Verbs

ingestion-verb-food = Manger
ingestion-verb-drink = Boire

# Edible Component

edible-nom = Miam. {$flavors}
edible-nom-other = Miam.
edible-slurp = Slurp. {$flavors}
edible-slurp-other = Slurp.
edible-swallow = Vous avalez { THE($food) }
edible-gulp = Glou. {$flavors}
edible-gulp-other = Glou.

edible-has-used-storage = Vous ne pouvez pas {$verb} { THE($food) } avec un objet stocké à l'intérieur.

## Nouns

edible-noun-edible = comestible
edible-noun-food = nourriture
edible-noun-drink = boisson
edible-noun-pill = pilule

## Verbs

edible-verb-edible = ingérer
edible-verb-food = manger
edible-verb-drink = boire
edible-verb-pill = avaler

## Force feeding

edible-force-feed = {CAPITALIZE(THE($user))} essaie de vous forcer à {$verb} quelque chose !
edible-force-feed-success = {CAPITALIZE(THE($user))} vous a forcé à {$verb} quelque chose ! {$flavors}
edible-force-feed-success-user = Vous avez réussi à nourrir {THE($target)}
