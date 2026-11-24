# Шырақ (Shyraq)

2D-платформер для мобильных (landscape) на Unity 6. Девочка **Ай** с фонарём деда идёт через ночную казахскую степь, чтобы вернуть на небо звёзды, которые сорвали семь разбойников **Жеті қарақшы**. Сюжет основан на легенде о Темірқазық (Полярной звезде) и небесных конях Ақбозат и Көкбозат.

- **Жанр:** 2D-платформер с механикой света.
- **Платформа:** мобильные устройства (Android / iOS), только альбомная ориентация. В редакторе можно играть с клавиатуры или геймпада.

Курс Game Development Fundamentals (Narxoz). Автор: Алмасбек.

## Как играть
- Главная механика — **свет фонаря**:
  - **луч** (удерживать, наводится стиком) открывает тайные тропы (они твёрдые только в свете) и замораживает духов;
  - **Жарқыл** (FIRE) — снаряд света;
  - на луч и Жарқыл тратится **масло**; бутылки масла можно подбирать на уровне.
- Цель уровня — собрать все **звёзды**, тогда открываются ворота на следующий уровень.
- У Ай 3 HP, после урона она мигает. Двойной прыжок.
- Уровни: Main Menu → Level 1 «Дала» (степь) → Level 2 «Сай» (ущелье и мазар) → Level 3 «Темірқазық» (босс — вожак Жеті қарақшы) → Win.
- Босс прячется в тени: сначала сними тень лучом, потом бей Жарқылом. Во второй фазе он зовёт малых духов.

## Управление
| Действие | Мобильный (кнопки на экране) | Клавиатура (тест) | Геймпад |
|---|---|---|---|
| Ходьба / наведение луча | стик | A / D, стрелки (W / S — наведение) | левый стик |
| Прыжок (двойной) | JUMP | Space | нижняя кнопка (A / Cross) |
| Луч | LIGHT (удержание) | E / Left Shift | верхняя кнопка (Y / Triangle) |
| Жарқыл | FIRE | J / X | левая кнопка (X / Square) |
| Пауза | кнопка паузы справа вверху | — | — |

Ввод сделан на Input System, раскладка — `Assets/PlayerControls.inputactions`.

## Как открыть проект
1. Установить Unity **6000.6.9f1** (Unity 6) через Unity Hub.
2. Склонировать репозиторий: `git clone https://github.com/vexbyr/shyraq.git`
3. Unity Hub → Add → Add project from disk → выбрать папку проекта.
4. Открыть сцену `Assets/Scenes/MainMenu.unity` (она первая в Build Settings) и нажать Play.

Шаблон проекта — Universal 2D (URP 2D). Для проверки мобильного экрана: Game view → разрешение landscape, например 1920x1080, или Device Simulator.

## Data and saves
Числа баланса и расстановка уровней лежат в ассетах ScriptableObject (`Assets/Data/`), а не в коде. Скрипты только читают ассеты и читают числа в момент использования, поэтому правку ассета во время Play видно сразу. Ассет при этом нельзя менять из кода: текущее HP, масло и таймеры хранятся в самих объектах.

### Ассеты данных
| Ассет | Класс | Что задаёт |
|---|---|---|
| `Data/Stats/Player_Stats` | `PlayerStats` | Бег, прыжок, число прыжков, HP, неуязвимость, отскок от удара, максимум и расход масла, Жарқыл (цена, урон, скорость, время жизни, пауза) |
| `Data/Stats/Wolf_Stats` | `ShadowWolfStats` | HP, урон, патруль, зрение, угол обзора, погоня, дальность замаха, замах, укус, отдых |
| `Data/Stats/Albasty_Stats` | `AlbastyStats` | HP, урон снаряда, зрение, прицел, перезарядка, скорость и время жизни снаряда |
| `Data/Stats/Bura_Stats` | `BuraStats` | HP, урон, зрение, допуск по высоте, замах, скорость и длина разбега, оглушение |
| `Data/Stats/Boss_Stats` | `BossStats` | HP, урон аркана, патруль в тени, проявление после луча, порог фазы 2, паузы между бросками в фазе 1 и 2, длина аркана, призыв духов |
| `Data/Stats/Spirit_Stats` | `EnemyStats` | HP, скорость и урон духа и малого духа |
| `Data/Stats/Item_Values` | `ItemValues` | Масло из бутылочки и из малого духа, возрождение бутылочки на арене, урон шипов, сколько держится тайная тропа |
| `Data/Stats/Score_Settings` | `ScoreSettings` | Очки за звезду, бонус за время, сколько бонуса теряется за секунду сверх целевого времени |
| `Data/Levels/Level_01..03` | `LevelData` | Номер, название, сцена, следующий уровень, сколько звёзд нужно для ворот, масло на старте, целевое время, точка старта Ай, враги (префаб, позиция, маршрут) и предметы (звёзды, масло) |

