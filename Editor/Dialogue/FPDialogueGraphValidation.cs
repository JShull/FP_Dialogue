namespace FuzzPhyte.Dialogue.Editor
{
    using System.Linq;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    using FuzzPhyte.Utility;
    using System.Collections.Generic;
    public static class FPDialogueGraphValidation
    {
        // graph based
        public const string GRAPHID = "GraphID";
        public const string MAIN_PORT_DEFAULT_NAME = "ExecutionPort";
        public const string MAIN_PORT_TIMELINE = "TimelinePort";
        public const string MAIN_PORT_TIMELINEDETAILS = "TimelineDetails";
        public const string PORT_COMBINE_OPONE = "Option1";
        public const string PORT_COMBINE_OPTWO = "Option2";
        public const string PORT_NUMBER_OPTIONS = "NumOptions";
        public const string PORT_INDEX_OP = "Option_";//builder string
        
        //options based
        public const string GETDATAFILE = "GetDataFile";

        // scriptable object based
        public const string PORT_CHARACTER_DATA = "FPCharacter";
        public const string ACTOR_THEME = "Theme";

        // character based
        public const string ACTOR_NAME = "Name";
        public const string ACTOR_GENDER = "Gender";
        public const string ACTOR_ETH = "Ethnicity";
        public const string ACTOR_LANGUAGES_PRIMARY = "Primary";
        public const string ACTOR_LANGUAGES_SECONDARY = "Secondary";
        public const string ACTOR_LANGUAGES_TIERTIARY = "Tertiary";
        public const string ACTOR_AGE = "Age";
        public const string GO_WORLD_LOCATION = "GameObjectLocation";

        //port character based
        public const string PORT_ACTOR = "CharacterPort";
        public const string TALK_PORT_NODE = "TalkPort";
        
        //Language Based
        public const string LANG_NAME = "Language";
        public const string DIALOGUE = "Dialogue";
        public const string DIALOGUE_AUDIO_NAME = "DialogueAudio";

        //Text Blocks
        public const string DIALOGUE_HEADER = "HeaderText";
        public const string MAIN_TEXT = "MainText";
        public const string TRANSLATION_TEXT = "TranslationText";
        public const string PORT_ICON = "PortIcon";

        //Animation Based
        public const string ANIM_EMOTION_STATE = "AnimationEmotion";
        public const string ANIM_DIALOGUE_STATE = "AnimationDialogue";
        public const string ANIM_MOTION_STATE = "AnimationMotion";
        public const string ANIM_BLEND_FACE = "AnimationFace";
        public const string ANIM_BLEND_BODY = "AnimationBody";
        public const string GAMEOBJECT_ID = "GameObjectID";
        public const string GAMEOBJECT_BLENDSHAPE = "BlendShape";
        
        //User Choices - Response based (prompts as well)
        public const string USER_PROMPT_PORT = "PromptExecutionOut";
        public const string USER_NUMBER_OPTIONS = "PromptNumOptions";
        public const string USER_PROMPTX_OP = "PromptOption_";
        public const string USER_WAIT_FOR_USER = "WaitForUserResponse"; //wait for user response or just go to the next dialogue automatically
        
        //Graph/User parameters that drive our realtime outcomes
        public const string USE_THREED_OBJECTS = "UseWorldObjects"; //false is traditional 2D
        public const string USE_PREFABS = "UsePrefabs"; //false we point to game object locations in scene
        public const string USE_WORLD_LOCATION = "UseDialogueWorldLocation";

        public const string DIALOGUE_TIMELINE_OUT = "DialogueTimelineOut";
        public const string DIALOGUE_UI_PANEL = "DialogueUIPanel";
        public const string DIALOGUE_UI_BUTTON = "DialogueUIButton";
        public const string RESPONSE_PREFAB_YES = "YesGameObjectPrefab";
        public const string RESPONSE_PREFAB_NO = "NoGameObjectPrefab";
        public const string RESPONSE_WORLD_YES_LOCATION = "YesWorldLocationName";
        public const string RESPONSE_WORLD_NO_LOCATION = "NoWorldLocationName";

        public static void Run(FPDialogueGraph graph, GraphLogger logger)
        {
            // 1) Exactly one EntryNode
            var entries = graph.GetNodes().OfType<EntryNode>().ToList();
            if (entries.Count == 0)
            {
                logger.LogError("No EntryNode in graph.", graph);
            }
            else if (entries.Count > 1)
            {
                logger.LogWarning("Multiple EntryNodes found. Only one will work.", graph);
            }else if (entries.Count == 1)
            {
                var entryNode = entries[0];
                var graphIDOption = entryNode.GetNodeOptionByName(FPDialogueGraphValidation.GRAPHID);
                if (graphIDOption != null)
                {
                    graphIDOption.TryGetValue<string>(out string gIDValue);
                    if(gIDValue == string.Empty)
                    {
                        logger.LogWarning($"Need a graphID!", entryNode);
                    }
                }
            }


            foreach (var n in graph.GetNodes().OfType<SetFPDialogueNode>())
                {
                    var inPort = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    var outPort = n.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                    if (inPort == null || !inPort.isConnected)
                    {
                        logger.LogWarning($"'{n.Name}' has no incoming connection.", n);
                    }
                    else
                    {
                        List<IPort> possibleIncomingPorts = new();
                        inPort.GetConnectedPorts(possibleIncomingPorts);
                        if (possibleIncomingPorts.Count > 1)
                        {
                            logger.LogWarning($"'{n.Name}' has too many incoming main connections, should only be one dialogue flow in", n);
                        }
                    }
                    if (outPort == null)
                    {

                    }
                    else
                    {
                        List<IPort> possibleOutgoingPorts = new();

                        outPort.GetConnectedPorts(possibleOutgoingPorts);

                        if (possibleOutgoingPorts.Count > 1)
                        {
                            logger.LogWarning($"'{n.Name}' has too many outgoing main connections, should only be one dialogue flow out", n);
                        }
                    }

                }
            foreach(var n in graph.GetNodes().OfType<SetFPCharacterNode>())
            {
                var inPort = n.GetInputPortByName(ACTOR_LANGUAGES_PRIMARY);
                FP_Language portLanguage = FP_Language.NA;
                inPort.TryGetValue<FP_Language>(out portLanguage);
               
                if(portLanguage== FP_Language.NA)
                {
                    logger.LogWarning($"'{n.Name}' character is missing a first language", n);
                }
            }
            foreach(var n in graph.GetNodes().OfType<FPOnewayNode>())
            {
                var inPort = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                var outPort = n.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                if(inPort==null || !inPort.isConnected)
                {
                    logger.LogWarning($"'{n.Name}' has no incoming connection",n);
                }
                
                if(outPort==null || !outPort.isConnected)
                {
                    logger.LogWarning($"'{n.Name}' has no output connection",n);
                }
                if (n.outputPortCount>1)
                {
                    logger.LogWarning($"'{n.Name}'has too many output connections, should only be 1",n);
                }
                if (n.inputPortCount > 1)
                {
                    logger.LogWarning($"'{n.Name}'has too input connections, should only be 1",n);
                }
            }
            foreach(var n in graph.GetNodes().OfType<SetFPSinglePromptNode>())
            {
                var mainTextIn = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
                var mainTranslationIn = n.GetInputPortByName(FPDialogueGraphValidation.TRANSLATION_TEXT);
                if (mainTextIn == null || mainTranslationIn == null){
                    logger.LogWarning($" {n.Name} has no text coming in!",n);
                }
                if(mainTextIn!=null && mainTranslationIn == null)
                {
                    logger.LogWarning($" {n.Name} missing translation text!",n);
                }
                if(mainTextIn==null && mainTranslationIn != null)
                {
                    logger.LogWarning($" {n.Name} missing main text!",n);
                }
            }
            foreach(var n in graph.GetNodes().OfType<SetFPTalkNode>())
            {
                var mainTextOut = n.GetOutputPortByName(FPDialogueGraphValidation.MAIN_TEXT);
                if (mainTextOut != null)
                {
                    if (mainTextOut.firstConnectedPort == null)
                    {
                        logger.LogWarning($" {n.Name} missing an outward connection!",n);
                    }
                }
            }
            foreach (var n in graph.GetNodes().OfType<SetFPResponseNode>())
            {
                var numPromptCount = n.GetNodeOptionByName(FPDialogueGraphValidation.USER_NUMBER_OPTIONS);
                numPromptCount.TryGetValue<int>(out var numPrompts);
                for (int i = 0; i < numPrompts; i++)
                {
                    //check input and output are both connected
                    var inputPrompt = n.GetInputPortByName(FPDialogueGraphValidation.USER_PROMPTX_OP + i.ToString()).isConnected;
                    var outPutResponse = n.GetOutputPortByName(FPDialogueGraphValidation.USER_PROMPTX_OP + i.ToString()).isConnected;
                    if (inputPrompt != outPutResponse)
                    {
                        logger.LogWarning($"{n.Name} input/output doesn't match for {i + 1} port! ", n);
                    }
                }
            }
            foreach(var n in graph.GetNodes().OfType<ExitNode>())
            {
                //check if we have both a TimelineAsset and a Timeline Details
                var inputBoolTimelineAsset = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINE).isConnected;
                var inputTimelineDetailsPort = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS);
                var inputBoolTimelineDetails = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS).isConnected;

                if (inputTimelineDetailsPort != null && inputBoolTimelineAsset)
                {
                    RTTimelineDetails details = null;
                    inputTimelineDetailsPort.TryGetValue<RTTimelineDetails>(out details);
                    if (details != null)
                    {
                        logger.LogWarning($"{n.Name} shouldn't have both timeline asset and timeline details, just use one", n);
                    }
                }
                else if(inputBoolTimelineAsset && inputBoolTimelineDetails)
                {
                    logger.LogWarning($"{n.Name} shouldn't have both timeline asset and timeline details, just use one", n);
                }                
            }
        }
    }
}
