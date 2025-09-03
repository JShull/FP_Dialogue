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
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log("Starting Dialogue");
            bool condition = mediator.EvaluateCondition(node);
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
                Debug.LogError($"No Mediator found on our director!");
                return false;
            }
            Debug.Log($"Exitting Dialogue");
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
            return true;
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
            return true;
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
