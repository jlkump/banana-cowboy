# Banana Cowboy
## High Concept
You are the long-lost son of the banana king of the banana world. You were long an outcast from society and became a cowboy to explore the universe with your trusty space-faring steed. However, now a tyrannical blender has taken the banana homeworld and many other fruits have fallen under servitude to him out of fear. Now it is up to you to liberate your homeworld, saving your scattered banana citizens from various fruit planets, such as the Strawberry Mafia, the Blueberry Pirates, and the Orange Bikers.


## Notes for Organization

Each person will generally keep to modifying their own test scenes and letting one person modify the actual levels at a time. 
This will prevent merge conflicts from coming up during development.

| File Type | Examples |
| --------- | -------- |
| 3D Model Data | `Models/BananaCowboy/BananaCowboy.obj` <br/> `Models/Fence/Fence.obj`|
| Audio Files | `Audio/SCORE_OrangePlanet_Explore_V2.mp3` <br/> `Audio/Old/SFX_Grunt_Short_V1.mp3` |
| Prefab Files | `Prefabs/Banana Cowboy` <br/> `Prefabs/Managers/UI Manager` |
| Scene Files | `Scenes/Screens/Menu` <br/> `Scenes/Levels/Orange Level` <br/> `Scenes/Tests/Lasso Test` |
| Script Files | `Scripts/Components/GravityObject.cs` <br/> `Scripts/PlayerController.cs` |

### Coding Style
When naming any function, method, variable, etc, always err on the side of clarity rather than brevity. 
No abbreviations except for common abbreviations, such as UI or GUI.

| Type | Style |
| ---- | ----- |
| `FunctionOrMethodDeclaration()` | PascalCase |
| `parameterNames` | lowerCamelCase |
| `publicMemberField` | lowerCamelCase |
| `_privateMemberField` | Prefixed with "_" lowerCamelCase |
| `s_staticField` | Prefixed with "s_" lowerCamelCase |
| `S_StaticMethod()` | Prefixed with "S_" PascalCase |

