namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using FuzzPhyte.Dialogue;
    using Unity.GraphToolkit.Editor;
    using UnityEditor.AssetImporters;
    using UnityEngine.Timeline;
    using FuzzPhyte.Utility;
    using System;

    [ScriptedImporter(1, FPDialogueGraph.AssetExtension)]
    internal class FPDialogueImporter:ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<FPDialogueGraph>(ctx.assetPath);

            if (graph == null)
            {
                Debug.LogError($"Failed to load the FPDialogue Graph asset: {ctx.assetPath}");
                return;
            }
            // get start/entry node
            var entryNodeModel = graph.GetNodes().OfType<EntryNode>().FirstOrDefault();
            if (entryNodeModel==null)
            {
                Debug.LogError($"No start/entry point for our dialogue model! No runtime!");
                return;
            }
            // we need to get the Runtime graph (Create it as a scriptable object)
            var runtimeAsset = ScriptableObject.CreateInstance<RTFPDialogueGraph>();
            ctx.AddObjectToAsset("RuntimeAssetGraphTest", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
            // we need to build our node map
            var nodeMap = new Dictionary<INode, int>();
            // first loop
            ConfirmNodeNames(graph);
            // second loop: create all nodes
            CreateRuntimeNodes(entryNodeModel, runtimeAsset,nodeMap);
            //set graphID by entry node
            var nodeGraphIDOption = entryNodeModel.GetNodeOptionByName(FPDialogueGraphValidation.GRAPHID);
            if (nodeGraphIDOption != null)
            {
                nodeGraphIDOption.TryGetValue<string>(out string gID);
                if (gID != string.Empty)
                {
                    runtimeAsset.GraphID = gID;
                }
                else
                {
                    Debug.LogError($"Missing GraphID! Make sure your EntryNode has a value for GraphID!");
                }
            }
            //second loop: establish connections
            SetupConnections(runtimeAsset, nodeMap);
        }
        /// <summary>
        /// Confirms all nodes have their correct name
        /// </summary>
        /// <param name="graph"></param>
        void ConfirmNodeNames(FPDialogueGraph graph)
        {
            var nodeList = graph.GetNodes().OfType<FPVisualNode>().ToList();

            //setup our node names
            for (int i = 0; i < nodeList.Count; i++)
            {
                var cNode = nodeList[i];
                cNode.SetupIndex(i + "_node");
            }
        }
        /// <summary>
        /// Will generate all runtime nodes and traverse the graph from an outport perspective
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="runtimeGraph"></param>
        /// <param name="nodeMap"></param>
        void CreateRuntimeNodes(INode startNode, RTFPDialogueGraph runtimeGraph, Dictionary<INode, int> nodeMap)
        {
            //confirm node names are valid

            //walk the graph
            var nodesToProcess = new Queue<INode>();
            nodesToProcess.Enqueue(startNode);
            //loop over nodes
            while (nodesToProcess.Count > 0)
            {
                var currentNode = nodesToProcess.Dequeue();
                string debugNotes = string.Empty;
                if (nodeMap.ContainsKey(currentNode))
                {
                    //Debug.Log($"NODE is Not in the nodemap");
                    continue;
                }

                debugNotes += "[Current node is in map]";
                if (currentNode == null) 
                {
                    //Debug.Log($"{debugNotes} In Ports|{currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
                    continue;
                }
                debugNotes += "[Current node is not null]";
                var runtimeNodes = ConvertEditorNodeToRealTimeNode(currentNode);
                if (runtimeNodes == null)
                {
                    //Debug.Log($"{debugNotes} In Ports|{currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
                    continue;
                }
                debugNotes += "[Current node has runtime nodes]";
                foreach(var runtimeNode in runtimeNodes)
                {
                    if (runtimeNode != null)
                    {
                        nodeMap[currentNode] = runtimeGraph.Nodes.Count;
                        if (runtimeNode is RTEntryNode entryNode)
                        {
                            //Debug.Log($"Logging Entry Node...");
                            runtimeGraph.AddEntryNode(entryNode);
                            //setup entry node
                            runtimeGraph.SetupGraphEntryPoint(entryNode);
                        }else if(runtimeNode is RTExitNode exitNode)
                        {
                            //Debug.Log($"Logging Exit Node...");
                            runtimeGraph.AddExitNode(exitNode);
                        }else if(runtimeNode is RTDialogueNode dialogueNode)
                        {
                            //Debug.Log($"Logging Dialogue Node...");
                            runtimeGraph.AddDialogueNode(dialogueNode);
                        }else if(runtimeNode is RTResponseNode responseNode)
                        {
                            //Debug.Log($"Logging Response Node...");
                            runtimeGraph.AddResponseNode(responseNode);
                        }else if (runtimeNode is RTCharacterNode characterNode)
                        {
                            //Debug.Log($"Logging Character Node...");
                            runtimeGraph.AddCharacterNode(characterNode);
                        }

                            runtimeGraph.AddRTNodeToList(runtimeNode);
                    }
                }
                //Debug.Log($"{debugNotes}: Current Node: In Ports| {currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
                //queue up all connected nodes
                for (int i = 0; i < currentNode.outputPortCount; i++)
                {
                    //Debug.Log($"....output port index: {i}");
                    var outPort = currentNode.GetOutputPort(i);
                    if (outPort.isConnected)
                    {
                        //Debug.Log($"   .... {outPort.firstConnectedPort.dataType.ToString()}");
                        nodesToProcess.Enqueue(outPort.firstConnectedPort.GetNode());
                    }
                }
            }
        }
        /// <summary>
        /// Back over our generated nodeMap to map runtime connections
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="runtimeGraph"></param>
        /// <param name="nodeMap"></param>
        void SetupConnections(RTFPDialogueGraph runtimeGraph, Dictionary<INode, int> nodeMap)
        {
            foreach (var kvp in nodeMap)
            {
                var editorNode = kvp.Key;
                var runtimeIndex = kvp.Value;
                var runtimeNode = runtimeGraph.Nodes[runtimeIndex];

                for (int i = 0; i < editorNode.outputPortCount; i++)
                {
                    var outPort = editorNode.GetOutputPort(i);
                    if(outPort.isConnected &&nodeMap.TryGetValue(outPort.firstConnectedPort.GetNode(),out int nextIndex))
                    {
                        runtimeNode.NextNodeIndices.Add(nextIndex);
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns a Real-Time Node
        /// </summary>
        /// <param name="editorNode"></param>
        /// <returns></returns>
        static List<RTFPNode> ConvertEditorNodeToRealTimeNode(INode editorNode)
        {
            List<RTFPNode>createdNodes = new List<RTFPNode>();
            switch (editorNode)
            {
                case EntryNode entryNode:
                    //have to evaluate information on RTEntryNode (Input Timeline asset)
                    TimelineAsset tAsset = null;
                    RTTimelineDetails tAssetDetails = null;
                    string mainGraphID = string.Empty;
                    var testOutPort = entryNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var graphOption = entryNode.GetNodeOptionByName(FPDialogueGraphValidation.GRAPHID);
                    if (testOutPort == null)
                    {
                        Debug.LogError($"Missing out port?");
                        return null;
                    }
                    else
                    {
                        //var otherNodes = GetConnectedNodeNamesByPort(testOutPort);
                        var otherNodes = ReturnNodePortDetails(testOutPort,FPPortType.OUTPort);
                        graphOption.TryGetValue<string>(out mainGraphID);
                        if (otherNodes.ConnectedNodes.Length>0)
                        {
                            //Debug.Log($"Entry Node Connected to: {otherNodes.Name}");
                            //only one timeline file
                            var timelinePort = entryNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE);
                            var timelinePortDetails = entryNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS);
                            if (timelinePort != null)
                            {
                                tAsset = GetPortValue<TimelineAsset>(timelinePort);
                            }
                            if(timelinePortDetails != null)
                            {
                                tAssetDetails = GetPortValue<RTTimelineDetails>(timelinePortDetails);
                            }

                            if(timelinePort !=null && timelinePortDetails != null)
                            {
                                //use details
                                var RTentryNode = new RTEntryNode(entryNode.Name, mainGraphID,otherNodes);
                                RTentryNode.incomingTimelineAsset = tAssetDetails;
                                createdNodes.Add(RTentryNode);
                            }
                            else if(timelinePort!=null && timelinePortDetails ==null)
                            {
                                //use timeline directly
                                var RTentryNode = new RTEntryNode(entryNode.Name, mainGraphID,otherNodes);
                                RTentryNode.timelineAsset = tAsset;
                                createdNodes.Add(RTentryNode);
                            }else if(timelinePort==null && timelinePortDetails == null)
                            {
                                Debug.LogWarning($"No Timeline files found - tAsset is null");
                                var RTentryNode = new RTEntryNode(entryNode.Name, mainGraphID, otherNodes);
                                createdNodes.Add(RTentryNode);
                            }
                            return createdNodes;
                        }
                        else
                        {
                            Debug.LogError($"Entry node connected to index is wrong");
                            return null;
                        }   
                    }
                case ExitNode exitNode:
                    TimelineAsset tAssetOut = null;
                    RTTimelineDetails tAssetDetailsOut = null;
                    string playableAssetName = string.Empty;
                    var incomingPort = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (incomingPort==null)
                    {
                        Debug.LogError($"Missing in port!?");
                        return null;
                    }
                    else
                    {
                        var otherNodes = ReturnNodePortDetails(incomingPort,FPPortType.INPort);
                        var timelinePort = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE);
                        var timelinePortDetails = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS);
                        var nodeOptionName = exitNode.GetNodeOptionByName(FPDialogueGraphValidation.GAMEOBJECT_ID);
                        if (nodeOptionName != null)
                        {
                            nodeOptionName.TryGetValue<string>(out playableAssetName);
                        }
                        if (timelinePort != null)
                        {
                            tAssetOut = GetPortValue<TimelineAsset>(timelinePort);
                        }
                        if (timelinePortDetails != null)
                        {
                            tAssetDetailsOut = GetPortValue<RTTimelineDetails>(timelinePortDetails);
                        }

                        if (timelinePort != null && timelinePortDetails != null)
                        {
                            //use details
                            var RTexitNode = new RTExitNode(exitNode.Name, otherNodes);
                            RTexitNode.PlayableDirectorRef = playableAssetName;
                            RTexitNode.outgoingTimelineDetails = tAssetDetailsOut;
                            createdNodes.Add(RTexitNode);
                        }
                        else if (timelinePort != null && timelinePortDetails == null)
                        {
                            //use timeline directly
                            var RTexitNode = new RTExitNode(exitNode.Name, otherNodes);
                            RTexitNode.PlayableDirectorRef = playableAssetName;
                            RTexitNode.timelineAsset = tAssetOut;
                            createdNodes.Add(RTexitNode);
                        }
                        else if (timelinePort == null && timelinePortDetails == null)
                        {
                            Debug.LogWarning($"No Timeline files found - tAsset is null");
                            var RTexitNode = new RTExitNode(exitNode.Name, otherNodes);
                            RTexitNode.PlayableDirectorRef = playableAssetName;
                            createdNodes.Add(RTexitNode);
                        }
                        return createdNodes;
                    }
                case FPCombineNode combineNode:
                    //loop over possible input ports
                    var portOptionCount = combineNode.GetNodeOptionByName(FPDialogueGraphValidation.PORT_NUMBER_OPTIONS);
                    portOptionCount.TryGetValue<int>(out var portCount);
                    List<IPort>possibleIncomingPorts = new List<IPort>();
                    for(var i = 0; i < portCount; i++)
                    {
                        var aPort = combineNode.GetInputPortByName(FPDialogueGraphValidation.PORT_INDEX_OP+i.ToString());
                        if (aPort != null)
                        {
                            possibleIncomingPorts.Add(aPort);
                        }
                    }
                    var outPort = combineNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    RTFPNodePort nodeOne = new RTFPNodePort();
                    RTFPNodePort nodeTwo = new RTFPNodePort();
                    RTFPNodePort nodeOut = new RTFPNodePort();
                    if (outPort != null)
                    {
                        nodeOut = ReturnNodePortDetails(outPort, FPPortType.OUTPort);
                    }
                    if (possibleIncomingPorts.Count == 2)
                    {
                        var portOne = possibleIncomingPorts[0];
                        var portTwo = possibleIncomingPorts[1];
                        if (portOne != null)
                        {
                            //
                            nodeOne = ReturnNodePortDetails(portOne, FPPortType.INPort);
                            
                        }
                        if (portTwo != null)
                        {
                            nodeTwo = ReturnNodePortDetails(portTwo, FPPortType.INPort);
                        }
                        
                        if (nodeOne.ConnectedNodes.Length==0||nodeTwo.ConnectedNodes.Length==0 || nodeOut.ConnectedNodes.Length == 0)
                        {
                            Debug.LogError($"Combine node something");
                            return null;
                        }
                        else
                        {
                            List<RTFPNodePort>allNodeOuts = new List<RTFPNodePort>();
                            allNodeOuts.Add(nodeOut);
                            var RTcombinedNode = new RTCombineNode(combineNode.Name, nodeOne, nodeTwo, allNodeOuts);
                            createdNodes.Add(RTcombinedNode);
                            return createdNodes;
                        }
                    }
                    else
                    {
                        //we have more than two coming in
                        List<RTFPNodePort> nodeIn = new();
                        foreach (var aPort in possibleIncomingPorts)
                        {
                            var allNodesOnPort = ReturnNodePortDetails(aPort, FPPortType.INPort);
                            //var nodeIndexIn = GetFirstNodeNameByPort(aPort);
                            if(allNodesOnPort.ConnectedNodes.Length>0)
                            {
                                nodeIn.Add(allNodesOnPort);
                            }
                        }
                        if (nodeIn.Count == 0 || nodeOut.ConnectedNodes.Length==0)
                        {
                            Debug.LogError($"Combine node something error on list?");
                            return null;
                        }
                        else
                        {
                            List<RTFPNodePort> allNodeOuts = new List<RTFPNodePort>();
                            allNodeOuts.Add(nodeOut);
                            var RTCombineNode = new RTCombineNode(combineNode.Name, nodeIn, allNodeOuts);
                            createdNodes.Add(RTCombineNode);
                            return createdNodes;
                        }
                    }            
                case FPOnewayNode onewayNode:
                    var inDirectionPort = onewayNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outDirectionPort = onewayNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (inDirectionPort != null && outDirectionPort != null)
                    {
                        var inNodeNames = ReturnNodePortDetails(inDirectionPort, FPPortType.INPort);
                        var outNodeNames = ReturnNodePortDetails(outDirectionPort, FPPortType.OUTPort);
                        //string inNodeName = GetFirstNodeNameByPort(inDirectionPort);
                        //string OutNodeName = GetFirstNodeNameByPort(outDirectionPort);
                        var RToneWayNode= new RTOnewayNode(onewayNode.Name, inNodeNames, outNodeNames);
                        createdNodes.Add(RToneWayNode);
                        return createdNodes;
                    }
                    return null;
                case SetFPCharacterNode characNode:
                    var RTCharNode= ReturnNewCharacterNode(characNode);
                    createdNodes.Add(RTCharNode);
                    return createdNodes;
                case SetFPResponseNode responseNode:
                    
                    //generate a loop over number of prompts
                    var numPromptCount = responseNode.GetNodeOptionByName(FPDialogueGraphValidation.USER_NUMBER_OPTIONS);
                    var useWorldLocationOption = responseNode.GetNodeOptionByName(FPDialogueGraphValidation.USE_WORLD_LOCATION);
                    bool useResponseWorldLocations = false;
                    if (useWorldLocationOption != null)
                    {
                        useWorldLocationOption.TryGetValue<bool>(out useResponseWorldLocations);
                    }
                    numPromptCount.TryGetValue<int>(out var numPrompts);
                    var responseNodeFlowIN = responseNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    RTFPNodePort incomingNodeDetails = new RTFPNodePort();
                    if (responseNodeFlowIN == null)
                    {
                        Debug.LogError($"Missing in port on response!?");
                        return null;
                    }
                    else
                    {
                        incomingNodeDetails = ReturnNodePortDetails(responseNodeFlowIN, FPPortType.INPort);
                    }

                    List<RTSinglePromptNode> incomingItems = new List<RTSinglePromptNode>();
                    List<RTFPNodePort> outgoingIndex = new();
                    for (int i=0; i<numPrompts; i++)
                    {
                        //go through four user prompts
                        //go through four user outputs
                        SetFPSinglePromptNode nodePrompt = null;
                        FPVisualNode nodePromptData = null;
                        //ports for each
                        var promptX = responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPTX_OP+i.ToString());
                        var directedPromptX = responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPTX_OP + i.ToString());
                        if(promptX!=null && directedPromptX != null)
                        {
                            nodePrompt = ReturnFirstNodeByPort(promptX) as SetFPSinglePromptNode;
                            nodePromptData = ReturnFirstNodeByPort(directedPromptX);
                            
                            if (nodePrompt!=null || nodePromptData != null)
                            {
                                incomingItems.Add(ReturnNewPromptNode(nodePrompt));
                                var outNodeNames = ReturnNodePortDetails(directedPromptX, FPPortType.INPort);
                                if (outNodeNames.ConnectedNodes.Length > 0)
                                {
                                    outgoingIndex.Add(outNodeNames);
                                }
                                else
                                {
                                    Debug.LogError($"Check Out Nodes on {responseNode.Name}");
                                }
                            }
                            //Debug.LogWarning($"Adding Prompts!! Prompt 1");
                        }
                    }
                    
                    /// Character and finalize Response Node
                    createdNodes.AddRange(incomingItems);
                    SetFPCharacterNode characterNode = null;
                    var characterPortResponse = responseNode.GetInputPortByName(FPDialogueGraphValidation.PORT_ACTOR);
                    if (characterPortResponse != null)
                    {
                        characterNode = ReturnFirstNodeByPort(characterPortResponse) as SetFPCharacterNode;

                        if (characterNode != null)
                        {
                            var aCharacter = ReturnNewCharacterNode(characterNode);
                            if (aCharacter != null)
                            {
                                createdNodes.Add(aCharacter);
                                var RTresponseNode = new RTResponseNode(responseNode.Name, useResponseWorldLocations,incomingNodeDetails, incomingItems, outgoingIndex, aCharacter);
                                createdNodes.Add(RTresponseNode);
                                return createdNodes;
                            }
                            else
                            {
                                Debug.LogError($"Real Time Character failed!");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Unable to create RT Character, failed");
                        }
                    }
                    return null;
                case SetFPTalkNode talkieNode:
                    var RTtalkNode = ReturnNewTalkNode(talkieNode);
                    createdNodes.Add(RTtalkNode);
                    return createdNodes;
                case SetFPDialogueNode dialogueNode:
                    EmotionalState animState = EmotionalState.Neutral;
                    DialogueState dialogueState = DialogueState.Normal;
                    MotionState motionState = MotionState.NA;
                    bool autoScroll = false;
                  
                    RTTalkNode talkRTNode = null;
                    RTTalkNode talkRTTransNode = null;
                    RTCharacterNode rTCharacterNodeIn = null;
                    RTCharacterNode rtCharacterNodeOut = null;
                    RTFPNodePort inputDNode = new RTFPNodePort();
                    RTFPNodePort outputDNode = new RTFPNodePort();
                    var inputPort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outputPort = dialogueNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                   
                    if(inputPort != null)
                    {
                        inputDNode = ReturnNodePortDetails(inputPort,FPPortType.INPort);
                    }
                    else
                    {
                        Debug.LogError($"Missing an input connection on a dialogue!");
                        return null;
                    }
                    if (outputPort != null)
                    {
                        outputDNode = ReturnNodePortDetails(outputPort,FPPortType.OUTPort);
                    }
                    else
                    {
                        Debug.LogError($"Missing an output connection on a dialogue");
                        return null;
                    }
                    var autoScrollPort = dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.USER_WAIT_FOR_USER);
                    if (autoScrollPort != null)
                    {
                        autoScrollPort.TryGetValue<bool>(out autoScroll);
                    }
                    /// Main Text
                    var dialogueInPort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
                    FPVisualNode mainTalk = null;
                    if (dialogueInPort != null)
                    {
                        mainTalk = ReturnFirstNodeByPort(dialogueInPort);
                        if (mainTalk != null)
                        {
                            talkRTNode = ReturnNewTalkNode(mainTalk as SetFPTalkNode);
                            createdNodes.Add(talkRTNode);
                        }
                        else
                        {
                            Debug.LogError($"Missing a FPTalk Node Incoming connected data");
                            return null;
                        }
                    }
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_EMOTION_STATE)?.TryGetValue(out animState);
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_DIALOGUE_STATE)?.TryGetValue(out dialogueState);
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_MOTION_STATE)?.TryGetValue(out motionState);

                    /// Translation
                    var dialogueINTranslationPort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.TRANSLATION_TEXT);
                    FPVisualNode transTalk = null;
                    if (dialogueINTranslationPort != null)
                    {
                        transTalk = ReturnFirstNodeByPort(dialogueINTranslationPort);
                        if (transTalk != null)
                        {
                            talkRTTransNode = ReturnNewTalkNode(transTalk as SetFPTalkNode);
                            createdNodes.Add(talkRTTransNode);
                        }
                    }
                    /// Character In
                    FPVisualNode charNode = null;
                    var characterPortIn = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.PORT_ACTOR);
                    if (characterPortIn != null)
                    {
                        charNode = ReturnFirstNodeByPort(characterPortIn);
                        if (charNode != null)
                        {
                            rTCharacterNodeIn = ReturnNewCharacterNode(charNode as SetFPCharacterNode);
                            createdNodes.Add(rTCharacterNodeIn);
                        }
                    }
                    /// Character Out
                    FPVisualNode charNodeOut = null;
                    var characterPortOut = dialogueNode.GetOutputPortByName(FPDialogueGraphValidation.PORT_ACTOR);
                    if (characterPortOut != null)
                    {
                        charNodeOut = ReturnFirstNodeByPort( characterPortOut);
                        if(charNodeOut != null)
                        {
                            rtCharacterNodeOut = ReturnNewCharacterNode(charNodeOut as SetFPCharacterNode);
                            createdNodes.Add(rtCharacterNodeOut);
                        }
                    }
                    /// auto scroll added
                    /// world prefab options for response
                    var worldObjectUseCase = dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.USE_THREED_OBJECTS);
                    var worldObjectUseCasePrefabs = dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.USE_PREFABS);
                    var useWorldLocationDialogue = dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.GO_WORLD_LOCATION);
                    
                    bool useWorldLoc = false;
                    bool useThreeD = false;
                    bool usePrefabs = false;
                    string worldDialogueSpawnLocation = string.Empty;
                    worldObjectUseCase.TryGetValue<bool>(out useThreeD);
                    worldObjectUseCasePrefabs.TryGetValue<bool>(out usePrefabs);
                    // dialogue world location?
                    if (useWorldLocationDialogue != null)
                    {
                        useWorldLocationDialogue.TryGetValue<bool>(out useWorldLoc);
                        if (useWorldLoc)
                        {
                            var useWorldPortLocation = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.USE_WORLD_LOCATION);
                            if (useWorldPortLocation != null)
                            {
                                useWorldPortLocation.TryGetValue<string>(out worldDialogueSpawnLocation);
                            }   
                        }
                    }
                    if (worldObjectUseCase != null && useThreeD && worldObjectUseCasePrefabs != null && usePrefabs)
                    {
                        var uiDialoguePanelPrefab = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_UI_PANEL);
                        var uiDialogueButtonPrefab = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.DIALGUE_UI_BUTTON);
                        GameObject panelRef;
                        GameObject buttonRef;
                        if (uiDialoguePanelPrefab != null && uiDialogueButtonPrefab != null)
                        {
                            uiDialoguePanelPrefab.TryGetValue(out panelRef);
                            uiDialogueButtonPrefab.TryGetValue(out buttonRef);
                            var RTdialogueNode = new RTDialogueNode(dialogueNode.Name, inputDNode, outputDNode, talkRTNode, rTCharacterNodeIn, panelRef, buttonRef,worldDialogueSpawnLocation,useWorldLoc, autoScroll, rtCharacterNodeOut, talkRTTransNode);
                            RTdialogueNode.GeneralDialogueState = dialogueState;
                            RTdialogueNode.GeneralMotionState = motionState;
                            RTdialogueNode.GeneralEmotionState = animState;
                            createdNodes.Add(RTdialogueNode);
                        }
                        else
                        {
                            Debug.LogError($"Something's wrong with the YES/NO Prefab references");
                        }
                    }
                    else if (worldObjectUseCase != null && useThreeD)
                    {
                        var yesGameObjectNamePort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.RESPONSE_WORLD_YES_LOCATION);
                        var noGameObjectNamePort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.RESPONSE_WORLD_NO_LOCATION);
                        if (yesGameObjectNamePort != null && noGameObjectNamePort != null)
                        {
                            yesGameObjectNamePort.TryGetValue<string>(out string yesGOName);
                            noGameObjectNamePort.TryGetValue<string>(out string noGOName);
                            var RTdialogueNode = new RTDialogueNode(dialogueNode.Name, inputDNode, outputDNode, talkRTNode, rTCharacterNodeIn, yesGOName, noGOName, worldDialogueSpawnLocation,useWorldLoc,autoScroll, rtCharacterNodeOut, talkRTTransNode);
                            RTdialogueNode.GeneralDialogueState = dialogueState;
                            RTdialogueNode.GeneralMotionState = motionState;
                            RTdialogueNode.GeneralEmotionState = animState;
                            createdNodes.Add(RTdialogueNode);
                        }
                        else
                        {
                            Debug.LogError($"Somethings wrong with the Yes/no string ports");
                        }
                    }
                    else
                    {
                        var RTdialogueNode = new RTDialogueNode(dialogueNode.Name, inputDNode, outputDNode, talkRTNode, rTCharacterNodeIn, worldDialogueSpawnLocation,useWorldLoc,autoScroll, rtCharacterNodeOut, talkRTTransNode);
                        RTdialogueNode.GeneralDialogueState = dialogueState;
                        RTdialogueNode.GeneralMotionState = motionState;
                        RTdialogueNode.GeneralEmotionState = animState;
                        createdNodes.Add(RTdialogueNode);
                    }
                        
                    return createdNodes;       
                default:
                    return null;
            }
        }
        static RTCharacterNode ReturnNewCharacterNode(SetFPCharacterNode nodeData)
        {
            string characterName = "blank";
            FP_Gender gender = FP_Gender.NA;
            FP_Ethnicity eth = FP_Ethnicity.Unknown;
            FP_Language firstL = FP_Language.NA;
            FP_Language secondL = FP_Language.NA;
            FP_Language thirdL = FP_Language.NA;
            bool useCharData = false;
            FP_Character charData = null;
            int age = -10;
            string characterMeshIndex = string.Empty;
            string characterBlendObjectName = string.Empty;
            FP_Theme characterTheme = null;
            string nodeIndex = nodeData.Name;
           
            string connectingOutNode = string.Empty;
            var actorPortOut = nodeData.GetOutputPortByName(FPDialogueGraphValidation.PORT_ACTOR);
            if (actorPortOut != null)
            {
                connectingOutNode = GetFirstNodeNameByPort(actorPortOut);
            }
            nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GETDATAFILE)?.TryGetValue(out useCharData);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_NAME)?.TryGetValue(out characterName);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_GENDER)?.TryGetValue(out gender);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_ETH)?.TryGetValue(out eth);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_PRIMARY)?.TryGetValue(out firstL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_SECONDARY)?.TryGetValue(out secondL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_TIERTIARY)?.TryGetValue(out thirdL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_AGE)?.TryGetValue(out age);

            //Debug.Log($"Node Data Option Size? {nodeData.nodeOptionCount}");
            for (int i = 0; i < nodeData.nodeOptionCount; i++)
            {
                var nodeOptionIndex = nodeData.GetNodeOption(i);
                //Debug.Log($"Node Option Index: [{i}] has a name of: {nodeOptionIndex.name}");
            }
            var nodeOption = nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GAMEOBJECT_ID);
            var blendOption = nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GAMEOBJECT_BLENDSHAPE);
            //NODE OPTION with binder
            var resolver = FindAnyObjectByType<RTExposedBinder>();
            if (nodeOption != null)
            {
                nodeOption.TryGetValue<string>(out characterMeshIndex);
            }
            if (blendOption != null)
            {
                blendOption.TryGetValue<string>(out characterBlendObjectName);
            }

            var characterThemePort = nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_THEME);//?.TryGetValue(out characterTheme);
            if (characterThemePort != null)
            {
                characterTheme = GetPortValue<FP_Theme>(characterThemePort);
                if (characterTheme == null)
                {
                    Debug.LogError($"Missing FP_Theme on a Character Node!");
                }
            }
            var charPort = nodeData.GetInputPortByName(FPDialogueGraphValidation.PORT_CHARACTER_DATA);//?.TryGetValue(out charData);
            if (charPort != null)
            {
                charData = GetPortValue<FP_Character>(charPort);
                if (charData == null)
                {
                    Debug.LogError($"Character, FP_Character wasn't on a Character Node?!");
                }
            }
            if (connectingOutNode == string.Empty)
            {
                Debug.LogError("Character data isn't connected to anything - floating in space");
                return null;
            }
            else
            {
                if (useCharData && charData != null)
                {
                    return new RTCharacterNode(nodeIndex, connectingOutNode, charData, characterMeshIndex, characterTheme, characterBlendObjectName,useCharData);
                }
                return new RTCharacterNode(nodeIndex, connectingOutNode, characterName, gender, eth, firstL, secondL, thirdL, age, characterMeshIndex, characterTheme, characterBlendObjectName);

            }
        }
        static RTSinglePromptNode ReturnNewPromptNode(SetFPSinglePromptNode nodeData)
        {
            RTSinglePromptNode aNewReturnNode = null;
            SetFPTalkNode talkNodeMain = null;
            SetFPTalkNode talkNodeTranslation = null;
            RTFPNodePort outNode = new RTFPNodePort();
            Sprite icon = null;
            GameObject locationObject = null;
            string locationObjectIndex = string.Empty;
            var talkNodePort = nodeData.GetInputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
            if (talkNodePort != null)
            {
                talkNodeMain = ReturnFirstNodeByPort(talkNodePort) as SetFPTalkNode;
                if (talkNodeMain == null)
                {
                    Debug.LogError($"Missing Main Text!");
                }
                
            }
            var talkNodeTranslationPort = nodeData.GetInputPortByName(FPDialogueGraphValidation.TRANSLATION_TEXT);
            if (talkNodeTranslationPort != null)
            {
                talkNodeTranslation = ReturnFirstNodeByPort(talkNodeTranslationPort) as SetFPTalkNode;
                if (talkNodeTranslation == null)
                {
                    Debug.LogError($"Missing Node Translation!");
                }
                //talkNodeTranslation = GetPortValue<SetFPTalkNode>(talkNodeTranslationPort);
            }
            //this won't work due to scene-reference vs editor asset reference
            
            //
            var nodeOption = nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GAMEOBJECT_ID);
            //NODE OPTION with binder
            var resolver = FindAnyObjectByType<RTExposedBinder>();
            if (nodeOption != null)
            {
                nodeOption.TryGetValue<string>(out locationObjectIndex);
            }

            
            nodeData.GetInputPortByName(FPDialogueGraphValidation.PORT_ICON)?.TryGetValue(out icon);
            //FPDialogueGraphValidation.USER_PROMPT_PORT
            var outNodePort = nodeData.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_PORT);
            if (outNodePort!=null)
            {
                outNode = ReturnNodePortDetails(outNodePort, FPPortType.OUTPort);
                if (outNode.ConnectedNodes.Length==0)
                {
                    Debug.LogError($"Missing Out node?!");
                }
            }
            if (outNode.ConnectedNodes.Length > 0 && talkNodeMain != null&&talkNodeTranslation!=null)
            {
                //go get real talk
                
                var talkNodeRT = ReturnNewTalkNode(talkNodeMain);
                var translationNodeRT = ReturnNewTalkNode(talkNodeTranslation);
                aNewReturnNode = new RTSinglePromptNode(nodeData.Name,outNode, talkNodeRT, translationNodeRT, icon, locationObjectIndex,locationObject);
            }
            else
            {
                Debug.LogError($"Failed New Prompt Node");
            }
            return aNewReturnNode;
        }
        static RTTalkNode ReturnNewTalkNode(SetFPTalkNode nodeData)
        {
            FP_Language nodeLanguage = FP_Language.NA;
            string header = string.Empty;
            string text = string.Empty;
            AudioClip textClip = null;
            AnimationClip animClip = null;
            AnimationClip bodyAnimClip = null;
            RTFPNodePort outputNode = new RTFPNodePort();
            
            nodeData.GetInputPortByName(FPDialogueGraphValidation.LANG_NAME)?.TryGetValue(out nodeLanguage);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_HEADER)?.TryGetValue(out header);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE)?.TryGetValue(out text);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_AUDIO_NAME)?.TryGetValue(out textClip);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ANIM_BLEND_FACE)?.TryGetValue(out animClip);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ANIM_BLEND_BODY)?.TryGetValue(out bodyAnimClip);
            var talkOutputPort = nodeData.GetOutputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
            if (talkOutputPort != null)
            {
                outputNode = ReturnNodePortDetails(talkOutputPort,FPPortType.OUTPort);
                if (outputNode.ConnectedNodes.Length==0)
                {
                    Debug.LogError($"Talk node isn't connected!");
                }
            }
            
            return new RTTalkNode(nodeData.Name, outputNode, nodeLanguage, header, text, textClip, animClip,bodyAnimClip);
        }
        /// <summary>
        /// Pass a port, return a value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="port"></param>
        /// <returns></returns>
        static T GetInputPortValue<T>(IPort port)
        {
            T value = default;
            if (port.isConnected)
            {
                switch (port.firstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                    default:
                        break;
                }
            }
            else
            {
                //not connected get value?
                port.TryGetValue(out value);
            }
            return value;
        }
        static T GetPortValue<T>(IPort port)
        {
            if (port == null) return default;
            if (port.isConnected)
            {
                if(port.firstConnectedPort.GetNode() is IVariableNode variableNode)
                {
                    variableNode.variable.TryGetDefaultValue(out T value);
                    return value;
                }else if(port.firstConnectedPort.GetNode() is IConstantNode constantNode)
                {
                    constantNode.TryGetValue<T>(out T value);
                    return value;
                }
            }
            
            port.TryGetValue(out T fallbackValue);
            return fallbackValue;
        }
        static List<string> GetConnectedNodeNamesByPort(IPort port)
        {
            List<string> nodeNames = new();
            var visualNodes = ConnectedNodesByPort(port);
            for(int i = 0; i < visualNodes.Count; i++)
            {
                var node = visualNodes[i];
                if (node != null)
                {
                    nodeNames.Add(node.Name);
                }
            }
            
            return nodeNames;
        }
        static string GetFirstNodeNameByPort(IPort port)
        {
            string nodeName = string.Empty;
            var aNode = ReturnFirstNodeByPort(port);
            if (aNode != null)
            {
                nodeName = aNode.Name;
            }
            return nodeName;
        }
        
        /// <summary>
        /// Returns RTFPNodePort based on a port you give it
        /// </summary>
        /// <param name="port"></param>
        /// <param name="thePassedPortDirection">the port direction</param>
        /// <returns></returns>
        static RTFPNodePort ReturnNodePortDetails(IPort port, FPPortType thePassedPortDirection)
        {
            //get other ports
            List<IPort> portsConnectedToPort = new List<IPort>();
            port.GetConnectedPorts(portsConnectedToPort);
            //build array of nodes by individual port
            List<RTFPNodeDetails> connectedNodeList = new();
            foreach(var aPort in portsConnectedToPort)
            {
                var curNode = aPort.GetNode() as FPVisualNode;
                FPPortType portDirection = FPPortType.NA;
                switch (thePassedPortDirection)
                {
                    case FPPortType.NA:
                        break;
                    case FPPortType.INPort:
                        portDirection = FPPortType.OUTPort;
                        break;
                    case FPPortType.OUTPort:
                        portDirection = FPPortType.INPort;
                        break;
                }
                RTFPNodeDetails details = new RTFPNodeDetails
                {
                    PortType = portDirection,
                    NodeIndex = curNode.Name,
                    PortIndex = aPort.name
                };
                connectedNodeList.Add(details);
            }
            return new RTFPNodePort
            {
                MyPort = port.name,
                ConnectedNodes = connectedNodeList.ToArray()
            };
        }
        /// <summary>
        /// will return the first INode on the other end of a connected iPort
        /// E.g. Node 0 is connected to Node 1, you pass the outgoing port on Node 0 and it returns Node 1
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        static List<FPVisualNode> ConnectedNodesByPort(IPort port)
        {
            List<IPort> possiblePorts = new List<IPort>();
            port.GetConnectedPorts(possiblePorts);

            //FPVisualNode outNode = null;
            List<FPVisualNode> allOutNodes = new();
            for(int i = 0; i < possiblePorts.Count; i++)
            {
                var possiblePort = possiblePorts[i];
                var outNode = possiblePort.GetNode() as FPVisualNode;
                if (outNode != null)
                {
                    allOutNodes.Add(outNode);
                }
            }
           
            return allOutNodes;
        }
        static FPVisualNode ReturnFirstNodeByPort(IPort port)
        {
            List<IPort> possiblePorts = new List<IPort>();
            port.GetConnectedPorts(possiblePorts);
            if (possiblePorts.Count > 0)
            {
                return possiblePorts[0].GetNode() as FPVisualNode;
            }
            return null;
        }
        [Obsolete("This should be moved to runtime operation - not editor")]
        static (GameObject,string) ResolveObject(INodeOption nodeOption, RTExposedBinder resolver)
        {
            if (nodeOption == null || resolver == null)
            {
                Debug.LogError($"Both were null");
                return (null,string.Empty);
            }

            if (!nodeOption.TryGetValue<string>(out var id) || string.IsNullOrEmpty(id))
            {
                Debug.LogError($"Failed on first pass");
                return (null,string.Empty);
            }
            //Debug.LogWarning($"ID value = {id}");
            var er = new ExposedReference<GameObject>
            {
                exposedName = new PropertyName(id),
                defaultValue = null   // optional; only used if no binding in resolver
            };

            return (er.Resolve(resolver),id); // returns the bound scene object (or null)
        }
    }
}
