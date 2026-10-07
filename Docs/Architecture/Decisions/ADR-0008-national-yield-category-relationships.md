# ADR-0008: National Yield Category Relationships Specification

## Status
Accepted (Specification Only)

## Context
Across Steps 4A through 4D, Republic established:
- The foundational National Yield domain model across 14 canonical categories ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)).
- The calculation input contract and modifier representation ([ADR-0005](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0005-national-yield-calculation-inputs.md)).
- The six-stage calculation rules and architectural responsibilities ([ADR-0006](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0006-national-yield-calculation-rules.md)).
- The isolated, deterministic calculation engine with a minimal neutral baseline ([ADR-0007](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0007-isolated-national-yield-calculation-engine.md)).

Step 4D established the execution engine and deliberately neutralized all premature numerical balancing coefficients (such as arbitrary multipliers, scaling percentages, and invented State Capacity formulas). 

Before numerical calibration can occur, Republic must resolve the underlying structural question:
> **"What can influence what, and why?"**
> Rather than:
> **"By how much?"**

This document establishes the formal, implementation-ready architectural specification for the conceptual relationships, causal directions, and structural boundaries connecting the 14 National Yield categories.

> [!IMPORTANT]
> **Specification-Only Scope Boundary**:
> This document defines conceptual classifications, relationship topologies, and causal roles.
> It deliberately does **not** introduce mathematical equations, coefficients, multipliers, percentages, balancing curves, or yield cycles.
> Numerical calibration is explicitly deferred to subsequent design and calibration phases.

---

## Decision

Republic adopts a **sparse, semantically typed causal network** across the 14 canonical categories. The specification is structured into:
1. Category Classifications
2. Relationship Vocabulary
3. National Yield Relationship Map (Category-by-Category Analysis)
4. Grouped Relationship Typologies (Direct, Enabling, Conditional, External, Governance)
5. Circular Dependency & Temporal Decoupling Analysis
6. Deferred Relationships & External System Boundaries
7. Explicit Non-Relationships
8. Future Numerical Calibration Boundaries

---

### 1. Category Classifications

Republic evaluates sovereign capability across exactly **14 canonical categories**.
Food is an emergent output of the wider civilian economy and is **strictly excluded** from National Yield.

The categories are not interchangeable numerical points. Each occupies distinct functional roles within the state:

| # | Canonical Category | Primary Role | Secondary Roles | Functional Nature |
|---|--------------------|--------------|-----------------|-------------------|
| 1 | **REPU Treasury Revenue** | Flow | Strategic capability | Dynamic periodic monetary income generated over elapsed simulation time. Not an accumulated stock; feeds sovereign liquid reserves. |
| 2 | **Industrial Capacity** | Capacity | Output | Operational physical fabrication, heavy manufacturing, construction execution, and material processing capability. |
| 3 | **Energy Capacity** | Capacity | Enabling capability | Sovereign power generation, fuel refinement, electrical grid throughput, and distribution capability. |
| 4 | **Infrastructure Capacity** | Capacity | Enabling capability | Physical transport logistics (ports, rail, highways) and digital telecommunication connectivity. |
| 5 | **Financial Capacity** | Capacity | Enabling capability | Domestic commercial banking capitalization, credit availability, sovereign liquidity, and capital market depth. |
| 6 | **Human Capital** | Capacity | Enabling capability | Population educational attainment, workforce technical skills, public health resilience, and demographic productivity. |
| 7 | **Science / Research Capacity** | Capacity | Enabling capability | Academic research infrastructure, laboratory capabilities, technological exploration, and foundational discovery. |
| 8 | **Innovation Capacity** | Capacity | Strategic capability | Commercial technology adoption, entrepreneurial agility, industrial process modernization, and patent application. |
| 9 | **Administrative Capacity** | Governance capability | Enabling capability | Civil service meritocracy, institutional competence, bureaucratic integrity, and public service execution. |
| 10 | **Security Capacity** | Governance capability | Strategic capability | Internal law enforcement, domestic civil order, critical infrastructure protection, and judicial stability. |
| 11 | **Intelligence Capacity** | Strategic capability | Governance capability | Early warning detection, cyber security, counter-espionage, and strategic information gathering. |
| 12 | **Military Readiness** | Strategic capability | Capacity | Armed forces operational preparedness, combat doctrine execution, mobilization logistics, and deterrence posture. |
| 13 | **Diplomatic Capacity** | Strategic capability | Enabling capability | International bilateral leverage, treaty negotiation bandwidth, foreign mission reach, and soft power. |
| 14 | **Natural Resource Output** | Output | Capacity | Primary extractive yield of mineral, hydrocarbon, timber, and raw material endowments. |

