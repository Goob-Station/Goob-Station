roles-antag-blood-brother-name = Кровна клятва (перший)
roles-antag-blood-brother-convertible-name = Кровна клятва (навернений)
roles-antag-blood-brother-objective = Співпрацюйте зі своїм побратимом, щоб виконати всі ваші цілі.
role-subtype-blood-brother = Кровна клятва
guide-entry-blood-brothers = Кровна клятва
admin-verb-make-blood-brother = Зробити ціль учасником кровної клятви.
admin-verb-text-make-blood-brother = Зробити Кровною Клятвою
blood-brothers-round-end-agent-name = кровна клятва
objective-issuer-blood-brother = [color=mediumvioletred]Кровна клятва[/color]
blood-brother-round-end-no-mind = [color=white]{CAPITALIZE($name)}[/color] ([color=gray]{$username}[/color]) був пов'язаний кровною клятвою з [color=white]{$brotherName}[/color].
blood-brother-round-end = [color=white]{CAPITALIZE($name)}[/color] ([color=gray]{$username}[/color]) був пов'язаний кровною клятвою з [color=white]{$brotherName}[/color] ([color=gray]{$brotherUsername}[/color]).
blood-brother-initial-role-greeting = Ви пов'язані кровною клятвою.
    Ви отримали здатність укласти кровну клятву з одним членом екіпажу.
    Використайте її розумно і виконайте цілі, які вам доручило анонімне джерело.
blood-brother-role-greeting = Вас втягнули в кровну клятву.
    Слухайте свого побратима і допоможіть йому виконати його цілі.
blood-brother-briefing = Співпрацюйте зі своїм побратимом, щоб виконати всі ваші цілі.
blood-brother-break-control = {CAPITALIZE(THE($name))} згадав свою справжню вірність!
ent-ActionBloodBrotherConvert = Навернути
    .desc = Наверніть члена екіпажу до вашої кровної клятви.
ent-ActionBloodBrotherCheckConvert = Перевірити навернення
    .desc = Перевірте, чи член екіпажу піддається наверненню.
blood-brother-convert-failed-no-mind = {CAPITALIZE(THE($converted))} не має свідомості
blood-brother-convert-failed-already-brother = {CAPITALIZE(THE($converted))} вже пов'язаний кровною клятвою
blood-brother-convert-failed-target = {CAPITALIZE(THE($converted))} є вашою ціллю
blood-brother-convert-failed-zombie = {CAPITALIZE(THE($converted))} є зомбі
blood-brother-convert-failed-shielded = свідомість {THE($converted)} надто добре захищена
blood-brother-convert-failed-dead = {CAPITALIZE(THE($converted))} мертвий
blood-brother-convert-failed-preference = свідомість {THE($converted)} не піддається наверненню
blood-brother-convert-convertible = {CAPITALIZE(THE($converted))} піддається наверненню
blood-brother-conversion-popup = {CAPITALIZE(THE($converter))} міцно хапає {THE($converted)} і вирізає глибокі символи на руках.
objective-condition-blood-brother-progress-title = Переконайтеся, що ваш побратим {$targetName}, {CAPITALIZE($job)} виконає принаймні половину своїх цілей.
objective-condition-blood-brother-escape-title = Втечіть до Центкому живими й не скутими разом із вашим побратимом {$targetName}, {CAPITALIZE($job)}.
objective-condition-blood-brother-survive-title = Переконайтеся, що ви і ваш побратим {$targetName}, {CAPITALIZE($job)} виживете.
ent-BloodBrotherConvertedObjective = Допоможіть своєму побратиму.
    .desc = Допоможіть своєму побратиму виконати всі його цілі, чого б це не коштувало.
ent-BloodBrotherEscapeShuttleObjective = Втечіть до Центкому живими й не скутими.
    .desc = Ви маєте втекти до Центрального командування живими й не скутими разом зі своїм побратимом.
ent-BloodBrotherSurviveObjective = Виживіть.
    .desc = Ви і ваш побратим маєте вижити. Втеча до Центкому не обов'язкова.
steal-target-groups-access-configurator = конфігуратор доступу