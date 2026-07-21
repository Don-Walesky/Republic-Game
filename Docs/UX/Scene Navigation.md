# Scene Navigation

How scenes and views connect in the Republic UI.

## Scene Hierarchy

```
Root
├── Main Menu
│   ├── New Game Dialog
│   ├── Continue Game
│   ├── Load Game Dialog
│   ├── Settings Dialog
│   └── Credits
│
├── Campaign
│   ├── Executive Workspace (main scene)
│   ├── Map View
│   ├── Economy System
│   ├── Government System
│   ├── Military System
│   ├── Diplomacy System
│   ├── Population System
│   ├── Reports System
│   └── Pause Menu
│
└── Settings (global)
    ├── Graphics
    ├── Audio
    ├── Gameplay
    ├── Accessibility
    └── Controls
```

## Navigation Patterns

### Primary Navigation (Main Campaign)
- **Executive Workspace**: Always accessible via Home button
- **System Windows**: Open as dialogs/panels
- **Map View**: Toggle from Executive Workspace
- **Pause Menu**: Press ESC anytime

### Secondary Navigation (Within Systems)
- **Breadcrumb Trail**: Show path through menus
- **Back Button**: Return to previous view
- **Home Button**: Jump to Executive Workspace
- **Tab Navigation**: Switch between related views

## Transitions

### Scene Changes
- Fade to black
- Duration: 0.3-0.5 seconds
- Load new scene
- Fade in

### Panel/Window Transitions
- Slide in from edge or center
- Duration: 0.2-0.3 seconds
- No scene reload
- Smooth animation

### Notification Animations
- Pop-in with scale
- Slide from edge
- Duration: 0.1-0.2 seconds

## Responsive Design

### Desktop (1920x1080+)
- Full sidebar
- Detailed information
- Multiple panels visible
- Hover tooltips

### Tablet (1280x720)
- Collapsible sidebar
- Optimized layouts
- Touch-friendly buttons
- Swipe navigation

### Mobile (if supported)
- Full-screen dialogs
- Hamburger menu
- Simplified displays
- Touch-optimized

## Keyboard Navigation

### Main Controls
- `H` - Home (Executive Workspace)
- `ESC` - Pause menu
- `Tab` - Next panel/system
- `Shift+Tab` - Previous panel/system
- `Enter` - Confirm selection
- `Space` - Play/Pause
- `1-9` - Quick access to systems

### System-Specific
- `E` - Economy system
- `G` - Government system
- `M` - Military system
- `D` - Diplomacy system
- `P` - Population system
- `R` - Reports
- `S` - Settings

## Controller Support

### Navigation
- D-Pad/Analog Stick - Menu navigation
- A/Cross - Confirm
- B/Circle - Back
- X/Square - Alternate action
- Y/Triangle - Info/Details

### Game Controls
- LT/L2 - Play/Pause
- RT/R2 - Increase time speed
- LB/L1 - Decrease time speed
- RB/R1 - Next system
- Menu Button - Pause

## Accessibility

### Visual Accessibility
- High contrast mode
- Colorblind-friendly palettes
- Large text option
- Custom font sizes

### Motor Accessibility
- Customizable controls
- Hold-to-activate options
- Voice control support
- Switch access support

### Cognitive Accessibility
- Simplified UI mode
- Tooltips and help
- Tutorial system
- Customizable complexity

---

*Navigation should feel intuitive and responsive.*