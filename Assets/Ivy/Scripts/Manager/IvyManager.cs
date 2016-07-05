
using System.Collections;
using UnityEngine;

namespace Ivy
{
    public class IvyManager : MonoBehaviour
    {
        public Ivy IvyGenerator { get { return m_IvyGenerator; } }
        private Ivy m_IvyGenerator = new Ivy();

        static public BasicMesh SceneObjMesh { get { return m_SceneObj; } }
        static private BasicMesh m_SceneObj = new BasicMesh();

        // ivy root gameobject
        public GameObject IvyRoot { get; set; }

        // Texture
        public Texture branchTex { get; set; }
        public Texture Leaf0Tex { get; set; }
        public Texture Leaf1Tex { get; set; }

        // Texture file name
        static public string branchTexPathName { get; set; }
        static public string LeafAdultTexPathName { get; set; }
        static public string LeafYoungTexPathName { get; set; }

        public void SetDrawGizmos() { m_DoNotDrawGizmos = false; }
        private bool m_DoNotDrawGizmos = false;

        void Awake()
        {

        }        

        void Update()
        {

        }



        public void StartGrow()
        {
            if (IvyRoot.transform.parent == null)
                return;

            m_DoNotDrawGizmos = false;

            Mesh _mesh = IvyRoot.transform.parent.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] _vertices = _mesh.vertices;
            Vector2[] _UVs = _mesh.uv;
            Vector3[] _normals = _mesh.normals;
            int[] _triangles = _mesh.triangles;

            //Debug.Log(_vertices.Length + "   " + _UVs.Length + "    " + _normals.Length + "    " + _triangles.Length);

            for (int i = 0; i < _mesh.vertexCount; i++)
            {
                SceneObjMesh.vertices.Add(new BasicVertex(_vertices[i]));
            }
            if (_mesh.normals.Length > 0)
            {
                for (int j = 0; j < _mesh.normals.Length; j++)
                {
                    SceneObjMesh.normals.Add(new BasicNormal(_normals[j]));
                }
            }
            else
            {
                for (int j = 0; j < _mesh.vertexCount; j++)
                {
                    SceneObjMesh.normals.Add(new BasicNormal());
                }
            }
            if (_mesh.uv.Length > 0)
            {
                for (int k = 0; k < _mesh.uv.Length; k++)
                {
                    SceneObjMesh.texCoords.Add(new BasicTexCoord(_UVs[k]));
                }
            }
            else
            {
                for (int k = 0; k < _mesh.vertexCount; k++)
                {
                    SceneObjMesh.texCoords.Add(new BasicTexCoord());
                }
            }
            for (int t = 0; t < _triangles.Length / 3; t++)
            {
                BasicTriangle temptri = new BasicTriangle();
                temptri.v0id = (uint)_triangles[t * 3] + 1;
                temptri.v1id = (uint)_triangles[t * 3 + 1] + 1;
                temptri.v2id = (uint)_triangles[t * 3 + 2] + 1;
                temptri.n0id = (uint)_triangles[t * 3] + 1;
                temptri.n1id = (uint)_triangles[t * 3 + 1] + 1;
                temptri.n2id = (uint)_triangles[t * 3 + 2] + 1;
                temptri.t0id = (uint)_triangles[t * 3] + 1;
                temptri.t1id = (uint)_triangles[t * 3 + 1] + 1;
                temptri.t2id = (uint)_triangles[t * 3 + 2] + 1;

                SceneObjMesh.triangles.Add(temptri);
            }

            SceneObjMesh.prepareData();
            SceneObjMesh.calculateVertexNormals();
            SceneObjMesh.prepareData();
        }

        public void DeleteOldIvyObject()
        {
            GameObject oldRoot = GameObject.Find("IvyGenObject");
            if (oldRoot != null)
                DestroyImmediate(oldRoot);
 
        }

        public void StartBirth()
        {
            m_DoNotDrawGizmos = true;

            IvyGenerator.birth();

            // delete old birth Ivy object
            DeleteOldIvyObject();

            // Create root object
            GameObject rootObj = new GameObject("IvyGenObject");

            // Gererate new Ivy meshrender
            {
                // Branch
                int branchtriangleNum = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 3) branchtriangleNum++;
                }

                int index = 0;
                Mesh _branchMesh = CreateIvyMeshObject(rootObj, "Branch_", index, "Unlit/Texture", branchTex);


                Vector3[] _vertex = new Vector3[Mathf.Min(65000-2, branchtriangleNum * 3)];
                Vector2[] _uv = new Vector2[Mathf.Min(65000 - 2, branchtriangleNum * 3)];
                Vector3[] _normal = new Vector3[Mathf.Min(65000 - 2, branchtriangleNum * 3)];
                int[]     _tri = new int[Mathf.Min(65000 - 2, branchtriangleNum * 3)];

