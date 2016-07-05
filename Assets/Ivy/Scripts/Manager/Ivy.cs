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
using System.Collections;
using System.Collections.Generic;

namespace Ivy
{
    /** an ivy node */
    public class IvyNode
    {
	    /** node position */
	    public Vector3 pos;			

	    /** primary grow direction, a weighted sum of the previous directions */
	    public Vector3 primaryDir;

	    /** adhesion vector as a result from other scene objects */
	    public Vector3 adhesionVector;

	    /** a smoothed adhesion vector computed and used during the birth phase,
	       since the ivy leaves are align by the adhesion vector, this smoothed vector
	       allows for smooth transitions of leaf alignment */
	    public Vector3 smoothAdhesionVector;

	    /** length of the associated ivy branch at this node */
	    public float length;

	    /** length at the last node that was climbing */
	    public float floatingLength;

	    /** climbing state */
	    public bool climb;

        public IvyNode()
        {
            climb = false;
            length = 0;
            floatingLength = 0;
        }
    }


    /** an ivy root point */
    public class IvyRoot
    {
	    /** a number of nodes */
	    public List<IvyNode> nodes = new List<IvyNode>();

	    /** alive state */
	    public bool alive;

	    /** number of parents, represents the level in the root hierarchy */
	    public int parents;
    }


    /** the ivy itself, derived from basic mesh that allows to handle the final ivy mesh as a drawable object */
    public class Ivy : BasicMesh
    {
        public Ivy()
        {
            resetSettings();
        }

	    public void resetSettings()
        {
	        primaryWeight = 0.5f;
	        randomWeight = 0.2f;
	        gravityWeight = 1.0f;
	        adhesionWeight = 0.1f;

	        branchingProbability = 0.95f;
	        leafProbability = 0.7f;

	        ivySize = 0.005f;

	        ivyLeafSize = 1.5f;
	        ivyBranchSize = 0.15f;


	        maxFloatLength = 0.1f;
	        maxAdhesionDistance = 0.1f;
        }

	    /** initialize a new ivy root */
	    public void seed(Vector3 seedPos)
        {
	        reset();
	        roots.Clear();


	        IvyNode tmpNode = new IvyNode();
	        tmpNode.pos = seedPos;
	        tmpNode.primaryDir = new Vector3(0.0f, 1.0f, 0.0f);
	        tmpNode.adhesionVector = new Vector3(0.0f, 0.0f, 0.0f);

	        tmpNode.length = 0.0f;
	        tmpNode.floatingLength = 0.0f;
	        tmpNode.climb = true;


	        IvyRoot tmpRoot = new IvyRoot();
	        tmpRoot.nodes.Add( tmpNode );

	        tmpRoot.alive = true;
	        tmpRoot.parents = 0;

	        roots.Add( tmpRoot );
        }

