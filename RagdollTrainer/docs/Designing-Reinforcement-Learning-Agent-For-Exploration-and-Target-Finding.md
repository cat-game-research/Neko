# Designing a Reinforcement Learning Agent for Exploration and Target Finding

In this project, we are trying to create a reinforcement learning agent that can learn to wander around and look for a target in a 3D environment, using a vector3 as the output. The vector3 represents the direction and the speed of the agent's movement, and it is determined by the agent's observations and actions. The agent does not know the location of the target beforehand, and it must discover it by exploring the environment and using its sensors.

## Observations

The agent uses a Ray Perception Sensor Component 3D to simulate its eyes and perceive the environment. This component allows us to use raycast observations in Unity ML-Agents. We can configure the number, angle, length, and layer mask of the rays, as well as the tags and sizes of the detectable objects. We can also choose whether to use ray hits or ray sectors as the observation type.

In addition to the raycast observations, we also include some additional information, such as the agent's position, orientation, velocity, and acceleration, as well as the target's position, distance, and visibility. These features can help the agent to locate itself and the target in the environment, and to plan its actions accordingly.

## Actions

The agent uses continuous actions to control the speed and the vector3 of its movement, and discrete actions to control any other behaviors we want. We use the OnActionReceived method to access the ActionBuffers parameter, which contains both continuous and discrete actions. We also use the Heuristic method to provide manual input for testing and debugging.

## Rewards

The agent uses a reward function that encourages it to explore the environment and find the target. We use a reward function that is based on the cosine similarity between the vector3 output and the vector from the agent to the target. This way, the agent will be rewarded for aligning its output with the direction of the target, regardless of where the target is. We also add some penalties for collisions, timeouts, or deviations from the desired speed.

## Training

The agent uses an off-policy reinforcement learning algorithm to learn from its experiences and improve its policy. We use the ML-Agents Trainer to set up the training configuration, such as the hyperparameters, the reward function, the curriculum, and the behavior cloning. We also use the ML-Agents TensorBoard to monitor the training progress and visualize the results.

We also consider some techniques to improve the exploration and exploitation trade-off, such as epsilon-greedy, entropy regularization, or curiosity-driven learning. These techniques can help the agent to balance between trying new actions and exploiting the best ones, and to avoid getting stuck in local optima.

We also consider some techniques to incorporate hierarchical learning, such as the Hierarchical Reinforcement Learning (HRL) or the Option-Critic Architecture (OCA). These techniques can help the agent to learn to decompose complex tasks into simpler ones, and to generalize across different situations.

## Resources

- [Ray Perception Sensor Component Tutorial — Immersive Limit]: This is a tutorial that explains how to add a RayPerceptionSensorComponent3D to a Unity ML-Agents project and how to use it in a simple example.
- [ML-Agents: Hummingbirds]: This is a Unity learn course that teaches us how to create and train intelligent agents in a hummingbird simulation. It uses raycast observations and continuous actions to control the agent's movement and feeding behavior.
- [Unity ML-Agents Toolkit Documentation]: This is the official documentation of the Unity ML-Agents Toolkit, which covers the installation, usage, and reference of the framework. It also includes some examples and best practices for creating and training agents.
- [Reinforcement Learning (DQN) Tutorial]: This is a tutorial that shows how to use PyTorch to train a DQN agent on the CartPole-v1 task from Gymnasium. It covers the basics of reinforcement learning, such as observations, actions, rewards, and neural networks.
- [Reinforcement Learning Tips and Tricks]: This is a guide that provides some best practices and common pitfalls when using reinforcement learning. It covers topics such as reward design, hyperparameter tuning, normalization, and evaluation.
- [Build a reinforcement learning recommendation application using Vertex AI]: This is a blog post that shows how to use Vertex AI to build a reinforcement learning application for movie recommendations. It covers the steps of data generation, ingestion, training, and deployment.
- [Reinforcement learning - GeeksforGeeks]: This is an article that gives an overview of reinforcement learning, its types, applications, and challenges. It also includes some examples and links to other resources.
- [Curiosity-driven Exploration by Self-supervised Prediction]: This is a paper that introduces the Intrinsic Curiosity Module (ICM), a neural network that generates intrinsic rewards for the agent based on the prediction error of the next state. It shows that the ICM can enable the agent to explore large and sparse environments, and to learn useful skills.
- [Exploration by Random Network Distillation]: This is a paper that introduces the Random Network Distillation (RND), a neural network that generates intrinsic rewards for the agent based on the output difference between two randomly initialized networks. It shows that the RND can enable the agent to explore hard exploration environments, and to achieve state-of-the-art results.
- [Hierarchical Reinforcement Learning]: This is a survey paper that provides an overview of hierarchical reinforcement learning, its methods, applications, and challenges. It covers topics such as temporal abstraction, state abstraction, policy hierarchy, and meta-learning.
- [The Option-Critic Architecture]: This is a paper that introduces the Option-Critic Architecture (OCA), a neural network that learns both the options and the option-selection policy in an end-to-end manner. It shows that the OCA can enable the agent to learn faster and better than flat policies, and to transfer knowledge across tasks.