#### Conceptual Role Definitions
- **Capacity**: An accumulated or persistent operational capability of the country to perform, sustain, or execute actions over time.
- **Flow**: A periodic generation rate evaluated across simulation time intervals (specifically REPU Treasury Revenue).
- **Output**: Measurable, tangible material volume extracted or produced by the country's physical and industrial systems.
- **Enabling Capability**: A foundational capability that unblocks bottlenecks or provides essential physical/institutional throughput for other sectors.
- **Governance Capability**: An institutional capability that governs how effectively sovereign decisions, capital, and endowments are converted into realized national outcomes.
- **Strategic Capability**: A sovereign capability oriented toward national defense, external projection, geopolitical deterrence, or sovereign risk management.

---

### 2. Relationship Vocabulary

To maintain a rigorous, implementation-ready architectural model without mathematical ambiguity, all category interactions must adhere to a controlled relationship vocabulary:

```
┌────────────────────────────────────────────────────────────────────────┐
│                   CONTROLLED RELATIONSHIP VOCABULARY                   │
├────────────────────────────────────────────────────────────────────────┤
│ 1. DIRECT                                                              │
│    An upstream category forms the immediate core input, substrate, or  │
│    primary driver of a downstream category.                            │
│    Example: Science Capacity → Innovation Capacity                     │
├────────────────────────────────────────────────────────────────────────┤
│ 2. ENABLING                                                            │
│    An upstream capability provides the operational throughput,         │
│    logistical network, or physical foundation that unblocks and        │
│    empowers another capability to function.                            │
│    Example: Infrastructure Capacity → Industrial Capacity              │
├────────────────────────────────────────────────────────────────────────┤
│ 3. GOVERNANCE                                                          │
│    An institutional capability modulates conversion efficiency,        │
│    execution integrity, compliance, and reduction of bureaucratic      │
│    leakage across other national systems.                              │
│    Example: Administrative Capacity → State Capacity Conversion        │
├────────────────────────────────────────────────────────────────────────┤
│ 4. CONDITIONAL                                                         │
│    The relationship only activates or becomes meaningful when a        │
│    specific national condition, prerequisite endowment, or policy      │
│    stance exists.                                                      │
│    Example: Natural Resource Output → Industrial Capacity              │
│    (Requires domestic refining facilities and transport corridors)     │
├────────────────────────────────────────────────────────────────────────┤
│ 5. EXTERNAL                                                            │
│    The capability primarily projects influence, arbitrates relations,  │
│    or manages threats outside the sovereign border.                    │
│    Example: Diplomatic Capacity → International Accords & Trade Access │
└────────────────────────────────────────────────────────────────────────┘
```

---

### 3. National Yield Relationship Map

The following map defines the conceptual relationships across all 14 canonical categories.
In accordance with the **sparse causal network** principle, relationships are documented only where strong sovereign and systemic rationale exists.

#### 3.1. REPU Treasury Revenue (Flow)
- **Role**: Sovereign Monetary Flow.
- **Upstream Influences**:
  - *Industrial Capacity* (`DIRECT`, Foundational): Formal industrial production generates the primary domestic corporate tax base and taxable commercial turnover.
  - *Natural Resource Output* (`CONDITIONAL`, Foundational): Extraction yields state royalties, export severance taxes, and sovereign resource dividends (conditioned on state ownership or royalty regimes).
  - *Financial Capacity* (`ENABLING`, Secondary): Deep domestic financial markets enable efficient monetary velocity, transaction settlement, and financial service taxation.
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Modern tax administration, customs enforcement, and anti-corruption integrity prevent revenue leakage.
- **Downstream Effects**:
  - *Sovereign Treasury Balance* (`EXTERNAL_SYSTEM`, Foundational): Revenue flow accumulates into liquid sovereign money (`R`) across simulation cycles.
- **Non-Relationships**: Does *not* directly generate physical industrial goods, research breakthroughs, or diplomatic treaties without explicit executive spending decisions.

#### 3.2. Industrial Capacity (Capacity / Output)
- **Role**: Heavy Physical Fabrication & Manufacturing.
- **Upstream Influences**:
  - *Energy Capacity* (`ENABLING`, Foundational): Heavy fabrication and production lines require continuous, uninterrupted base-load power.
  - *Infrastructure Capacity* (`ENABLING`, Foundational): Logistics corridors, heavy rail, and container ports transport raw inputs and finished manufactured goods.
  - *Human Capital* (`ENABLING`, Secondary): Skilled technicians, industrial engineers, and factory operators are required for complex fabrication.
  - *Natural Resource Output* (`CONDITIONAL`, Secondary): Domestic metal ores, chemicals, and fuels supply raw industrial inputs (conditioned on domestic processing facilities).
  - *Innovation Capacity* (`DIRECT`, Secondary): Advanced manufacturing techniques, automation, and tooling upgrade plant productivity.
