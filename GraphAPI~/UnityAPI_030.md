# Unity Graph Toolkit — API Reference (0.3.0-exp.1)

**Package:** `com.unity.graphtoolkit@0.3`  
**Namespace:** `Unity.GraphToolkit.Editor`

> **Experimental Preview (0.3.0-exp.1):** Expect API changes in future versions.

---

## Table of Contents

1. [Overview](#overview)  
2. [Core Types](#core-types)  
   - [Graph](#graph)  
   - [Node](#node)  
   - [BlockNode](#blocknode)  
   - [ContextNode](#contextnode)  
3. [Attributes](#attributes)  
   - [GraphAttribute](#graphattribute)  
   - [SubgraphAttribute](#subgraphattribute)  
   - [UseWithGraphAttribute](#usewithgraphedifacture)  
   - [UseWithContextAttribute](#usewithcontextattribute)  
4. [Editor Services & Utilities](#editor-services--utilities)  
   - [GraphDatabase](#graphdatabase)  
   - [GraphLogger](#graphlogger)  
   - [INodeExtensions](#inodenoderssions)  
5. [Interfaces](#interfaces)  
   - [Node & Variable Interfaces](#node--variable-interfaces)  
   - [Port Interface](#port-interface)  
   - [Definition Contexts & Builders](#definition-contexts--builders)  
6. [Enums](#enums)

---

## Overview

Graph Toolkit provides a framework for building **graph-based tools** inside the Unity Editor—letting developers define custom graphs, nodes, ports, variables, and subgraphs with ready-made UI components.  
Primarily designed for editor tooling, not runtime execution.

---

## Core Types

### Graph

The core graph definition with lifecycle hooks (`OnEnable`, `OnDisable`, `OnGraphChanged`) and access to nodes & variables. Decorate with `GraphAttribute` to register as a graph asset. :contentReference[oaicite:1]{index=1}

**Key members:**

- **Properties**  
  - `name` — Graph's name.  
  - `nodeCount` — Number of nodes.  
  - `variableCount` — Number of declared variables.

- **Methods**  
  - `GetNode(int)`, `GetNodes()` — Access `INode` items (excludes `BlockNode`s).  
  - `GetVariable(int)`, `GetVariables()` — Access `IVariable`s.  
  - `OnEnable()`, `OnDisable()`, `OnGraphChanged(GraphLogger)` — Lifecycle/validation callbacks. :contentReference[oaicite:2]{index=2}

### Node

Base class for user-facing nodes with options and ports. Override:

- `OnDefineOptions(IOptionDefinitionContext)`  
- `OnDefinePorts(IPortDefinitionContext)` :contentReference[oaicite:3]{index=3}

### BlockNode

A node that can only reside inside a `ContextNode`. Use `UseWithContextAttribute` to specify compatibility. :contentReference[oaicite:4]{index=4}

### ContextNode

Container for ordered `BlockNode`s. Provides access to its blocks. :contentReference[oaicite:5]{index=5}

---

## Attributes

### GraphAttribute

Marks a class as a graph type, with specified file extension and behavior via `GraphOptions`. :contentReference[oaicite:6]{index=6}

### SubgraphAttribute

Defines a relationship between a subgraph and its main graph type. Used when subgraphs are supported. :contentReference[oaicite:7]{index=7}

### UseWithGraphAttribute

Restricts a `Node` subclass to specific `Graph` types. :contentReference[oaicite:8]{index=8}

### UseWithContextAttribute

Restricts a `BlockNode` subclass to specified `ContextNode` types. :contentReference[oaicite:9]{index=9}

---

## Editor Services & Utilities

### GraphDatabase

Editor-focused utility for managing graph assets: creation, loading, saving, asset identity (path/GUID). Methods include:

- `CreateGraph<T>(string)`, `PromptInProjectBrowserToCreateNewAsset<T>(string)`  
- `LoadGraph<T>(string)`, `LoadGraphForImporter<T>(string)`  
- `SaveGraphIfDirty(Graph)`  
- `GetGraphAssetPath(Graph)`, `GetGraphAssetGUID(Graph)` :contentReference[oaicite:10]{index=10}

### GraphLogger

Reports errors, warnings, info in the graph editor context. Its output appears in the console when the graph is open. :contentReference[oaicite:11]{index=11}

### INodeExtensions

Includes helper methods such as `.GetNode(this IPort)`, which retrieves the `INode` owning the port. :contentReference[oaicite:12]{index=12}

---

## Interfaces

### Node & Variable Interfaces

- **INode** — Basic node structure (ports counts, getters).  
- **IConstantNode** — Node outputting a constant value.  
- **IVariable** — Declared graph variable (`dataType`, `name`, `variableKind`).  
- **IVariableNode** — Node referencing a variable.  
- **ISubgraphNode** — Node referencing a subgraph.  
- **IErrorsAndWarnings** — Logging interface. :contentReference[oaicite:13]{index=13}

### Port Interface

- **IPort** — Port metadata and connections: `name`, `displayName`, `direction`, `dataType`, `isConnected`, `firstConnectedPort`. Methods to get connected ports or default values. Supports connection rules based on direction and type compatibility. :contentReference[oaicite:14]{index=14}

### Definition Contexts & Builders

Builders used inside `OnDefineOptions` and `OnDefinePorts`:

- **IOptionDefinitionContext**, **IPortDefinitionContext** — Used to define node options and ports. :contentReference[oaicite:15]{index=15}
- **IOptionBuilder**, **IInput/OutputPortBuilder** (and generic variants) — Fluent APIs to configure default values, display names, tooltips, connector UI, delayed evaluation, and to finalize with `.Build()`. :contentReference[oaicite:16]{index=16}

---

## Enums

- **GraphOptions** — Flags for graph behavior like subgraph support.  
- **PortDirection** — Input, Output (defines connection direction).  
- **PortConnectorUI** — Visual style of the port connector.  
- **VariableKind** — Scope/type of variables (e.g., input/output of subgraphs). :contentReference[oaicite:17]{index=17}

---

## Example Template

```csharp
using Unity.GraphToolkit.Editor;
using System;

[Graph(".mygraph", GraphOptions.SupportsSubgraphs)]
public class MyGraph : Graph
{
    public override void OnGraphChanged(GraphLogger logger)
    {
        if (nodeCount == 0)
            logger.LogWarning("Graph is empty", this);
    }
}

[UseWithGraph(typeof(MyGraph))]
[Serializable]
public class AddNode : Node
{
    protected override void OnDefineOptions(IOptionDefinitionContext options)
    {
        options.AddOption<int>("Count").WithDefaultValue(1).ShowInInspectorOnly();
    }

    protected override void OnDefinePorts(IPortDefinitionContext ports)
    {
        ports.AddInputPort<int>("A").WithDefaultValue(0).Build();
        ports.AddInputPort<int>("B").WithDefaultValue(0).Delayed();
        ports.AddOutputPort<int>("Sum").Build();
    }
}
