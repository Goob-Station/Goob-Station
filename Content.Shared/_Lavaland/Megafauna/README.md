# Megafauna AI Small Documentation
## (we've implemented PLUEY)

This .MD file contains some basic info about how to use this new AI system for lavaland bosses.

For convenience this system will be referenced as **P.L.U.E.Y.** Why it called like that? I don't know just for gags lol

### 1. What the HELL is PLUEY and WHY???

The core idea of PLUEY is to make a similar system to HTN, but much simpler to use without loosing too much functionality.
Basically because HTN is too hard, and we've written an entire HTN 2.0 that is scoped specifically for bosses.

This system still uses classes to execute code, because oly with them the code for bosses can be made reusable in YAML.
So, there are **MegafaunaAction**s and **MegafaunaCondition**s.

MegafaunaAction stores the main code for execution.
To decide which attacks should be executed, with all MegafaunaActions you can also specify a list of conditions for it to run.
After making all conditions, and picking most valuable actions at this moment, boss executes this action and goes on a cooldown until making the next attack.

### 2. Is this total HTN death real?

No, it's not. PLUEY is more code- and boss- focused, so you can't do much with it besides from creating a new boss with pretty simple attack patterns.

Tho this system is technically compatible with HTN, so depending on your goals, you should use HTN, PLUEY, or even both.

Also, this system doesn't have HTN's optimizations and runs in the main thread. So be careful when adding a new boss that has 10000 conditions and 100 attacks per tick.

### 3. Where I can see all examples for the code?

Unfortunately I'm lazy and I did not add enough bosses yet, so there will be functionality that is left unused in the actual code.
Currently, the simplest example of lavaland boss is Hierophant, so you can look at his prototype and components and MegafaunaActions, and then use them for your own purposes.

### Conclusion

Use this thing for lavaland bosses, don't be stupid, read summaries they're everywhere. Gud luck.