`EnemyStats` содержит общие числа врагов. `ShadowWolfStats`, `AlbastyStats`, `BuraStats` и `BossStats` — его наследники и добавляют только свои числа.

Геометрия уровня (земля, платформы, тайные тропы, ворота, шипы, лодка) остаётся в сценах. Врагов, звёзды и масло создаёт `LevelLoader`: это один скрипт, одинаковый во всех трёх сценах. Уровень выбирается в меню и передаётся через `GameSession.CurrentLevel`. Если сцену уровня запустить прямо в редакторе, берётся её собственный `LevelData`.

### Очки
`очки = звёзды × 100 + бонус за время`. Бонус равен 500, если уровень пройден не дольше целевого времени (`parTimeSeconds`: 90 / 150 / 180 с). За каждую лишнюю секунду бонус уменьшается на 5, но не ниже 0. Все множители лежат в `Score_Settings`.

### Файл save.json
Сохранение — JSON-файл (`JsonUtility`) в `Application.persistentDataPath`:
- Windows (редактор): `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Shyraq\save.json`
- Android: `/storage/emulated/0/Android/data/com.vexbyr.shyraq/files/save.json`

```json
{
    "version": 1,
    "unlockedLevels": [1, 2],
    "levelRecords": [
        { "levelNumber": 1, "bestStars": 6, "bestScore": 1100, "bestTimeSeconds": 74.5 }
    ],
    "totalBestScore": 1100,
    "musicVolume": 0.8,
    "sfxVolume": 0.8
}
```

| Поле | Значение |
|---|---|
| `version` | Версия формата. Если версия незнакомая, начинается новая игра |
| `unlockedLevels` | Открытые уровни. Уровень 1 открыт всегда |
| `levelRecords` | Лучший результат каждого пройденного уровня: звёзды, очки, время в секундах |
| `totalBestScore` | Сумма лучших очков (при загрузке пересчитывается) |
| `musicVolume`, `sfxVolume` | Громкость 0..1. Пока звука в игре нет, обе применяются через `AudioListener.volume` (среднее двух). Раздельная громкость через AudioMixer появится в A12 |

Новая игра (значения по умолчанию задаются в одном месте — конструкторе `SaveData`): открыт только уровень 1, рекордов нет, громкость 0,8.

Безопасная загрузка (`SaveSystem`):
- файла нет — новая игра;
- файл пустой, не JSON или читается с ошибкой — предупреждение в Console, копия плохого файла в `save_broken.json`, новая игра. Игра не падает;
- файл читается, но данные неправильные — они исправляются: номера уровней вне 1..3 удаляются, уровень 1 открывается, громкость обрезается до 0..1, сумма очков пересчитывается.

Запись идёт сначала в `save.tmp`, потом этот файл заменяет `save.json`. Если запись прервётся, старое сохранение останется целым.

### Когда игра сохраняет
- уровень пройден (ворота или победа над боссом): обновляется рекорд и открывается следующий уровень;
- настройки: палец отпустил ползунок Music или SFX, и при закрытии окна Settings (в главном меню и в паузе);
- игру свернули (`OnApplicationPause(true)`) или закрыли (`OnApplicationQuit`).

Прогресс внутри незаконченного уровня (собранные звёзды, позиция, масло) **не сохраняется**: после выхода уровень начинается заново.

### Меню
- **Play** открывает выбор уровня. Открытые уровни показывают лучшие звёзды и очки. Закрытые кнопки тусклые, не нажимаются и подписаны «Locked».
- В конце уровня показывается экран итогов: звёзды, время, очки, «New best!» при новом рекорде, кнопки **Next level** и **Level select**. После босса — экран Win, как раньше.
- **Settings** (главное меню справа вверху и окно паузы) — ползунки Music и SFX.

### Как сбросить прогресс
- В игре: Play → **Reset Progress** → «Reset all progress? Settings are kept.» → **Yes**. Открытые уровни и рекорды сбрасываются, громкость остаётся.
- В редакторе: меню **Shyraq → Delete Save File** удаляет `save.json`, **Shyraq → Open Save Folder** открывает папку сохранения.