- **Downstream Effects**:
  - *REPU Treasury Revenue* (`DIRECT`, Foundational): Supplies the core domestic industrial tax base.
  - *Infrastructure Capacity* (`ENABLING`, Secondary): Produces steel, cement, machinery, and equipment required to construct and repair infrastructure.
  - *Military Readiness* (`ENABLING`, Secondary): Fabricates military hardware, munitions, armored hulls, and spare parts.
- **Non-Relationships**: Does *not* directly produce scientific discoveries, intelligence dossiers, or diplomatic influence.

#### 3.3. Energy Capacity (Capacity / Enabling)
- **Role**: Base-load Generation, Refining, and Grid Distribution.
- **Upstream Influences**:
  - *Infrastructure Capacity* (`ENABLING`, Foundational): Pipelines, high-voltage transmission lines, substations, and transport fuel links distribute generated power.
  - *Natural Resource Output* (`CONDITIONAL`, Foundational): Domestic coal, hydrocarbons, natural gas, and uranium provide primary generation fuel (conditioned on thermal/fossil grid mix).
  - *Innovation Capacity* (`DIRECT`, Secondary): Grid modernization, renewable capture, and generation efficiency upgrades.
- **Downstream Effects**:
  - *Industrial Capacity* (`ENABLING`, Foundational): Provides the primary power input for factories and foundries.
  - *Human Capital* (`ENABLING`, Secondary): Electrification powers hospitals, schools, and civic sanitation.
  - *Infrastructure Capacity* (`ENABLING`, Secondary): Powers electrified rail corridors, port gantry cranes, and telecommunications towers.
- **Non-Relationships**: Does *not* directly generate tax revenue or diplomatic prestige without intermediate utilization.

#### 3.4. Infrastructure Capacity (Capacity / Enabling)
- **Role**: Logistics Networks, Transport Arteries, and Telecommunications.
- **Upstream Influences**:
  - *Industrial Capacity* (`ENABLING`, Foundational): Supplies structural materials, heavy earthmoving equipment, and construction components.
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Public works planning, procurement integrity, and maintenance scheduling prevent asset degradation.
  - *Financial Capacity* (`ENABLING`, Secondary): Underwrites capital-intensive public infrastructure bonds and private concessions.
- **Downstream Effects**:
  - *Industrial Capacity* (`ENABLING`, Foundational): Connects industrial zones with supply chains and export hubs.
  - *Energy Capacity* (`ENABLING`, Foundational): Enables transmission and fuel distribution corridors.
  - *Natural Resource Output* (`ENABLING`, Foundational): Connects remote mines and wells with processing plants and maritime export terminals.
  - *Security Capacity* (`ENABLING`, Secondary): Provides rapid road and rail mobility for national law enforcement and territorial units.
- **Non-Relationships**: Infrastructure does *not* automatically create money in the treasury without economic activity utilizing the assets.

#### 3.5. Financial Capacity (Capacity / Enabling)
- **Role**: Banking Capitalization, Credit Depth, and Sovereign Liquidity.
- **Upstream Influences**:
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Regulatory oversight, contract enforcement, and institutional rule of law protect depositor confidence.
  - *Security Capacity* (`ENABLING`, Secondary): Domestic peace and property protection safeguard financial assets from civil destruction.
  - *Diplomatic Capacity* (`EXTERNAL`, Secondary): Bilateral banking accords, international payment network access, and sovereign credit relations.
- **Downstream Effects**:
  - *REPU Treasury Revenue* (`ENABLING`, Secondary): Lubricates transaction velocity, bond auctions, and commercial capital formation.
  - *Innovation Capacity* (`ENABLING`, Foundational): Provides venture financing, commercial loans, and working capital for technology startups.
  - *Infrastructure Capacity* (`ENABLING`, Secondary): Facilitates project financing for national infrastructure initiatives.
- **Non-Relationships**: Financial capacity is not liquid treasury cash; it is the structural strength of the banking and capital system.

