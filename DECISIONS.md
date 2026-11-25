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

## Week 11 - Assignment 11: Data and Saves

**2026-11-23**
I asked the agent to move all balance numbers out of the scripts into ScriptableObject assets. Now the player, the four enemies (Wolf, Albasty, Bura, Boss) and the pickups read their values from assets in Assets/Data/Stats. Shared enemy values are in EnemyStats, and each enemy has its own subclass for its special numbers. I checked that the game plays the same as before, and that changing the wolf's speed in the asset during Play changes the game immediately.

**2026-11-23**
The three levels are now built from LevelData assets: enemies, spawn points, stars needed, start oil and a target time. One LevelLoader script loads any level. Two problems came up: the boss summon points were objects in the scene, so the boss could not be spawned from data. They became offsets inside the boss prefab. The heroine is not spawned again; she is moved to the start point, because the HUD, pause and camera already reference her. The GDD did not have a target time or a score, but the brief asks for time and best score, so I added a score based on stars and time.

**2026-11-23**
Progress, best score, unlocked levels and volume settings are saved to save.json in persistentDataPath. The game saves after a level, when settings change and when the app is paused. The file is written to a temporary file first and then replaces save.json. If save.json is broken, the game copies it to save_broken.json and starts with default values instead of crashing. I added a Reset Progress button with a confirm panel and a level select that shows locked levels.

**2026-11-24 - UI fixes**
The agent hid the star on the menu logo with an Image "Filled" trick. It worked, but it was hard to explain and easy to break, so I replaced it with a separate logo sprite without the star. I also stopped the menu background from stretching, swapped the pause buttons (Continue at the bottom, Settings as a gear icon), put Play, Settings and Quit in one row, and made the main menu hide behind the settings window.

**2026-11-24 - Phone test**
I built the APK and tested it on an Android phone. I finished Level 1, changed the music and SFX volume, closed the app completely and opened it again. Level 2 was still unlocked and the volume settings were restored.

## Week 12: Sound Pass (A12)

**2026-11-24**
I asked the agent to make all sounds with a Python script, with no downloads. I listened to them and they sounded fake and cheap, nothing like the steppe mood of the game. I switched to the ElevenLabs tool and regenerated every sound from text prompts (wolf growl, camel snort, crystal chime for the star, dombra for the music). I also chose the sound style myself: game-like, not realistic.

**2026-11-24**
For the boss I asked for a lasso sound, but the first version was a sword being drawn. I changed the prompt to a cartoon-style whirling lasso and a separate snap sound for the throw, because the boss attacks with a lasso, not a sword.

**2026-11-24**
The agent wanted to skip the beam attack sound, because the lantern beam only freezes enemies and does not damage them. I did not want to lose the sound, so I told it to play it when a Zharqyl hit lands on an enemy that survives. When the enemy dies, a separate dispel sound plays.

**2026-11-24**
The agent suggested playing the enemy dispel sound on the enemy itself. The sound was cut off when the enemy was destroyed. The other option, PlayClipAtPoint, creates a source without a mixer group, which breaks the requirement that every source goes through the mixer. I told the agent to play it through the player's SfxPlayer instead.

**2026-11-25**
The level restarted after death with a fixed timer, and the death animation could be cut in the middle. I asked the agent to remove the timer and call RestartLevel from an Animation Event on the last frame of the death animation. Now the restart always comes after the animation.

**2026-11-25**
On death I heard two sounds at once: the death sound and the lose jingle. I replaced the death sound with my own voice recording and removed the separate lose jingle. In my game death restarts the level at once, so death and lose are the same moment and one sound is clearer. I also replaced the win sound with a simpler, shorter one, because the first version was too long.