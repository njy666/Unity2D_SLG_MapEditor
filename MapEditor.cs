using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    private enum BrushMode { AddTile, RemoveTile, PaintFaction }
    private BrushMode currentMode = BrushMode.AddTile;
    private FactionType selectedFaction = FactionType.Player;
    private bool isBrushEnabled = false;
    private TileType selectedTileType = TileType.Empty;
    public override void OnInspectorGUI()
    {
        MapGenerator gen = (MapGenerator)target;

        // 绘制基础属性
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUI.backgroundColor = isBrushEnabled ? Color.green : Color.white;
        if (GUILayout.Button(isBrushEnabled ? "地图编辑器：启动" : "地图编辑器：关闭", GUILayout.Height(30)))
        {
            isBrushEnabled = !isBrushEnabled;
        }
        GUI.backgroundColor = Color.white;

        if (isBrushEnabled)
        {
            GUILayout.BeginVertical("box");
            currentMode = (BrushMode)GUILayout.Toolbar((int)currentMode, new string[] { "新增/修改地块", "删除地块" });

            if (currentMode == BrushMode.AddTile) // 将原本的 PaintFaction 逻辑整合进 AddTile
            {
                GUILayout.Label("配置当前刷子预设:", EditorStyles.boldLabel);
                selectedFaction = (FactionType)EditorGUILayout.EnumPopup("归属势力:", selectedFaction);
                selectedTileType = (TileType)EditorGUILayout.EnumPopup("地块类型:", selectedTileType);

                GUILayout.Space(5);
                EditorGUILayout.HelpBox("提示：在地图上点击或拖动，将按上述配置创建或覆盖地块。", MessageType.Info);
            }
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("一键清除所有数据"))
        {
            if (EditorUtility.DisplayDialog("警告", "确定要清空所有地块数据吗？", "确定", "取消"))
            {
                gen.mapDataList.Clear();
                EditorUtility.SetDirty(gen);
            }
        }
        if (GUILayout.Button("导出为 JSON 文件"))
        {
            Export(gen);
        }
        if (GUILayout.Button("从 JSON 导入"))
        {
            Import(gen);
        }
    }

    void OnSceneGUI()
    {
        if (!isBrushEnabled) return;
        MapGenerator gen = (MapGenerator)target;
        Event e = Event.current;

        // 屏蔽默认点击，防止误选其他物体
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // 只有按下左键或拖动时触发逻辑
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector2Int gridPos = FindNearestGridPos(ray.origin, gen);

            ProcessBrush(gridPos, gen);
            e.Use(); // 消耗掉事件
        }

        // 绘制势力预览颜色（在 Scene 窗口实时看到结果）
        DrawFactionPreviews(gen);
    }

    void ProcessBrush(Vector2Int pos, MapGenerator gen)
    {
        if (pos.x < 0 || pos.y < 0) return;
        var targetData = gen.mapDataList.Find(d => d.gridPos == pos);

        switch (currentMode)
        {
            case BrushMode.AddTile:
                if (targetData == null)
                {
                    // 如果是新地块，直接添加并应用当前选中的所有属性
                    gen.mapDataList.Add(new TileData
                    {
                        gridPos = pos,
                        type = selectedTileType,
                        owner = selectedFaction
                    });
                }
                else
                {
                    // 如果地块已存在，直接更新其属性
                    targetData.type = selectedTileType;
                    targetData.owner = selectedFaction;
                }
                EditorUtility.SetDirty(gen);
                break;

            case BrushMode.RemoveTile:
                if (targetData != null)
                {
                    gen.mapDataList.Remove(targetData);
                    EditorUtility.SetDirty(gen);
                }
                break;

            
        }
        
        if (Application.isPlaying)
        {
            gen.RefreshTileVisuals(pos);
        }
    }

    // 辅助预览：在 Scene 窗口用不同颜色显示不同势力
    void DrawFactionPreviews(MapGenerator gen)
    {
        foreach (var data in gen.mapDataList)
        {
            Vector3 worldPos = GetWorldPos(data.gridPos, gen);
            Color factionColor = Color.white;
            switch (data.owner)
            {
                case FactionType.Player: factionColor = Color.cyan; break;
                case FactionType.FactionA: factionColor = Color.red; break;
                case FactionType.FactionB: factionColor = Color.blue; break;
                case FactionType.FactionC: factionColor = Color.magenta; break;
                case FactionType.Neutral: factionColor = Color.grey; break;
            }
            factionColor.a = 0.4f;
            Handles.color = factionColor;
            Handles.DrawSolidDisc(worldPos, Vector3.forward, 0.35f);
            
            Color typeColor = Color.white;
            bool isSpecialType = true;
            switch (data.type)
            {
                case TileType.Wood: typeColor = new Color(0.13f, 0.54f, 0.13f); break; // 深绿色
                case TileType.Metal: typeColor = new Color(0.5f, 0.5f, 0.8f); break; // 灰色 
                case TileType.NobleMetal: typeColor = Color.yellow; break;
                case TileType.Food: typeColor = new Color(0.8f, 0.5f, 0.5f); break; //肉色
                case TileType.Empty: isSpecialType = false; break; 
                default: isSpecialType = false; break;
            }

            if (isSpecialType)
            {
                Handles.color = typeColor;
                // 在势力圆圈中心画一个更小更实、更亮的小圆点
                Handles.DrawSolidDisc(worldPos, Vector3.forward, 0.15f);
                // 画个线框圆圈
                Handles.DrawWireDisc(worldPos, Vector3.forward, 0.18f);
            }
        }
    }

    // 复用对齐公式反推物理位置
    Vector3 GetWorldPos(Vector2Int pos, MapGenerator gen)
    {
        Vector3 origin = gen.transform.position;
        float xPos = origin.x + (pos.x * gen.xSpacing) + (pos.y * gen.xSlope);
        float yPos = origin.y + (pos.y * gen.ySpacing) + (pos.x * gen.ySlope);
        if (pos.x % 2 == 1) { xPos += gen.oddColumnXOffset; yPos += gen.oddColumnYOffset; }
        return new Vector3(xPos, yPos, 0);
    }
    //  自定义地图斜率
    Vector2Int FindNearestGridPos(Vector3 worldPos, MapGenerator gen)
    {
        Vector2Int best = Vector2Int.zero;
        float minDist = float.MaxValue;
        Vector3 origin = gen.transform.position;

        for (int x = 0; x < gen.width; x++)
        {
            for (int y = 0; y < gen.height; y++)
            {
                float xPos = origin.x + (x * gen.xSpacing) + (y * gen.xSlope);
                float yPos = origin.y + (y * gen.ySpacing) + (x * gen.ySlope);
                if (x % 2 == 1) { xPos += gen.oddColumnXOffset; yPos += gen.oddColumnYOffset; }

                float d = Vector2.Distance(worldPos, new Vector2(xPos, yPos));
                if (d < minDist) { minDist = d; best = new Vector2Int(x, y); }
            }
        }
        if (minDist > 0.5f)
        {
            return new Vector2Int(-1, -1);
        }
        return best;
    }
    // 导入导出
    void Export(MapGenerator gen)
    {
        // 包装一下 List，否则 JsonUtility 认不出来
        MapDataWrapper wrapper = new MapDataWrapper { dataList = gen.mapDataList };
        string json = JsonUtility.ToJson(wrapper, true);

        // 弹出保存框
        string path = EditorUtility.SaveFilePanel("保存地图数据", "Assets", "NewMapData", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log("导出成功！");
        }
    }

    void Import(MapGenerator gen)
    {
        string path = EditorUtility.OpenFilePanel("选择地图数据", "Assets", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            MapDataWrapper wrapper = JsonUtility.FromJson<MapDataWrapper>(json);

            // 直接覆盖组件里的那个 List
            gen.mapDataList = wrapper.dataList;

            // 关键：告诉 Unity 数据变了，需要保存场景
            EditorUtility.SetDirty(gen);
            Debug.Log("导入成功！");
        }
    }

    // 在 MapEditor 脚本末尾定义这个简单的包装类
    [System.Serializable]
    public class MapDataWrapper { public List<TileData> dataList; }
}