#### 3.6. Human Capital (Capacity / Enabling)
- **Role**: Workforce Capability, Public Health, and Demographic Skill.
- **Upstream Influences**:
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Effective delivery of public education, public health campaigns, and medical regulation.
  - *Infrastructure Capacity* (`ENABLING`, Secondary): Physical schools, universities, hospitals, clean water networks, and civic transport.
  - *Energy Capacity* (`ENABLING`, Secondary): Power for healthcare facilities, diagnostic equipment, and cold-chain vaccine storage.
- **Downstream Effects**:
  - *Science / Research Capacity* (`DIRECT`, Foundational): Supplies qualified researchers, mathematicians, and laboratory scientists.
  - *Administrative Capacity* (`DIRECT`, Foundational): Supplies educated, meritocratic civil servants and judicial officers.
  - *Industrial Capacity* (`ENABLING`, Secondary): Supplies skilled industrial labor and plant management.
  - *Military Readiness* (`ENABLING`, Secondary): Supplies trainable, educated recruits capable of operating modern military platforms.
- **Non-Relationships**: Does *not* directly yield natural resources or diplomatic agreements.

#### 3.7. Science / Research Capacity (Capacity / Enabling)
- **Role**: Foundational R&D, Academia, and Scientific Discovery.
- **Upstream Influences**:
  - *Human Capital* (`DIRECT`, Foundational): Advanced scientific discovery strictly requires specialized academic and technical intellect.
  - *Administrative Capacity* (`GOVERNANCE`, Secondary): Public research grant management, intellectual property law, and academic freedom protections.
- **Downstream Effects**:
  - *Innovation Capacity* (`DIRECT`, Foundational): Scientific breakthroughs feed commercial and industrial technology pipelines.
  - *Security Capacity* (`ENABLING`, Secondary): Advances forensic science, signals processing, and digital infrastructure resilience.
  - *Military Readiness* (`ENABLING`, Secondary): Supplies advanced ballistics, materials science, and defense electronics research.
- **Non-Relationships**: Does *not* directly generate REPU revenue; science creates capability that later translates into economic value through commercialization.

#### 3.8. Innovation Capacity (Capacity / Strategic)
- **Role**: Commercial Technology Adoption and Process Modernization.
- **Upstream Influences**:
  - *Science / Research Capacity* (`DIRECT`, Foundational): Draws directly from foundational discoveries and patent repositories.
  - *Financial Capacity* (`ENABLING`, Foundational): Commercialization requires risk capital, venture investment, and debt facilities.
  - *Human Capital* (`ENABLING`, Secondary): Requires entrepreneurial engineers, product designers, and adaptive managers.
- **Downstream Effects**:
  - *Industrial Capacity* (`DIRECT`, Secondary): Enhances manufacturing productivity, robotics adoption, and plant efficiency.
  - *Energy Capacity* (`DIRECT`, Secondary): Drives efficiency in energy generation, grid distribution, and storage.
  - *Intelligence Capacity* (`DIRECT`, Secondary): Provides advanced cyber tools, encryption, and data analytics systems.
- **Non-Relationships**: Innovation does not replace basic infrastructure or raw materials.

#### 3.9. Administrative Capacity (Governance / Enabling)
- **Role**: Institutional Conversion, Bureaucratic Integrity, and Civil Execution.
- **Upstream Influences**:
  - *Human Capital* (`DIRECT`, Foundational): Requires educated civil servants, judicial experts, and public policy analysts.
  - *Security Capacity* (`ENABLING`, Secondary): Civil administration requires freedom from institutional coercion, extortion, and violence.
- **Downstream Effects**:
  - *State Capacity Conversion* (`GOVERNANCE`, Foundational): Serves as the primary operational engine converting national potential into realized yields across all categories.
  - *REPU Treasury Revenue* (`GOVERNANCE`, Foundational): Enforces tax compliance and minimizes customs/fiscal leakage.
  - *Infrastructure Capacity* (`GOVERNANCE`, Foundational): Ensures public asset maintenance schedules are executed without bureaucratic paralysis.
  - *Human Capital* (`GOVERNANCE`, Foundational): Directs effective public health and educational curricula delivery.
- **Non-Relationships**: Administrative capacity does not directly produce physical manufactured goods or military victories.

#### 3.10. Security Capacity (Governance / Strategic)
- **Role**: Domestic Civil Order, Law Enforcement, and Rule of Law.
- **Upstream Influences**:
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Judicial integrity, police oversight, and prosecution management.
  - *Infrastructure Capacity* (`ENABLING`, Secondary): Transport networks and telecommunications links enabling rapid response.
  - *Science / Research Capacity* (`ENABLING`, Secondary): Forensic methodologies, communications encryption, and biometric records.
