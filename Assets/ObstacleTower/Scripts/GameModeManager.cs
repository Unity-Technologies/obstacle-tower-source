using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
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
		var academy = GameObject.FindWithTag("academy").GetComponent<ObstacleTowerAcademy>();
		var devTools = GameObject.FindWithTag("debug");

		var portArgExists = Environment.GetCommandLineArgs().Contains("--port");
		var playMode = !portArgExists && !EnableEditorTraining;
		if (playMode)
		{
			Debug.Log("In play mode");
			academy.broadcastHub.Clear();
			agent.brain = agent.playerBrain;
		}
		academy.enabled = true;
		agent.enabled = true;

		if (!Application.isEditor)
		{
			devTools.SetActive(false);
		}
	}
}
