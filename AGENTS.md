# ECHO//SHIFT Codex Development Rules

## Project

ECHO//SHIFT is a Unity 6 URP game built around recording and replaying the player's previous actions as physical Echo actors.

Phase 3 is formally validated. After the first Phase 4 primitive-led presentation failed human Visual Review, Phase 4.1 integrated a curated CC0 subset of Quaternius modular sci-fi/robot assets and Kenney audio without changing P0-P3 mechanics, Stable IDs, or colliders. **Phase 4.1: Automation Passed; Human Visual Review Pending.** Do not call Phase 4 validated until `Docs/Phase4VisualAcceptance.md` is completed against the Phase 4.1 Build and any Critical/High visual finding is fixed and revalidated.

## Required workflow

Before changing files:

1. Read this file.
2. Inspect the existing repository and Unity project.
3. Read every Markdown document in the repository. Documents already read do not need to be reread unless changed.
4. Write or update the implementation plan for the active milestone.
5. Proceed with implementation without waiting for approval.

After changing files:

1. Check for compilation errors.
2. Run available EditMode and PlayMode tests.
3. Review the diff.
4. Report changed files, tests, manual verification steps, and unresolved risks.

Do not commit, tag, push, create branches, or rewrite Git history unless the active user request explicitly authorizes the specific operation. Never push or rewrite history for Phase 4. Do not create `phase4-validated` until every required visual acceptance item is recorded and accepted.

## Architecture rules

- Use namespaces beginning with `EchoShift`.
- Keep runtime, editor, and test code in separate assembly definitions.
- Keep gameplay logic outside Editor assemblies.
- Prefer plain C# classes for data and calculations.
- Use MonoBehaviours only as Unity-facing adapters or scene components.
- Do not place unrelated responsibilities in one manager class.
- Do not introduce a service locator, dependency-injection framework, ECS, DOTS, Addressables, UniTask, or third-party framework.
- Avoid global mutable singletons. A scene-owned coordinator is acceptable when its lifetime and ownership are explicit.
- Serialized fields must normally be private and use `[SerializeField]`.
- Validate required references in `Awake` or `OnValidate`.
- Do not use `FindObjectOfType`, `GameObject.Find`, tags, or repeated hierarchy searches during gameplay.
- Do not use LINQ, reflection, string formatting, or avoidable allocations inside per-frame or per-tick paths.
- Preallocate replay storage for the configured loop length.
- Do not silently catch exceptions.
- Do not suppress compiler warnings without a documented reason.

## Simulation rules

- Gameplay simulation must run through an explicit fixed-tick clock.
- The Phase 0 tick rate is 60 ticks per second.
- Input collection must be separated from movement simulation.
- Player and Echo movement must use the same movement implementation.
- Replay data must be immutable after a loop has been finalized.
- Each replay frame must retain the command and an expected pose for drift validation.
- Existing Echoes restart from tick zero whenever a new loop begins.
- World reset must be explicit and coordinated. Do not reload the scene to implement a normal loop reset.
- Player and Echo actors must not physically block each other.
- They must still be detectable by trigger-based gameplay objects such as pressure plates.
- Successful recorded interactions must resolve the exact serialized Stable ID and must never substitute a nearby target.
- Interaction target IDs are authored and repaired only by Editor tooling, never generated at runtime.
- Carryable ownership and reset cleanup must not rely on trigger-exit or hierarchy-destruction order.

## Testing rules

At minimum, maintain tests for:

- Replay frame ordering.
- Replay recording immutability after finalization.
- Replay capacity based on loop settings.
- Reset restoration.
- Loop transition behavior.
- Echo playback reaching the expected final pose within tolerance.

Tests must not depend on Asset Store content.

## Scope restrictions after Phase 4 automation

Do not implement enemy AI, combat, inventory, dialogue, save data, Steamworks, achievements, online features, procedural generation, arbitrary timeline scrubbing, full physics rewinding, final character/environment art, production BGM, or advanced animation. The bounded Phase 4 VFX/audio/HUD flow is allowed; do not expand it into a production front end or content pipeline without a later plan.

Do not expand the validated Phase 1 interaction model into a general inventory, arbitrary physics rewind, cross-scene persistence, or production content system without a later milestone plan and ADR. Curated third-party originals stay isolated under `Assets/_Project/ThirdParty`; project-owned wrappers/materials stay under `Assets/_Project/Art`. Gameplay roots, Stable IDs, colliders, and replay behavior must not be delegated to imported assets.

## Documentation

Important architectural decisions must be recorded in `Docs/ADR/` with the selected design, reasons, alternatives, current limitations, and replacement conditions.

Code comments should explain non-obvious decisions, not restate the code.