- **Downstream Effects**:
  - *Financial Capacity* (`ENABLING`, Secondary): Protects commercial contracts, banking institutions, and property rights.
  - *Administrative Capacity* (`ENABLING`, Secondary): Secures government buildings and prevents criminal infiltration of state offices.
  - *Industrial Capacity* (`ENABLING`, Secondary): Protects critical factories, power stations, and transport nodes from sabotage.
- **Non-Relationships**: Security capacity does not directly conduct foreign diplomacy or generate export commodities.

#### 3.11. Intelligence Capacity (Strategic / Governance)
- **Role**: Threat Early Warning, Counter-Espionage, and Strategic Analysis.
- **Upstream Influences**:
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Counter-intelligence vetting, operational secrecy, and oversight.
  - *Innovation Capacity* (`DIRECT`, Secondary): Signals interception hardware, cyber intrusion capabilities, and cryptanalysis.
  - *Diplomatic Capacity* (`EXTERNAL`, Secondary): Foreign station cover, embassy defense attaches, and liaison exchanges.
- **Downstream Effects**:
  - *Security Capacity* (`ENABLING`, Foundational): Provides preemptive detection of domestic insurgencies, terror networks, and organized crime.
  - *Military Readiness* (`ENABLING`, Foundational): Feeds tactical target packages, enemy force disposition, and strategic early warning.
  - *Diplomatic Capacity* (`ENABLING`, Secondary): Informs bilateral negotiations with secret assessments of foreign leadership intentions.
- **Non-Relationships**: Intelligence is *not* a generic economic multiplier and does not boost agricultural or industrial output.

#### 3.12. Military Readiness (Strategic / Capacity)
- **Role**: Armed Forces Operational Posture, Combat Defense, and Deterrence.
- **Upstream Influences**:
  - *Industrial Capacity* (`ENABLING`, Foundational): Heavy vehicle manufacturing, arms fabrication, and ordnance replenishment.
  - *Human Capital* (`ENABLING`, Foundational): Officer education, soldier technical proficiency, and physical resilience.
  - *Intelligence Capacity* (`ENABLING`, Foundational): Battlefield situational awareness and strategic warning.
  - *Energy Capacity* (`ENABLING`, Secondary): Operational fuel stocks, naval bunker fuel, and aviation kerosene.
- **Downstream Effects**:
  - *Geopolitical Deterrence* (`EXTERNAL`, Foundational): Prevents foreign territorial encroachment and coercive diplomacy.
  - *Diplomatic Capacity* (`ENABLING`, Secondary): Provides hard-power backing to bilateral diplomatic negotiations.
- **Non-Relationships**: Military readiness does *not* automatically generate REPU revenue or commercial innovation; it consumes resources to protect the nation.

#### 3.13. Diplomatic Capacity (Strategic / Enabling)
- **Role**: Bilateral Leverage, Treaty Negotiation, and Foreign Representation.
- **Upstream Influences**:
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Professional diplomatic corps, treaty drafting competence, and international legal standing.
  - *Military Readiness* (`ENABLING`, Secondary): Sovereign defensive strength grants negotiating leverage in geopolitical disputes.
  - *Financial Capacity* (`EXTERNAL`, Secondary): Sovereign credit rating and financial standing enhance bilateral credibility.
- **Downstream Effects**:
  - *International Accords & Treaties* (`EXTERNAL`, Foundational): Unlocks bilateral trade agreements, preferential customs zones, and defense pacts.
  - *Financial Capacity* (`EXTERNAL`, Secondary): Facilitates cross-border banking access, currency swap lines, and sovereign borrowing.
  - *Intelligence Capacity* (`EXTERNAL`, Secondary): Provides embassy platforms and diplomatic cover for foreign reporting.
- **Non-Relationships**: Diplomatic capacity does not directly construct domestic roads, harvest resources, or power factory lines.

#### 3.14. Natural Resource Output (Output / Capacity)
- **Role**: Primary Extractive Yield (Ores, Fuels, Minerals, Raw Timber).
- **Upstream Influences**:
  - *Infrastructure Capacity* (`ENABLING`, Foundational): Bulk rail, slurry pipelines, and deepwater export berths connect extraction sites.
  - *Energy Capacity* (`ENABLING`, Foundational): Power for mining drags, smelters, pumps, and refining machinery.
  - *Administrative Capacity* (`GOVERNANCE`, Foundational): Concession leasing, environmental monitoring, and anti-smuggling enforcement.
  - *Geological Endowment* (`EXTERNAL_SYSTEM`, Foundational): The country's physical territorial deposits (governed by the external World/Geography subsystem).
