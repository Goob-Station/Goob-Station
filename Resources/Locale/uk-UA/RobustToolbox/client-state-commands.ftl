# Loc strings for various entity state & client-side PVS related commands

cmd-reset-ent-help = Використання: resetent <UID сутності>
cmd-reset-ent-desc = Скидає сутність до останнього отриманого стану з сервера. Це також скине сутності, які були від'єднані до нульового простору. 

cmd-reset-all-ents-help = Використання: resetallents
cmd-reset-all-ents-desc = Скидає всі сутності до останнього отриманого стану з сервера. Це впливає лише на сутності, які не були від'єднані до нульового простору. 

cmd-detach-ent-help = Використання: detachent <UID сутності>
cmd-detach-ent-desc = Від'єднати сутність до нульового простору, ніби вона вийшла за межі діапазону PVS.

cmd-local-delete-help = Використання: localdelete <UID сутності>
cmd-local-delete-desc = Видаляє сутність. На відміну від звичайної команди видалення, ця команда є КЛІЄНТСЬКОЮ. Якщо сутність не є клієнтською, це, ймовірно, спричинить помилки.

cmd-full-state-reset-help = Використання: fullstatereset
cmd-full-state-reset-desc = Відкидає всю інформацію про стан сутностей і запитує повний стан з сервера.
