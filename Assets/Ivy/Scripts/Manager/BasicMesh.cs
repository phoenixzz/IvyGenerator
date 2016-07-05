/**************************************************************************************
**
**  Copyright (C) 2006 Thomas Luft, University of Konstanz. All rights reserved.
**
**  This file is part of the Ivy Generator Tool.
**
**  This program is free software; you can redistribute it and/or modify it
**  under the terms of the GNU General Public License as published by the
**  Free Software Foundation; either version 2 of the License, or (at your
**  option) any later version.
**  This program is distributed in the hope that it will be useful, but
**  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
**  or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
**  for more details.
**  You should have received a copy of the GNU General Public License along
**  with this program; if not, write to the Free Software Foundation,
**  Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110, USA 
**
***************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

namespace Ivy
{
    /** a simple vertex */
    public class BasicVertex
    {
        public Vector3 pos = Vector3.zero;
        public BasicVertex(Vector3 vert) { pos = vert; }
        public BasicVertex() { pos = Vector3.zero; }
    }


    /** a simple normal vector */
    public class BasicNormal
    {
        public Vector3 dir = Vector3.zero;
        public BasicNormal(Vector3 vert) { dir = vert; }
        public BasicNormal() { dir = Vector3.zero; }
    }


    /** a simple uv texture coordinate */
    public class BasicTexCoord
    {
        public Vector2 uv = Vector2.zero;
        public BasicTexCoord(Vector2 _uv) { uv = _uv; }
        public BasicTexCoord() { uv = Vector2.zero; }
    }


    /** a simple material containing only a single texture */
    public class BasicMaterial
    {
        public uint id = 0;

        public string name;

        public string texFile;

        public uint texObj = 0;
    }


    /** a simple triangle containing vertices, normals, texCoords, and a material */
    public class BasicTriangle
    {
	    public BasicVertex v0 = null;
	    public uint v0id = 0;

	    public BasicVertex v1 = null;
	    public uint v1id = 0;

	    public BasicVertex v2 = null;
	    public uint v2id = 0;

	    public BasicNormal n0 = null;
	    public uint n0id = 0;

	    public BasicNormal n1 = null;
	    public uint n1id = 0;

	    public BasicNormal n2 = null;
	    public uint n2id = 0;

	    public BasicTexCoord t0 = null;
	    public uint t0id = 0;

	    public BasicTexCoord t1 = null;
	    public uint t1id = 0;

	    public BasicTexCoord t2 = null;
	    public uint t2id = 0;

        public BasicMaterial mat = null;
        public uint matid = 0;

	    public Vector3 norm = Vector3.zero;
    }


    /** a simple triangle mesh */
    public class BasicMesh
    {
        public BasicMesh()
        {
            boundingSphereRadius = 1.0f;
            boundingSpherePos = new Vector3(0.0f, 0.0f, 0.0f);
        }


	    public void reset()
        {
	        file = string.Empty;
            path = string.Empty;
	        vertices.Clear();
            normals.Clear();
            texCoords.Clear();
            triangles.Clear();
            materials.Clear();
	        boundingSphereRadius = 1.0f;
	        boundingSpherePos = new Vector3(0.0f, 0.0f, 0.0f);
        }

	    /** setup the triangles pointer to their vertices, normals, texCoords, and materials; computes the bounding sphere */
	    public void prepareData()
        {
	        //update pointers of triangle
	        foreach (var t in triangles)
	        {
		        t.v0 = vertices[(int)(t.v0id - 1)];
		        t.v1 = vertices[(int)(t.v1id - 1)];
		        t.v2 = vertices[(int)(t.v2id - 1)];
    
		        if (t.n0id != 0) t.n0 = normals[(int)(t.n0id - 1)];
		        if (t.n1id != 0) t.n1 = normals[(int)(t.n1id - 1)];
		        if (t.n2id != 0) t.n2 = normals[(int)(t.n2id - 1)];

		        if (t.t0id != 0) t.t0 = texCoords[(int)(t.t0id - 1)];
		        if (t.t1id != 0) t.t1 = texCoords[(int)(t.t1id - 1)];
		        if (t.t2id != 0) t.t2 = texCoords[(int)(t.t2id - 1)];
 
		        if (t.matid != 0) t.mat = materials[(int)(t.matid - 1)];
	        }

	        //compute bounding sphere
	        boundingSpherePos = new Vector3(0.0f, 0.0f, 0.0f);

	        foreach (var v in vertices)
	        {
		        boundingSpherePos += v.pos;
	        }
            if (vertices.Count != 0)
	            boundingSpherePos /= (float)vertices.Count;

	        boundingSphereRadius = 0.0f;

	        foreach (var v in vertices)
	        {
		        boundingSphereRadius = Mathf.Max(boundingSphereRadius, (v.pos - boundingSpherePos).magnitude);
	        }
        }

	    /** computes the vertex normals */
        public void calculateVertexNormals()
        {
	        normals.Clear();

            for(int i=0; i<vertices.Count; i++)
	            normals.Add( new BasicNormal() );


	        foreach ( var t in triangles )
	        {
		        Vector3 tmp1 = t.v0.pos - t.v1.pos;
		        Vector3 tmp2 = t.v1.pos - t.v2.pos;
		        t.norm = Vector3.Cross( tmp1, tmp2 );

		        t.norm.Normalize();
	        }


	        foreach (var t in triangles)
	        {
		        t.n0id = t.v0id;
		        t.n0 = normals[(int)(t.n0id - 1)];
		        t.n0.dir += t.norm;

		        t.n1id = t.v1id;
		        t.n1 = normals[(int)(t.n1id - 1)];
		        t.n1.dir += t.norm;


		        t.n2id = t.v2id;
		        t.n2 = normals[(int)(t.n2id - 1)];
		        t.n2.dir += t.norm;	
	        }

	        foreach (var n in normals)
	        {
		        n.dir.Normalize();
	        }
        }

	    /** flips the vertex normals */
	    public void flipNormals()
        {
	        foreach (var t in triangles)
	        {
		        t.norm *= -1.0f;
	        }

	        foreach (var n in normals)
	        {
		        n.dir *= -1.0f;
	        }
        }



	    public List<BasicVertex> vertices = new List<BasicVertex>();
	    public List<BasicNormal> normals = new List<BasicNormal>();
	    public List<BasicTexCoord> texCoords = new List<BasicTexCoord>();
	    public List<BasicMaterial> materials = new List<BasicMaterial>();
	    public List<BasicTriangle> triangles = new List<BasicTriangle>();

	    public Vector3 boundingSpherePos;
	    public float boundingSphereRadius;
        public string file;
	    public string path;
    }


}


