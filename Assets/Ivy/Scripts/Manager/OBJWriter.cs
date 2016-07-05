using UnityEngine;
using System.IO;
using System.Collections;
using System.Text;

namespace Ivy
{

    public class OBJWriter
    {
        static public bool writeOBJ(string path, string file, BasicMesh model)
        {
            StringBuilder sb0 = new StringBuilder();
            //export material library
            string matlibFileName = path + file + ".mtl";

            FileStream fs0 = new FileStream(matlibFileName, FileMode.CreateNew);
            using(StreamWriter sw0 = new StreamWriter(fs0))
            {
	            if (model.materials.Count != 0)
	            {
		            foreach (var m in model.materials)
		            {
                        sb0.Append("newmtl ").Append(m.name).Append("\n");
                        sb0.Append("map_Kd ").Append(m.texFile).Append("\n\n");
		            }

	            }
                sw0.Write(sb0.ToString());
                sw0.Close();
            }
            fs0.Close();




            StringBuilder sb = new StringBuilder();
            sb.Append("mtllib ").Append(matlibFileName).Append("\n");

	        //export vertices
	        foreach (var v in model.vertices)	
            {
                sb.Append("v ").Append(v.pos.x).Append(" ").Append(v.pos.y).Append(" ").Append(v.pos.z).Append("\n");
	        }
            //export normals
	        foreach (var n in model.normals)
	        {
                sb.Append("vn ").Append(n.dir.x).Append(" ").Append(n.dir.y).Append(" ").Append(n.dir.z).Append("\n");
	        }
	        //export texCoords
	        foreach (var t in model.texCoords)
	        {
		        sb.Append("vt ").Append(t.uv.x).Append(" ").Append(t.uv.y).Append("\n");
	        }

	        //export triangles
	        if (model.materials.Count == 0)
	        {
		        foreach ( var t in model.triangles )
		        {
			        if ((model.texCoords.Count == 0) && (model.normals.Count == 0))
			        {
                        sb.Append("f ").Append(t.v0id).Append(" ").Append(t.v1id).Append(" ").Append(t.v2id).Append("\n");
			        }

			        if ((model.texCoords.Count != 0) && (model.normals.Count == 0))
			        {
				         sb.Append("f ").Append(t.v0id).Append("/").Append(t.t0id).Append(" ").Append(t.v1id).Append("/").Append(t.t1id).Append(" ").Append(t.v2id).Append("/").Append(t.t2id).Append("\n");
			        }

			        if ((model.texCoords.Count == 0) && (model.normals.Count!= 0))
			        {
                        sb.Append("f ").Append(t.v0id).Append("//").Append(t.n0id).Append(" ").Append(t.v1id).Append("//").Append(t.n1id).Append(" ").Append(t.v2id).Append("//").Append(t.n2id).Append("\n");
			        }

			        if ((model.texCoords.Count != 0) && (model.normals.Count != 0))
			        {
                        sb.Append("f ").Append(t.v0id).Append("/").Append(t.t0id).Append("/").Append(t.n0id).Append(" ").Append(t.v1id).Append("/").Append(t.t1id).Append("/").Append(t.n1id).Append(" ").Append(t.v2id).Append("/").Append(t.t2id).Append("/").Append(t.n2id).Append("\n");
			        }
		        }
	        }
	        else
	        {
		        foreach (var m in model.materials)
		        {
			        sb.Append("usemtl ").Append(m.name).Append("\n");

			        foreach (var t in model.triangles)
			        {
				        if (t.matid != m.id) continue;

				        if ((model.texCoords.Count == 0) && (model.normals.Count == 0))
				        {
                            sb.Append("f ").Append(t.v0id).Append(" ").Append(t.v1id).Append(" ").Append(t.v2id).Append("\n");
                        }

                        if ((model.texCoords.Count != 0) && (model.normals.Count == 0))
				        {
                            sb.Append("f ").Append(t.v0id).Append("/").Append(t.t0id).Append(" ").Append(t.v1id).Append("/").Append(t.t1id).Append(" ").Append(t.v2id).Append("/").Append(t.t2id).Append("\n");
                        }

                        if ((model.texCoords.Count == 0) && (model.normals.Count != 0))
				        {
                            sb.Append("f ").Append(t.v0id).Append("//").Append(t.n0id).Append(" ").Append(t.v1id).Append("//").Append(t.n1id).Append(" ").Append(t.v2id).Append("//").Append(t.n2id).Append("\n");
                        }

                        if ((model.texCoords.Count != 0) && (model.normals.Count != 0))
				        {
                            sb.Append("f ").Append(t.v0id).Append("/").Append(t.t0id).Append("/").Append(t.n0id).Append(" ").Append(t.v1id).Append("/").Append(t.t1id).Append("/").Append(t.n1id).Append(" ").Append(t.v2id).Append("/").Append(t.t2id).Append("/").Append(t.n2id).Append("\n");
                        }
			        }
		        }
	        }

            FileStream fs = new FileStream(path + file + ".obj", FileMode.CreateNew);
            using(StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
            fs.Close();

            return true;


        }
    }
}