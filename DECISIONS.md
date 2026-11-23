**2026-11-19**  
I worked on adding three enemies to the game: Wolf, Albasty, and Bura. During implementation, I noticed that some enemy behaviors and attack mechanics did not work as expected. I reviewed the generated code, corrected the incorrect behavior, and adjusted the enemy mechanics to fit the gameplay.

**2026-11-20**  
I noticed that the boss's lasso attack hitbox was much longer than the rope shown on the screen. This made the attack unfair because the player could get hit even when standing outside the visible rope. I reduced the hitbox length from 3.5 to 0.5.

**2026-11-20 — Playtest 1 (old build)**  
I tested the game with players and recorded the number of attempts needed to complete each level.

- Level 1: 2 and 3 attempts.
- Level 2: 5 and 6 attempts.
- Level 3: 7 and 9 attempts.

I observed that most deaths on Level 2 were caused by Bura and Albasty, while the boss caused difficulties on Level 3. On Level 1, players often fell from platforms and did not understand the light mechanic.

**2026-11-21**  
After testing the boss fight myself, I found another issue with the lasso attack. Players could stand on the right side of the boss and avoid being hit completely. I adjusted the lasso hitbox to cover approximately 85% of the visible light beam (3.4). I also fixed the boss's targeting so that it could throw the lasso toward the player's position on either side.

**2026-11-21 — Gameplay Improvements**  
Based on the first playtest and my own testing, I made several gameplay adjustments:

- Increased Bura's vertical detection tolerance from 1.5 to 3 so it could detect the player more reliably.
- Fixed Bura's wind-up animation so that jumping would not cancel its attack.
- Increased the wolf's chase speed from 4 to 7.2 to make its movement more effective.
- Increased Bura's maximum charge time from 1.2 to 1.8 seconds.
- Replaced the original joystick with TouchStick after experiencing freezing issues. I could not consistently reproduce the freezing problem, but decided to change the joystick implementation.

**2026-11-21 — Playtest 2 (new build, approximately 04:00)**  
I tested the updated build again with the same tester who had played the previous version.

- Level 1: 1 attempt.
- Level 2: 3 attempts.
- Level 3: 2 attempts.

The results showed improvement compared with the first playtest. However, the tester had already played the game before, so familiarity with the levels may also have influenced the results.