# LOOP PLANET SOCCER
## GMTK Game Jam 2025

---

## Game Concept

**Core Idea:** A minimalist 2-player soccer game set on a small **circular planet**, inspired by the theme "Loop." Each player stands upright on opposing sides of the planet and must defend their goal while attempting to kick the ball around the planet's circumference into the opponent's goal.

### Key Features
- **Theme Integration:** The gameplay loop is literal - the ball loops around the circular planet
- **Perspective:** Side view with mechanics operating in polar coordinates  
- **Visual Style:** Minimalist black-and-white pencil sketch aesthetic with blue space ambiance
- **Gameplay:** Chaotic, fast-paced action as the ball orbits the planet

---

## Core Mechanics

1. **Player Movement:** Players move around the surface of the circular planet (perpendicular to surface)
2. **Kick System:** Directional impulse system - players can add momentum to the ball
3. **Ball Physics:** Ball orbits based on circular momentum (fake gravity optional)
4. **Goal System:** Each side has a goal (side-view nets) with boundary lines preventing player entry
5. **Scoring:** Ball scores by entering the opponent's goal area
6. **Ball Behavior:** Bounces, wraps around planet surface
7. **Controls:** Local multiplayer - WASD vs Arrow Keys

---

## Project Timeline (30 Work Hours Remaining)

### Day 1: Thursday Late Start (10 hours remaining)
**Focus: Core Systems & Foundation**
- [ ] Set up project structure
- [ ] Implement basic circular planet physics
- [ ] Create player movement system (polar coordinates)
- [ ] Basic ball physics and orbiting mechanics
- [ ] Simple kick system
- [ ] Basic collision detection

### Day 2: Friday (12 hours)
**Focus: Gameplay & Polish**
- [ ] Goal detection and scoring system
- [ ] Player boundaries and restrictions
- [ ] Ball bouncing and wrapping
- [ ] Score tracking and win conditions
- [ ] Basic UI (scoreboard, restart)
- [ ] Audio implementation
- [ ] Visual effects and polish

### Day 3: Saturday (8 hours)
**Focus: Final Polish & Submission**
- [ ] Bug fixes and balancing
- [ ] Final art pass and visual polish
- [ ] Menu systems and game flow
- [ ] Playtesting and refinement
- [ ] Build preparation and submission

---

## Asset Production List

### Environment Assets
- [ ] **Planet Base** - Main circular playing field
- [ ] **Divider Line** - Mid-surface boundary marker
- [ ] **Background** - Space or abstract blue theme
- [ ] **Parallax Elements** - Stars/decorative space objects (optional)

### Player Assets
- [ ] **Player 1 Sprite** - Black stick figure
- [ ] **Player 2 Sprite** - White stick figure  
- [ ] **Kick Animation** - 1-2 frame animation cycle
- [ ] **Movement Animations** - Jump/walk cycles (optional)
- [ ] **Player Shadows** - Ground contact indicators

### Ball Assets
- [ ] **Soccer Ball Sprite** - Main game object
- [ ] **Ball Trail Effect** - Motion visualization
- [ ] **Bounce Animation** - Squash/stretch on impact
- [ ] **Kick Impact Effect** - Burst animation on player contact

### Goal Assets
- [ ] **Goal Structure** - Side-view goal posts
- [ ] **Net Overlay** - Goal net graphic
- [ ] **Goal Flash Effect** - Scoring visual feedback
- [ ] **Goal Animation** - Shake or bounce on score

### UI Assets
- [ ] **Scoreboard Display** - Current game score
- [ ] **Goal Popup** - "Goal!" celebration text
- [ ] **Game End Screen** - "Win!" / "Draw" results
- [ ] **Restart Button** - Game reset control
- [ ] **Title Screen** - Main menu panel
- [ ] **Pause Menu** - Game pause interface (optional)

### VFX & Polish Assets
- [ ] **Kick Burst Effect** - Player action feedback
- [ ] **Trail Particles** - Motion enhancement
- [ ] **Goal Celebration** - Sparkle/confetti effects
- [ ] **Background Elements** - Animated stars/loops
- [ ] **Blue Theme Shader** - Color overlay system

---

## Visual Style Guide

### Art Direction
- **Line Style:** Hand-drawn sketch/pencil line aesthetic
- **Color Palette:** Black and white primary with blue accent tones
- **Complexity:** Minimal geometric shapes and clean lines
- **Motion:** Soft trails and stylized arcs for movement visualization

### Technical Style
- **Resolution:** Consistent pixel art or vector-based assets
- **Animation:** Simple 1-2 frame cycles for performance
- **Effects:** Particle-based trails and impacts
- **Shader:** Blue tint overlay for space ambiance

---

