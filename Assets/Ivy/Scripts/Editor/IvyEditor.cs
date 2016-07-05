using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Ivy
{
    [CustomEditor(typeof(IvyManager))]
    public class IvyEditor : Editor
    {
        //define reference we want to use/control
        private IvyManager script;

        //enables 2D mode placement (auto-detection)
        private bool mode2D = false;

        //the count of alive IvyGenerator 's roots
        private uint livingbranch = 0;

        //ivy vegetable is growing?
        private bool IsGrowing = false;
        private bool IsBirth = false;

        //editor group
        private bool showGrowingEditor = true;
        private bool showBirthEditor = true;

        // editor Coroutine
        private FreeEditorCoroutines.EditorCoroutine m_Timer = null;

        // export obj path + file
        private string ObjExportPath = @"Assets\Ivy\Examples\";
        private string ObjExportFile = "IvyExportObj";

        public void OnSceneGUI()
        {
            //with creation mode enabled, place new root on keypress
            if (Event.current.type != EventType.keyDown) 
                return;
            
            if (Event.current.keyCode == KeyCode.P)
            {
                //cast a ray against mouse position
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;

                //2d placement
                if (mode2D)
                {
                    Event.current.Use();
                    //convert screen to 2d position
                    Vector3 pos2D = worldRay.origin;
                    pos2D.z = 0;

                    //place a root at clicked point
                    PlaceRoot(pos2D);
                }
                //3d placement
                else
                {
                    if (Physics.Raycast(worldRay, out hitInfo))
                    {
                        Event.current.Use();

                        //place a root at clicked point
                        PlaceRoot(hitInfo.point);
                        script.IvyRoot.transform.parent = hitInfo.transform;
                    }
                    else
                    {
                        Debug.LogWarning("Ivy Manager: 3D Mode. Trying to place a root but couldn't "
                                         + "find valid target. Have you clicked on a collider?");
                    }
                }
            }

        }



        //called whenever the inspector gui gets rendered
        public override void OnInspectorGUI()
        {
            //show default variables in inspector
            DrawDefaultInspector();
            //get manager reference
            script = (IvyManager)target;

            //get sceneview to auto-detect 2D mode
            SceneView view = GetSceneView();
            mode2D = view.in2DMode;

            //plant root creation button
            if (GUILayout.Button("Start Plant", GUILayout.Height(40)))
            {
                //focus sceneview for placement
                view.Focus();
            }

            //plant instructions
            GUI.backgroundColor = Color.white;
            GUILayout.TextArea("Hint:\nPress 'Start Plant' to begin a new ivy root, then press"
                            + " 'p' on your keyboard to place new root in the SceneView. "
                            + "In 3D Mode you have to place root onto objects with colliders."
                            + " You can only place one root at the scene view position.");
            EditorGUILayout.Space();


            if (script.IvyGenerator.roots.Count == 0 || script.IvyGenerator.roots.Count == 1)
                EditorGUILayout.LabelField("living branches : ", "0");
            else
                EditorGUILayout.LabelField("living branches : ", livingbranch.ToString() + " of " + script.IvyGenerator.roots.Count.ToString());

            showGrowingEditor = EditorGUILayout.ToggleLeft("growing setting", showGrowingEditor);
            if (EditorGUILayout.BeginFadeGroup(showGrowingEditor ? 1 : 0))
            {
                GUIContent content = null;
                EditorGUILayout.Space();
                
                content = new GUIContent("Ivy Size : ", "adapts the ivy growing and geometry to the scene size and content");
                script.IvyGenerator.ivySize = EditorGUILayout.Slider(content, script.IvyGenerator.ivySize, 0, 0.05f);
                ProcessBar(script.IvyGenerator.ivySize * 20000.0f / 1000.0f, "Ivy Size");

                EditorGUILayout.Space();
                content = new GUIContent("Primary Weight : ", "defines the weight of the primary growing direction");
                script.IvyGenerator.primaryWeight = EditorGUILayout.Slider(content, script.IvyGenerator.primaryWeight, 0, 1f);
                ProcessBar(script.IvyGenerator.primaryWeight, "Primary Weight");

                EditorGUILayout.Space();
                content = new GUIContent("Random Weight : ", "defines the weight of a random growing direction");
                script.IvyGenerator.randomWeight = EditorGUILayout.Slider(content, script.IvyGenerator.randomWeight, 0, 1f);
                ProcessBar(script.IvyGenerator.randomWeight, "Random Weight");

                EditorGUILayout.Space();
                content = new GUIContent("Gravity Weight : ", "defines the weight of gravity");
                script.IvyGenerator.gravityWeight = EditorGUILayout.Slider(content, script.IvyGenerator.gravityWeight, 0, 2f);
                ProcessBar(script.IvyGenerator.gravityWeight * 0.5f, "Gravity Weight");

                EditorGUILayout.Space();
                content = new GUIContent("Adhesion Weight : ", "defines the weight of adhesion towards attracting surfaces");
                script.IvyGenerator.adhesionWeight = EditorGUILayout.Slider(content, script.IvyGenerator.adhesionWeight, 0, 1f);
                ProcessBar(script.IvyGenerator.adhesionWeight, "Adhesion Weight");

                EditorGUILayout.Space();
                content = new GUIContent("Branch Probability : ", "defines the density of branching structure during growing");
                script.IvyGenerator.branchingProbability = EditorGUILayout.Slider(content, script.IvyGenerator.branchingProbability, 0, 1f);
                ProcessBar(script.IvyGenerator.branchingProbability, "Branch Probability");

                EditorGUILayout.Space();
                content = new GUIContent("Max Float Length : ", "defines the length at which a freely floating branch will die");
                script.IvyGenerator.maxFloatLength = EditorGUILayout.Slider(content, script.IvyGenerator.maxFloatLength, 0, 1f);
                ProcessBar(script.IvyGenerator.maxFloatLength, "Max Float Length");

                EditorGUILayout.Space();
                content = new GUIContent("Max Adhesion Dist : ", "defines the maximum distance to a surface at which the surface will attract the ivy");
                script.IvyGenerator.maxAdhesionDistance = EditorGUILayout.Slider(content, script.IvyGenerator.maxAdhesionDistance, 0, 1f);
                ProcessBar(script.IvyGenerator.maxAdhesionDistance, "Max Adhesion Dist");
            }
            EditorGUILayout.EndFadeGroup();

            if (script.IvyRoot != null && GUILayout.Button(IsGrowing ? "Pause Grow" : "Start Grow", GUILayout.Height(40)))
            {
                
                // Pause Ivy growing
                if (IsGrowing)
                {
                    IsGrowing = false;
                    if (m_Timer != null)
                        m_Timer.Stop();                    
                }
                else
                {
                    IsGrowing = true;
                    if (IsBirth)
                    {
                        IsBirth = false;
                        script.DeleteOldIvyObject();
                    }

                    if (m_Timer == null)
                    {
                        IvyManager.SceneObjMesh.reset();

                        script.StartGrow();

                        m_Timer = FreeEditorCoroutines.EditorCoroutine.StartCoroutine(AlwaysGrowing());
                    }
                    else
                    {
                        script.IvyGenerator.reset();
                        m_Timer = FreeEditorCoroutines.EditorCoroutine.StartCoroutine(AlwaysGrowing());
                    }

                }

                //focus sceneview for placement
                view.Focus(); 
            }


            showBirthEditor = EditorGUILayout.ToggleLeft("birth setting", showBirthEditor);
            if (EditorGUILayout.BeginFadeGroup(showBirthEditor ? 1 : 0))
            {
                GUIContent content = null;
                EditorGUILayout.Space();

                content = new GUIContent("Ivy Branch Size : ", "defines the diameter of the branch geometry relative to the ivy size");
                script.IvyGenerator.ivyBranchSize = EditorGUILayout.Slider(content, script.IvyGenerator.ivyBranchSize, 0, 0.5f);
                ProcessBar(script.IvyGenerator.ivyBranchSize * 2000.0f / 1000.0f, "Ivy Branch Size");

                EditorGUILayout.Space();
                content = new GUIContent("Ivy Leaf Size : ", "defines the diameter of the leaf geometry relative to the ivy size");
                script.IvyGenerator.ivyLeafSize = EditorGUILayout.Slider(content, script.IvyGenerator.ivyLeafSize, 0, 2f);
                ProcessBar(script.IvyGenerator.ivyLeafSize * 0.5f, "Ivy Leaf Size");

                EditorGUILayout.Space();
                content = new GUIContent("Leaf Probability : ", "defines the density of the leaves during geometry generation");
                script.IvyGenerator.leafProbability = EditorGUILayout.Slider(content, script.IvyGenerator.leafProbability, 0, 1f);
                ProcessBar(script.IvyGenerator.leafProbability, "Ivy Leaf Size");

                EditorGUILayout.Space();
                script.branchTex = EditorGUILayout.ObjectField("branch Tex", script.branchTex, typeof(Texture), true) as Texture;
                IvyManager.branchTexPathName = UnityEditor.AssetDatabase.GetAssetPath(script.branchTex);
                EditorGUILayout.Space();
                script.Leaf0Tex = EditorGUILayout.ObjectField("LeafAdult Tex", script.Leaf0Tex, typeof(Texture), true) as Texture;
                IvyManager.LeafAdultTexPathName = UnityEditor.AssetDatabase.GetAssetPath(script.Leaf0Tex);
                EditorGUILayout.Space();
                script.Leaf1Tex = EditorGUILayout.ObjectField("LeafYoung Tex", script.Leaf1Tex, typeof(Texture), true) as Texture;
                IvyManager.LeafYoungTexPathName = UnityEditor.AssetDatabase.GetAssetPath(script.Leaf1Tex);
            }
            EditorGUILayout.EndFadeGroup();
            if (script.IvyRoot != null && GUILayout.Button("Start Birth", GUILayout.Height(40)))
            {
                if (IsGrowing)
                {
                    IsGrowing = false;
                    if (m_Timer != null)
                        m_Timer.Stop();
                }

                IsBirth = true;
                IsGrowing = false;
                script.StartBirth();

                //focus sceneview for placement
                view.Focus();

                // Refresh Inspector
                Repaint();
            }

            // Export setting
            if (script.IvyRoot != null && IsBirth)
            {
                EditorGUILayout.Space();
                ObjExportPath = EditorGUILayout.TextField("Obj Export Path:", ObjExportPath);
                ObjExportFile = EditorGUILayout.TextField("Obj File Name :", ObjExportFile);
                if (GUILayout.Button("Export OBJ", GUILayout.Height(40)))
                {
                    script.ExportOBJFile(ObjExportPath, ObjExportFile);
                }
            }
        }

        /// <summary>
        /// Gets the active SceneView or creates one.
        /// </summary>
        public static SceneView GetSceneView()
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null)
                view = EditorWindow.GetWindow<SceneView>();

            return view;
        }

        private void PlaceRoot(Vector3 placePos)
        {
            //instantiate root gameobject
            if (script.IvyRoot == null)
            {
                script.IvyRoot = new GameObject("IvyRoot");
                Debug.LogWarning("Ivy Manager: Create IvyRoot at position : " + placePos.ToString());
            }
            //position current waypoint at clicked position in scene view
            if (mode2D) placePos.z = 0f;
            script.IvyRoot.transform.position = placePos;

            script.DeleteOldIvyObject();
            script.SetDrawGizmos();

            // seed Ivy Generator
            script.IvyGenerator.seed(script.IvyRoot.transform.position);
                    
            //reset
            livingbranch = 0;
            IsGrowing = false;
            IsBirth = false;
            m_Timer = null;

            // Refresh Inspector
            Repaint();
        }

        private IEnumerator AlwaysGrowing()
        {
            while (IsGrowing)
            {
                script.IvyGenerator.grow();

                livingbranch = 0;
                foreach (var root in script.IvyGenerator.roots)
                {
                    if (root.alive)
                        livingbranch++;
                }

                yield return new FreeEditorCoroutines.WaitForSeconds(0.0333f);

                if (livingbranch == 0)
                    IsGrowing = false;
                else
                    Repaint();

                GetSceneView().Repaint();
            }
        }

        void ProcessBar(float value, string label)
        {
            Rect tect = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(tect, value, label);
            EditorGUILayout.Space();
        }

    }
}