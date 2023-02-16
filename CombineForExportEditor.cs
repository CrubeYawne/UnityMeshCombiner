using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombineForExport))]
public class CombineForExportEditor : Editor
{
    private const int HEADER_SPACE_PX = 20;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUIStyle headStyle = new GUIStyle() { fontStyle = FontStyle.Bold};

        GUILayout.Space(HEADER_SPACE_PX);

        GUILayout.Label("1. Prepare", headStyle);
        
        if(GUILayout.Button("Cleanup Bad Materials"))
        {
            var localT = (CombineForExport)target;

            localT.CleanupBadSubMaterials();
        }

        GUILayout.Space(HEADER_SPACE_PX);
        GUILayout.Label("2. Generate", headStyle);

        if(GUILayout.Button("Combine Filter Target (Single Material Mode)"))
        {
            var localT = (CombineForExport)target;

            localT.CombineTarget();
        }

        if(GUILayout.Button("Combine Filter Target Sub Meshes"))
        {
            var localT = (CombineForExport)target;

            localT.CombineTargetSubMesh();
        }

        

         if(GUILayout.Button("Combine Generated"))
        {
            var localT = (CombineForExport)target;

            localT.CombineGenerated();
        }

        GUILayout.Space(HEADER_SPACE_PX);
        GUILayout.Label("3. Verify", headStyle);

         if(GUILayout.Button("Check Generated"))
        {
            var localT = (CombineForExport)target;

            localT.CheckGenerated();
        }

        GUILayout.Space(HEADER_SPACE_PX);
        GUILayout.Label("4. Cleanup", headStyle);

        if(GUILayout.Button("Remove Generated"))
        {
            var localT = (CombineForExport)target;

            localT.RemoveGenerated();
        }

       
    }
}
