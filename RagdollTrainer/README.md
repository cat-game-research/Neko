![Ragdoll Screenshot](./docs/CombinedAgent.png)

# Ragdoll Trainer Unity Project

Active ragdoll training with Unity ML-Agents (PyTorch). 

## Ragdoll Agent

Based on [walker example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md)

The Robot Kyle model from the Unity assets store is used for the ragdoll.

![RobotKyleBlend Image](./docs/RobotKyleBlend.png)

### Features:

* Default Robot Kyle rig replaced with a new rig created in blender. FBX and blend file included.

* Heuristic function inlcuded to drive the joints by user input (for development testing only).

* Added stabilizer to hips and spine. The stabilizer applies torque to help ragdoll balance.

* Added "earlyTraining" bool for initial balance/walking toward target.

* Added WallsAgent prefab for navigating around obstacles (using Ray Perception Sensor 3D).

* Added StairsAgent prefab for navigating small and large steps.

* Added curiosity to yaml to improve walls and stairs training.

---

# Training Process

The following is the current basic overview of the iteration of the model and which environment and settings where used to achieve the latest checkpoint release.

* Model: Walker
  1. WalkerAgent (Basic Training - Easy) [**Walker-a0a-40m**]
     * early training = true
     * steps = 40 million
  2. WalkerAgent (Basic Training - Hard) [**Walker-a0a-120m**]
     * early_training = false
     * steps = 80 million
  3. SlopesWalkerAgent (Basic Training - Easy) [**Walker-a0a-140m**]
     * early_training = false
     * steps = 20 million
     * Note: Cooldown phase of 20 million steps of (Basic Training - Hard)
  4. WalkerMotionAgent (Basic Training - Hard) [**Walker-a0a-150m**]
     * early_training = false
     * has_columns = true
     * steps = 10 million
     * Note: Added small columns to introduce blocked line of sight.
  5. WalkerMotionAgent (Basic Training - Easy)
     * early_training = false
     * has_columns = false
     * use_motion_perception = true
     * steps = 10 million
     * Note: Added Motion Perception Agent for direction and velocity
  6. WalkerMotionAgent (Basic Training - Intermediate)
     * early_training = false
     * has_columns = true
     * use_motion_perception = false
     * steps = 5 million
     * Note: Added wall avoidance

The following is the basic overview of how the motion perception agent model was trained in which environment and their respective settings.

* Motion Perception
  1. MotionPerceptionAgent (Basic Training - Easy)
     * early_training = false
     * has_columns = false
     * use_motion_perception = true
     * steps = 10 million
  2. MotionPerceptionAgent (Basic Training - Hard)
     * early_training = false
     * has_columns = true
     * use_motion_perception = true
     * steps = 10 million
     * ... work in progress 
---

# How the WalkerAgent Prefab Model Works

This is a brief explanation of how the walkeragent prefab model works in the ragdolltrainer sub project using the ML-Agents framework.

## FixedUpdate()

The `FixedUpdate()` method is where the logic for updating the agent's state and rewards at each fixed timestep is implemented. In the code, several methods are used to calculate different rewards based on the agent's behavior, such as foot spacing, look at target, and match speed. These rewards are then added to the agent's total reward using the `AddReward()` method.

The transform position of the randomly placed target sphere is used in the `UpdateOrientationObjects()` method, which is defined in the `WalkerBase.cs` script. This method is responsible for updating the orientation cube and the direction indicator, which are used to provide visual feedback to the agent and the user. The orientation cube is a transparent cube that rotates to match the target's position, and the direction indicator is a colored arrow that points from the agent's head to the target. The transform position of the target sphere is used to calculate the rotation of the orientation cube and the direction indicator using the `Quaternion.LookRotation()` method.

## CollectObservations(VectorSensor sensor)

The `CollectObservations(VectorSensor sensor)` method is where the observations that the agent collects are defined. In the code, the target's transform position is added as a vector observation using the `sensor.AddObservation()` method. This way, the agent can perceive the location of the target relative to itself.

The transform position of the target sphere is not used directly in the agent's actions, but it affects the agent's rewards and observations, which in turn affect the agent's learning process. The agent learns to maximize its rewards by taking actions that move it closer to the target, while avoiding falling off the platform or crossing its feet. The agent also learns to use its observations to determine the best actions to take in different situations.

## References

* [Unity ML-Agents Tutorials – Complete Machine Learning Guide](https://www.markdownguide.org/basic-syntax/)
* [Unity-Technologies/ml-agents - GitHub](https://www.markdownguide.org/extended-syntax/)
* [Getting Started Guide - GitHub](https://markdown.land/markdown-code-block)
