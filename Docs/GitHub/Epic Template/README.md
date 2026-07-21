# GitHub Epic Template

**How to convert a Wave System into a GitHub Epic.**

Epics are large bodies of work within a Wave. Each Epic contains 3-8 related Issues.

---

## Epic Template

```markdown
# Epic: [System Name]

## Wave
Wave X: [Wave Name]

## Objective
[What does this Epic accomplish?]

## Success Criteria
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

## Related Issues
- [Issue: Component A](#)
- [Issue: Component B](#)
- [Issue: Component C](#)

## Technical Notes
[Any architectural considerations]

## Related Documentation
- **Wave README**: [Development Bible Wave Link]
- **Technical Specs**: [Link to Technical Specs]
```

---

## Example: Wave 0 Epic

```markdown
# Epic: Time Engine

## Wave
Wave 0: Foundation

## Objective
Build a deterministic time engine that runs at 60 ticks per second, independent of frame rate. This is the heartbeat of all Republic simulation.

## Success Criteria
- [ ] Time engine runs at stable 60 Hz
- [ ] Frame-rate independent stepping
- [ ] Time scaling (pause, normal, fast-forward)
- [ ] Deterministic (same seed = same progression)
- [ ] Serializable for save/load
- [ ] Performance: <1ms per tick
- [ ] No memory leaks over 8-hour session

## Related Issues
- [Issue: TimeEngine class structure](#)
- [Issue: TickManager implementation](#)
- [Issue: Time scaling system](#)
- [Issue: Deterministic RNG](#)
- [Issue: Performance monitoring](#)
- [Issue: Unit tests for Time Engine](#)

## Technical Notes
- Uses fixed timestep pattern
- Integrates with Event Bus for tick announcements
- Must support both real-time and turn-based modes
- Serialization format: [describe format]

## Related Documentation
- **Wave 0 README**: [Development Bible/Wave 00 - Foundation/README.md](../../Development%20Bible/Wave%2000%20-%20Foundation/README.md)
- **TDD**: [TDD/README.md](../../TDD/README.md)
```

---

## How to Create Epics

1. Read the Wave README
2. Identify each System (10 per Wave)
3. Create one Epic per System
4. Break each Epic into 3-8 Issues
5. Convert Issues into AI Implementation Prompts

---

## Epic Conversion Process

**From Development Bible to GitHub:**

```
Wave README
  └─ 10 Systems per Wave
      └─ 1 Epic per System
          └─ 3-8 Issues per Epic
              └─ 1 AI Prompt per Issue
```

---

## Tips

✅ Each Epic should take 1-2 weeks to complete
✅ Success criteria must be measurable
✅ Link related Issues clearly
✅ Include technical notes for developers
✅ Reference Wave documentation

---

## Next Steps

Once Epic is created:
1. Break into Issues using [Issue Template](../Issue%20Template/README.md)
2. Convert Issues to AI prompts using [AI Prompt Template](../AI%20Implementation%20Prompt%20Template/README.md)
3. Begin implementation
