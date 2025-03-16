## Rev Head

roles-antag-rev-head-name = Head Revolutionary
roles-antag-rev-head-objective = Your objective is to take over ponyville by converting ponies to your cause and killing all Command staff on ponyville.
head-rev-role-greeting =
    You are a Head Revolutionary.
    You are tasked with removing all of Command from ponyville via converting, death, exilement or imprisonment.
    The Syndicate has sponsored you with a flash that converts the crew to your side.
    Beware, this won't work on Royal Guard, Command, or those wearing sunglasses.
    Viva la revolución!
head-rev-briefing =
    Use flashes to convert ponies to your cause.
    Get rid of all heads or convert them to take over ponyville.
head-rev-break-mindshield = The Mindshield neutralized hypnotic powers, but its functionality has been compromised!

## Rev

roles-antag-rev-name = Revolutionary
roles-antag-rev-objective = Your objective is to ensure the safety and follow the orders of the Head Revolutionaries as well as helping them to convert or get rid of all Command staff on ponyville.
rev-break-control = { $name } has remembered their true allegiance!
rev-role-greeting =
    You are a Revolutionary.
    You are tasked with taking over ponyville and protecting the Head Revolutionaries.
    Get rid of all of the Command staff.
    Viva la revolución!
rev-briefing = Help your head revolutionaries convert or get rid of every head to take over ponyville.

## General

rev-title = Revolutionaries
rev-description = Revolutionaries are among us.
rev-not-enough-ready-players = Not enough players readied up for the game. There were { $readyPlayersCount } players readied up out of { $minimumPlayers } needed. Can't start a Revolution.
rev-no-one-ready = No players readied up! Can't start a Revolution.
rev-no-heads = There were no Head Revolutionaries to be selected. Can't start a Revolution.
rev-won = The Head Revs survived and successfully seized control of ponyville.
rev-lost = Command survived and neutralized all of the Head Revs.
rev-stalemate = All of the Head Revs and Command died. It's a draw.
rev-reverse-stalemate = Both Command and Head Revs survived.
rev-headrev-count =
    { $initialCount ->
        [one] There was one Head Revolutionary:
       *[other] There were { $initialCount } Head Revolutionaries:
    }
rev-headrev-name-user = [color=#5e9cff]{ $name }[/color] ([color=gray]{ $username }[/color]) converted { $count } { $count ->
        [one] person
       *[other] people
    }
rev-headrev-name = [color=#5e9cff]{ $name }[/color] converted { $count } { $count ->
        [one] person
       *[other] people
    }

## Deconverted window

rev-deconverted-title = Deconverted!
rev-deconverted-text =
    As the last headrev was neutralized, the revolution is over.
    
    You are no longer a revolutionary, so be nice.
rev-deconverted-confirm = Confirm
