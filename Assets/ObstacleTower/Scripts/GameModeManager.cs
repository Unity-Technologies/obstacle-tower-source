using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// Responsible for intelligently switching between player and agent modes.
/// </summary>
public class GameModeManager : MonoBehaviour
{
	public bool EnableEditorTraining;

	private void Awake ()
	{
		var agent = GameObject.FindWithTag("agent").GetComponent<ObstacleTowerAgent>();
		var academy = GameObject.FindWithTag("academy").GetComponent<ObstacleTowerManager>();
		var devTools = GameObject.FindWithTag("debug");

		var portArgExists = Environment.GetCommandLineArgs().Contains("--mlagents-port");
		var playMode = !portArgExists && !EnableEditorTraining;
        
        if (playMode)
		{
			Debug.Log("In play mode");
            agent.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
            academy.InferenceOn = true;
        }
		else
		{
			agent.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.Default;
            academy.InferenceOn = false;
		}
		academy.enabled = true;
		agent.enabled = true;

		if (!Application.isEditor)
		{
			devTools.SetActive(false);
		}
	}
}
