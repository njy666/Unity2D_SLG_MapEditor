# Unity2D_SLG_MapEditor
This is a lightweight unity editor extension tool, which is used to directly and visually draw, edit and export hexagonal map data in the scene view. You can change the internal calculation formula and customize the way your map is generated so that they have a 2.5D perspective, rather than having to be horizontal or vertical like tilemap.
这是一个轻量级的unity编辑器扩展工具，用于在场景视图中直接直观地绘制、编辑和导出六边形贴图数据。你可以更改内部计算公式并自定义贴图的生成方式，使其具有2.5D透视图，而不必像tilemap那样水平或垂直。
## Core functions 核心功能
-* * scene view drawing * *: click or drag directly in the scene window to generate parcels.
-* * multi mode editing * *: supports "paint", "erase" and "redraw" modes.
-* * real time preview * *: display the color identification of different faces and tiletypes in real time in the scene.
-* * data persistence * *: map data can be exported as JSON files and can be re imported for editing at any time.
-   **场景视图绘制**：在 Scene 窗口中直接点击或拖拽生成地块。
-   **多模式编辑**：支持“绘制”、“擦除”和“重绘”模式。
-   **实时预览**：在 Scene 中实时显示不同势力（Faction）和地块类型（TileType）的颜色标识。
-   **数据持久化**：支持将地图数据导出为 JSON 文件，并可随时重新导入进行编辑。
## Preparation before use (key) 使用前的准备工作 (关键)
Since this editor is universal, you need to define the corresponding * * data structure * * in your project. Please ensure that your project contains the following enumeration definitions, otherwise the editor cannot compile.
由于本编辑器是通用的，它需要你项目中定义好对应的 **数据结构**。请确保你的项目中包含以下枚举定义，否则编辑器无法编译。

### 1 Define 'GameData' class
1. 定义 `GameData` 类
Please create a 'gamedata.cs' file in your project and include the following contents (you can modify the options in it according to your game needs, such as adding "neutral forces" or "special terrain"):
请在你的项目中创建一个 `GameData.cs` 文件，并包含以下内容（你可以根据你的游戏需求修改里面的选项，比如增加“中立势力”或者“特殊地形”）：

```C#
// 将此代码放在你项目的任意脚本中，或者单独创建一个 GameData.cs
public enum FactionType 
{ 
    Player, 
    FactionA, 
    FactionB, 
    FactionC,
    ...
    Neutral 
}

public enum TileType 
{ 
    Empty, 
    Wood, 
    Metal, 
    NobleMetal, 
    Food ,
    ...
}
### 2. Define mapgenerator data container 定义 MapGenerator 数据容器
You need a monobehavior script to mount mapeditor. Please make sure you have the following two classes:
你需要一个 MonoBehaviour 脚本来挂载 MapEditor。请确保你有以下两个类：
[System.Serializable]
public class TileData
{
    public Vector2Int gridPos;
    public TileType type;
    public FactionType owner;
    ...
}

public class MapGenerator : MonoBehaviour
{
    public List<TileData> mapDataList = new List<TileData>();
    ...
}
### How to install 如何安装
Drag mapeditor.cs into your unity project. 将 MapEditor.cs 拖入你的 Unity 项目。
Make sure your project contains the GameData definition above. 确保你的项目中包含上述的 GameData 定义。
Create an empty object in the scene and mount the mapgenerator script.  在场景中创建一个空物体，挂载 MapGenerator 脚本。
In the inspector panel, click "map editor: start" to start drawing.  在 Inspector 面板中，点击 "地图编辑器：启动" 开始绘制。


developer：
Njy
78357527+njy666@users.noreply.github.com