	    /** one single grow iteration */
	    public void grow()
        {
            //parameters that depend on the scene object bounding sphere
	        float local_ivySize = IvyManager.SceneObjMesh.boundingSphereRadius * ivySize;

            float local_maxFloatLength = IvyManager.SceneObjMesh.boundingSphereRadius * maxFloatLength;

	
	        //normalize weights of influence
	        float sum = primaryWeight + randomWeight + adhesionWeight;

	        primaryWeight /= sum;
	        randomWeight /= sum;
	        adhesionWeight /= sum;


	        //lets grow
	        foreach (var root in roots)
	        {
		        //process only roots that are alive
		        if (!root.alive) 
                    continue;

                IvyNode lastnode = root.nodes[root.nodes.Count-1];
		        //let the ivy die, if the maximum float length is reached
		        if (lastnode.floatingLength > local_maxFloatLength) 
                    root.alive = false;


		        //grow vectors: primary direction, random influence, and adhesion of scene objectss

			        //primary vector = weighted sum of previous grow vectors
			        Vector3 primaryVector = lastnode.primaryDir;
			
			        //random influence plus a little upright vector
			        Vector3 randomVector = (new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ) + new Vector3(0.0f, 0.2f, 0.0f) ).normalized;

			        //adhesion influence to the nearest triangle = weighted sum of previous adhesion vectors
			        Vector3 adhesionVector = computeAdhesion(lastnode.pos);	

			        //compute grow vector
			        Vector3 growVector = local_ivySize * (primaryVector * primaryWeight + randomVector * randomWeight + adhesionVector * adhesionWeight);


		        //gravity influence

			        //compute gravity vector
			        Vector3 gravityVector = local_ivySize * new Vector3(0.0f, -1.0f, 0.0f) * gravityWeight; 

			        //gravity depends on the floating length
			        gravityVector *= Mathf.Pow(lastnode.floatingLength / local_maxFloatLength, 0.7f);


		        //next possible ivy node
	
			        //climbing state of that ivy node, will be set during collision detection
			        bool climbing = false;

			        //compute position of next ivy node
			        Vector3 newPos = lastnode.pos + growVector + gravityVector;

			        //combine alive state with result of the collision detection, e.g. let the ivy die in case of a collision detection problem
			        root.alive = root.alive && computeCollision(lastnode.pos, ref newPos, ref climbing);

			        //update grow vector due to a changed newPos
			        growVector = newPos - lastnode.pos - gravityVector;
	

		        //create next ivy node
			        IvyNode tmpNode = new IvyNode();

			        tmpNode.pos = newPos;

			        tmpNode.primaryDir = ( 0.5f * lastnode.primaryDir + 0.5f * growVector.normalized ).normalized;

			        tmpNode.adhesionVector = adhesionVector;

			        tmpNode.length = lastnode.length + (newPos - lastnode.pos).magnitude;

			        tmpNode.floatingLength = climbing ? 0.0f : lastnode.floatingLength + (newPos - lastnode.pos).magnitude;

			        tmpNode.climb = climbing;

		        root.nodes.Add( tmpNode );
	        }



	        //lets produce child ivys
	        foreach (var root in roots)
	        {
		        //process only roots that are alive
		        if (!root.alive)
                    continue;

		        //process only roots up to hierarchy level 3, results in a maximum hierarchy level of 4
		        if (root.parents > 3) 
                    continue;


		        //add child ivys on existing ivy nodes
		        foreach (var node in root.nodes)
		        {
			        //weight depending on ratio of node length to total length
			        float weight = 1.0f - ( Mathf.Cos( node.length / root.nodes[root.nodes.Count-1].length * 2.0f * Mathf.PI) * 0.5f + 0.5f );

			        //random influence
			        float probability = Random.value;

			        if (probability * weight > branchingProbability)
			        {
				        //new ivy node
				        IvyNode tmpNode = new IvyNode();
				        tmpNode.pos = node.pos;
				        tmpNode.primaryDir = new Vector3(0.0f, 1.0f, 0.0f);
				        tmpNode.adhesionVector = new Vector3(0.0f, 0.0f, 0.0f);
				        tmpNode.length = 0.0f;
				        tmpNode.floatingLength = node.floatingLength;
				        tmpNode.climb = true;


				        //new ivy root
				        IvyRoot tmpRoot = new IvyRoot();
				        tmpRoot.nodes.Add( tmpNode );
				        tmpRoot.alive = true;
				        tmpRoot.parents = root.parents + 1;
				        roots.Add( tmpRoot );


				        //limit the branching to only one new root per iteration, so return
				        return;
			        }
		        }
	        }
        }

	    /** compute the adhesion of scene objects at a point pos*/
	    public Vector3 computeAdhesion(Vector3 pos)
        {
	        //the resulting adhesion vector
	        Vector3 adhesionVector = Vector3.zero;


	        //define a maximum distance
            float local_maxAdhesionDistance = IvyManager.SceneObjMesh.boundingSphereRadius * maxAdhesionDistance;

	        float minDistance = local_maxAdhesionDistance;


	        //find nearest triangle
            foreach (var t in IvyManager.SceneObjMesh.triangles)
	        {
		        //scalar product projection
		        float nq = Vector3.Dot(t.norm, pos - t.v0.pos);

		        //continue if backside of triangle
		        if ( nq < 0.0f ) continue;

		        //project last node onto triangle plane, e.g. scalar product projection
		        Vector3 p0 = pos - t.norm * nq;

		        //compute barycentric coordinates of p0
		        float alpha = 0;
                float beta = 0;
                float gamma = 0;
			
		        if (getBarycentricCoordinates(t.v0.pos, t.v1.pos, t.v2.pos, p0, ref alpha, ref beta, ref gamma))
		        {
			        //compute distance
			        float distance = (p0 - pos).magnitude;

			        //find shortest distance
			        if (distance < minDistance)
			        {
				        minDistance = distance;

				        adhesionVector = (p0 - pos).normalized;
					
				        //distance dependent adhesion vector
				        adhesionVector *= 1.0f - distance / local_maxAdhesionDistance;
			        }
		        }
	        }

	        return adhesionVector;
        }

