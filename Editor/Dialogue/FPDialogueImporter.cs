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

        void CreateRuntimeNodes(INode startNode, RTFPDialogueGraph runtimeGraph, Dictionary<INode, int> nodeMap)
        {
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
            var returnedNodes = new List<RTFPNode>();
            switch (editorNode)
            {
                case EntryNode entryNode:
                    //have to evalute information on RTEnetryNode (Input Timeline asset)
                    TimelineAsset tAsset = null;
                    entryNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)?.TryGetValue(out tAsset);
                    returnedNodes.Add(new RTEntryNode(tAsset));
                    break;
                case ExitNode exitNode:
                    TimelineAsset tAssetOut = null;
                    exitNode.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)?.TryGetValue(out tAssetOut);
                    returnedNodes.Add(new RTExitNode(tAssetOut));
                    break;
                case FPCombineNodes combineNode:
                    returnedNodes.Add(new RTCombineNode());
                    break;
                case SetFPCharacterNode characterNode:
                    string characterName = "blank";
                    FP_Gender gender = FP_Gender.NA;
                    FP_Ethnicity eth = FP_Ethnicity.Unknown;
                    FP_Language firstL = FP_Language.NA;
                    FP_Language secondL = FP_Language.NA;
                    FP_Language thirdL = FP_Language.NA;
                    int age = -10;
                    SkinnedMeshRenderer characterMeshR = null;
                    FP_Theme characterTheme = null;
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_NAME)?.TryGetValue(out characterName);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_GENDER)?.TryGetValue(out gender);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_ETH)?.TryGetValue(out eth);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_PRIMARY)?.TryGetValue(out firstL);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_SECONDARY)?.TryGetValue(out secondL);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_LANGUAGES_TIERTIARY)?.TryGetValue(out thirdL);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_AGE)?.TryGetValue(out age);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ANIM_SKIN_MESHR)?.TryGetValue(out characterMeshR);
                    characterNode.GetInputPortByName(FPDialogueGraphValidation.ACTOR_THEME)?.TryGetValue(out characterTheme);
                    returnedNodes.Add(new RTCharacterNode(characterName, gender, eth, firstL, secondL, thirdL, age, characterMeshR, characterTheme));
                    break;
                case SetFPResponseNode:
                    break;
                case SetFPDialogueNode:
                    break;
                case SetFPTalkNode:
                    break;
                default:
                    throw new NotImplementedException();
            }
            return returnedNodes;
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
    }
}
