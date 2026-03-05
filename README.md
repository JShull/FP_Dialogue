# FuzzPhyte Unity Tools

## Dialogue

This package provides a graph-authored dialogue workflow built on Unity Graph Toolkit (preview) and compiled into runtime graph data.

- Graph authoring happens in `Editor/Dialogue` with a custom graph asset type: `.fpdialogue`.
- Runtime execution happens in `Runtime/Dialogue` using `RTFPDialogueGraph`, `RTDialogueDirector`, `RTDialogueOrchestrator`, and event/timeline helpers.

Graph Toolkit reference (preview package docs): [Unity Graph Toolkit introduction](https://docs.unity3d.com/Packages/com.unity.graphtoolkit@0.4/manual/introduction.html)

## Current Dependency Baseline

From `package.json` (this repository):

- `com.unity.graphtoolkit`: `0.4.0-exp.2`
- `com.unity.timeline`: `1.8.9`
- `com.unity.textmeshpro`: `3.0.9`

## How The Graph Pipeline Works

- Create a graph asset from menu:
  - `Assets/Create/FuzzPhyte/Dialogue/Graph/Create Blank Dialogue Graph` or `FuzzPhyte/Dialogue/Graph/Create Blank Graph`
- Author nodes in the `.fpdialogue` graph (`FPDialogueGraph`).
  - During import, `FPDialogueImporter`:
    - loads the editor graph,
    - validates presence of Entry,
    - creates a runtime `RTFPDialogueGraph` sub-asset,
    - converts editor nodes to runtime nodes,
    - builds connection indices (`NextNodeIndices`) and node-port metadata.
- At runtime, `RTDialogueDirector` traverses the runtime graph and raises typed events through `RTGraphDialogueEventHandler`.
- `RTDialogueOrchestrator` listens to these events and performs scene/UI/timeline actions.

## Folder Responsibilities

- `Editor/Dialogue`
- Graph type (`FPDialogueGraph`) and validation (`FPDialogueGraphValidation`)
- Scripted importer (`FPDialogueImporter`) that compiles `.fpdialogue` to runtime graph data
- Node definitions under `Editor/Dialogue/Nodes`
- Exposed reference binding utility window (`FPExposedBinderWindow`)

- `Runtime/Dialogue`
- Runtime graph data structures (`RTFPDialogueGraph`, `RTFPNode` and node subclasses)
- Graph traversal/state (`RTDialogueDirector`)
- Runtime checks (`RTDialogueMediator`)
- Event hub/data (`RTGraphDialogueEventHandler`, `GraphEventData`)
- Scene/UI/timeline glue (`RTDialogueOrchestrator`)
- Timeline wrappers and exposed object resolver (`RTFPPlayableDirector`, `RTExposedBinder`, `RTTimelineDetails`)

## Editor Node Reference (Graph Toolkit Nodes)

All graph nodes derive from `FPVisualNode` and are restricted to `FPDialogueGraph` via `[UseWithGraph(typeof(FPDialogueGraph))]`.

### 1) `EntryNode`

Purpose: graph start.

Options:

- `GraphID` (string, required for clean runtime identification)

Ports:

- Output `ExecutionPort` (Flow Out)
- Input `TimelinePort` (`TimelineAsset`, optional)
- Input `TimelineDetails` (`RTTimelineDetails`, optional)

Runtime mapping: `RTEntryNode`

### 2) `ExitNode`

Purpose: graph end.

Options:

- `GameObjectID` (PlayableDirector lookup name / binding id)

Ports:

- Input `ExecutionPort` (Flow In)
- Input `TimelinePort` (`TimelineAsset`, optional)
- Input `TimelineDetails` (`RTTimelineDetails`, optional)

Runtime mapping: `RTExitNode`

### 3) `SetFPDialogueNode`

Purpose: a single dialogue beat shown to the user.

Options:

- `WaitForUserResponse` (bool)
- `AnimationEmotion` (`EmotionalState`)
- `AnimationDialogue` (`DialogueState`)
- `AnimationMotion` (`MotionState`)
- `DialogueTimelineOut` (`RTTimelineDetails`, optional timeline action after this node)
- `FPDataTag` (`FP_Data`, optional event tag)
- `UseWorldObjects` (bool)
- `UsePrefabs` (bool)
- `GameObjectLocation` (bool toggle that enables world-location input)

Ports:

- Input `ExecutionPort` (Flow In)
- Output `ExecutionPort` (Flow Out)
- Input `CharacterPort` (`SetFPCharacterNode`)
- Input `MainText` (`SetFPTalkNode`)
- Input `TranslationText` (`SetFPTalkNode`, optional)
- Optional Input `UseDialogueWorldLocation` (string location name)
- If `UseWorldObjects=true` and `UsePrefabs=true`:
- Input `DialogueUIPanel` (`GameObject` prefab)
- Input `DialogueUIButton` (`GameObject` prefab)
- If `UseWorldObjects=true` and `UsePrefabs=false`:
- Input `YesWorldLocationName` (string)
- Input `NoWorldLocationName` (string)

Runtime mapping: `RTDialogueNode`

### 4) `SetFPResponseNode`

Purpose: user choice branch node.

Options:

- `PromptNumOptions` (int, delayed; controls dynamic port count)
- `UseDialogueWorldLocation` (bool)
- `FPDataTag` (`FP_Data`, optional)

Ports:

- Input `ExecutionPort` (Flow In)
- Input `CharacterPort` (`SetFPCharacterNode`)
- Dynamic pairs from option count:
- Input `PromptOption_i` (`SetFPSinglePromptNode`)
- Output `PromptOption_i` (flow branch if user picks prompt `i`)

Runtime mapping: `RTResponseNode`

### 5) `SetFPSinglePromptNode`

Purpose: one selectable user response item.

Options:

- `GameObjectID` (binding id for prompt location/object)

Ports:

- Input `MainText` (`SetFPTalkNode`, required)
- Input `TranslationText` (`SetFPTalkNode`, optional)
- Input `PortIcon` (`Sprite`, optional)
- Output `PromptExecutionOut`

Runtime mapping: `RTSinglePromptNode`

### 6) `SetFPTalkNode`

Purpose: text/audio/animation payload for dialogue or prompt text.

Ports:

- Input `Language` (`FP_Language`)
- Input `HeaderText` (string)
- Input `Dialogue` (string)
- Input `DialogueAudio` (`AudioClip`)
- Input `AnimationFace` (`AnimationClip`)
- Input `AnimationSpeed` (float)
- Input `AnimationBody` (`AnimationClip`)
- Input `AnimationBodySpeed` (float)
- Output `MainText`

Runtime mapping: `RTTalkNode`

### 7) `SetFPCharacterNode`

Purpose: character identity/theme/binding context for downstream dialogue/response nodes.

Options:

- `GetDataFile` (bool, use `FP_Character` data asset)
- `GameObjectID` (animator/body binding id)
- `BlendShape` (face/blendshape binding id)

Ports:

- Output `CharacterPort`
- Input `FPCharacter` (`FP_Character`)
- Input `Name` (string)
- Input `Gender` (`FP_Gender`)
- Input `Ethnicity` (`FP_Ethnicity`)
- Input `Primary` (`FP_Language`)
- Input `Secondary` (`FP_Language`)
- Input `Tertiary` (`FP_Language`)
- Input `Age` (int)
- Input `Theme` (`FP_Theme`)

Runtime mapping: `RTCharacterNode`

### 8) `FPCombineNode`

Purpose: merge multiple flow paths into one output.

Options:

- `NumOptions` (int; number of input flow ports)

Ports:

- Dynamic inputs `Option_i`
- Output `ExecutionPort` (Flow Out)

Runtime mapping: `RTCombineNode`

### 9) `FPOnewayNode`

Purpose: explicit one-way flow gate (used by runtime previous-navigation logic).

Ports:

- Input `ExecutionPort` (Flow In)
- Output `ExecutionPort` (Flow Out)

Runtime mapping: `RTOnewayNode`

## Validation Rules (Editor-Time)

`FPDialogueGraphValidation` runs on graph change and reports warnings/errors in Graph Toolkit logger.

Key checks include:

- Entry count (missing or multiple entries)
- Empty `GraphID` on Entry
- Invalid/multi flow links for dialogue/oneway nodes
- Missing primary language on character nodes
- Prompt input/output mismatch for response branches
- Missing prompt text
- Exit node using both timeline asset and timeline details

## Runtime Execution Model

### Core roles

- `RTDialogueDirector`
- Owns current node state and progression stack
- Traverses non-interactive nodes automatically
- Stops at interactive nodes (`RTDialogueNode`, `RTResponseNode`) and waits for user actions
- Supports next/previous/repeat/translate/response APIs via `IDialogueDirectorActions`

- `RTGraphDialogueEventHandler`
- Central event bus for runtime graph events
- Emits events such as `DialogueStart`, `DialogueUserNext`, `DialogueUserResponseNext`, `DialogueTimeline`, `DialogueEnd`

- `RTDialogueOrchestrator`
- Scene/UI/timeline glue
- Subscribes to graph events and drives visualization/timeline actions
- Uses mediator checks before acting

- `RTDialogueMediator`
- Lightweight runtime validation/evaluation layer

### Traversal behavior

- Primary next-node lookup uses `NextNodeIndices` (built by importer).
- Fallback lookup uses port metadata (`outNodeIndices`/`inNodeIndices`) by node name/index.
- `RTDialogueNode` and `RTResponseNode` are interaction boundaries.
- `RTOnewayNode` blocks reverse traversal past that point.

## Timeline + Exposed Binding

### Timeline support

`RTTimelineDetails` can be provided on Entry, Dialogue, or Exit paths and includes:

- target director lookup name (`BinderDirectorLookUpName`)
- `TimelineAsset`
- action (`Play`, `Pause`, `Resume`, `Stop`, `Reset`, `Setup`, `SetupAndPlay`)

`RTDialogueOrchestrator` handles timeline events and routes commands to `IDialogueTimeline` implementations (for example `RTFPPlayableDirector`).

### Exposed object binding

`RTExposedBinder` is a runtime `IExposedPropertyTable` implementation that maps string ids to scene objects.

Use the editor utility window:

- `FuzzPhyte/Dialogue/Binder Window`

This helps generate ids and bind scene objects so runtime systems can resolve references by id.

## Recommended Authoring Pattern

- Start with one `EntryNode` and one `GraphID`.
- Add flow nodes (`SetFPDialogueNode`, `SetFPResponseNode`, `FPCombineNode`, `FPOnewayNode`) first.
- Attach payload nodes (`SetFPTalkNode`, `SetFPCharacterNode`, `SetFPSinglePromptNode`).
- End with `ExitNode`
- Resolve validation warnings before runtime testing.
- Add `RTDialogueDirector`, `RTDialogueOrchestrator`, `RTDialogueMediator`, and `RTGraphDialogueEventHandler` in scene wiring.

## Sample Graph Walkthrough

[Sample](./Samples~/SamplesURP/Graph/FPDialogueGraphTest_001.fpdialogue`)

This sample graph is a compact reference for the main runtime behaviors in this package.

What it includes:

- `EntryNode` with graph id set to `AnnaGraphSampleTest`
- Multiple `SetFPDialogueNode` beats (including titled blocks like `First Dialogue Block`, `Second Dialogue Block`, `Yes Response`, `No Response`)
- One `SetFPResponseNode` configured with `PromptNumOptions = 4`
- Four `SetFPSinglePromptNode` options (example prompt text includes `Yes`, `Sure`, `No`, `Not Really` plus translation text)
- Two `FPCombineNode` nodes to merge branches back into shared flow
- Two `FPOnewayNode` gates to demonstrate restricted backward traversal
- Two `ExitNode` endpoints with timeline/output hooks

What this demonstrates at runtime:

- User response branching from a single response node
- Re-joining branch paths after different user choices
- `Previous` navigation in normal flow sections
- One-way sections where reverse navigation is intentionally blocked
- Entry/exit timeline detail hookups and graph event-driven orchestration

Related sample assets:

- Graph scene: `Samples~/SamplesURP/Graph/GraphDialogueSample.unity`
- Dialogue branch screenshots: `Samples~/SamplesURP/Graph/ConfirmDialogue.png` and `Samples~/SamplesURP/Graph/DeclineDialogue.png`
- Timeline detail assets: `Samples~/SamplesURP/Graph/GraphTimelines/`

## Dependencies

Please see the [package.json](./package.json) file for more information.

## License Notes

- This software running a dual license
- Most of the work this repository holds is driven by the development process from the team over at Unity3D to their never ending work on providing fantastic documentation and tutorials that have allowed this to be born into the world.
- I personally feel that software and it's practices should be out in the public domain as often as possible, I also strongly feel that the capitalization of people's free contribution shouldn't be taken advantage of.
- If you want to use this software to generate a profit for you/business I feel that you should equally 'pay up' and in that theory I support strong copyleft licenses.
- If you feel that you cannot adhere to the GPLv3 as a business/profit please reach out to me directly as I am willing to listen to your needs and there are other options in how licenses can be drafted for specific use cases.

### Educational and Research Use MIT Creative Commons

- If you are using this at a Non-Profit and/or are you yourself an educator and want to use this for your classes and for all student use please adhere to the MIT Creative Commons License
- If you are using this back at a research institution for personal research and/or funded research please adhere to the MIT Creative Commons License
- If the funding line is affiliated with an [SBIR](https://www.sbir.gov) be aware that when/if you transfer this work to a small business that work will have to be moved under the secondary license as mentioned below.

### Commercial and Business Use GPLv3 License

- For commercial/business use please adhere by the GPLv3 License
- Even if you are giving the product away and there is no financial exchange you still must adhere to the GPLv3 License

## Contact

- [John Shull](mailto:JShull@fuzzphyte.com)
