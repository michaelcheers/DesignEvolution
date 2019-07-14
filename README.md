# DesignEvolution
An evolution simulator by Michael and Laurie Cheers.

[Download the executable](https://www.dropbox.com/sh/d5lgydlq18yzkg4/AAAKshVFkKHFmEpfO31D0EyWa?dl=1)

This sandbox simulates tiny pixelated creatures living, growing, dying, and reproducing. Each pixel in their body has a function -

* A leaf (green) absorbs sunlight.
* A bubble (white) makes the organism rise.
* A sinker (black) makes the organism fall.
* A engine (grey) makes the organism move left or right (depending on where it is relative to the heart).
* A bone (pink) cannot be destroyed by collisions.
* A heart (red) is the core of the creature. Every organism always has exactly one: if it's destroyed, the creature dies. If a new one grows, that's a new organism, i.e. this is how the creatures reproduce.

The simulation starts out with a simple pond-scum creature that simply grows a leaf right, and then when it has absorbed enough energy, one heart right, and one heart up (offspring), producing a solid block of organisms. But each time one reproduces, there's a chance of mutations occurring, and pretty soon they're doing who knows what.

![Bubble/Sinker creatures](EvolutionBubbles.png?raw=true "Bubble/sinker creatures")

To keep the simulation from stagnating too much, organisms have a maximum lifetime. When an organism dies, it drops whatever extra energy it has in the form of food, which will be absorbed by nearby creatures or sink to the bottom.