                Debug.LogWarning("Ivy Manager: Create Branch_0 object vertex num is : " + _vertex.Length.ToString());

                int VerIdx = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 3)
                    {
                        _vertex[VerIdx] = tri.v0.pos;
                        _uv[VerIdx] = tri.t0.uv;
                        _normal[VerIdx] = tri.n0.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v1.pos;
                        _uv[VerIdx] = tri.t1.uv;
                        _normal[VerIdx] = tri.n1.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v2.pos;
                        _uv[VerIdx] = tri.t2.uv;
                        _normal[VerIdx] = tri.n2.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;

                        branchtriangleNum--;


                        if (VerIdx >= 65000 - 2)
                        {                              

                            _branchMesh.vertices = _vertex;
                            _branchMesh.uv = _uv;
                            _branchMesh.normals = _normal;
                            _branchMesh.triangles = _tri;

                            _branchMesh.RecalculateNormals();
                            _branchMesh.RecalculateBounds();


                            index++;
                            _branchMesh = CreateIvyMeshObject(rootObj, "Branch_", index, "Unlit/Texture", branchTex);

                            VerIdx = 0;

                            _vertex = new Vector3[Mathf.Min(65000 - 2, branchtriangleNum * 3)];
                            _uv = new Vector2[Mathf.Min(65000 - 2, branchtriangleNum * 3)];
                            _normal = new Vector3[Mathf.Min(65000 - 2, branchtriangleNum * 3)];
                            _tri = new int[Mathf.Min(65000 - 2, branchtriangleNum * 3)];

                            Debug.LogWarning(string.Format("Ivy Manager: Create Branch_{0} object vertex num is : ", index) + _vertex.Length.ToString());

                        }
                        
                    }
                }

                if (VerIdx > 0)
                {
                    _branchMesh.vertices = _vertex;
                    _branchMesh.uv = _uv;
                    _branchMesh.normals = _normal;
                    _branchMesh.triangles = _tri;

                    _branchMesh.RecalculateNormals();
                    _branchMesh.RecalculateBounds();
                }
            }

            {
                // LeafAdult
                int leaf0triangleNum = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 1) leaf0triangleNum++;
                }


                int index = 0;
                Mesh _LeafAdultMesh = CreateIvyMeshObject(rootObj, "LeafAdult_", index, "Transparent/Diffuse", Leaf0Tex);


                Vector3[] _vertex = new Vector3[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                Vector2[] _uv = new Vector2[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                Vector3[] _normal = new Vector3[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                int[] _tri = new int[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];

                Debug.LogWarning(string.Format("Ivy Manager: Create LeafAdult_0 object vertex num is : ") + _vertex.Length.ToString());

                int VerIdx = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 1)
                    {
                        _vertex[VerIdx] = tri.v0.pos;
                        _uv[VerIdx] = tri.t0.uv;
                        _normal[VerIdx] = tri.n0.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v1.pos;
                        _uv[VerIdx] = tri.t1.uv;
                        _normal[VerIdx] = tri.n1.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v2.pos;
                        _uv[VerIdx] = tri.t2.uv;
                        _normal[VerIdx] = tri.n2.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;

                        leaf0triangleNum--;

                        if (VerIdx >= 65000 - 2)
                        {

                            _LeafAdultMesh.vertices = _vertex;
                            _LeafAdultMesh.uv = _uv;
                            _LeafAdultMesh.normals = _normal;
                            _LeafAdultMesh.triangles = _tri;

                            _LeafAdultMesh.RecalculateNormals();
                            _LeafAdultMesh.RecalculateBounds();


                            index++;
                            _LeafAdultMesh = CreateIvyMeshObject(rootObj, "LeafAdult_", index, "Transparent/Diffuse", Leaf0Tex);

                            VerIdx = 0;

                            _vertex = new Vector3[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                            _uv = new Vector2[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                            _normal = new Vector3[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];
                            _tri = new int[Mathf.Min(65000 - 2, leaf0triangleNum * 3)];

                            Debug.LogWarning(string.Format("Ivy Manager: Create LeafAdult_{0} object vertex num is : ", index) + _vertex.Length.ToString());

                        }

                    }
                }

                if (VerIdx > 0)
                {
                    _LeafAdultMesh.vertices = _vertex;
                    _LeafAdultMesh.uv = _uv;
                    _LeafAdultMesh.normals = _normal;
                    _LeafAdultMesh.triangles = _tri;

                    _LeafAdultMesh.RecalculateNormals();
                    _LeafAdultMesh.RecalculateBounds();
                }

            }

            {
                // leaf_young
                int leaf1triangleNum = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 2) leaf1triangleNum++;
                }
                int index = 0;
                Mesh _LeafYoungMesh = CreateIvyMeshObject(rootObj, "LeafYoung_", index, "Transparent/Diffuse", Leaf1Tex);

                Vector3[] _vertex = new Vector3[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                Vector2[] _uv = new Vector2[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                Vector3[] _normal = new Vector3[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                int[] _tri = new int[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];

                Debug.LogWarning(string.Format("Ivy Manager: Create LeafYoung_0 object vertex num is : ") + _vertex.Length.ToString());

                int VerIdx = 0;
                foreach (var tri in IvyGenerator.triangles)
                {
                    if (tri.matid == 2)
                    {
                        _vertex[VerIdx] = tri.v0.pos;
                        _uv[VerIdx] = tri.t0.uv;
                        _normal[VerIdx] = tri.n0.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v1.pos;
                        _uv[VerIdx] = tri.t1.uv;
                        _normal[VerIdx] = tri.n1.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;
                        _vertex[VerIdx] = tri.v2.pos;
                        _uv[VerIdx] = tri.t2.uv;
                        _normal[VerIdx] = tri.n2.dir;
                        _tri[VerIdx] = VerIdx;
                        VerIdx++;

                        leaf1triangleNum--;

                        if (VerIdx >= 65000 - 2)
                        {

                            _LeafYoungMesh.vertices = _vertex;
                            _LeafYoungMesh.uv = _uv;
                            _LeafYoungMesh.normals = _normal;
                            _LeafYoungMesh.triangles = _tri;

                            _LeafYoungMesh.RecalculateNormals();
                            _LeafYoungMesh.RecalculateBounds();


                            index++;
                            _LeafYoungMesh = CreateIvyMeshObject(rootObj, "LeafYoung_", index, "Transparent/Diffuse", Leaf1Tex);

                            VerIdx = 0;

                            _vertex = new Vector3[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                            _uv = new Vector2[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                            _normal = new Vector3[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];
                            _tri = new int[Mathf.Min(65000 - 2, leaf1triangleNum * 3)];

                            Debug.LogWarning(string.Format("Ivy Manager: Create LeafYoung_{0} object vertex num is : ", index) + _vertex.Length.ToString());

                        }
                    }
                }

                if (VerIdx > 0)
                {
                    _LeafYoungMesh.vertices = _vertex;
                    _LeafYoungMesh.uv = _uv;
                    _LeafYoungMesh.normals = _normal;
                    _LeafYoungMesh.triangles = _tri;

                    _LeafYoungMesh.RecalculateNormals();
                    _LeafYoungMesh.RecalculateBounds();
                }

            }
 
        }

        public void ExportOBJFile(string path, string file)
        {
            bool bSuccess = OBJWriter.writeOBJ(path, file, IvyGenerator);
            if (bSuccess)
                Debug.LogWarning("Ivy Manager: export obj file : " + path + file + ".obj succeed.");
        }


        Mesh CreateIvyMeshObject(GameObject rootObj, string ObjName, int ObjIdx, string shaderName, Texture mainTex)
        {
            GameObject PartObj = new GameObject(ObjName + ObjIdx.ToString());
            PartObj.transform.parent = rootObj.transform;

            PartObj.AddComponent<MeshFilter>();
            PartObj.AddComponent<MeshRenderer>();

            Mesh _PartMesh = new Mesh();
            _PartMesh.name = ObjName + ObjIdx.ToString();


            Material _PartMaterial = new Material(Shader.Find(shaderName));
            _PartMaterial.mainTexture = mainTex;
            _PartMaterial.color = Color.white;
            PartObj.GetComponent<Renderer>().material = _PartMaterial;

            PartObj.transform.GetComponent<MeshFilter>().mesh = _PartMesh;

            return _PartMesh;
        }

        //editor visualization
        void OnDrawGizmos()
        {
            if (IvyRoot == null)
                return;

            if (m_DoNotDrawGizmos)
                return;

            // Ivy roots
            Gizmos.color = Color.green;
            foreach (var root in IvyGenerator.roots)
            {
                if (root.parents == 0)
                    Gizmos.DrawSphere(root.nodes[0].pos, 0.03f);
            }

            // Ivy skeleton
            Gizmos.color = new Color(0.3f, 0.4f, 1.0f);
            foreach (var root in IvyGenerator.roots)
            {
                for (int node = 0; node < root.nodes.Count - 1; node++)
                    Gizmos.DrawLine(root.nodes[node].pos, root.nodes[node + 1].pos);
            }

            // Ivy adhesion vectors
            Gizmos.color = new Color(0.3f, 0.8f, 0.6f);
            foreach (var root in IvyGenerator.roots)
            {
                for (int node = 0; node < root.nodes.Count; node++)
                    Gizmos.DrawLine(root.nodes[node].pos, root.nodes[node].pos + root.nodes[node].adhesionVector * 0.1f);
            }

        }
    }
}


