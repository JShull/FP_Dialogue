// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

namespace FuzzPhyte.Dialogue.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class ActionTestReceiver : MonoBehaviour, IFPDialogueAction
    {
        public int Calls;
        public GraphEventData LastData;
        public bool Throw;
        public void ExecuteDialogueAction(GraphEventData data)
        {
            Calls++;
            LastData = data;
            if (Throw) throw new InvalidOperationException("Action test failure");
        }
    }

    public class ActionTestOrchestrator : RTDialogueOrchestrator
    {
        protected override void Awake() { }
        protected override void OnEnable() { }
        public void Configure(RTExposedBinder table)
        {
            binder = table;
            graphID = "graph";
            conversationID = "conversation";
        }
        public void Receive(GraphEventData data) => Handle(data);
    }

    public class ActionTestDirector : RTDialogueDirector
    {
        public void Configure(RTFPDialogueGraph graph, RTGraphDialogueEventHandler hub)
        {
            Awake();
            RuntimeGraph = graph;
            graphID = "graph";
            conversationID = "conversation";
            typeof(RTDialogueDirector).GetField("eventHandler", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, hub);
        }
        public void Traverse(RTFPNode first)
        {
            currentNode = first;
            AdvanceUntilInteractive();
        }
        public void BackFrom(RTFPNode node)
        {
            currentNode = node;
            PreviousUntilInteractive();
        }
        public RTFPNode Current => currentNode;
    }

    public class DialogueActionTests
    {
        readonly List<UnityEngine.Object> owned = new();
        GameObject Create(string name)
        {
            var go = new GameObject(name);
            owned.Add(go);
            return go;
        }
        RTActionNode Node(string lookup = "gate") => new RTActionNode("action", lookup, default, default);
        GraphEventData Data(RTActionNode node) => new GraphEventData
        {
            EventType = GraphDialogueEventType.DialogueAction,
            GraphId = "graph", ConversationId = "conversation", CurrentNode = node
        };
        [TearDown]
        public void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }
        ActionTestOrchestrator Setup(out RTExposedBinder binder, out ActionTestReceiver receiver)
        {
            var host = Create("host");
            binder = host.AddComponent<RTExposedBinder>();
            var orchestrator = host.AddComponent<ActionTestOrchestrator>();
            orchestrator.Configure(binder);
            var target = Create("target");
            receiver = target.AddComponent<ActionTestReceiver>();
            binder.SetReferenceValue(new PropertyName("gate"), target);
            return orchestrator;
        }
        [Test]
        public void DispatchResolvesGameObjectAndPreservesContext()
        {
            var orchestrator = Setup(out _, out var receiver);
            var node = Node();
            orchestrator.Receive(Data(node));
            Assert.That(receiver.Calls, Is.EqualTo(1));
            Assert.That(receiver.LastData.CurrentNode, Is.SameAs(node));
        }
        [TestCase("other", "conversation")]
        [TestCase("graph", "other")]
        public void DispatchRejectsOtherGraphOrConversation(string graph, string conversation)
        {
            var orchestrator = Setup(out _, out var receiver);
            var data = Data(Node()); data.GraphId = graph; data.ConversationId = conversation;
            orchestrator.Receive(data);
            Assert.That(receiver.Calls, Is.Zero);
        }
        [TestCase("")]
        [TestCase("missing")]
        public void MissingLookupWarnsAndReturns(string lookup)
        {
            var orchestrator = Setup(out _, out var receiver);
            LogAssert.Expect(LogType.Warning, new Regex("^\\[Dialogue Action\\]"));
            orchestrator.Receive(Data(Node(lookup)));
            Assert.That(receiver.Calls, Is.Zero);
        }
        [Test]
        public void MissingInterfaceWarnsAndReturns()
        {
            var orchestrator = Setup(out var binder, out _);
            binder.SetReferenceValue(new PropertyName("gate"), Create("empty"));
            LogAssert.Expect(LogType.Warning, new Regex("^\\[Dialogue Action\\].*needs an IFPDialogueAction"));
            orchestrator.Receive(Data(Node()));
        }
        [Test]
        public void BacktrackingSkipsActionAndForwardRevisitFiresAgain()
        {
            var graph = ScriptableObject.CreateInstance<RTFPDialogueGraph>(); owned.Add(graph);
            var before = new RTDialogueNode("before", default, default, null, null, waitForUser: true, timelineProcessFile: null);
            var after = new RTDialogueNode("after", default, default, null, null, waitForUser: true, timelineProcessFile: null);
            var action = new RTActionNode("action", "gate", new RTFPNodePort
            {
                ConnectedNodes = new[] { new RTFPNodeDetails { NodeIndex = "before" } }
            }, default);
            after.inNodeIndices = new[] { new RTFPNodePort
            {
                ConnectedNodes = new[] { new RTFPNodeDetails { NodeIndex = "action" } }
            } };
            action.NextNodeIndices.Add(2);
            graph.Nodes.Add(before); graph.Nodes.Add(action); graph.Nodes.Add(after);
            var hub = Create("hub").AddComponent<RTGraphDialogueEventHandler>();
            var events = new List<GraphEventData>(); hub.OnGraphDialogueEvent += events.Add;
            var director = Create("director").AddComponent<ActionTestDirector>(); director.Configure(graph, hub);
            director.ProgressionPath.Push(before.Index);
            director.Traverse(action);
            Assert.That(director.Current, Is.SameAs(after));
            events.Clear();
            director.BackFrom(after);
            Assert.That(director.Current, Is.SameAs(before));
            Assert.That(events.Exists(e => e.EventType == GraphDialogueEventType.DialogueAction), Is.False);
            events.Clear();
            director.Traverse(action);
            Assert.That(events.FindAll(e => e.EventType == GraphDialogueEventType.DialogueAction).Count, Is.EqualTo(1));
            Assert.That(director.Current, Is.SameAs(after));
        }
        [Test]
        public void ConsecutiveActionsContinueSynchronouslyEvenWhenReceiverThrows()
        {
            var orchestrator = Setup(out _, out var receiver);
            receiver.Throw = true;
            var graph = ScriptableObject.CreateInstance<RTFPDialogueGraph>(); owned.Add(graph);
            var first = Node(); var second = Node(); second.Index = "second";
            first.NextNodeIndices.Add(1);
            graph.Nodes.Add(first); graph.Nodes.Add(second);
            var hub = Create("hub").AddComponent<RTGraphDialogueEventHandler>();
            hub.DontDestroy = false;
            hub.OnGraphDialogueEvent += orchestrator.Receive;
            var events = new List<GraphEventData>();
            hub.OnGraphDialogueEvent += events.Add;
            var director = Create("director").AddComponent<ActionTestDirector>();
            director.Configure(graph, hub);
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: Action test failure"));
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: Action test failure"));
            director.Traverse(first);
            Assert.That(receiver.Calls, Is.EqualTo(2));
            Assert.That(director.Current, Is.Null);
            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].NextNode, Is.SameAs(second));
            Assert.That(director.ProgressionPath.Count, Is.EqualTo(2));
        }
    }
}
