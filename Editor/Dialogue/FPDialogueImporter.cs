namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FuzzPhyte.Dialogue;
    using Unity.GraphToolkit.Editor;
    using UnityEditor.AssetImporters;
    using UnityEngine.Timeline;
    using FuzzPhyte.Utility;
    using PlasticGui.WorkspaceWindow.Items;

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
            //get start/entry node
            
            var entryNodeModel = graph.GetNodes().OfType<EntryNode>().FirstOrDefault();
            if (entryNodeModel==null)
            {
                Debug.LogError($"No start/entry point for our dialogue model! No runtime!");
                return;
            }
            //lets confirm nodes have a name
            ConfirmNodeNames(graph);
            //we need to get the Runtime graph (Create it as a scriptable object)
            var runtimeAsset = ScriptableObject.CreateInstance<RTFPDialogueGraph>();
            //we need to build our node map
            var nodeMap = new Dictionary<INode, int>();

            ctx.AddObjectToAsset("RuntimeAssetGraphTest", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);

            //first loop: create all nodes

            CreateRuntimeNodes(entryNodeModel, runtimeAsset,nodeMap);
            //second loop: establish connections
            SetupConnections(entryNodeModel, runtimeAsset, nodeMap);
        }
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
                if (nodeMap.ContainsKey(currentNode)) continue;
                var runtimeNodes = EditorModelToRuntimeNodes(currentNode);
                foreach(var runtimeNode in runtimeNodes)
                {

                }
            }
        }
        void SetupConnections(INode startNode, RTFPDialogueGraph runtimeGraph, Dictionary<INode, int> nodeMap)
        {

        }
        /// <summary>
        /// Translate/convert all editor node data to RT nodes
        /// </summary>
        /// <param name="editorNode"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        static List<RTFPNode>EditorModelToRuntimeNodes(INode editorNode)
        {
            /// We have flow nodes and data nodes
            /// flow
            ///   Entry
            ///   SetFPDialogueNode
            ///   SetFPResponseNode
            ///   FPCombineNode
            ///   FPOnewayNode
            ///   Exit
            ///   
            /// data
            ///   SetFPCharacterNode
            ///   SetFPTalkNode
            var returnedNodes = new List<RTFPNode>();
            var realTimeNode = ConvertEditorNodeToRealTimeNode(editorNode);
            if (realTimeNode != null)
            {
                returnedNodes.Add(realTimeNode);
            }
            return returnedNodes;
        }
        
        static RTFPNode ConvertEditorNodeToRealTimeNode(INode editorNode)
        {
            switch (editorNode)
            {
                case EntryNode entryNode:
                    //have to evalute information on RTEnetryNode (Input Timeline asset)
                    TimelineAsset tAsset = null;
                    //FPVisualNode outNode = null;
                    var testOutPort = entryNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (testOutPort == null)
                    {
                        Debug.LogError($"Missing out port?");
                        return null;
                    }
                    else
                    {
                        var otherNode = FirstConnectedNodeByPort(testOutPort);
                        if (otherNode != null)
                        {
                            Debug.Log($"Entry Node Connected to: {otherNode.Name}");
                            entryNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)?.TryGetValue(out tAsset);
                            return new RTEntryNode(entryNode.Name, otherNode.Name, tAsset);
                        }
                        else
                        {
                            Debug.LogError($"Entry node connected to index is wrong");
                            return null;
                        }   
                    }
                case ExitNode exitNode:
                    TimelineAsset tAssetOut = null;
                    string nodeInIndex = string.Empty;
                    var incomingPort = exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (incomingPort!=null)
                    {
                        nodeInIndex = GetConnectedNodeNameByPort(incomingPort);
                    }
                    exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)?.TryGetValue(out tAssetOut);
                    return new RTExitNode(exitNode.Name, nodeInIndex, tAssetOut);
                case FPCombineNode combineNode:
                    var portOne = combineNode.GetInputPortByName(FPDialogueGraphValidation.PORT_COMBINE_OPONE);
                    var portTwo = combineNode.GetOutputPortByName(FPDialogueGraphValidation.PORT_COMBINE_OPTWO);
                    var outPort = combineNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    string nodeOne = string.Empty;
                    string nodeTwo = string.Empty;
                    string nodeOut = string.Empty;
                    
                    if (portOne != null)
                    {
                        nodeOne = GetConnectedNodeNameByPort(portOne);
                    }
                    if (portTwo != null)
                    {
                        nodeTwo = GetConnectedNodeNameByPort(portTwo);
                    }
                    if (outPort != null)
                    {
                        nodeOut = GetConnectedNodeNameByPort(outPort);
                    }
                    if (nodeOne == string.Empty || nodeTwo == string.Empty || nodeOut == string.Empty)
                    {
                        Debug.LogError($"Combine node missing names: {nodeOne}, {nodeTwo}, going to {nodeOut}");
                        return null;
                    }
                    else
                    {
                        return new RTCombineNode(combineNode.Name, nodeOne, nodeTwo, nodeOut);
                    }
                case FPOnewayNode onewayNode:
                    var inDirectionPort = onewayNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outDirectionPort = onewayNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (inDirectionPort != null && outDirectionPort != null)
                    {
                        string inNodeName = GetConnectedNodeNameByPort(inDirectionPort);
                        string OutNodeName = GetConnectedNodeNameByPort(outDirectionPort);
                        return new RTOnewayNode(onewayNode.Name, inNodeName, OutNodeName);
                    }
                    return null;
                case SetFPCharacterNode characNode:
                    return ReturnNewCharacterNode(characNode);
                case SetFPResponseNode responseNode:
                    //go through four user prompts
                    //go through four user outputs
                    SetFPSinglePromptNode firstPrompt = null;
                    SetFPSinglePromptNode secondPrompt = null;
                    SetFPSinglePromptNode thirdPrompt = null;
                    SetFPSinglePromptNode fourthPrompt = null;
                    SetFPCharacterNode characterNode = null;
                    FPVisualNode firstOut = null;
                    FPVisualNode secondOut = null;
                    FPVisualNode thirdOut = null;
                    FPVisualNode fourthOut = null;
                    responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_ONE)?.TryGetValue(out firstPrompt);
                    responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_TWO)?.TryGetValue(out secondPrompt);
                    responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_THREE)?.TryGetValue(out thirdPrompt);
                    responseNode.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPT_FOUR)?.TryGetValue(out fourthPrompt);
                    responseNode.GetInputPortByName(FPDialogueGraphValidation.PORT_ACTOR)?.TryGetValue(out characterNode);
                    responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_ONE)?.TryGetValue(out firstOut);
                    responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_TWO)?.TryGetValue(out secondOut);
                    responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_THREE)?.TryGetValue(out thirdOut);
                    responseNode.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_FOUR)?.TryGetValue(out fourthOut);
                   
                    List<RTSinglePromptNode> incomingItems = new List<RTSinglePromptNode>();
                    List<string> outcomingIndex = new List<string>();
                    if (firstPrompt != null && firstOut != null)
                    {

                        incomingItems.Add(ReturnNewPromptNode(firstPrompt));
                        outcomingIndex.Add(firstOut.Name);
                    }
                    if (secondPrompt != null && secondOut != null)
                    {
                        incomingItems.Add(ReturnNewPromptNode(secondPrompt));
                        outcomingIndex.Add(secondOut.Name);
                    }
                    if (thirdPrompt != null && thirdOut != null)
                    {
                        incomingItems.Add(ReturnNewPromptNode(thirdPrompt));
                        outcomingIndex.Add(thirdOut.Name);
                    }
                    if (fourthPrompt != null && fourthOut != null)
                    {
                        incomingItems.Add(ReturnNewPromptNode(fourthPrompt));
                        outcomingIndex.Add(fourthOut.Name);
                    }
                    if (characterNode != null)
                    {

                    }
                    var aCharacter = ReturnNewCharacterNode(characterNode);
                    if (aCharacter != null)
                    {
                        return new RTResponseNode(responseNode.Name, incomingItems, outcomingIndex, aCharacter);
                    }
                    return null;
                case SetFPTalkNode talkieNode:
                    return ReturnNewTalkNode(talkieNode);
                case SetFPDialogueNode dialogueNode:
                    EmotionalState animState = EmotionalState.Neutral;
                    DialogueState dialogueState = DialogueState.Normal;
                    MotionState motionState = MotionState.NA;
                    SetFPTalkNode mainTalk = null;
                    SetFPTalkNode transTalk = null;
                    SetFPCharacterNode charNode = null;
                    SetFPCharacterNode charNodeOut = null;
                    RTTalkNode talkRTNode = null;
                    RTTalkNode talkRTTransNode = null;
                    RTCharacterNode rTCharacterNodeIn = null;
                    RTCharacterNode rtCharacterNodeOut = null;
                    FPVisualNode inputDNode = null;
                    FPVisualNode outputDNode = null;
                    dialogueNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)?.TryGetValue(out inputDNode);
                    dialogueNode.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)?.TryGetValue(out outputDNode);
                    if (inputDNode == null && outputDNode == null)
                    {
                        Debug.LogError($"Missing an input or output connection on a dialogue!");
                        return null;
                    }
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_EMOTION_STATE)?.TryGetValue(out animState);
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_DIALOGUE_STATE)?.TryGetValue(out dialogueState);
                    dialogueNode.GetNodeOptionByName(FPDialogueGraphValidation.ANIM_MOTION_STATE)?.TryGetValue(out motionState);
                    dialogueNode.GetInputPortByName(FPDialogueGraphValidation.PORT_ACTOR)?.TryGetValue(out charNode);
                    dialogueNode.GetOutputPortByName(FPDialogueGraphValidation.PORT_ACTOR)?.TryGetValue(out charNodeOut);
                    dialogueNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_TEXT)?.TryGetValue(out mainTalk);
                    dialogueNode.GetInputPortByName(FPDialogueGraphValidation.TRANSLATION_TEXT)?.TryGetValue(out transTalk);
                    if (mainTalk != null)
                    {
                        talkRTNode=ReturnNewTalkNode(mainTalk);
                    }
                    if (transTalk != null)
                    {
                        talkRTTransNode = ReturnNewTalkNode(transTalk);
                    }
                    if (charNode != null)
                    {
                        rTCharacterNodeIn = ReturnNewCharacterNode(charNode);
                    }
                    if(charNodeOut != null)
                    {
                        rtCharacterNodeOut = ReturnNewCharacterNode(charNodeOut);
                    }
                    return new RTDialogueNode(dialogueNode.Name, inputDNode.Name, outputDNode.Name, talkRTNode,rTCharacterNodeIn,rtCharacterNodeOut, talkRTTransNode);
                
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
            SkinnedMeshRenderer characterMeshR = null;
            FP_Theme characterTheme = null;
            string nodeIndex = nodeData.Name;
           
            string connectingOutNode = string.Empty;
            var actorPortOut = nodeData.GetOutputPortByName(FPDialogueGraphValidation.PORT_ACTOR);
            if (actorPortOut != null)
            {
                connectingOutNode = GetConnectedNodeNameByPort(actorPortOut);
            }
            nodeData.GetNodeOptionByName(FPDialogueGraphValidation.GETDATAFILE)?.TryGetValue(out useCharData);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_NAME)?.TryGetValue(out characterName);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_GENDER)?.TryGetValue(out gender);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_ETH)?.TryGetValue(out eth);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_PRIMARY)?.TryGetValue(out firstL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_SECONDARY)?.TryGetValue(out secondL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_TIERTIARY)?.TryGetValue(out thirdL);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_AGE)?.TryGetValue(out age);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ANIM_SKIN_MESHR)?.TryGetValue(out characterMeshR);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ACTOR_THEME)?.TryGetValue(out characterTheme);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.PORT_CHARACTER_DATA)?.TryGetValue(out charData);
            if (connectingOutNode == string.Empty)
            {
                Debug.LogError("Character data isn't connected to anything - floating in space");
                return null;
            }
            else
            {
                if (useCharData && charData != null)
                {
                    return new RTCharacterNode(nodeIndex, connectingOutNode, charData, characterMeshR, characterTheme, useCharData);
                }
                return new RTCharacterNode(nodeIndex, connectingOutNode, characterName, gender, eth, firstL, secondL, thirdL, age, characterMeshR, characterTheme);
            }
           
        }
        static RTSinglePromptNode ReturnNewPromptNode(SetFPSinglePromptNode nodeData)
        {
            RTSinglePromptNode aNewReturnNode = null;
            SetFPTalkNode talkNodeMain = null;
            SetFPTalkNode talkNodeTranslation = null;
            SetFPSinglePromptNode outNode = null;
            Sprite icon = null;
            GameObject location = null;
            nodeData.GetInputPortByName(FPDialogueGraphValidation.MAIN_TEXT)?.TryGetValue(out talkNodeMain);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.TRANSLATION_TEXT)?.TryGetValue(out talkNodeTranslation);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.PORT_ICON)?.TryGetValue(out icon);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.GO_WORLD_LOCATION)?.TryGetValue(out location);
            nodeData.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPT_PORT)?.TryGetValue(out outNode);
            if (outNode!=null&&talkNodeMain != null&&talkNodeTranslation!=null)
            {
                //go get real talk
                var talkNodeRT = ReturnNewTalkNode(talkNodeMain);
                var translationNodeRT = ReturnNewTalkNode(talkNodeTranslation);
                aNewReturnNode = new RTSinglePromptNode(nodeData.Name,outNode.Name, talkNodeRT, translationNodeRT, icon, location);
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
            SetFPTalkNode outputNode = null;
            string outputNodeIndex = string.Empty;
            nodeData.GetInputPortByName(FPDialogueGraphValidation.LANG_NAME)?.TryGetValue(out nodeLanguage);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_HEADER)?.TryGetValue(out header);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE)?.TryGetValue(out text);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.DIALOGUE_AUDIO_NAME)?.TryGetValue(out textClip);
            nodeData.GetInputPortByName(FPDialogueGraphValidation.ANIM_BLEND_FACE)?.TryGetValue(out animClip);
            nodeData.GetOutputPortByName(FPDialogueGraphValidation.MAIN_TEXT)?.TryGetValue(out outputNode);
            if (outputNode != null)
            {
                outputNodeIndex = outputNode.Name;
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
        static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
        static string GetConnectedNodeNameByPort(IPort port)
        {
            var visualNode = FirstConnectedNodeByPort(port);
            if (visualNode!=null)
            {
                return visualNode.Name;
            }
            return string.Empty;
        }
        /// <summary>
        /// will return the first INode on the other end of a connected iPort
        /// E.g. Node 0 is connected to Node 1, you pass the outgoing port on Node 0 and it returns Node 1
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        static FPVisualNode FirstConnectedNodeByPort(IPort port)
        {
            List<IPort> possiblePorts = new List<IPort>();
            port.GetConnectedPorts(possiblePorts);
            FPVisualNode outNode = null;
            if (possiblePorts.Count > 1)
            {
                Debug.LogError($"Entry Node should only connect to one node");
                outNode = possiblePorts[0].GetNode() as FPVisualNode;
            }
            else
            {
                if (possiblePorts.Count == 1)
                {
                    outNode = possiblePorts[0].GetNode() as FPVisualNode;
                }
            }
            return outNode;
        }
    }
}
