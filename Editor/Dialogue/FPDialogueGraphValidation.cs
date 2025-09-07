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
        public const string MAIN_PORT_DEFAULT_NAME = "ExecutionPort";
        public const string MAIN_PORT_TIMELINE = "TimelinePort";
        public const string MAIN_PORT_TIMELINEDETAILS = "TimelineDetails";
        public const string PORT_COMBINE_OPONE = "Option1";
        public const string PORT_COMBINE_OPTWO = "Option2";

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
        public const string ANIM_SKIN_MESHR = "GameObjectWithRenderer";
        public const string ANIM_SKIN_DISPLAY = "GameObject W Renderer";
        //User Choices
        public const string USER_PROMPT_PORT = "UPGeneric";
        public const string USER_PROMPT_ONE = "UPOne";
        public const string USER_PROMPT_TWO = "UPTwo";
        public const string USER_PROMPT_THREE = "UPThree";
        public const string USER_PROMPT_FOUR = "UPFour";
        public static void Run(FPDialogueGraph graph, GraphLogger logger)
        {
            // 1) Exactly one EntryNode
            var entries = graph.GetNodes().OfType<EntryNode>().ToList();
            if (entries.Count == 0)
                logger.LogError("No EntryNode in graph.", graph);
            else if (entries.Count > 1)
                logger.LogWarning("Multiple EntryNodes found. Only one is recommended.", graph);

            // 2) Every DialogueBlockNode must have input connected
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
                        logger.LogWarning($"'{n.Name}' has too many incoming main connections, should only be one dialogue flow in");
                    }
                }
                if(outPort == null)
                {

                }
                else
                {
                    List<IPort> possibleOutgoingPorts = new();

                    outPort.GetConnectedPorts(possibleOutgoingPorts);

                    if (possibleOutgoingPorts.Count > 1)
                    {
                        logger.LogWarning($"'{n.Name}' has too many outgoing main connections, should only be one dialogue flow out");
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
                    logger.LogWarning($"'{n.Name}' character is missing a first language");
                }
            }
            foreach(var n in graph.GetNodes().OfType<FPOnewayNode>())
            {
                var inPort = n.GetInputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                var outPort = n.GetOutputPortByName(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME);
                if(inPort==null || !inPort.isConnected)
                {
                    logger.LogWarning($"'{n.Name}' has no incoming connection");
                }
                
                if(outPort==null || !outPort.isConnected)
                {
                    logger.LogWarning($"'{n.Name}' has no output connection");
                }
                if (n.outputPortCount>1)
                {
                    logger.LogWarning($"'{n.Name}'has too many output connections, should only be 1");
                }
                if (n.outputPortCount > 1)
                {
                    logger.LogWarning($"'{n.Name}'has too input connections, should only be 1");
                }
            }
            
        }
    }
}
