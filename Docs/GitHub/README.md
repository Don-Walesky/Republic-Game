# GitHub Conversion Guide

**How to convert Republic Development Bible into GitHub Epics, Issues, and AI Implementation Prompts.**

---

## The Conversion Pipeline

```
Development Bible (Docs/Development Bible/)
  ↓
Epics (GitHub, one per System)
  ↓
Issues (GitHub, 3-8 per Epic)
  ↓
AI Implementation Prompts (Docs/GitHub/AI Implementation Prompt Template/)
  ↓
Implementation (Copilot or developer)
  ↓
Pull Request
  ↓
Code Review
  ↓
Merge to main
```

---

## Templates

### 1. [Epic Template](./Epic%20Template/README.md)
**Convert Wave Systems into GitHub Epics**
- One Epic per System (10 per Wave)
- 1-2 weeks of work
- Contains 3-8 related Issues
- Includes success criteria

### 2. [Issue Template](./Issue%20Template/README.md)
**Convert Epics into actionable Issues**
- One Issue per component
- 1-3 days of work
- Specific, measurable requirements
- Clear acceptance criteria

### 3. [AI Implementation Prompt Template](./AI%20Implementation%20Prompt%20Template/README.md)
**Convert Issues into AI Implementation Prompts**
- Detailed technical specification
- Code examples and APIs
- Integration points documented
- Testing strategy included

---

## Conversion Process

### Step 1: Start with Development Bible
Read `Docs/Development Bible/Wave X/README.md`

### Step 2: Create Epics
For each of the 10 Systems in the Wave:
1. Use [Epic Template](./Epic%20Template/README.md)
2. Create GitHub Epic with the template
3. Link to Wave documentation

### Step 3: Create Issues
For each Epic, create 3-8 Issues:
1. Use [Issue Template](./Issue%20Template/README.md)
2. Create GitHub Issue
3. Link to parent Epic
4. Show dependencies

### Step 4: Create AI Prompts
For each Issue:
1. Use [AI Implementation Prompt Template](./AI%20Implementation%20Prompt%20Template/README.md)
2. Store in `Docs/GitHub/AI Prompts/Wave X/Issue-Name.md`
3. Link from GitHub Issue
4. Include technical spec

### Step 5: Implementation
1. Copilot reads AI Prompt
2. Implements according to spec
3. Creates Pull Request
4. PR is reviewed
5. Merged to main
6. GitHub Issue closed

---

## Example: Wave 0 Conversion

### Wave 0 has 10 Systems:
1. Project Bootstrap
2. Logging System
3. Configuration Manager
4. Save System (Framework)
5. Event Bus
6. **Time Engine** ← Example
7. Simulation Core
8. Dependency Injection
9. Game Loop
10. Folder Structure

### For Time Engine:

**Epic:**
- Epic: Time Engine
- 1-2 weeks
- 6 related Issues

**Issues:**
1. Issue: TimeEngine class structure
2. Issue: TickManager implementation
3. Issue: Time scaling system
4. Issue: Deterministic RNG
5. Issue: Performance monitoring
6. Issue: Unit tests for Time Engine

**AI Prompts:**
- One detailed prompt per Issue
- Stored in `Docs/GitHub/AI Prompts/Wave 00/TimeEngine-Class-Structure.md`
- Includes code examples
- Shows expected behavior
- Lists success criteria

---

## Tips

✅ **Start simple** - Begin with Wave 0
✅ **Complete one Wave at a time** - Don't jump ahead
✅ **Link everything** - Epics link to Issues, Issues link to Prompts
✅ **Be specific** - The more detail, the better the AI implementation
✅ **Include examples** - Show code, not just descriptions
✅ **Test everything** - Unit tests are part of the spec

---

## File Organization

```
Docs/GitHub/
├── README.md (this file)
├── Epic Template/
├── Issue Template/
├── AI Implementation Prompt Template/
└── AI Prompts/
    ├── Wave 00/
    ├── Wave 01/
    └── ...
```

---

## Quick Links

- **[Development Bible](../Development%20Bible/README.md)** - Wave documentation
- **[Epic Template](./Epic%20Template/README.md)** - How to create Epics
- **[Issue Template](./Issue%20Template/README.md)** - How to create Issues
- **[AI Prompt Template](./AI%20Implementation%20Prompt%20Template/README.md)** - How to create AI prompts

---

## Next Steps

1. Read all three templates
2. Start with Wave 0
3. Create Epics for each of the 10 Systems
4. Break Epics into Issues
5. Create AI Implementation Prompts
6. Begin implementation
