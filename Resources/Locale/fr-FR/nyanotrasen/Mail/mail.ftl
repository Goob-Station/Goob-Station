# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

mail-recipient-mismatch = Le nom ou le poste du destinataire ne correspond pas.
mail-invalid-access = Le nom et le poste correspondent, mais l'accès n'est pas celui attendu.
mail-locked = Le verrou anti-effraction n'a pas été retiré. Tapotez l'ID du destinataire.
mail-desc-far = Un colis postal. Vous ne pouvez pas lire à qui il est adressé depuis cette distance.
mail-desc-close = Un colis postal adressé à {CAPITALIZE($name)}, {$job}.
mail-desc-fragile = Il porte une [color=red]étiquette rouge fragile[/color].
mail-desc-priority = Le [color=yellow]ruban jaune prioritaire[/color] du verrou anti-effraction est actif. Mieux vaut le livrer à temps !
mail-desc-priority-inactive = Le [color=#886600]ruban jaune prioritaire[/color] du verrou anti-effraction est inactif.
mail-unlocked = Système anti-effraction déverrouillé.
mail-unlocked-by-emag = Système anti-effraction *BZZT*.
mail-unlocked-reward = Système anti-effraction déverrouillé. {$bounty} spesos ont été ajoutés au compte logistique.
mail-penalty-lock = VERROU ANTI-EFFRACTION BRISÉ. COMPTE BANCAIRE LOGISTIQUE PÉNALISÉ DE {$credits} SPESOS.
mail-penalty-fragile = INTÉGRITÉ COMPROMISE. COMPTE BANCAIRE LOGISTIQUE PÉNALISÉ DE {$credits} SPESOS.
mail-penalty-expired = LIVRAISON EN RETARD. COMPTE BANCAIRE LOGISTIQUE PÉNALISÉ DE {$credits} SPESOS.
mail-item-name-addressed = courrier ({$recipient})

command-mailto-description = Queue a parcel to be delivered to an entity. Example usage: `mailto 1234 5678 false false`. The target container's contents will be transferred to an actual mail parcel.
### Frontier: add is-large description
command-mailto-help = Usage: {$command} <recipient entityUid> <container entityUid> [is-fragile: true or false] [is-priority: true or false] [is-large: true or false, optional]
command-mailto-no-mailreceiver = Target recipient entity does not have a {$requiredComponent}.
command-mailto-no-blankmail = The {$blankMail} prototype doesn't exist. Something is very wrong. Contact a programmer.
command-mailto-bogus-mail = {$blankMail} did not have {$requiredMailComponent}. Something is very wrong. Contact a programmer.
command-mailto-invalid-container = Target container entity does not have a {$requiredContainer} container.
command-mailto-unable-to-receive = Target recipient entity was unable to be setup for receiving mail. ID may be missing.
command-mailto-no-teleporter-found = Target recipient entity was unable to be matched to any station's mail teleporter. Recipient may be off-station.
command-mailto-success = Success! Mail parcel has been queued for next teleport in {$timeToTeleport} seconds.

command-mailnow = Force all mail teleporters to deliver another round of mail as soon as possible. This will not bypass the undelivered mail limit.
command-mailnow-help = Usage: {$command}
command-mailnow-success = Success! All mail teleporters will be delivering another round of mail soon.
