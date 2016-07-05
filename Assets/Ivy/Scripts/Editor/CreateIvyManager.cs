
using UnityEngine;
using UnityEditor;

namespace Ivy
{

    /// <summary>
    /// Adds a new Ivy Manager gameobject to the scene.
    /// <summary>
    public class CreateIvyManager : EditorWindow
    {
        //add menu named "Waypoint Manager" to the Window menu
        [MenuItem("Window/Ivy Generation System/Ivy Manager")]

        //initialize method
        static void Init()
        {
            //search for a waypoint manager object within current scene
            GameObject _IvyManager = GameObject.Find("Ivy Manager");

            //if no waypoint manager object was found
            if (_IvyManager == null)
            {
                //create a new gameobject with that name
                _IvyManager = new GameObject("Ivy Manager");
                //and attach the WaypointManager component to it
                _IvyManager.AddComponent<IvyManager>();
            }

            //in both cases, select the gameobject
            Selection.activeGameObject = _IvyManager;
        }
    }
}