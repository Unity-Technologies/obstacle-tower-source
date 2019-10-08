# Obstacle Tower (Source code) 

![alt text](banner.png "Obstacle Tower")

The Obstacle Tower is a procedurally generated environment consisting of an endless number of floors to be solved by a learning agent. It is designed to examine how machines operate in a variety of areas, including computer vision, locomotion skills, and high-level planning. It combines platforming-style gameplay with puzzles and planning problems, all in a tower with an endless number of floors for agents to learn to solve. Critically, the floors become progressively more difficult as the agent progresses.

Within each floor, the goal of the agent is to arrive at the set of stairs leading to the next level of the tower. These floors are composed of multiple rooms, each which can contain their own unique challenges. Furthermore, each floor contains a number of procedurally generated elements, such as visual appearance, puzzle configuration, and floor layout. This ensures that in order for an agent to be successful at the Obstacle Tower task, they must be able to generalize to new and unseen combinations of conditions.

## Reference paper

To learn more, please read our IJCAI 2019 paper:

[**Obstacle Tower: A Generalization Challenge in Vision, Control, and Planning**](https://arxiv.org/abs/1902.01378).

If you use Obstacle Tower in your research, we ask that you cite the paper above.

## Training agents with a pre-built environment

If you are interested primarily in AI research using Obstacle Tower without the need to modify the environment itself, we provide pre-built binaries and a gym wrapper to interact with the environment. It is available [here](https://github.com/Unity-Technologies/obstacle-tower-env).

## Requirements

* [Unity 2019.2](https://unity3d.com/get-unity/download)
* [ML-Agents v0.10.0](https://github.com/Unity-Technologies/ml-agents/)

## Opening the project

1. Open Unity Editor and load project from the root of this directory.
2. From editor, load `Procedural` scene, located in `Assets/ObstacleTower/Scenes`.
3. Click on `Play` button in Editor to run environment with player controls.

## Understanding and extending the project

To learn more about the project and how to extend Obstacle Tower for your own custom research, see [here](extending.md).

## Obstacle Tower Challenge

On February 11, 2019, Unity Technologies launched a challenge using the Obstacle Tower. The challenge ended on July 15, 2019. See below for more information on the Obstacle Tower Challenge.

* [AICrowd Site and Final Leaderboard](https://www.aicrowd.com/challenges/unity-obstacle-tower-challenge)
* [Obstacle Tower Challenge Environment Repository](https://github.com/Unity-Technologies/obstacle-tower-env)
* [Obstacle Tower Challenge Starter Kit Repository](https://github.com/Unity-Technologies/obstacle-tower-challenge)
* [Obstacle Tower Challenge: Test the limits of intelligence systems](https://blogs.unity3d.com/2019/01/28/obstacle-tower-challenge-test-the-limits-of-intelligence-systems/)
* [The Obstacle Tower Challenge is live!](https://blogs.unity3d.com/2019/02/18/the-obstacle-tower-challenge-is-live/)
* [Obstacle Tower Challenge Round 2 begins today](https://blogs.unity3d.com/2019/05/15/obstacle-tower-challenge-round-2-begins-today/)
* [Announcing the Obstacle Tower Challenge winners and open source release](https://blogs.unity3d.com/2019/08/07/announcing-the-obstacle-tower-challenge-winners-and-open-source-release)