## Final Submission Checklist

### Core Functionality
- [ ] Two-player local multiplayer working
- [ ] Ball physics and planet orbiting functional
- [ ] Scoring system implemented
- [ ] Win/lose conditions working
- [ ] Restart functionality

### Technical Requirements
- [ ] Game runs smoothly (target 60 FPS)
- [ ] No game-breaking bugs
- [ ] Controls responsive and intuitive
- [ ] Audio implemented (SFX at minimum)

### Polish & Presentation
- [ ] Visual style consistent throughout
- [ ] UI clear and functional
- [ ] Game feel polished (juice, feedback)
- [ ] Title screen and basic menus
- [ ] Game loop complete (play → score → restart)

### Submission Prep
- [ ] Build tested on target platform
- [ ] Game description written
- [ ] Screenshots/GIFs captured
- [ ] Source files backed up
- [ ] Submission uploaded before deadline

---

## Success Metrics

**Minimum Viable Product:**
- Two players can control characters on a circular planet
- Ball can be kicked and orbits the planet
- Goals can be scored
- Basic score tracking

**Target Experience:**
- Chaotic, fun multiplayer action
- Clear visual feedback for all actions
- Smooth, responsive controls
- Polished presentation with theme integration

---

## Development Status - CURRENT BUILD

### ✅ **Implemented Features**

#### Core Gameplay Systems
- ✅ **Circular Planet Physics** - Players stick to planet surface with realistic gravity
- ✅ **360° Player Movement** - Full freedom of movement around the planet (no restrictions)
- ✅ **Ball Physics System** - Realistic ball physics with orbital mechanics
- ✅ **Kick System** - Dynamic kicking with velocity-based force calculation
- ✅ **Goal Detection** - Multi-collider goal system with top prevention (crossbar)
- ✅ **Scoring System** - Player score tracking with win conditions
- ✅ **Game State Management** - MainMenu → Credits → Playing → Paused → GameOver

#### User Interface
- ✅ **Main Menu System** - Start button, credits button, title display
- ✅ **Credits Screen** - Dedicated credits page with back navigation
- ✅ **Game UI** - Live score display and timer system
- ✅ **Pause System** - ESC to pause/resume with overlay
- ✅ **Game Over Screen** - Win/lose/draw detection with restart functionality
- ✅ **Background Tinting** - UI overlay system with fade effects
- ✅ **Custom Button Animations** - Hover scaling and click effects

#### Advanced Features  
- ✅ **Audio Management** - Centralized audio system with SFX support
- ✅ **Visual Effects** - Particle effects for goals, kicks, and impacts
- ✅ **Custom UI Components** - Pixel-perfect button detection for custom sprites
- ✅ **Power-up System** - Kick multiplier, size increase, jump boost
- ✅ **Ball Spawning System** - Automatic ball respawn after goals
- ✅ **Footstep Audio** - Contextual movement sound effects

#### Technical Implementation
- ✅ **Modern Unity APIs** - Updated to use Unity 2023+ methods
- ✅ **Component Architecture** - Modular, reusable script design  
- ✅ **Debug Visualization** - Comprehensive gizmo system for development
- ✅ **Performance Optimized** - Efficient collision detection and physics
- ✅ **Input System** - Dual-player controls (WASD vs Arrow Keys)

### 🎮 **Current Game Flow**
1. **Main Menu** → Start Game / View Credits
2. **Credits** → Back to Main Menu  
3. **Gameplay** → Real-time soccer with scoring
4. **Pause** → ESC to pause/resume
5. **Game Over** → Win/lose screen with restart

### 🔧 **Recent Major Updates**
- **Goal System Enhancement** - Added top collider prevention (realistic crossbar)
- **Movement Liberation** - Removed all player movement restrictions (full 360° freedom)
- **UI System Overhaul** - Complete menu system with credits and navigation
- **Custom Button System** - Advanced hover/click animations for UI elements
- **Audio Integration** - Comprehensive sound system with effects

---

## Development Log

### Friday, August 2, 2025 - Current Session
- **Status:** Major feature completion and polish phase
- **Recent Work:** 
  - Multi-collider goal system with crossbar prevention
  - Complete UI system with credits and navigation
  - Custom button animation system  
  - Player movement restriction removal
  - Modern Unity API updates
- **Next:** Final polish, builds, and submission prep

### Thursday, August 1, 2025 - 11:20 AM
- **Status:** Starting development (Late start - 2.5 days remaining instead of full 4 days)
- **Focus:** Project setup and foundation
- **Time Remaining:** ~30 hours total
- **Notes:** Beginning with reduced timeline, need to prioritize core mechanics and MVP features

---

*GMTK Game Jam 2025 - Theme: "Loop"*