- **Downstream Effects**:
  - *REPU Treasury Revenue* (`CONDITIONAL`, Foundational): Yields severance taxes, export tariffs, and state mining royalties.
  - *Energy Capacity* (`CONDITIONAL`, Foundational): Supplies coal, gas, and uranium to domestic power stations.
  - *Industrial Capacity* (`CONDITIONAL`, Secondary): Supplies raw iron ore, bauxite, and copper to domestic smelters and fabrication plants.
- **Non-Relationships**: Does *not* generate scientific discovery or administrative merit; raw resources without processing remain basic bulk exports.

---

### 4. Direct Relationships

Direct relationships represent instances where an upstream category directly forms the substrate or primary driver of a downstream category:
- **Science / Research Capacity $\rightarrow$ Innovation Capacity**: Foundational scientific research forms the direct basis for patentable technological innovations and processes.
- **Human Capital $\rightarrow$ Science / Research Capacity**: Educational attainment and technical skill directly populate research laboratories and universities.
- **Human Capital $\rightarrow$ Administrative Capacity**: The civil service directly draws its personnel and organizational competence from national human capital.
- **Innovation Capacity $\rightarrow$ Industrial Capacity**: Advanced industrial tooling and process innovations directly enhance factory productivity.
- **Innovation Capacity $\rightarrow$ Energy Capacity**: Modern grid control, renewable technologies, and generation efficiency directly upgrade energy capacity.
- **Innovation Capacity $\rightarrow$ Intelligence Capacity**: Cutting-edge software, encryption algorithms, and signals tech directly upgrade cyber intelligence capabilities.
- **Industrial Capacity $\rightarrow$ REPU Treasury Revenue**: Industrial enterprise turnover and corporate activity directly feed the domestic sovereign tax base.

---

### 5. Enabling Relationships

Enabling relationships represent instances where an upstream capability provides physical throughput, logistics, or an operating baseline that unblocks another sector:
- **Energy Capacity $\rightarrow$ Industrial Capacity**: Uninterrupted electrical base-load is an indispensable prerequisite for operating heavy machinery and manufacturing lines.
- **Infrastructure Capacity $\rightarrow$ Industrial Capacity**: Transport links (freight rail, ports, highways) unblock supply chains and finished goods movement.
- **Infrastructure Capacity $\rightarrow$ Energy Capacity**: Transmission grids, fuel pipelines, and access corridors are necessary to distribute energy from generation plants.
- **Infrastructure Capacity $\rightarrow$ Natural Resource Output**: Heavy freight transport is essential to convey bulk raw materials from remote mines/wells to refineries and ports.
- **Industrial Capacity $\rightarrow$ Infrastructure Capacity**: Heavy industry provides the physical steel, concrete, and machinery needed to build and repair transport networks.
- **Industrial Capacity $\rightarrow$ Military Readiness**: Domestic manufacturing fabricates fighting vehicles, munitions, armor, and spare parts.
- **Financial Capacity $\rightarrow$ Innovation Capacity**: Venture funding, business loans, and liquid capital allow innovative enterprises to scale.
- **Financial Capacity $\rightarrow$ REPU Treasury Revenue**: Efficient commercial banking facilitates transaction velocity and financial services tax collection.
- **Intelligence Capacity $\rightarrow$ Security Capacity**: Threat detection alerts domestic law enforcement to organized crime, terror, and subversion.
- **Intelligence Capacity $\rightarrow$ Military Readiness**: Early warning and operational intelligence allow armed forces to position forces effectively.
- **Military Readiness $\rightarrow$ Diplomatic Capacity**: Credible armed deterrence provides geopolitical backing to sovereign ambassadors.

---

### 6. Conditional Relationships

Conditional relationships only activate when explicit institutional, technological, or geographic conditions exist:
- **Natural Resource Output $\rightarrow$ Energy Capacity**: Fossil and fissile resource extraction enables domestic energy generation *only if* the country possesses matching thermal/nuclear power plants.
- **Natural Resource Output $\rightarrow$ Industrial Capacity**: Mineral and chemical extraction enables domestic manufacturing *only if* domestic smelting and processing plants exist.
- **Natural Resource Output $\rightarrow$ REPU Treasury Revenue**: Resource extraction converts into sovereign treasury revenue *only under* active state royalties, export severance taxes, or state-owned resource enterprise regimes.

---

### 7. External Relationships

