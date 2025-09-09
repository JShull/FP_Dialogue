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
            //second loop: establish connections
            SetupConnections(entryNodeModel, runtimeAsset, nodeMap);
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
                Debug.Log($"Node: {cNode.Name} Confirmed");
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
                    Debug.Log($"NODE is Not in the nodemap");
                    continue;
                }

                debugNotes += "[Current node is in map]";
                if (currentNode == null) 
                {
                    Debug.Log($"{debugNotes} In Ports|{currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
                    continue;
                }
                debugNotes += "[Current node is not null]";
                var runtimeNodes = ConvertEditorNodeToRealTimeNode(currentNode);
                if (runtimeNodes == null)
                {
                    Debug.Log($"{debugNotes} In Ports|{currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
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
                            Debug.Log($"Logging Entry Node...");
                            runtimeGraph.AddEntryNode(entryNode);
                        }else if(runtimeNode is RTExitNode exitNode)
                        {
                            Debug.Log($"Logging Exit Node...");
                            runtimeGraph.AddExitNode(exitNode);
                        }else if(runtimeNode is RTDialogueNode dialogueNode)
                        {
                            Debug.Log($"Logging Dialoge Node...");
                            runtimeGraph.AddDialogueNode(dialogueNode);
                        }else if(runtimeNode is RTResponseNode responseNode)
                        {
                            Debug.Log($"Logging Response Node...");
                            runtimeGraph.AddResponseNode(responseNode);
                        }else if (runtimeNode is RTCharacterNode characterNode)
                        {
                            Debug.Log($"Logging Character Node...");
                            runtimeGraph.AddCharacterNode(characterNode);
                        }

                            runtimeGraph.AddRTNodeToList(runtimeNode);
                    }
                }
                Debug.Log($"{debugNotes}: Current Node: In Ports| {currentNode.inputPortCount}|| Out Ports|{currentNode.outputPortCount}");
                //queue up all connected nodes
                for (int i = 0; i < currentNode.outputPortCount; i++)
                {
                    Debug.Log($"....output port index: {i}");
                    var outPort = currentNode.GetOutputPort(i);
                    if (outPort.isConnected)
                    {
                        Debug.Log($"   .... {outPort.firstConnectedPort.dataType.ToString()}");
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
        void SetupConnections(INode startNode, RTFPDialogueGraph runtimeGraph, Dictionary<INode, int> nodeMap)
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
                    //have to evalute information on RTEnetryNode (Input Timeline asset)
                    TimelineAsset tAsset = null;
                    RTTimelineDetails tAssetDetails = null;
                    var testOutPort = entryNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    
                    if (testOutPort == null)
                    {
                        Debug.LogError($"Missing out port?");
                        return null;
                    }
                    else
                    {
                        var otherNodes = GetConnectedNodeNamesByPort(testOutPort);
                        if (otherNodes != null)
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
                                var RTentryNode = new RTEntryNode(entryNode.Name, otherNodes, tAssetDetails);
                                createdNodes.Add(RTentryNode);
                            }
                            else if(timelinePort!=null && timelinePortDetails ==null)
                            {
                                //use timeline directly
                                var RTentryNode = new RTEntryNode(entryNode.Name, otherNodes, tAsset);
                                createdNodes.Add(RTentryNode);
                            }else if(timelinePort==null && timelinePortDetails == null)
                            {
                                Debug.LogWarning($"No Timeline files found - tAsset is null");
                                var RTentryNode = new RTEntryNode(entryNode.Name, otherNodes, tAsset);
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
                    List<string> nodeInIndexs = new();
                    var incomingPort = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (incomingPort==null)
                    {
                        Debug.LogError($"Missing in port!?");
                        return null;
                        
                    }
                    else
                    {
                        nodeInIndexs = GetConnectedNodeNamesByPort(incomingPort);
                       
                        var timelinePort = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE);
                        var timelinePortDetails = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS);
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
                            var RTentryNode = new RTExitNode(exitNode.Name, nodeInIndexs, tAssetDetailsOut);
                            createdNodes.Add(RTentryNode);
                        }
                        else if (timelinePort != null && timelinePortDetails == null)
                        {
                            //use timeline directly
                            var RTentryNode = new RTEntryNode(exitNode.Name, nodeInIndexs, tAssetOut);
                            createdNodes.Add(RTentryNode);
                        }
                        else if (timelinePort == null && timelinePortDetails == null)
                        {
                            Debug.LogWarning($"No Timeline files found - tAsset is null");
                            var RTentryNode = new RTEntryNode(exitNode.Name, nodeInIndexs, tAssetOut);
                            createdNodes.Add(RTentryNode);
                        }
                        return createdNodes;
                    }
                case FPCombineNode combineNode:
                    var portOne = combineNode.GetInputPortByName(FPDialogueGraphValidation.PORT_COMBINE_OPONE);
                    var portTwo = combineNode.GetInputPortByName(FPDialogueGraphValidation.PORT_COMBINE_OPTWO);
                    var outPort = combineNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    string nodeOne= string.Empty;
                    string nodeTwo= string.Empty;
                    List<string> nodeOut = new();
                    
                    if (portOne != null)
                    {
                        nodeOne = GetFirstNodeNameByPort(portOne);
                    }
                    if (portTwo != null)
                    {
                        nodeTwo = GetFirstNodeNameByPort(portTwo);
                    }
                    if (outPort != null)
                    {
                        nodeOut = GetConnectedNodeNamesByPort(outPort);
                    }
                    if (nodeOne == string.Empty || nodeTwo == string.Empty || nodeOut.Count ==0 )
                    {
                        Debug.LogError($"Combine node something");
                        return null;
                    }
                    else
                    {
                        var RTcombinedNode =  new RTCombineNode(combineNode.Name, nodeOne, nodeTwo, nodeOut);
                        createdNodes.Add(RTcombinedNode);
                        return createdNodes;
                    }
                case FPOnewayNode onewayNode:
                    var inDirectionPort = onewayNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outDirectionPort = onewayNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (inDirectionPort != null && outDirectionPort != null)
                    {
                        string inNodeName = GetFirstNodeNameByPort(inDirectionPort);
                        string OutNodeName = GetFirstNodeNameByPort(outDirectionPort);
                        var RToneWayNode= new RTOnewayNode(onewayNode.Name, inNodeName, OutNodeName);
                        createdNodes.Add(RToneWayNode);
                        return createdNodes;
                    }
                    return null;
                case SetFPCharacterNode characNode:
                    var RTCharNode= ReturnNewCharacterNode(characNode);
                    createdNodes.Add(RTCharNode);
                    return createdNodes;
                case SetFPResponseNode responseNode:
                    //go through four user prompts
                    //go through four user outputs
                    SetFPSinglePromptNode nodeData = null;
                    SetFPSinglePromptNode nodeTwoData = null;
                    SetFPSinglePromptNode nodeThreeData = null;
                    SetFPSinglePromptNode nodeFourthData = null;

                    FPVisualNode nodeDataMap = null;
                    FPVisualNode nodeTwoOut = null;
                    FPVisualNode nodeThreeOut = null;
                    FPVisualNode nodeFourthOut = null;

                    List<RTSinglePromptNode> incomingItems = new List<RTSinglePromptNode>();
                    List<string> outcomingIndex = new List<string>();
                    var promptOnePort = responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_ONE);
                    var directedPromptOne = responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_ONE);
                    if (promptOnePort != null && directedPromptOne != null)
                    {
                        nodeData = ReturnFirstNodeByPort(promptOnePort) as SetFPSinglePromptNode;
                        nodeDataMap = ReturnFirstNodeByPort(directedPromptOne);
                        //firstPrompt = GetPortValue<SetFPSinglePromptNode>(promptOnePort);
                        //firstOut = GetPortValue<FPVisualNode>(directedPromptOne);
                        if(nodeData != null || nodeDataMap != null)
                        {
                            incomingItems.Add(ReturnNewPromptNode(nodeData));
                            outcomingIndex.Add(nodeDataMap.Name);
                            Debug.LogWarning($"Adding Prompts!! Prompt 1");
                        }
                    }
                    else
                    {
                        Debug.LogError($"We should be using the first prompt!");
                        return null;
                    }
                    var promptTwoPort = responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_TWO);
                    var directedPromptTwo = responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_TWO);
                    if (promptTwoPort != null && directedPromptTwo != null)
                    {
                        nodeTwoData = ReturnFirstNodeByPort(promptTwoPort) as SetFPSinglePromptNode;
                        nodeTwoOut = ReturnFirstNodeByPort(directedPromptTwo);
                        if (nodeTwoData != null || nodeTwoOut != null)
                        {
                            incomingItems.Add(ReturnNewPromptNode(nodeTwoData));
                            outcomingIndex.Add(nodeTwoOut.Name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Second Prompt == null or second out == null");
                    }
                    /// third prompt
                    var promptThreePort = responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_THREE);
                    var directedPromptThree = responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_THREE);
                    if (promptThreePort != null && directedPromptThree != null)
                    {
                        nodeThreeData = ReturnFirstNodeByPort(promptThreePort) as SetFPSinglePromptNode;
                        nodeThreeOut = ReturnFirstNodeByPort(directedPromptThree);
                        if (nodeThreeData != null || nodeThreeOut != null)
                        {
                            incomingItems.Add(ReturnNewPromptNode(nodeThreeData));
                            outcomingIndex.Add(nodeThreeOut.Name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Third Prompt == null or Third out == null");
                    }
                    /// fourth prompt
                    var promptFourPort = responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_FOUR);
                    var directedPromptFour = responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_FOUR);
                    if (promptFourPort != null && directedPromptFour != null)
                    {
                        nodeFourthData = ReturnFirstNodeByPort(promptFourPort) as SetFPSinglePromptNode;
                        nodeFourthOut = ReturnFirstNodeByPort(directedPromptFour);
                        if (nodeFourthData != null || nodeFourthOut != null)
                        {
                            incomingItems.Add(ReturnNewPromptNode(nodeFourthData));
                            outcomingIndex.Add(nodeFourthOut.Name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Fourth Prompt == null or Fourth out == null");
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
                                var RTresponseNode = new RTResponseNode(responseNode.Name, incomingItems, outcomingIndex, aCharacter);
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

                    RTTalkNode talkRTNode = null;
                    RTTalkNode talkRTTransNode = null;
                    RTCharacterNode rTCharacterNodeIn = null;
                    RTCharacterNode rtCharacterNodeOut = null;
                    
                    FPVisualNode inputDNode = null;
                    FPVisualNode outputDNode = null;
                    var inputPort = dialogueNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outputPort = dialogueNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if(inputPort != null)
                    {
                        inputDNode = ReturnFirstNodeByPort(inputPort);
                    }
                    else
                    {
                        Debug.LogError($"Missing an input connection on a dialogue!");
                        return null;
                    }
                    if (outputPort != null)
                    {
                        outputDNode = ReturnFirstNodeByPort(outputPort);
                    }
                    else
                    {
                        Debug.LogError($"Missing an output connection on a dialogue");
                        return null;
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
                    var RTdialogueNode =new RTDialogueNode(dialogueNode.Name, inputDNode.Name, outputDNode.Name, talkRTNode,rTCharacterNodeIn,rtCharacterNodeOut, talkRTTransNode);
                    createdNodes.Add(RTdialogueNode);
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
            GameObject characterMeshR = null;
            string characterMeshIndex = string.Empty;
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

            Debug.Log($"Node Data Option Size? {nodeData.nodeOptionCount}");
            for (int i = 0; i < nodeData.nodeOptionCount; i++)
            {
                var nodeOptionIndex = nodeData.GetNodeOption(i);
                Debug.Log($"Node Option Index: [{i}] has a name of: {nodeOptionIndex.name}");
            }
            var nodeOption = nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GAMEOBJECT_ID);
            //NODE OPTION with binder
            var resolver = FindAnyObjectByType<RTExposedBinder>();
            if (nodeOption != null)
            {
        
                (characterMeshR, characterMeshIndex) = ResolveObject(nodeOption, resolver);
                //this still ends up null
                if (characterMeshR != null)
                {
                    Debug.LogWarning($"IT WORKED!!! {characterMeshR.name}");
                }
                else
                {
                    Debug.LogError($"Still no fucking gameobject!!!");
                }
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
                    return new RTCharacterNode(nodeIndex, connectingOutNode, charData, characterMeshIndex, characterTheme, useCharData);
                }
                return new RTCharacterNode(nodeIndex, connectingOutNode, characterName, gender, eth, firstL, secondL, thirdL, age, characterMeshIndex, characterTheme);
            }
        }
        static RTSinglePromptNode ReturnNewPromptNode(SetFPSinglePromptNode nodeData)
        {
            RTSinglePromptNode aNewReturnNode = null;
            SetFPTalkNode talkNodeMain = null;
            SetFPTalkNode talkNodeTranslation = null;
            SetFPResponseNode outNode = null;
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

                (locationObject, locationObjectIndex) = ResolveObject(nodeOption, resolver);
                //this still ends up null
                if (locationObject != null)
                {
                    Debug.LogWarning($"IT WORKED!!! {locationObject.name}");
                }
                else
                {
                    Debug.LogError($"Still no fucking gameobject!!!");
                }
            }

            
            nodeData.GetInputPortByName(FPDialogueGraphValidation.PORT_ICON)?.TryGetValue(out icon);
            var outNodePort = nodeData.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_PORT);
            if (outNodePort!=null)
            {
                outNode = ReturnFirstNodeByPort(outNodePort) as SetFPResponseNode;
                if (outNode == null)
                {
                    Debug.LogError($"Missing Out node?!");
                }
            }
            if (outNode!=null&&talkNodeMain != null&&talkNodeTranslation!=null)
            {
                //go get real talk
                
                var talkNodeRT = ReturnNewTalkNode(talkNodeMain);
                var translationNodeRT = ReturnNewTalkNode(talkNodeTranslation);
                aNewReturnNode = new RTSinglePromptNode(nodeData.Name,outNode.Name, talkNodeRT, translationNodeRT, icon, locationObjectIndex,locationObject);
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
            FPVisualNode outputNode = null;
            string outputNodeIndex = string.Empty;
            nodeData.GetInputPortByName(FPDialogueGraphValidation.LANG_NAME)?.TryGetValue(out nodeLanguage);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_HEADER)?.TryGetValue(out header);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE)?.TryGetValue(out text);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_AUDIO_NAME)?.TryGetValue(out textClip);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ANIM_BLEND_FACE)?.TryGetValue(out animClip);
            var talkOutputPort = nodeData.GetOutputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
            if (talkOutputPort != null)
            {
                outputNode = ReturnFirstNodeByPort(talkOutputPort) as FPVisualNode;
                if (outputNode != null)
                {
                    outputNodeIndex = outputNode.Name;
                }
            }
            
            return new RTTalkNode(nodeData.Name, outputNodeIndex, nodeLanguage, header, text, textClip, animClip);
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
            Debug.LogWarning($"ID value = {id}");
            var er = new ExposedReference<GameObject>
            {
                exposedName = new PropertyName(id),
                defaultValue = null   // optional; only used if no binding in resolver
            };

            return (er.Resolve(resolver),id); // returns the bound scene object (or null)
        }


    }
}
