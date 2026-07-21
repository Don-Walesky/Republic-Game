# Player Flow

How players move through the game interface and progression loop.

## Main Menu Flow

```
Main Menu
├── New Game → Campaign Setup → Difficulty → Start Game
├── Continue → Last Save → Resume
├── Load Game → Save File List → Resume
├── Settings → Options → Return
├── About → Credits/Info → Return
└── Exit → Confirm → Quit
```

## Campaign Flow

```
Start Campaign
↓
Executive Workspace (main hub)
├── View Status
├── Access Systems
├── Make Decisions
├── Advance Time
└── Loop
```

## Decision Loop

The core gameplay cycle:

1. **Review Status** (2-5 minutes)
   - Check notifications
   - View current metrics
   - Assess situation

2. **Make Decisions** (5-15 minutes)
   - Select system to interact with
   - Review options
   - Analyze consequences
   - Execute decision
   - Receive confirmation

3. **Advance Time** (1 minute)
   - Click play button
   - Watch time advance
   - Observe changes
   - Receive notifications

4. **Monitor Effects** (5-10 minutes)
   - Read notifications
   - Check updated metrics
   - Plan next moves
   - Return to step 1

## System Navigation

From Executive Workspace, access:

```
Executive Workspace
├── Economy → Production/Trade/Taxation → Decisions → Return
├── Government → Legislation/Appointments → Decisions → Return
├── Military → Forces/Warfare/Strategy → Decisions → Return
├── Diplomacy → Relations/Treaties/Alliances → Decisions → Return
├── Population → Demographics/Culture → Decisions → Return
├── Reports → Analytics/History/Forecasting → Insights → Return
└── Settings → Options/Save/Load → Configuration → Return
```

## Pause & Save Flow

```
During Gameplay
↓
Press ESC (Pause)
↓
Pause Menu
├── Resume
├── Save Game
├── Load Game
├── Settings
└── Exit Campaign
```

## Time Control Flow

```
Time Controls (bottom of screen)
├── Pause Button (||)
├── Play Button (▶)
├── Speed Controls (1x, 2x, 0.5x)
├── Stop Fast Forward
└── Advance One Day
```

## Save/Load System

```
Save Game
↓
Enter Name
↓
Choose Slot
↓
Confirm
↓
Save Complete

Load Game
↓
Browse Saves
↓
Select Save
↓
Confirm
↓
Load in Progress
```

## Undo/Rewind Options

- Load last save
- Load previous checkpoint (every 10 game years)
- Reload from beginning

---

*The decision loop is the core gameplay experience.*