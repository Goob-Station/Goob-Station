server-currency-name-singular = Goob Coin
server-currency-name-plural = Goob Coins

## Commands

server-currency-gift-command = gift
server-currency-gift-command-description = Gifts some of your balance to another player.
server-currency-gift-command-help = Usage: gift <player> <value>
server-currency-gift-command-error-1 = You can't gift yourself!
server-currency-gift-command-error-2 = You can not afford to gift this! You have a balance of {$balance}.
server-currency-gift-command-giver = You gave {$player} {$amount}.
server-currency-gift-command-reciever = {$player} gave you {$amount}.

server-currency-balance-command = balance
server-currency-balance-command-description = Returns your balance.
server-currency-balance-command-help = Usage: balance
server-currency-balance-command-return = You have {$balance}.

server-currency-add-command = balance:add
server-currency-add-command-description = Adds currency to a player's balance.
server-currency-add-command-help = Usage: balance:add <player> <value>

server-currency-remove-command = balance:rem
server-currency-remove-command-description = Removes currency from a player's balance.
server-currency-remove-command-help = Usage: balance:rem <player> <value>

server-currency-set-command = balance:set
server-currency-set-command-description = Sets a player's balance.
server-currency-set-command-help = Usage: balance:set <player> <value>

server-currency-get-command = balance:get
server-currency-get-command-description = Gets the balance of a player.
server-currency-get-command-help = Usage: balance:get <player>

server-currency-command-completion-1 = Username
server-currency-command-completion-2 = Value
server-currency-command-error-1 = Unable to find a player by that name.
server-currency-command-error-2 = Value must be an integer.
server-currency-command-return = {$player} has {$balance}.
