# Neko Cat Game

Neko Cat Game is a 3D simulation game that aims to create realistic and interactive cat behaviors using machine learning and physics-based animation. The game is still in development and currently features some ragdoll robot biped walkers roaming around a scene.

![Screenshot of the game](./WalkerMotionAgent_01.png)

## Installation

To install the game, you need to have Unity Hub and Unity 2020.3.18f1 or later. You can download them from [here](^2^) and [here](^3^) respectively. 

Then, clone this repository to your local machine using the following command:

```bash
git clone https://github.com/cat-game-research/NekoCatGame.git
```

Alternatively, you can download the zip file of the repository and extract it.

Next, open Unity Hub and click on the "Add" button. Browse to the folder where you cloned or extracted the repository and select it. You should see the project appear in the Unity Hub window. Click on it to open it in Unity.

## Usage

Once the project is open in Unity, you can explore the different scenes and scripts that are part of the game. The main scene is located in `Assets/RagdollTrainer/Scenes/MainScene.unity`. You can play the scene by clicking on the play button at the top of the editor. You should see some robots walking around and falling over.

The game logic is mostly implemented in C# scripts that are attached to various game objects in the scene. You can find the scripts in `Assets/RagdollTrainer/Scripts`. Some of the scripts are:

- `BipedAgent.cs`: This is the main script that controls the behavior and animation of the robot. It uses a reinforcement learning algorithm to learn how to walk and balance. It also communicates with the `RagdollController.cs` script to switch between ragdoll and animated modes.
- `RagdollController.cs`: This script enables and disables the rigidbodies and colliders of the robot's body parts to create a ragdoll effect. It also applies forces and torques to the body parts to simulate muscle contractions.
- `BodyPart.cs`: This script represents a single body part of the robot, such as a head, a torso, or a limb. It stores the information about the body part's position, rotation, velocity, and angular velocity. It also has a reference to the `RagdollController.cs` script to access the ragdoll mode.
- `GroundContact.cs`: This script detects whether a body part is in contact with the ground or not. It uses raycasts to check for collisions and sends a signal to the `BipedAgent.cs` script.

## Roadmap

The game is still under development and we have many features and improvements planned for the future. Some of them are:

- Adding a cat model and animations to replace the robot model.
- Improving the learning algorithm and the reward function to achieve more natural and robust walking behaviors.
- Adding more interactivity and gameplay elements, such as obstacles, goals, and user input.
- Creating more diverse and realistic environments for the cat to explore.

## Contributing

We welcome any contributions to the game, whether it is bug fixes, new features, or suggestions. If you want to contribute, please follow these steps:

- Fork the repository and create a new branch for your changes.
- Make sure your code follows the [C# coding conventions](^4^) and the [Unity best practices](^5^).
- Test your code thoroughly and make sure it does not break the existing functionality.
- Commit and push your changes to your forked repository.
- Create a pull request and describe your changes in detail.
- Wait for our review and feedback.

## License

The game is licensed under the [MIT License](^6^), which means you can use, modify, and distribute it as you wish, as long as you give credit to the original authors and include the license file in your distribution.