External relationships govern interactions projecting across sovereign borders or into the international sphere:
- **Diplomatic Capacity $\rightarrow$ International Treaties & Trade Accords**: Diplomatic missions negotiate bilateral agreements, trade concessions, and defense alliances.
- **Diplomatic Capacity $\rightarrow$ Financial Capacity**: Bilateral accords unlock foreign capital market access and international payment networks.
- **Military Readiness $\rightarrow$ Geopolitical Deterrence**: Defense posture deters foreign aggression, cross-border coercion, and piracy.
- **Diplomatic Capacity $\rightarrow$ Intelligence Capacity**: Foreign embassies provide legal cover and platforms for overseas intelligence reporting.

---

### 8. Governance Relationships

Governance relationships define how institutional integrity and State Capacity modulate the realization of national potential:
- **Administrative Capacity $\rightarrow$ State Capacity Conversion**: Bureaucratic effectiveness governs the efficiency with which national inputs are converted into realized capacity across the entire six-stage pipeline.
- **Administrative Capacity $\rightarrow$ REPU Treasury Revenue**: Competent, non-corrupt tax administration maximizes revenue collection and minimizes fiscal leakage.
- **Administrative Capacity $\rightarrow$ Infrastructure Maintenance**: Professional public works administration ensures existing infrastructure is systematically maintained rather than deteriorating.
- **Administrative Capacity $\rightarrow$ Security Oversight**: Judicial integrity and civilian oversight ensure law enforcement acts effectively within the rule of law.
- **Security Capacity $\rightarrow$ Domestic Operating Environment**: Internal peace protects financial assets, civil offices, and industrial plants from destruction or extortion.

---

## Circular Dependency Analysis

### 1. Systemic Feedback Loops in Real Nations
Real nations exhibit cyclical feedback over extended historical timelines:
1. **Infrastructure $\leftrightarrow$ Industry**: Industry fabricates steel/concrete $\rightarrow$ builds transport infrastructure $\rightarrow$ lowers logistics friction $\rightarrow$ expands industry.
2. **Science $\leftrightarrow$ Human Capital**: High Human Capital educates scientists $\rightarrow$ scientific discoveries elevate educational and medical standards $\rightarrow$ elevates Human Capital.
3. **Administration $\leftrightarrow$ Security**: Administrative meritocracy disciplines police $\rightarrow$ strong rule of law protects courts and civil servants $\rightarrow$ strengthens administration.
4. **Energy $\leftrightarrow$ Infrastructure**: Infrastructure distributes power $\rightarrow$ energy powers rail systems and ports $\rightarrow$ improves logistics reliability.

### 2. Acyclic Single-Tick DAG Guarantee
To maintain pure determinism inside [`INationalYieldCalculator`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/INationalYieldCalculator.cs):

> [!IMPORTANT]
> **Single-Tick Evaluation Rule**:
> In any single evaluation tick $t$, the National Yield calculation engine must execute as a **strictly feed-forward Directed Acyclic Graph (DAG)**.
> **No instantaneous circular calculation is permitted within a single tick.**

