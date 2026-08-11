## graphify (Knowledge Graph Management & Usage)

This project has an active knowledge graph at `graphify-out/` representing the architecture and relationships of `Assets/Scripts/`.

The agent MUST actively maintain, consult, and update this knowledge graph throughout the project lifecycle:
- **Mandatory Usage Before Changes / Research**: Always query the graph first (`graphify query "<question>"`, `graphify path "<A>" "<B>"`, `graphify explain "<concept>"`, or inspecting `graphify-out/graph.json`) before making assumptions about architecture, dependencies, state transitions, or god nodes.
- **Mandatory Graph Maintenance & Updates**: After creating, modifying, refactoring, or deleting any scripts in `Assets/Scripts/`, ALWAYS run `graphify update Assets/Scripts` (or `graphify update .`) to keep the knowledge graph fresh for current and future agent sessions.
- **Fast Navigation**: Use `graphify-out/graph.json`, `graphify-out/GRAPH_REPORT.md`, and `graphify-out/graph.html` for understanding communities, hubs (`IronSourceManager`, `CountryInitializer`, `BuildingShopManager`, etc.), and cross-system dependencies.
- Dirty `graphify-out/` files are expected and normal after incremental updates; keep them in sync.
- When the user types `/graphify`, invoke the `skill` tool with `skill: "graphify"` before doing anything else.

## Unity MCP

This is an old Unity project. For Unity-specific inspection, scene/prefab work, asset checks, build settings, and editor-side validation, use Unity MCP when it is available.

Prefer Unity MCP over unrelated CLI tooling for Unity work. Do not assume `dotnet`, generated `.csproj` files, or other external build commands are reliable sources of truth for this project unless the user explicitly asks for them or Unity MCP is unavailable.

## AI Agent Efficiency

Use the smallest reliable context and tool set that can answer the task.

Rules:
- Start from project-local truth: `AGENTS.md`, `README.md`, `graphify-out/GRAPH_REPORT.md`, `graphify query`, Unity MCP, and exact files involved in the request.
- Prefer scoped graph queries and targeted `rg` searches before reading large files or directories.
- Read only the files needed for the next decision. Do not dump large generated files, package code, Library, Temp, obj, Builds, or vendor folders unless they are directly relevant.
- Keep plans short and operational. Use a checklist only for multi-step work; otherwise proceed directly.
- Make the smallest effective change. Avoid broad cleanup, formatting churn, renames, folder moves, or architecture rewrites unless the user explicitly asks.
- Reuse existing project patterns even if they are imperfect. This is a legacy project; consistency beats idealized refactors.
- Ask only when blocked by a risky unknown. Otherwise make a conservative assumption, state it briefly, and continue.
- Summarize tool findings instead of pasting long outputs. Include exact file paths, symbols, and line numbers when useful.
- Batch independent reads/searches in parallel when possible.
- After code edits, run the cheapest meaningful validation first: Unity MCP/editor checks when available, graphify update for graph freshness, and targeted asset/script inspection before full builds.
- Do not use internet for stable local code questions. Use internet only for current external facts, package/API docs, platform rules, or when the user asks to look something up.
- When using internet guidance, prefer primary sources: Unity docs/manuals, package vendor docs, OpenAI docs, Android/Apple docs, and official SDK changelogs.

Source guidance used for these rules:
- OpenAI prompt/context guidance: https://platform.openai.com/docs/guides/prompt-engineering
- OpenAI agents/tool guidance: https://platform.openai.com/docs/guides/agents

## Unity Development Guardrails

This project is an old mobile Unity game with unfinished and legacy systems. Treat every change as bug-fix surgery unless the user asks for redesign.

Workflow:
- Inspect the actual scene, prefab, ScriptableObject, serialized references, and runtime wiring before changing code.
- Prefer Unity MCP for scene/prefab/asset/build-setting validation. If Unity MCP is unavailable, say so and use file inspection carefully.
- Before modifying a system, identify its owner objects and entry points in the graph/report. Current high-risk hubs include `LevelManager`, `PlayingState`, `GameFeelManager`, `MenuView`, `MainMenuPanelController`, `IronSourceManager`, and IAP/ad services.
- Preserve `.meta` files and GUIDs. Move Unity assets only through Unity or with their matching `.meta` files.
- Do not trust generated `.csproj` files as project architecture. They are Unity output, not the design source.
- Avoid editing `Library`, `Temp`, `obj`, generated build folders, or imported vendor packages unless the bug is explicitly inside generated/vendor integration.
- Keep third-party SDK changes isolated. For ads/IAP/LevelPlay/GoogleMobileAds/ExternalDependencyManager, inspect installed package versions and callbacks before changing APIs.
- Prefer data/config fixes when the bug is serialized data, prefab wiring, addressable/resource reference, or ScriptableObject content.
- For gameplay bugs, trace the state flow first: input -> state/controller -> board/model/service -> animation/UI/event.
- For UI bugs, inspect canvas hierarchy, serialized references, anchoring/layout groups, animation/tween side effects, and presenter/view bindings before rewriting scripts.
- For save/progression bugs, inspect PlayerPrefs/data keys, model initialization order, migrations/default values, and scene startup order.
- For build bugs, check Unity project settings, package manifests, Android resolver output, Gradle templates, and SDK/plugin compatibility before code changes.

Code rules:
- Keep `MonoBehaviour` lifecycle methods small and predictable. Avoid hidden work in `Awake`, `OnEnable`, and `Update` unless the existing pattern requires it.
- Cache repeated component lookups on hot paths. Avoid repeated `Find`, `GetComponent`, LINQ allocations, string formatting, and new collection allocations in per-frame or per-tile loops.
- Avoid unnecessary garbage in gameplay loops. Reuse lists/objects where practical and prefer pooling for frequently created visual effects, tiles, UI popups, and projectiles.
- Do not introduce new global singletons/service locators unless that pattern already owns the touched system.
- Prefer explicit serialized fields and validated references over runtime scene searches.
- Keep async/coroutine lifetimes cancellable or tied to object lifetime. Guard against callbacks after scene unload or destroyed objects.
- Unsubscribe events/disposables on disable/destroy according to the existing pattern.
- Treat animations/tweens as stateful. Kill/complete previous tweens before starting conflicting ones when fixing UI/gameplay animation bugs.
- Keep mobile constraints in mind: memory, startup time, GC spikes, shader/material variants, draw calls, overdraw, texture sizes, and low-end Android behavior.

Validation:
- Profile before making performance claims. Unity recommends profiling and analysis tools to measure CPU, GPU, memory, rendering, scripts, and project issues.
- Test on the relevant target when possible, especially Android for ads, IAP, notifications, Gradle, permissions, and performance.
- For performance work, prefer Unity Profiler, Memory Profiler, Frame Debugger, Profile Analyzer, Project Auditor, and platform profilers over guesses.
- For memory/GC work, check allocation patterns and object churn before adding pools or caches.
- After edits, verify the changed path in Unity MCP/editor if possible and run `graphify update .` so future agents see the updated relationships.

Unity source guidance:
- Unity optimization overview: https://docs.unity3d.com/Manual/analysis.html
- Unity profiling tools: https://docs.unity3d.com/Manual/performance-profiling-tools.html
- Unity code optimization: https://docs.unity3d.com/Manual/scripting-optimization.html
- Unity managed memory optimization: https://docs.unity3d.com/Manual/performance-optimizing-code-managed-memory.html
- Unity C# style guide resource: https://unity.com/resources/create-code-c-sharp-style-guide-e-book
