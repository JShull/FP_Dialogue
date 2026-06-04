// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPSinglePromptNode: FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(FPDialogueGraphValidation.GAMEOBJECT_ID)
               .WithDisplayName("GameObject Binding Id")
               .WithDefaultValue(string.Empty);
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_TEXT)
                .WithDisplayName("User Prompt")
                .Build();
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.TRANSLATION_TEXT)
                .WithDisplayName("User Prompt Translation")
                .Build();
            context.AddInputPort<Sprite>(FPDialogueGraphValidation.PORT_ICON)
                .WithDisplayName("Icon")
                .Build();
            //context.AddInputPort<GameObject>(FPDialogueGraphValidation.GO_WORLD_LOCATION)
            //    .WithDisplayName("Spawn Location")
            //    .Build();
            context.AddOutputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_PORT)
                .WithDisplayName("Response Node?")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