### 3. Multi-Tick Temporal Decoupling
Systemic feedback loops must be resolved **temporally across elapsed simulation time**:
- In tick $t$, [`NationalYieldCalculationInputs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculationInputs.cs) represents the nation's state at the start of the cycle.
- The engine calculates capacities for tick $t$ feed-forward.
- Downstream effects (such as spending REPU revenue on infrastructure construction, or applying industrial capacity to public works) update the country's persistent state for tick $t+1$ or across multi-month project durations.
- This guarantees calculations are non-iterative, exact, and complete in a single deterministic pass.

---

## Deferred Relationships & External System Boundaries

National Yield measures **what the country is capable of producing or sustaining**. It does **not** simulate the entire sovereign society within a single calculation component.

The following systems are **strictly external** to the National Yield calculation engine and must remain decoupled:

```
┌────────────────────────────────────────────────────────────────────────┐
│                   EXTERNAL SUBSYSTEM BOUNDARIES                        │
├────────────────────────────────────────────────────────────────────────┤
│ 1. Fiscal, Tax & Budget Subsystem                                      │
│    - Determines tax rates, corporate tax policy, sovereign debt,       │
│      treasury bond yields, and deficit spending.                       │
├────────────────────────────────────────────────────────────────────────┤
│ 2. Sovereign Treasury Stock                                            │
│    - Tracks accumulated liquid balance in REPU (R). Evaluated when     │
│      periodic revenue flows are deposited across yield cycles.         │
├────────────────────────────────────────────────────────────────────────┤
│ 3. Macroeconomic Civilian Market                                       │
│    - Models civilian consumer demand, private firms, GDP accounting,   │
│      inflation rates, interest rates, and foreign exchange.           │
├────────────────────────────────────────────────────────────────────────┤
│ 4. Labor, Demographics & Population                                    │
│    - Tracks census headcounts, birth rates, urban migration,           │
│      unemployment percentages, and wage rates.                         │
├────────────────────────────────────────────────────────────────────────┤
│ 5. Agricultural & Food Production System                               │
│    - Food is an emergent civilian output of farmers, fertile land,     │
│      rainfall, logistics, and fertilizers. NOT a yield category.       │
├────────────────────────────────────────────────────────────────────────┤
│ 6. Territorial Resource Geology & Exploration                          │
│    - Maps physical subsurface ore bodies, proven oil reserves,         │
│      depletion rates, and prospecting concessions.                     │
├────────────────────────────────────────────────────────────────────────┤
│ 7. Construction & Capital Development Projects                         │
│    - Manages multi-month construction timelines, concrete pouring,     │
│      capital expenditure (CAPEX), and maintenance backlogs.            │
├────────────────────────────────────────────────────────────────────────┤
│ 8. Cabinet Appointments & Politics                                     │
│    - Handles ministerial appointments, competence vs. loyalty trade-   │
│      offs, parliamentary approval, elections, and public unrest.       │
├────────────────────────────────────────────────────────────────────────┤
│ 9. Military Operations & Theater Command                               │
│    - Simulates troop deployments, tactical engagements, air combat,    │
│      rules of engagement, and DEFCON alert states.                     │
├────────────────────────────────────────────────────────────────────────┤
│ 10. Bilateral Diplomacy & International Treaties                       │
│     - Manages diplomatic summits, bilateral trade pacts, embargoes,    │
│       customs unions, and military alliances.                          │
└────────────────────────────────────────────────────────────────────────┘
```

---

## Explicit Non-Relationships

To prevent common nation-building simulation design anti-patterns, the following relationships are explicitly declared **invalid**:

1. **Military Readiness $\rightarrow$ REPU Treasury Revenue**:
   - A large, mobilized military does not generate sovereign tax money; maintaining readiness consumes national wealth.
2. **Food as a Sovereign Yield**:
   - Food is never minted or stored in national yield vaults; the state facilitates food security through infrastructure, peace, and water rights, but citizens and agriculture produce food.
3. **Diplomatic Capacity $\rightarrow$ Factory Output**:
   - Diplomatic prestige does not fabricate steel beams or assemble heavy equipment.
4. **Intelligence Capacity $\rightarrow$ Economic / Industrial Yield**:
   - Intelligence agencies provide operational threat warning and counter-espionage; they do not operate commercial supply chains or boost factory productivity.
5. **Science Capacity $\rightarrow$ Direct Treasury Money**:
   - Basic scientific research does not deposit cash directly into government bank accounts; it upgrades knowledge and technology, which commercial enterprise subsequently monetizes.
6. **National Yield as an All-to-One Aggregate Score**:
   - National Yield must never be flattened into a single composite "Power Score" or "GDP Number" that masks underlying sector weaknesses.

---

## Future Numerical Calibration Boundary

This specification establishes **what** relationships exist and **why** they exist.
The following elements remain **strictly deferred** to subsequent calibration steps:

1. **Numerical Elasticities & Weights**:
   - The degree to which an upstream category influences a downstream category (e.g. the specific mathematical curve connecting Energy Capacity to Industrial Capacity).
2. **State Capacity Mathematical Conversion**:
   - The formal algebraic formulation converting governance inputs (effectiveness, stability, cabinet caliber) into realization factors.
3. **Diminishing Returns & Saturation Curves**:
   - Non-linear mathematical curves (e.g. logarithmic scaling, logistic S-curves) preventing infinite exponential runaway.
4. **Modifier Composition Mathematics**:
   - Formal precedence, stacking hierarchies, and multiplicative vs. additive rules for complex modifiers.
5. **Yield Cycle Temporal Step Size**:
   - The exact simulation clock period governing periodic yield resolution.

---

## Consequences

- **Clear Implementation Blueprint**: Future engine enhancements for Stage 3 (Cross-Category Upstream Relationships) and Stage 4 (State Capacity Conversion) have an unambiguous, approved conceptual specification.
- **Protection Against Feature Creep**: Establishes strict architectural walls separating National Yield from the wider macroeconomic, demographic, military, and diplomatic subsystems.
- **Zero Invalidation of Step 4D**: The pure calculation engine implemented in Step 4D remains completely valid and operational under its minimal neutral baseline.
- **Readiness for Numerical Calibration**: When Republic enters numerical balancing in subsequent steps, developers can calibrate individual sparse edges with clear intent rather than tweaking an opaque, interconnected formula web.
