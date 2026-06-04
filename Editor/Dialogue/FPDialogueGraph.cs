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
    using System.Collections.Generic;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;
    using System;
    using FuzzPhyte.Utility;
    using System.Linq;
    using UnityEngine.Timeline;

    [Serializable]
    [Graph(AssetExtension)]
    public class FPDialogueGraph:Graph
    {
        //const string k_graphName = "Dialogue Graph";
        public const string AssetExtension = "fpdialogue";
        [SerializeField] public string dialogueBaseGuid;

        public override void OnGraphChanged(GraphLogger logger)
        {

            base.OnGraphChanged(logger);
            Debug.Log($"FP Graph Data Changed");
            //get all FPVisual Nodes - might not need to do this here maybe only on importer
            var nodeList = GetNodes().OfType<FPVisualNode>().ToList() ;
            
            //setup our node names
            for (int i = 0; i< nodeList.Count; i++)
            {
                var cNode = nodeList[i] ;
                cNode.SetupIndex(i + "_node");
                //Debug.Log($"Node: {cNode.Name} is alive");
            }
           
            //process output based on node type and "flow nodes" - this might be on the importer class instead
            /// Entry
            /// SetFPDialogueNode
            /// SetFPResponseNode
            /// FPCombineNode
            /// FPOnewayNode
            /// Exit
            FPDialogueGraphValidation.Run(this, logger);
        }
    }
}
