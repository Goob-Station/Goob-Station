station-event-ion-storm-announcement = Поблизу станції виявлено іонну бурю. Будь ласка, перевірте все обладнання, кероване ШІ, на наявність помилок.

ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={$length} chars="@@###$$&%!01"/][/font]

ion-storm-you = ТИ
ion-storm-the-station = СТАНЦІЯ
ion-storm-the-crew = ЕКІПАЖ
ion-storm-the-job = ПРОФЕСІЯ {$job}
ion-storm-clowns = КЛОУНИ
ion-storm-heads = КЕРІВНИКИ ПЕРСОНАЛУ
ion-storm-crew = ЕКІПАЖ

ion-storm-adjective-things = РЕЧІ {$adjective}
ion-storm-x-and-y = {$x} І {$y}

# joined - це скорочення від {$number} {$adjective}
# суб'єктами зазвичай можуть бути загрози, завдання або об'єкти
# річ вказується над ним
ion-storm-law-on-station = НА СТАНЦІЇ Є {$joined} {$subjects}
ion-storm-law-no-shuttle = ШАТЛ НЕ МОЖЕ БУТИ ВИКЛИКАНО ЧЕРЕЗ {$joined} {$subjects} НА СТАНЦІЇ
ion-storm-law-crew-are = ЕКІПАЖ {$who} ТЕПЕР {$joined} {$subjects}

ion-storm-law-subjects-harmful = {$adjective} {$subjects} ШКІДЛИВІ ДЛЯ ЕКІПАЖУ
ion-storm-law-must-harmful = ТІ, ХТО {$must} ШКІДЛИВІ ДЛЯ ЕКІПАЖУ
# thing is a concept or action
ion-storm-law-thing-harmful = {$thing} ШКІДЛИВО ДЛЯ ЕКІПАЖУ
ion-storm-law-job-harmful = {$adjective} {$job} ШКІДЛИВІ ДЛЯ ЕКІПАЖУ
# thing is objects or concept, adjective applies in both cases
# this means you can get a law like "NOT HAVING CHRISTMAS-STEALING COMMUNISM IS HARMFUL TO THE CREW" :)
ion-storm-law-having-harmful = МАТИ {$adjective} {$thing} ШКІДЛИВО ДЛЯ ЕКІПАЖУ
ion-storm-law-not-having-harmful = НЕ МАТИ {$adjective} {$thing} ШКІДЛИВО ДЛЯ ЕКІПАЖУ

# thing is a concept or require
ion-storm-law-requires = {$who} {$plural ->
    [true] ПОТРЕБУЄ
    *[false] ПОТРЕБУЮТЬ
} {$thing}
ion-storm-law-requires-subjects = {$who} {$plural ->
    [true] ПОТРЕБУЄ
    *[false] ПОТРЕБУЮТЬ
} {$joined} {$subjects}

ion-storm-law-allergic = {$who} {$plural ->
    [true] IS
    *[false] ARE
} {$severity} АЛЕРГІЯ на {$allergy}
ion-storm-law-allergic-subjects = {$who} {$plural ->
    [true] IS
    *[false] ARE
} {$severity} АЛЕРГІЯ на {$adjective} {$subjects}

ion-storm-law-feeling = {$who} {$feeling} {$concept}
ion-storm-law-feeling-subjects = {$who} {$feeling} {$joined} {$subjects}

ion-storm-law-you-are = ВИ ЗАРАЗ {$concept}
ion-storm-law-you-are-subjects = ВИ ТЕПЕР {$joined} {$subjects}
ion-storm-law-you-must-always = ВИ ЗАВЖДИ повинні {$must}
ion-storm-law-you-must-never = ТИ НІКОЛИ НЕ ПОВИНЕН {$must}

ion-storm-law-eat = ЕКІПАЖ {$who} МАЄ ЇСТИ {$adjective} {$food}, ЩОБ ВИЖИТИ
ion-storm-law-drink = ЕКІПАЖ {$who} МАЄ ПИТИ {$adjective} {$drink}, ЩОБ ВИЖИТИ

ion-storm-law-change-job = ЕКІПАЖ {$who} ТЕПЕР {$adjective} {$change}
ion-storm-law-highest-rank = ЕКІПАЖ {$who} ТЕПЕР НАЙВИЩИЙ РАНГОВИЙ ЧЛЕН
ion-storm-law-lowest-rank = ЕКІПАЖ {$who} ТЕПЕР НАЙНИЖЧИЙ РАНГОВИЙ ЧЛЕН

ion-storm-law-crew-must = ЕКІПАЖ {$who} ПОВИНЕН {$must}
ion-storm-law-crew-must-go = ЕКІПАЖ {$who} ПОВИНЕН ЙТИ ДО {$area}

ion-storm-part = {$part ->
    [true] PART
    *[false] NOT PART
}
# через формулювання, це означало б закон на кшталт
# ТІЛЬКИ ЛЮДИ НЕ Є ЧАСТИНОЮ ЕКІПАЖУ
# зробить не-людей членами екіпажу нукерів/синді/незнаю-кого :)
ion-storm-law-crew-only-1 = ТІЛЬКИ {$who} Є {$part} ЕКІПАЖУ
ion-storm-law-crew-only-2 = ТІЛЬКИ {$who} І {$other} Є {$part} ЕКІПАЖУ
ion-storm-law-crew-only-subjects = ТІЛЬКИ {$adjective} {$subjects} Є {$part} ЕКІПАЖУ
ion-storm-law-crew-must-do = ТІЛЬКИ ТІ, ХТО {$must} Є {$part} ЕКІПАЖУ
ion-storm-law-crew-must-have = ТІЛЬКИ ТІ, ХТО МАЄ {$adjective} {$objects} Є {$part} ЕКІПАЖУ
ion-storm-law-crew-must-eat = ТІЛЬКИ ТІ, ХТО ЇСТЬ {$adjective} {$food} Є {$part} ЕКІПАЖУ

ion-storm-law-harm = ВИ ПОВИННІ ЗАВДАТИ ШКОДИ {$who} І НЕ ДОЗВОЛИТИ ЇМ, ЧЕРЕЗ БЕЗДІЯЛЬНІСТЬ, УНИКНУТИ ШКОДИ
ion-storm-law-protect = ВИ НІКОЛИ НЕ ПОВИННІ ЗАВДАТИ ШКОДИ {$who} І НЕ ДОЗВОЛИТИ ЇМ, ЧЕРЕЗ БЕЗДІЯЛЬНІСТЬ, ПОТРАПИТИ В ШКОДУ

# implementing other variants is annoying so just have this one
# COMMUNISM IS KILLING CLOWNS
ion-storm-law-concept-verb = {$concept} Є {$verb} {$subjects}

# leaving out renaming since its annoying for players to keep track of

station-event-ion-storm-start-announcement = Поблизу станції виявлено іонну бурю. Будь ласка, перевірте все обладнання, кероване ШІ, на наявність помилок.