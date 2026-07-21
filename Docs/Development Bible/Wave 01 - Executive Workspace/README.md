# Wave 01: Executive Workspace

**The player's office. Their first environment.**

**Duration**: 4-6 weeks

**Status**: 🔴 Not Started

**Depends On**: Wave 0 (Foundation)

**Milestone Gate**: "The player has an office"

---

## Objective

Build the player's workspace. The first thing they see. Where they will spend most of their time.

By the end of Wave 1:
- Player spawns in office
- Can move around and interact
- Phone calls work
- Emails arrive
- Visitors visit
- Calendar is visible
- Notifications appear
- Environment feels immersive and real

---

## Systems to Build

### 1. Campaign HQ Environment
- 3D office layout
- Furniture and decoration
- Lighting
- Sound design
- Interactive hotspots

### 2. Player Movement & Interaction
- First-person or third-person view
- Movement controls
- Object interaction
- Animation system
- Transition system

### 3. Phone System
- Phone model & UI
- Incoming calls
- Call interface
- Dialog system
- Call history

### 4. Email System
- Email interface
- Email inbox
- Read/unread tracking
- Email history
- Reply system

### 5. Visitor System
- NPC visitor models
- Visitor arrival triggers
- Meeting interface
- Visitor personalities
- Visitor memory

### 6. Desk
- Desk model
- Documents on desk
- Decision interface
- Calendar access
- Report viewing

### 7. Calendar
- Calendar interface
- Event scheduling
- Reminder system
- Integration with other systems

### 8. Notification System
- Notification UI
- Alert sounds
- Priority levels
- Notification history
- Notification filtering

### 9. Transition Manager
- Moving between locations
- Fade transitions
- Loading new spaces
- Quick travel

### 10. Audio System
- Background music
- Ambient sounds
- UI sounds
- Dialog audio
- Phone/notification sounds

---

## Deliverables

By end of Wave 1, we have:
✅ Fully immersive office environment
✅ Player can move and interact
✅ Phone system works (incoming calls, dialog)
✅ Email system works (inbox, reading, history)
✅ Visitor system works (NPCs arrive, meetings)
✅ Desk accessible with documents
✅ Calendar visible and usable
✅ Notifications working
✅ Audio atmosphere complete
✅ Ready for Wave 2 (world simulation)

---

## Technical Specs

**See**: `Technical Specs.md` (in this directory)

---

## Integration Points

Wave 1 integrates with:
- **Wave 0**: Uses time engine, event bus, save system
- **Wave 2**: Office displays information from simulation
- **Wave 3**: Campaign information displays in office
- **Wave 4**: Government decisions happen in office
- **Wave 5+**: All information flows through office

---

## Success Criteria

✅ Office fully navigable
✅ All UI elements functional
✅ No blocking bugs
✅ Phone calls work end-to-end
✅ Emails populate correctly
✅ Visitors arrive on schedule
✅ Calendar tracking events
✅ Notifications not spammy
✅ Performance stable (60 FPS)
✅ Ready for multiplayer office sharing (Wave 9)

---

## GitHub Epics

- Epic: Office Environment
- Epic: Player Movement & Interaction
- Epic: Phone System
- Epic: Email System
- Epic: Visitor System
- Epic: Calendar & Scheduling
- Epic: Notification System
- Epic: Audio System

---

## Related Documents

- **[ADR-001: Office First Gameplay](../../Architecture%20Decision%20Records/ADR-001%20Office%20First%20Gameplay.md)**
- **[ADR-003: Communication Driven Gameplay](../../Architecture%20Decision%20Records/ADR-003%20Communication%20Driven%20Gameplay.md)**
- **[Development Bible](../README.md)** - Master roadmap
