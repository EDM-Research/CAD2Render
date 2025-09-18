using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SceneIteratorInterface : MonoBehaviour
{
    public struct C2RModel
    {
        public UnityEngine.Matrix4x4 localToWorld;
        public int obj_id;
    }

    public struct C2RPose
    {
        public UnityEngine.Matrix4x4 projMat;
        public UnityEngine.Matrix4x4 worldToCam;
        public List<C2RModel> models;
    }

    public event Action NewSceneLoaded;
    protected void raiseNewSceneLoaded() { NewSceneLoaded.Invoke(); }
    public event Action LastSceneEnded;
    protected void raiseLastSceneEnded() { LastSceneEnded.Invoke(); }
    
    
    public abstract C2RPose GetPose();

    public abstract void Next();

}
