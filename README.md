# 01 Framework Structure

`GameCore` is the core of the entire framework, designed to access all global classes and initiate the game.

<img src="Attachments/FrameworkUML.png" alt="FrameworkUML" width="370" height="502"/>

 `GameCore` is a `MonoBehaviour` script attached to a GameObject. Global classes are also `MonoBehaviour` scripts, attached to the child objects of the GameObject where `GameCore` is located.

<img src="Attachments/Hierarchy.png" alt="Hierarchy" width="642" height="408"/>

# 02 Global Managers
The framework currently includes the following global managers:

- **DataManager**: Handles data access and modification.
- **EventManager**: Registers and broadcasts events.
- **LevelManager**: Manages specific in-level logic.
- **ResourceManager**: Loads and releases resources.
- **SceneManager**: Handles scene transitions.
- **SoundManager**: Plays music and sound effects.
- **UIManager**: Manages opening and closing of UI interfaces.

Click [here](./Build-in%20Managers%20Usage.md) to see the usage of these build-in managers. You can also refer to my open-source demo, [_ForeverUp!_](https://github.com/AngusK97/Game_ForeverUp_NoPaidResourceVersion), for additional insights.

# 03 Add A New Global Manager
1. Ensure the new manager inherits from `MonoBehaviour`.
2. Create a child object under the `GameCore` GameObject and attach the new manager script to it.
3. Add a variable for the new manager in `GameCore`.
4. Drag and drop the manager reference to `GameCore`.

# 04 Discussion
If you have any questions or suggestions, feel free to reach out and discuss with me: **anguskungcn@foxmail.com**.