	    /** computes the collision detection for an ivy segment oldPos->newPos, newPos will be modified if necessary */
        public bool computeCollision(Vector3 oldPos, ref Vector3 newPos, ref bool climbing)
        {
	        //reset climbing state
	        climbing = false;

	        bool intersection;
	
	        int deadlockCounter = 0;

	        do
	        {
		        intersection = false;

                foreach (var t in IvyManager.SceneObjMesh.triangles)
		        {
			        //compute intersection with triangle plane parametrically: intersectionPoint = oldPos + (newPos - oldPos) * t0;
			        float t0 = -Vector3.Dot( t.norm, oldPos - t.v0.pos ) / Vector3.Dot( t.norm, newPos - oldPos );

			        //plane intersection
			        if ((t0 >= 0.0f) && ( t0 <= 1.0f))
			        {
				        //intersection point
				        Vector3 intersectionPoint = oldPos + (newPos - oldPos) * t0;

				        float alpha = 0;
                        float beta = 0;
                        float gamma = 0;

				        //triangle intersection
				        if (getBarycentricCoordinates(t.v0.pos, t.v1.pos, t.v2.pos, intersectionPoint, ref alpha, ref  beta, ref gamma))
				        {                    
					        //test on entry or exit of the triangle mesh
					        bool entry = Vector3.Dot( t.norm, newPos - oldPos) < 0.0f ? true : false;

					        if (entry)
					        {
						        //project newPos to triangle plane
						        Vector3 p0 = newPos - t.norm * Vector3.Dot(t.norm, newPos - t.v0.pos);

						        //mirror newPos at triangle plane
						        newPos += 2.0f * (p0 - newPos);

						        intersection = true;

						        climbing = true;
					        }
				        }
			        }
		        }

		        //abort climbing and growing if there was a collistion detection problem
		        if (deadlockCounter++ > 5)
		        {
			        return false;
		        }
  	        }
	        while (intersection);

	        return true;
        }

