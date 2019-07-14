# DesignEvolution
An evolution program.

This sandbox simulates tiny pixelated creatures living, growing, dying, and reproducing. Each pixel in their body has a function -

* A leaf (green) absorbs sunlight.
* A bubble (white) makes the organism rise.
* A sinker (black) makes the organism fall.
* A engine (grey) makes the organism move left or right (depending on where it is relative to the heart).
* A bone (pink) cannot be destroyed by collisions.
* A heart (red) is the core of the creature. Every organism always has exactly one, and if it's destroyed, the creature dies. If a new one grows, that's a new organism, i.e. this is how the creatures reproduce.

The simulation starts out with a simple pond-scum creature that simply grows a leaf, and then reproduces twice (when it has absorbed enough energy). But each time it reproduces, there's a chance of mutations occurring, and... who knows what will happen next.
