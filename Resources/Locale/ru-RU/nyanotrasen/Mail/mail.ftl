mail-recipient-mismatch = Имя или должность получателя не совпадают.
mail-invalid-access = Имя и должность получателя совпадают, но не доступы.
mail-locked = Защита от несанкционированного доступа не была деактивированна, используйте КПК.
mail-desc-far = Посылка, вы не можете разобрать кому она адресована с этого расстояния.
mail-desc-close = Посылка адресованная { CAPITALIZE($name) }, { $job }.
mail-desc-fragile = [color=red]Хрупкое![/color].
mail-desc-priority = [color=yellow]Приоритеная доставка![/color] Лучше доставить вовремя!
mail-desc-priority-inactive = [color=#886600]Приоретеная доставка![/color] Срок доставки истек.
mail-unlocked = Защита от несанкционированного доступа отключена.
mail-unlocked-by-emag = Защита от несанкционированного доступа *БЗЗТ*.
mail-unlocked-reward = Защита от несанкционированного доступа отключена. { $bounty } кредитов было начислено на банковский счёт Отдела Снабжения.
mail-penalty-lock = ЗАЩИТА ОТ НЕСАНКЦИОНИРОВАННОГО ДОСТУПА ПОВРЕЖДЕНА. БАНКОВСКИЙ СЧЕТ ОТДЕЛА СНАБЖЕНИЯ ОШТРАФОВАН НА { $credits } КРЕДИТОВ.
mail-penalty-fragile = ЦЕЛОСТНОСТЬ НАРУШЕНА. БАНКОВСКИЙ СЧЕТ ОТДЕЛА СНАБЖЕНИЯ ОШТРАФОВАН НА { $credits } КРЕДИТОВ.
mail-penalty-expired = ДОСТАВКА ПРОСРОЧЕНА. БАНКОВСКИЙ СЧЕТ ОТДЕЛА СНАБЖЕНИЯ ОШТРАФОВАН НА { $credits } КРЕДИТОВ.
mail-item-name-unaddressed = посылка
mail-item-name-addressed = посылка ({ $recipient })
command-mailto-description = Queue a parcel to be delivered to an entity. Example usage: `mailto 1234 5678 false false`. The target container's contents will be transferred to an actual mail parcel.

### Frontier: add is-large description

command-mailto-help = Usage: { $command } <recipient entityUid> <container entityUid> [is-fragile: true or false] [is-priority: true or false] [is-large: true or false, optional]
command-mailto-no-mailreceiver = Target recipient entity does not have a { $requiredComponent }.
command-mailto-no-blankmail = The { $blankMail } prototype doesn't exist. Something is very wrong. Contact a programmer.
command-mailto-bogus-mail = { $blankMail } did not have { $requiredMailComponent }. Something is very wrong. Contact a programmer.
command-mailto-invalid-container = Target container entity does not have a { $requiredContainer } container.
command-mailto-unable-to-receive = Target recipient entity was unable to be setup for receiving mail. ID may be missing.
command-mailto-no-teleporter-found = Target recipient entity was unable to be matched to any station's mail teleporter. Recipient may be off-station.
command-mailto-success = Success! Mail parcel has been queued for next teleport in { $timeToTeleport } seconds.
command-mailnow = Force all mail teleporters to deliver another round of mail as soon as possible. This will not bypass the undelivered mail limit.
command-mailnow-help = Usage: { $command }
command-mailnow-success = Success! All mail teleporters will be delivering another round of mail soon.
