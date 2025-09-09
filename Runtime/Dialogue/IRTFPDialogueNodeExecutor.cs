namespace FuzzPhyte.Dialogue
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    public interface IRTFPDialogueNodeExecutor<in TNode> where TNode : RTFPNode
    {
        bool Execute(TNode node, RTDialogueDirector context);
    }
    public class EntryNodeExecutor : IRTFPDialogueNodeExecutor<RTEntryNode>
    {
        public bool Execute(RTEntryNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if(mediator == null)
            {
                Debug.LogError($"EntryExecutor:No Mediator found on our director!");
                return false;
            }
            Debug.Log("Starting Dialogue");
            bool condition = mediator.EvaluateEntryNode(node);
            Debug.Log($"Evaluating entry node ID: {node.Index} with returned {condition}");
            return condition;
        }
    }
    public class ExitNodeExecutor: IRTFPDialogueNodeExecutor<RTExitNode>
    {
        public bool Execute(RTExitNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"ExitExecutor:No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Exiting Dialogue");
            return true;
        }
    }
    public class OnewayExecutor : IRTFPDialogueNodeExecutor<RTOnewayNode>
    {
        public bool Execute(RTOnewayNode node, RTDialogueDirector context) 
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"OneWayExecutor:No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Oneway Dialogue!");
            return true;
        }
    }
    public class CombineNodeExecutor : IRTFPDialogueNodeExecutor<RTCombineNode>
    {
        public bool Execute(RTCombineNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Piping towards a single output!");
            bool condition = mediator.EvaluateCombination(node);
            return condition;
        }
    }
    public class CharacterNodeExecutor : IRTFPDialogueNodeExecutor<RTCharacterNode>
    {
        public bool Execute(RTCharacterNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Character Node Execution!");
            return true;
        }
    }
    public class ResponseNodeExecutor: IRTFPDialogueNodeExecutor<RTResponseNode>
    {
        public bool Execute(RTResponseNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Response Node Execution!");
            bool condition = mediator.EvaluateResponseNode(node);
            return condition;
        }
    }
    public class SinglePromptNodeExecutor: IRTFPDialogueNodeExecutor<RTSinglePromptNode>
    {
        public bool Execute(RTSinglePromptNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"SinglePromptExecutor: No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Single Prompt Node Execution!");
            return true;
        }
    }
    public class DialogueNodeExecutor : IRTFPDialogueNodeExecutor<RTDialogueNode>
    {
        public bool Execute(RTDialogueNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Dialogue Node Execution!");
            bool condition = mediator.EvaluateDialogueNode(node);
            return condition;
        }
    }
    public class TalkNodeExecutor: IRTFPDialogueNodeExecutor<RTTalkNode>
    {
        public bool Execute(RTTalkNode node, RTDialogueDirector context)
        {
            var mediator = context.GetComponent<RTDialogueMediator>();
            if (mediator == null)
            {
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Talking Node information execution!");
            return true;
        }
    }
}
