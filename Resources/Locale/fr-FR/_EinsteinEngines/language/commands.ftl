command-list-langs-desc = Lister les langues que votre entité actuelle peut parler en ce moment.
command-list-langs-help = Utilisation : {$command}

command-saylang-desc = Envoyer un message dans une langue spécifique. Pour choisir une langue, vous pouvez utiliser soit le nom de la langue, soit sa position dans la liste des langues.
command-saylang-help = Utilisation : {$command} <id langue> <message>. Exemple : {$command} TauCetiBasic "Bonjour Monde!". Exemple : {$command} 1 "Bonjour Monde!"

command-language-select-desc = Sélectionner la langue actuellement parlée de votre entité. Vous pouvez utiliser soit le nom de la langue, soit sa position dans la liste des langues.
command-language-select-help = Utilisation : {$command} <id langue>. Exemple : {$command} 1. Exemple : {$command} TauCetiBasic

command-language-spoken = Parlé :
command-language-understood = Compris :
command-language-current-entry = {$id}. {$language} - {$name} (current)
command-language-entry = {$id}. {$language} - {$name}

command-language-invalid-number = Le numéro de langue doit être entre 0 et {$total}. Alternativement, utilisez le nom de la langue.
command-language-invalid-language = La langue {$id} n'existe pas ou vous ne pouvez pas la parler.

# Toolshed

command-description-language-add = Ajouter une nouvelle langue à l'entité. Les deux derniers arguments indiquent si elle doit être parlée/comprise. Exemple : 'self language:add "Canilunzt" true true'
command-description-language-rm = Supprimer une langue de l'entité. Fonctionne de manière similaire à language:add. Exemple : 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Lister toutes les langues que l'entité peut parler. Exemple : 'self language:lsspoken'
command-description-language-lsunderstood = Lister toutes les langues que l'entité peut comprendre. Exemple : 'self language:lssunderstood'

command-description-translator-addlang = Ajouter une nouvelle langue cible à l'entité traductrice. Voir language:add pour les détails.
command-description-translator-rmlang = Supprimer une langue cible de l'entité traductrice. Voir language:rm pour les détails.
command-description-translator-addrequired = Ajouter une nouvelle langue requise à l'entité traductrice. Exemple : 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Supprimer une langue requise de l'entité traductrice. Exemple : 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Lister toutes les langues parlées pour l'entité traductrice. Exemple : 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Lister toutes les langues comprises pour l'entité traductrice. Exemple : 'ent 1234 translator:lsunderstood'
command-description-translator-lsrequired = Lister toutes les langues requises pour l'entité traductrice. Exemple : 'ent 1234 translator:lsrequired'

command-language-error-this-will-not-work = Cela ne fonctionnera pas.
command-language-error-not-a-translator = L'entité {$entity} n'est pas un traducteur.
