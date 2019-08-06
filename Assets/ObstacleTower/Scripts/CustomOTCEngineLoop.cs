using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// Obstacle Tower uses a custom engine loop to speed up the simulation during training.
/// </summary>
public class CustomOTCEngineLoop : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void RuntimeStart()
    {
        var defaultPlayerLoop = PlayerLoop.GetDefaultPlayerLoop();
        
        // Assumptions: project does not use:
        // XR, Analytics, WebRequest, Kinect, TangoUpdate, iOS, TextureStreaming, Audio, Physics2D, Wind,
        // Video, Pathfinding, runtime Substance, Englighten, VFX, PhysicsCloth,
        // ParticleSystems, ScreenCapture.
        // If one of those are used in the project, appropriate lines removing such functionality system updates should be commented out.
        RemoveEngineLoopSystem<Initialization.XREarlyUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.AnalyticsCoreStatsUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.UnityWebRequestUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.UpdateAllUnityWebStreams>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.XRUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.ProcessRemoteInput>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.TangoUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.UpdateKinect>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.DeliverIosPlatformEvents>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.SpriteAtlasManagerUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.UpdateStreamingManager>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<EarlyUpdate.UpdateTextureStreamingManager>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<FixedUpdate.AudioFixedUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<FixedUpdate.XRFixedUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<FixedUpdate.Physics2DFixedUpdate>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<PreUpdate.Physics2DUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreUpdate.AIUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreUpdate.WindUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreUpdate.UpdateVideo>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<PreLateUpdate.AIUpdatePostScript>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreLateUpdate.UNetUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreLateUpdate.UpdateMasterServerInterface>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreLateUpdate.UpdateNetworkManager>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PreLateUpdate.ParticleSystemBeginUpdateAll>(ref defaultPlayerLoop);
        
        RemoveEngineLoopSystem<PostLateUpdate.UpdateAudio>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.UpdateVideo>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.UpdateSubstance>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.UpdateVideoTextures>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.EnlightenRuntimeUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.VFXUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.XRPostPresent>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.ProcessWebSendMessages>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.ExecuteGameCenterCallbacks>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.PhysicsSkinnedClothBeginUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.PhysicsSkinnedClothFinishUpdate>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.UpdateCaptureScreenshot>(ref defaultPlayerLoop);
        RemoveEngineLoopSystem<PostLateUpdate.ParticleSystemEndUpdateAll>(ref defaultPlayerLoop);
       
     
        PlayerLoop.SetPlayerLoop(defaultPlayerLoop);

        Debug.Log("Setup of CustomOTCEngineLoop done.");
    }
    
    private static bool RemoveEngineLoopSystem<TSystem>(ref PlayerLoopSystem system)
    {
        
        if (system.subSystemList == null)
            return false;

        for (int idx = 0; idx < system.subSystemList.Length; idx++)
        {
            if (system.subSystemList[idx].type == typeof(TSystem))
            {
                var reducedSystemList = new List<PlayerLoopSystem>(system.subSystemList);
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
