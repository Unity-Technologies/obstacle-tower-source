using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Obstacle Tower uses a custom engine loop to speed up the simulation during training.
/// </summary>
public class CustomOTCEngineLoop : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void RuntimeStart()
    {
        var defaultPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
        
        // Assumptions: project does not use:
        // XR, Analytics, WebRequest, Kinect, TangoUpdate, iOS, TextureStreaming, Audio, Physics2D, Wind,
        // Video, Pathfinding, runtime Substance, Englighten, VFX, PhysicsCloth,
        // ParticleSystems, ScreenCapture.
        // If one of those are used in the project, appropriate lines removing such functionality system updates should be commented out.
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.Initialization.XREarlyUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.AnalyticsCoreStatsUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.UnityWebRequestUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.XRUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.ProcessRemoteInput>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.TangoUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.UpdateKinect>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.DeliverIosPlatformEvents>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.SpriteAtlasManagerUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.UpdateStreamingManager>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate.UpdateTextureStreamingManager>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.FixedUpdate.AudioFixedUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.FixedUpdate.XRFixedUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.FixedUpdate.Physics2DFixedUpdate>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreUpdate.Physics2DUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreUpdate.AIUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreUpdate.WindUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreUpdate.UpdateVideo>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreLateUpdate.AIUpdatePostScript>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreLateUpdate.UNetUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreLateUpdate.UpdateMasterServerInterface>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreLateUpdate.UpdateNetworkManager>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PreLateUpdate.ParticleSystemBeginUpdateAll>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.UpdateAudio>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.UpdateVideo>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.UpdateSubstance>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.UpdateVideoTextures>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.EnlightenRuntimeUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.VFXUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.XRPostPresent>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.ProcessWebSendMessages>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.ExecuteGameCenterCallbacks>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.PhysicsSkinnedClothBeginUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.PhysicsSkinnedClothFinishUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.UpdateCaptureScreenshot>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate.ParticleSystemEndUpdateAll>(ref defaultPlayerLoop);
       
     
        UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(defaultPlayerLoop);

        Debug.Log("Setup of CustomOTCEngineLoop done.");
    }
    
    private static bool RemoveEngineLoopSystem<TSystem>(ref UnityEngine.LowLevel.PlayerLoopSystem system)
    {
        
        if (system.subSystemList == null)
            return false;

        for (int idx = 0; idx < system.subSystemList.Length; idx++)
        {
            if (system.subSystemList[idx].type == typeof(TSystem))
            {
                var reducedSystemList = new List<UnityEngine.LowLevel.PlayerLoopSystem>(system.subSystemList);
                reducedSystemList.RemoveAt(idx);
                system.subSystemList = reducedSystemList.ToArray();
                return true;
            }
                
            if (RemoveEngineLoopSystem<TSystem>(ref system.subSystemList[idx]))
                return true;
        }

        return false;
    }
}
