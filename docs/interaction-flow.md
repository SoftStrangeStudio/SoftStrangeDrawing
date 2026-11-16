# Drawing flow — 0.3.0

Home → choose an outline and view → Start drawing → pinch to place → Lock → trace on paper.

Required actions: select a reference, place it, lock before physical drawing. Hero actions stay on Home and the three-button Outlines / Lock / Palette dock. Import is inside the library. Detailed styling, grid and studio controls appear only in their relevant drawer, with a clear Done action.

Ink: solid/dotted, opacity, width, runtime color; dot spacing appears only when dotted; edge sensitivity appears only for imported photos. White is the default. Vector conversion starts automatically when needed.

Grid: show/hide, Move grid, Align to outline, size. Grid transform and visibility are independent of the reference. Moving the grid selects it for the same pinch gestures, and the dock's center action becomes Done. Locking or changing the outline must not hide or reposition the grid.

Studio: sound/mute, volume, reduced decorative motion, bring menu here, Home, help, privacy/credits and optional frame-rate panel. These are advanced controls, not required steps.

The world-space menu remains upright. It follows larger head-yaw changes gently at an approximately 0.8 m radius, ignores small head movements and freezes while the user closes a pinch or manipulates an object. The drawing reference and grid stay in world tracking space.

Pinch: thumb/index midpoint for origin; wrist rotation’s forward axis for direction, with 75 ms smoothing (25 ms for the origin). Finger curl does not steer the ray; palm/middle-tip tracking is not required. Entry distance scales within 24–30 mm, release is 14 mm wider. A short debounce and open-hand rearm prevent accidental acquisition after tracking returns. Controller trigger selects UI and grip holds near the sheet.

System dashboard, pause, tracking loss and reserved hand-system gestures cancel ownership. Audio pauses with system focus. Return requires a fresh gesture; an old held pinch must not move anything.

Library opens on AI art: 60 filtered original illustrations. Side / top holds the 60 paired model references. Sharks and Nature include both sources; My images contains only imports. Single illustrations are labeled Use image, and the Side / top action appears only for a paired reference.