## Assets and licenses
**Арт.** Весь арт сделан для этого проекта: спрайты героини, босса, врагов A10 и духов сгенерированы в [PixelLab](https://pixellab.ai) и доработаны скриптами Python + Pillow, остальное нарисовано своим кодом на Python + Pillow. PixelLab: пробный тариф, использование по [Terms of Service](https://pixellab.ai/termsofservice).

| Что | Файлы | Источник | Доработка |
|---|---|---|---|
| Ай: idle, ходьба, прыжок, падение, урон, поза с фонарём | `Assets/Art/Hero/Ai_*`, `Ai_sheet.png` | PixelLab (Create Image Pro, animate_image_pixminimax, edit_image_pixen) | Pillow: только сдвиг, нарезка, сборка листа; пиксели не перерисовывались |
| Босс: idle, замах, бросок аркана | `Assets/Art/Boss/Boss_idle_0..1`, `Boss_attack_0..1` | PixelLab: дизайн — create_image_pixen, кадры — animate_image_pixminimax (один клип, 8 кадров) | Pillow: перекраска палитры, контур как у Ай, сдвиг ступнями вниз, 1 px ауры замаха |
| Босс: урон, тень, вторая фаза | `Assets/Art/Boss/Boss_hurt_0`, `Boss_shadow`, `Boss_phase2` | Перекраска `Boss_idle_0` скриптом Pillow | Урон и тень — только цвет; фаза 2 — красные глаза и звёзды, осколки-звёзды вокруг (Pillow) |
| Көлеңке-қасқыр: обычный, замах, укус, заморожен | `Assets/Art/Enemies/ShadowWolf_*` | PixelLab: дизайн — create_image_pixen, замах и укус — animate_image_pixminimax | Pillow: перекраска, контур как у Ай, аура замаха, сдвиг; заморозка — перекраска обычного кадра |
| Албасты: обычная, прицел, заморожена | `Assets/Art/Enemies/Albasty_*` | PixelLab: дизайн — create_image_pixen, прицел — animate_image_pixminimax | Pillow: перекраска глаз и свечения, аура прицела; заморозка — перекраска обычного кадра |
| Қара бура: обычный, подготовка, разбег, оглушён, заморожен | `Assets/Art/Enemies/Bura_*` | PixelLab: дизайн — create_image_pixen, подготовка и разбег — animate_image_pixminimax | Pillow: перекраска глаза, аура, полосы скорости; оглушён и заморожен — Pillow поверх обычного кадра |
| Снаряд Албасты (2 кадра) | `Assets/Art/Enemies/AlbastyProjectile_0..1` | Свой код Python + Pillow, без генеративных моделей | — |
| Духи: обычный, малый, заморожены | `Assets/Art/Enemies/Spirit_normal`, `Spirit_frozen`, `Spirit_small`, `Spirit_small_frozen` | PixelLab: дух — create_image_pixen, малый дух — edit_image_pixen (перерисован в 32x32) | Pillow: полупрозрачный низ, багровая палитра малого духа, заморозка — перекраска |
| Иконка приложения | `Assets/Art/UI/AppIcon_512` | Свой код Python + Pillow; фигура Ай — кадр `Ai_aim_0` (PixelLab) | — |
| Тайлы всех уровней, фоны, декор (юрта, балбал, кулпытас, мазар, кол Темірқазық, кони) | `Assets/Art/Tiles`, `Backgrounds`, `Props` | Свой код Python + Pillow, без генеративных моделей | — |
| Звёзды, масло, ворота, луч, Жарқыл, эффекты | `Assets/Art/Props`, `Light`, `FX` | Свой код Python + Pillow, без генеративных моделей | — |
| UI: кнопки, стик, полоски масла и HP босса, панель паузы, логотип, свой пиксельный шрифт 5x7 | `Assets/Art/UI` | Свой код Python + Pillow, без генеративных моделей | — |
| Общий стилевой проход врагов, духов и босса | `Assets/Art/Enemies`, `Assets/Art/Boss` | Скрипт Pillow | Контур как у Ай, одна палитра на персонажа (median cut, без дизеринга) |
| Лодка Level2 | `Assets/Art/Props/Boat` | Свой код Python + Pillow, без генеративных моделей (PixelLab не использовался) | — |
| Вода реки Level2 (2 кадра) | `Assets/Art/Tiles/River_Water`, `River_Water_1` | Свой код Python + Pillow, без генеративных моделей | Второй кадр — тот же рисунок, сдвинутый по кругу на 16 px |
| Шипы | `Assets/Art/Props/Thorn` | Свой код Python + Pillow, без генеративных моделей | Трава из палитры `Ground_top` |
| Greybox: физический материал `NoFriction` (на Ай) и старые спрайты-заглушки | `Assets/Greybox/` | Созданы в проекте (см. `AI_LOG.txt`) | Заглушки в игре больше не используются (кроме `Square` — красная подсветка зоны аркана босса); папка оставлена из-за физического материала |
| `circle.png`, `square.png` | `Assets/Art/` | Из шаблона Unity (Universal 2D), мной не менялись | — |

- Для эскизов Ай в PixelLab использовался референс `Assets/Art/Reference/Ai_reference.png` (в репозиторий не входит, папка в `.gitignore`). Референс сгенерирован в Gemini c ChatGPT и использован только как ориентир при создании эскизов, в игре его нет.
- Папка Assets/TextMesh Pro импортирована из Unity (Import TMP Essentials), шейдеры оттуда не мои.

**Шрифт.** LiberationSans из TextMesh Pro Essentials, лицензия SIL OFL 1.1 (`Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`).

**Музыка и звуки.** См. раздел [Audio](#audio).

## Audio
**Как устроен звук.** Микшер `Assets/Audio/ShyraqMixer.mixer`: Master → Music → MusicPlay и Master → SFX. Громкость Music и SFX в настройках — параметры `MusicVol` и `SfxVol` (ползунок 0,0001..1, в микшер идёт `Log10(v) * 20` дБ), сохраняются в `save.json`. На паузе снапшот Paused (музыка −10 дБ и Lowpass 800 Гц), после паузы — Normal. Музыку играет `MusicPlayer` (сцена MainMenu, живёт между сценами): меню и Win — `music_menu`, уровни — `music_level`, переход 1 с; на следующем уровне и после перезапуска трек не начинается заново. Звуки играет `SfxPlayer`: случайный клип из списка и высота 0,92..1,08.

**Звуки:** ElevenLabs Sound Effects (тариф Free, 25.11.2026), обработаны скриптом Python: моно, обрезка, фейды, пик −1 dBFS, WAV 24 кГц. **Sound effects: ElevenLabs (elevenlabs.io).** На бесплатном тарифе — только некоммерческое использование, учебный проект подходит. **Музыка:** ElevenLabs Music v2.5 через Higgsfield (тариф Ultra, 25.11.2026), по 30 с, MP3. **Music: ElevenLabs Music via Higgsfield (higgsfield.ai).**

| Файл (`Assets/Audio/`) | Для чего | Источник | Лицензия |
|---|---|---|---|
| `SFX/jump_01.wav, jump_02.wav, jump_03.wav` | Прыжок Ай (случайный из трёх) | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/hit_01.wav, hit_02.wav` | Урон по Ай (без вскрика) | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/pickup_star_01.wav … pickup_star_03.wav` | Звезда собрана | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/windup_wolf.wav` | Замах Көлеңке-қасқыр | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/windup_albasty.wav` | Прицел Албасты | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/windup_bura.wav` | Замах Қара бура | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/windup_boss.wav` | Замах аркана босса | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/windup_spirit.wav` | Не используется: дух не замахивается, кусает касанием | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/death_bruh.mp3` | Смерть Ай — «bruh» (без изменения высоты) | own voice recording by Almasbek | own work, CC0 |
| `SFX/win.wav` | Уровень пройден, босс побеждён | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/ui_click.wav` | Нажатие любой кнопки | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/beam_glow.wav` | Луч фонаря включился | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/zharqyl_flash.wav` | Выстрел Жарқылом | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/beam_attack.wav` | Жарқыл попал во врага, враг ещё жив | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/enemy_dispel.wav` | Враг побеждён Жарқылом и растворился | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/platform_appear.wav` | Тайная тропа появилась в свете | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/oil_pickup.wav` | Масло подобрано | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `SFX/boss_lasso_snap.wav` | Босс бросил аркан | [ElevenLabs](https://elevenlabs.io) Sound Effects, generated from my text prompt | ElevenLabs Free plan: non-commercial use only ([Terms of Use](https://elevenlabs.io/terms-of-use) §1(c)); credited anyway |
| `Music/music_menu.mp3` | Музыка меню и экрана Win (петля) | ElevenLabs Music v2.5 via Higgsfield (higgsfield.ai), generated from my text prompt | Higgsfield Terms of Use §4.4: no ownership claimed, commercial use not restricted; credited anyway |
| `Music/music_level.mp3` | Музыка уровней (петля) | ElevenLabs Music v2.5 via Higgsfield (higgsfield.ai), generated from my text prompt | Higgsfield Terms of Use §4.4: no ownership claimed, commercial use not restricted; credited anyway |

## Процесс разработки с ИИ-ассистентом
Каждая сессия записана в [`AI_LOG.txt`](AI_LOG.txt): что я просил, что сделал ИИ, что проверено и что поправлено.