	    /** creates the ivy triangle mesh */
        public void birth()
        {
	        //evolve a gaussian filter over the adhesian vectors

	        float [] gaussian = {1.0f, 2.0f, 4.0f, 7.0f, 9.0f, 10.0f, 9.0f, 7.0f, 4.0f, 2.0f, 1.0f }; 
	
	        foreach (var root in roots)
	        {
		        for (int g = 0; g < 5; ++g)
		        {
			        for (int node = 0; node < root.nodes.Count; node++)
			        {
				        Vector3 e = Vector3.zero;

				        for (int i = -5; i <= 5; ++i)
				        {
					        Vector3 tmpAdhesion = Vector3.zero;

					        if ((node + i) < 0) tmpAdhesion = root.nodes[0].adhesionVector;
					        if ((node + i) >= root.nodes.Count) tmpAdhesion = root.nodes[root.nodes.Count-1].adhesionVector;
					        if (((node + i) >= 0) && ((node + i) < root.nodes.Count)) tmpAdhesion = root.nodes[node + i].adhesionVector;

					        e += tmpAdhesion * gaussian[i+5];
				        }

				       root.nodes[node].smoothAdhesionVector = e / 56.0f;
			        }

			        foreach (var _node in root.nodes)
			        {
				        _node.adhesionVector = _node.smoothAdhesionVector;
			        }
		        }
	        }


	        //parameters that depend on the scene object bounding sphere
            float local_ivyLeafSize = IvyManager.SceneObjMesh.boundingSphereRadius * ivySize * ivyLeafSize;

            float local_ivyBranchSize = IvyManager.SceneObjMesh.boundingSphereRadius * ivySize * ivyBranchSize;


	        //reset existing geometry
	        reset();


	        //set data path
	        path = "../textures/";


	        //create material for leafs
	        BasicMaterial tmpMaterial = new BasicMaterial();

	        tmpMaterial.id = 1;
	        tmpMaterial.name = "leaf_adult";
            tmpMaterial.texFile = IvyManager.LeafAdultTexPathName;
	
	        materials.Add( tmpMaterial );


	        //create second material for leafs
            tmpMaterial = new BasicMaterial();
	        tmpMaterial.id = 2;
	        tmpMaterial.name = "leaf_young";
            tmpMaterial.texFile = IvyManager.LeafYoungTexPathName;
	
	        materials.Add( tmpMaterial );


	        //create material for branches
            tmpMaterial = new BasicMaterial();
	        tmpMaterial.id = 3;
	        tmpMaterial.name = "branch";
            tmpMaterial.texFile = IvyManager.branchTexPathName;
	
	        materials.Add( tmpMaterial );


	        //create leafs
	        foreach (var root in roots)
	        {
		        //simple multiplier, just to make it a more dense
		        for (int i = 0; i < 10; ++i)
		        {
			        //srand(i + (root - roots.begin()) * 10);

			        foreach (var node in root.nodes)
			        {
                        IvyNode back_node = root.nodes[root.nodes.Count - 1];
				        //weight depending on ratio of node length to total length
				        float weight = Mathf.Pow(node.length / back_node.length, 0.7f);

				        //test: the probability of leaves on the ground is increased
				        float groundIvy = Mathf.Max(0.0f, -Vector3.Dot( new Vector3(0.0f, 1.0f, 0.0f), node.adhesionVector.normalized ));
				        weight += groundIvy * Mathf.Pow(1.0f - node.length / back_node.length, 2.0f);
				
				        //random influence
				        float probability = Random.value;

				        if (probability * weight > leafProbability)
				        {
					        //alignment weight depends on the adhesion "strength"
					        float alignmentWeight = node.adhesionVector.magnitude;

                

					        //horizontal angle (+ an epsilon vector, otherwise there's a problem at 0?and 90?.. mmmh)
					        float phi = vector2ToPolar( new Vector2(node.adhesionVector.z, node.adhesionVector.x).normalized  + new Vector2(Vector2.kEpsilon, Vector2.kEpsilon) ) - Mathf.PI * 0.5f;

					        //vertical angle, trimmed by 0.5
					        float theta = Vector3.Angle( node.adhesionVector, new Vector3(0.0f, -1.0f, 0.0f) ) * 0.5f;

					        //center of leaf quad
					        Vector3 center = node.pos + new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ).normalized * local_ivyLeafSize;

					        //size of leaf
					        float sizeWeight = 1.5f - (Mathf.Cos(weight * 2.0f * Mathf.PI) * 0.5f + 0.5f);


					        //random influence
					        phi += Random.Range(-0.5f, 0.5f) * (1.3f - alignmentWeight);

					        theta += Random.Range(-0.5f, 0.5f) * (1.1f - alignmentWeight);


                    
					        //create vertices
					        BasicVertex tmpVertex = new BasicVertex();
   

					        tmpVertex.pos = center + new Vector3(-local_ivyLeafSize * sizeWeight, 0.0f, local_ivyLeafSize * sizeWeight);
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 0.0f, 1.0f), theta);					
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 1.0f, 0.0f), phi);
					        tmpVertex.pos += new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ).normalized * local_ivyLeafSize * sizeWeight * 0.5f;
					        vertices.Add( tmpVertex );

                            tmpVertex = new BasicVertex();
					        tmpVertex.pos = center + new Vector3( local_ivyLeafSize * sizeWeight, 0.0f, local_ivyLeafSize * sizeWeight);
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 0.0f, 1.0f), theta);					
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 1.0f, 0.0f), phi);					
					        tmpVertex.pos += new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ).normalized * local_ivyLeafSize * sizeWeight * 0.5f;
					        vertices.Add( tmpVertex );

                            tmpVertex = new BasicVertex();
					        tmpVertex.pos = center + new Vector3(-local_ivyLeafSize * sizeWeight, 0.0f, -local_ivyLeafSize * sizeWeight);
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 0.0f, 1.0f), theta);					
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 1.0f, 0.0f), phi);
					        tmpVertex.pos += new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ).normalized * local_ivyLeafSize * sizeWeight * 0.5f;
					        vertices.Add( tmpVertex );

                            tmpVertex = new BasicVertex();
					        tmpVertex.pos = center + new Vector3( local_ivyLeafSize * sizeWeight, 0.0f, -local_ivyLeafSize * sizeWeight);
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 0.0f, 1.0f), theta);					
					        tmpVertex.pos = rotateAroundAxis(tmpVertex.pos, center, new Vector3(0.0f, 1.0f, 0.0f), phi);
					        tmpVertex.pos += new Vector3( Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f) ).normalized * local_ivyLeafSize * sizeWeight * 0.5f;
					        vertices.Add( tmpVertex );


					        //create texCoords
					        BasicTexCoord tmpTexCoord = new BasicTexCoord();
					        tmpTexCoord.uv = new Vector2( 0.0f, 1.0f);
					        texCoords.Add( tmpTexCoord );

                            tmpTexCoord = new BasicTexCoord();
					        tmpTexCoord.uv = new Vector2( 1.0f, 1.0f);
					        texCoords.Add( tmpTexCoord );

                            tmpTexCoord = new BasicTexCoord();
					        tmpTexCoord.uv = new Vector2( 0.0f, 0.0f);
					        texCoords.Add( tmpTexCoord );

                            tmpTexCoord = new BasicTexCoord();
					        tmpTexCoord.uv = new Vector2( 1.0f, 0.0f);
					        texCoords.Add( tmpTexCoord );


					        //create triangle
					        BasicTriangle tmpTriangle = new BasicTriangle();
					        tmpTriangle.matid = 1;

					        float _probability = Random.value;
					        if (_probability * weight > leafProbability) tmpTriangle.matid = 2;

					        tmpTriangle.v0id = (uint)vertices.Count-1;
					        tmpTriangle.v1id = (uint)vertices.Count-3;
					        tmpTriangle.v2id = (uint)vertices.Count-2;

					        tmpTriangle.t0id = (uint)vertices.Count-1;
					        tmpTriangle.t1id = (uint)vertices.Count-3;
					        tmpTriangle.t2id = (uint)vertices.Count-2;

                            triangles.Add(tmpTriangle);

                            BasicTriangle tmpTriangle2 = new BasicTriangle();
                            tmpTriangle2.matid = tmpTriangle.matid;

					        tmpTriangle2.v0id = (uint)vertices.Count-2;
					        tmpTriangle2.v1id = (uint)vertices.Count-0;
					        tmpTriangle2.v2id = (uint)vertices.Count-1;

					        tmpTriangle2.t0id = (uint)vertices.Count-2;
					        tmpTriangle2.t1id = (uint)vertices.Count-0;
					        tmpTriangle2.t2id = (uint)vertices.Count-1;

					        triangles.Add( tmpTriangle2 );	
				        }
			        }
		        }
	        }



	        //branches
	        foreach (var root in roots)
	        {
		        //process only roots with more than one node
		        if (root.nodes.Count == 1) continue;


		        //branch diameter depends on number of parents
		        float local_ivyBranchDiameter = 1.0f / (float)(root.parents + 1) + 1.0f;


                for (int node = 0; node < root.nodes.Count - 1; node++)
		        {
			        //weight depending on ratio of node length to total length
			        float weight = root.nodes[node].length / root.nodes[root.nodes.Count-1].length;


			        //create trihedral vertices
			        Vector3 up = new Vector3(0.0f, -1.0f, 0.0f);

			        Vector3 basis = (root.nodes[node + 1].pos - root.nodes[node].pos).normalized;

			        Vector3 b0 = Vector3.Cross(up, basis).normalized * local_ivyBranchDiameter * local_ivyBranchSize * (1.3f - weight) + root.nodes[node].pos;

			        Vector3 b1 = rotateAroundAxis(b0, root.nodes[node].pos, basis, 2.09f);

			        Vector3 b2 = rotateAroundAxis(b0, root.nodes[node].pos, basis, 4.18f);

			        //create vertices
			        BasicVertex tmpVertex = new BasicVertex();
			        tmpVertex.pos = b0;
			        vertices.Add( tmpVertex );

                    tmpVertex = new BasicVertex();
			        tmpVertex.pos = b1;
			        vertices.Add( tmpVertex );

                    tmpVertex = new BasicVertex();
			        tmpVertex.pos = b2;
			        vertices.Add( tmpVertex );


			        //create texCoords
			        BasicTexCoord tmpTexCoord = new BasicTexCoord();

			        float texV = (node % 2 == 0 ? 1.0f : 0.0f);

			        tmpTexCoord.uv = new Vector2( 0.0f, texV);
			        texCoords.Add( tmpTexCoord );

                    tmpTexCoord = new BasicTexCoord();
                    tmpTexCoord.uv = new Vector2(0.3f, texV);
			        texCoords.Add( tmpTexCoord );

                    tmpTexCoord = new BasicTexCoord();
                    tmpTexCoord.uv = new Vector2(0.6f, texV);
			        texCoords.Add( tmpTexCoord );


			        if (node == 0) continue;


			        //create triangle
			        BasicTriangle tmpTriangle = new BasicTriangle();
			        tmpTriangle.matid = 3;

			        tmpTriangle.v0id = (uint)vertices.Count - 3;
                    tmpTriangle.v1id = (uint)vertices.Count - 0;
                    tmpTriangle.v2id = (uint)vertices.Count - 4;

                    tmpTriangle.t0id = (uint)vertices.Count - 3;
                    tmpTriangle.t1id = (uint)vertices.Count - 0;
                    tmpTriangle.t2id = (uint)vertices.Count - 4;

			        triangles.Add( tmpTriangle );

                    tmpTriangle = new BasicTriangle();
                    tmpTriangle.matid = 3;
                    tmpTriangle.v0id = (uint)vertices.Count - 4;
                    tmpTriangle.v1id = (uint)vertices.Count - 0;
                    tmpTriangle.v2id = (uint)vertices.Count - 1;

                    tmpTriangle.t0id = (uint)vertices.Count - 4;
                    tmpTriangle.t1id = (uint)vertices.Count - 0;
                    tmpTriangle.t2id = (uint)vertices.Count - 1;

			        triangles.Add( tmpTriangle );

                    tmpTriangle = new BasicTriangle();
                    tmpTriangle.matid = 3;
                    tmpTriangle.v0id = (uint)vertices.Count - 4;
                    tmpTriangle.v1id = (uint)vertices.Count - 1;
                    tmpTriangle.v2id = (uint)vertices.Count - 5;

                    tmpTriangle.t0id = (uint)vertices.Count - 4;
                    tmpTriangle.t1id = (uint)vertices.Count - 1;
                    tmpTriangle.t2id = (uint)vertices.Count - 5;

			        triangles.Add( tmpTriangle );

                    tmpTriangle = new BasicTriangle();
                    tmpTriangle.matid = 3;
                    tmpTriangle.v0id = (uint)vertices.Count - 5;
                    tmpTriangle.v1id = (uint)vertices.Count - 1;
                    tmpTriangle.v2id = (uint)vertices.Count - 2;

                    tmpTriangle.t0id = (uint)vertices.Count - 5;
                    tmpTriangle.t1id = (uint)vertices.Count - 1;
                    tmpTriangle.t2id = (uint)vertices.Count - 2;

			        triangles.Add( tmpTriangle );

                    tmpTriangle = new BasicTriangle();
                    tmpTriangle.matid = 3;
                    tmpTriangle.v0id = (uint)vertices.Count - 5;
                    tmpTriangle.v1id = (uint)vertices.Count - 2;
                    tmpTriangle.v2id = (uint)vertices.Count - 0;

                    tmpTriangle.t0id = (uint)vertices.Count - 5;
                    tmpTriangle.t1id = (uint)vertices.Count - 2;
                    tmpTriangle.t2id = (uint)vertices.Count - 0;

			        triangles.Add( tmpTriangle );

                    tmpTriangle = new BasicTriangle();
                    tmpTriangle.matid = 3;
                    tmpTriangle.v0id = (uint)vertices.Count - 5;
                    tmpTriangle.v1id = (uint)vertices.Count - 0;
                    tmpTriangle.v2id = (uint)vertices.Count - 3;

                    tmpTriangle.t0id = (uint)vertices.Count - 5;
                    tmpTriangle.t1id = (uint)vertices.Count - 0;
                    tmpTriangle.t2id = (uint)vertices.Count - 3;

			        triangles.Add( tmpTriangle );	
		        }
	        }


	        //initialize ivy mesh
	        //loadTextures();

	        prepareData();

	        calculateVertexNormals();

	        prepareData();

	        //createDisplayList(true);
        }






        private bool getBarycentricCoordinates( Vector3 vector1, Vector3 vector2, Vector3 vector3, Vector3 position, ref float alpha, ref float beta, ref float gamma )
	    {
		    float area = 0.5f * Vector3.Cross( vector2 - vector1, vector3 - vector1 ).magnitude;

            alpha = 0.5f * Vector3.Cross(vector2 - position, vector3 - position).magnitude / area;

            beta = 0.5f * Vector3.Cross(vector1 - position, vector3 - position).magnitude / area;

            gamma = 0.5f * Vector3.Cross(vector1 - position, vector2 - position).magnitude / area;

		    //if (abs( 1.0f - alpha - beta - gamma ) > std::numeric_limits<float>::epsilon()) return false;
		    if (Mathf.Abs( 1.0f - alpha - beta - gamma ) > 0.00001f) return false;

		    return true;
	    }

	    private float vector2ToPolar( Vector2 vector )
	    {
		    float phi = (vector.x == 0.0f) ? 0.0f : Mathf.Atan( vector.y / vector.x );

		    if ( vector.x < 0.0f )
		    {
			    phi += 3.1415926535897932384626433832795f;
		    }
		    else
		    {
			    if ( vector.y < 0.0f )
			    {
				    phi += 2.0f * 3.1415926535897932384626433832795f;
			    }
		    }

		    return phi;
	    }

	    private Vector2 polarToVector2(float phi)
	    {
            return new Vector2(Mathf.Cos(phi), Mathf.Sin(phi));
	    }
	    private Vector3 rotateAroundAxis( Vector3 vector, Vector3 axisPosition, Vector3 axis, float angle )
	    {
		    //determining the sinus and cosinus of the rotation angle
		    float cosTheta = Mathf.Cos(angle);
            float sinTheta = Mathf.Sin(angle);

		    //Vector3 from the given axis point to the initial point
		    Vector3 direction = vector - axisPosition;

		    //new vector which will hold the direction from the given axis point to the new rotated point 
		    Vector3 newDirection = Vector3.zero;

		    //x-component of the direction from the given axis point to the rotated point
		    newDirection.x = ( cosTheta + ( 1 - cosTheta ) * axis.x * axis.x ) * direction.x +
						     ( ( 1 - cosTheta ) * axis.x * axis.y - axis.z * sinTheta ) * direction.y +
						     ( ( 1 - cosTheta ) * axis.x * axis.z + axis.y * sinTheta ) * direction.z;

		    //y-component of the direction from the given axis point to the rotated point
		    newDirection.y = ( ( 1 - cosTheta ) * axis.x * axis.y + axis.z * sinTheta ) * direction.x +
						     ( cosTheta + ( 1 - cosTheta ) * axis.y * axis.y ) * direction.y +
						     ( ( 1 - cosTheta ) * axis.y * axis.z - axis.x * sinTheta ) * direction.z;

		    //z-component of the direction from the given axis point to the rotated point
		    newDirection.z = ( ( 1 - cosTheta ) * axis.x * axis.z - axis.y * sinTheta ) * direction.x +
						     ( ( 1 - cosTheta ) * axis.y * axis.z + axis.x * sinTheta ) * direction.y +
						     ( cosTheta + ( 1 - cosTheta ) * axis.z * axis.z) * direction.z;

		    //returning the result by addind the new direction vector to the given axis point
		    return axisPosition + newDirection;
	    }





	
	    /** the ivy roots */
	    public List<IvyRoot> roots = new List<IvyRoot>();	



	    /** the ivy size factor, influences the grow behaviour [0..0,1] */
	    public float ivySize;

	    /** leaf size factor [0..0,1] */
	    public float ivyLeafSize;

	    /** branch size factor [0..0,1] */
	    public float ivyBranchSize;

        /** maximum length of an ivy branch segment that is freely floating [0..1] */
	    public float maxFloatLength;

	    /** maximum distance for adhesion of scene object [0..1] */
	    public float maxAdhesionDistance;

	    /** weight for the primary grow vector [0..1] */
	    public float primaryWeight;

	    /** weight for the random influence vector [0..1] */
	    public float randomWeight;

	    /** weight for the gravity vector [0..1] */
	    public float gravityWeight;

	    /** weight for the adhesion vector [0..1] */
	    public float adhesionWeight;

	    /** the probability of producing a new ivy root per iteration [0..1]*/
	    public float branchingProbability;

	    /** the probability of creating a new ivy leaf [0..1] */
	    public float leafProbability;
    }


}