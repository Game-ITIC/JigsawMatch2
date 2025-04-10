using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Monobehaviours.Buildings;

public class BuildingShopManagerEditorWindow : EditorWindow
{
    private BuildingShopManager selectedManager;
    private SerializedObject serializedManager;
    private SerializedProperty shopItemsProperty;
    private SerializedProperty maxItemsProperty;
    
    // UI State
    private Vector2 shopItemsScrollPosition;
    private Vector2 buildingsScrollPosition;
    private int selectedItem = -1;
    private string searchString = "";
    private bool showHelpTips = true;
    
    // Editor Style
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle itemSelectedStyle;
    private GUIStyle itemStyle;
    private GUIStyle boxStyle;
    private GUIStyle centeredBoldLabel;
    
    // Drag & Drop
    private bool isDragging = false;
    private int draggedItemIndex = -1;
    private int dragTargetIndex = -1;
    
    // Cached lists
    private List<SerializedProperty> filteredItems = new List<SerializedProperty>();
    private bool stylesInitialized = false;
    
    [MenuItem("Tools/Building Shop Manager (Simple)")]
    public static void ShowWindow()
    {
        BuildingShopManagerEditorWindow window = GetWindow<BuildingShopManagerEditorWindow>("Building Shop");
        window.minSize = new Vector2(850, 700);
        window.Show();
    }
    
    private void OnEnable()
    {
        titleContent = new GUIContent("Building Shop");
        stylesInitialized = false;
    }
    
    private void InitializeStyles()
    {
        if (stylesInitialized)
            return;
            
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.margin = new RectOffset(10, 10, 10, 10);
        
        subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
        subHeaderStyle.fontSize = 12;
        subHeaderStyle.alignment = TextAnchor.MiddleLeft;
        
        centeredBoldLabel = new GUIStyle(EditorStyles.boldLabel);
        centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
        
        itemStyle = new GUIStyle(EditorStyles.helpBox);
        itemStyle.margin = new RectOffset(2, 2, 2, 2);
        itemStyle.padding = new RectOffset(5, 5, 5, 5);
        
        itemSelectedStyle = new GUIStyle(itemStyle);
        itemSelectedStyle.normal.background = MakeTex(1, 1, new Color(0.4f, 0.6f, 0.8f, 0.2f));
        
        boxStyle = new GUIStyle(EditorStyles.helpBox);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        
        stylesInitialized = true;
    }
    
    private void OnGUI()
    {
        // Initialize styles on first OnGUI call
        if (!stylesInitialized)
        {
            InitializeStyles();
        }
        
        DrawToolbar();
        
        EditorGUILayout.Space(5);
        
        if (selectedManager == null)
        {
            DrawWelcomeScreen();
            return;
        }
        
        // Check if manager reference is still valid
        if (serializedManager == null || serializedManager.targetObject == null)
        {
            selectedManager = null;
            return;
        }
        
        serializedManager.Update();
        
        EditorGUILayout.BeginHorizontal();
        
        // Left panel (Shop Items List)
        DrawShopItemsList();
        
        // Right panel (Item Details)
        DrawItemDetails();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        DrawBottomPanel();
        
        // Apply any changes
        serializedManager.ApplyModifiedProperties();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // Manager selector
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
        selectedManager = (BuildingShopManager)EditorGUILayout.ObjectField(
            selectedManager, typeof(BuildingShopManager), true, GUILayout.Width(200));
        
        if (EditorGUI.EndChangeCheck() && selectedManager != null)
        {
            InitializeSerializedProperties();
        }
        
        GUILayout.FlexibleSpace();
        
        // Search field
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        string newSearch = EditorGUILayout.TextField(searchString, GUILayout.Width(200));
        if (newSearch != searchString)
        {
            searchString = newSearch;
            FilterItems();
        }
        
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            searchString = "";
            FilterItems();
        }
        
        GUILayout.FlexibleSpace();
        
        // Find managers button
        if (GUILayout.Button("Find Managers", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            FindBuildingShopManagers();
        }
        
        // Help toggle
        showHelpTips = GUILayout.Toggle(showHelpTips, "Show Tips", EditorStyles.toolbarButton, GUILayout.Width(80));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawWelcomeScreen()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.LabelField("Building Shop Manager Editor", headerStyle);
        EditorGUILayout.Space(20);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("No Building Shop Manager selected", centeredBoldLabel);
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Find Managers in Scene", GUILayout.Height(30), GUILayout.Width(200)))
        {
            FindBuildingShopManagers();
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Or drag a BuildingShopManager here:", centeredBoldLabel);
        
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 70.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop BuildingShopManager Here", centeredBoldLabel);
        
        // Handle drag & drop
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;
                
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is BuildingShopManager manager)
                        {
                            selectedManager = manager;
                            InitializeSerializedProperties();
                            break;
                        }
                    }
                }
                Event.current.Use();
                break;
        }
        
        EditorGUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawShopItemsList()
    {
        EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(300), GUILayout.ExpandHeight(true));
        
        // Header with count
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Shop Items ({filteredItems.Count}/{shopItemsProperty?.arraySize ?? 0})", subHeaderStyle);
        
        GUILayout.FlexibleSpace();
        
        // Add button with plain text instead of icon
        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(50)))
        {
            AddNewShopItem();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Max items display
        if (maxItemsProperty != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Items:", GUILayout.Width(80));
            EditorGUILayout.PropertyField(maxItemsProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(5);
        
        // Items list
        shopItemsScrollPosition = EditorGUILayout.BeginScrollView(shopItemsScrollPosition);
        
        if (filteredItems != null && filteredItems.Count > 0)
        {
            for (int i = 0; i < filteredItems.Count; i++)
            {
                SerializedProperty itemProperty = filteredItems[i];
                int actualIndex = GetActualIndex(itemProperty);
                
                if (actualIndex >= 0)
                {
                    DrawShopItemRow(actualIndex, itemProperty, i);
                }
            }
        }
        else if (shopItemsProperty != null && shopItemsProperty.arraySize > 0)
        {
            EditorGUILayout.HelpBox("Use the search box to filter items or clear search to see all items.", MessageType.Info);
        }
        else if (shopItemsProperty != null)
        {
            EditorGUILayout.HelpBox("No shop items found. Click the Add button to add your first item.", MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
        
        // Help tip
        if (showHelpTips && shopItemsProperty != null && shopItemsProperty.arraySize > 0)
        {
            EditorGUILayout.HelpBox("Click an item to edit its details.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawShopItemRow(int actualIndex, SerializedProperty itemProperty, int displayIndex)
    {
        SerializedProperty itemIdProperty = itemProperty.FindPropertyRelative("itemId");
        SerializedProperty itemNameProperty = itemProperty.FindPropertyRelative("itemName");
        SerializedProperty costProperty = itemProperty.FindPropertyRelative("cost");
        SerializedProperty isPurchasedProperty = itemProperty.FindPropertyRelative("isPurchased");
        SerializedProperty buildingsProperty = itemProperty.FindPropertyRelative("buildingsToUnlock");
        
        if (itemIdProperty == null || itemNameProperty == null || costProperty == null || 
            isPurchasedProperty == null || buildingsProperty == null)
        {
            EditorGUILayout.HelpBox("Item property structure is invalid.", MessageType.Error);
            return;
        }
        
        bool isSelected = selectedItem == actualIndex;
        
        // Draw row background
        Rect rowRect = EditorGUILayout.BeginHorizontal(
            isSelected ? itemSelectedStyle : 
            (displayIndex % 2 == 0 ? itemStyle : EditorStyles.helpBox),
            GUILayout.Height(50)
        );
        
        // Check if this row is being dragged
        if (isDragging && draggedItemIndex == actualIndex)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.3f, 0.5f);
            GUI.Box(rowRect, "");
            GUI.backgroundColor = Color.white;
        }
        
        // Item details
        EditorGUILayout.BeginVertical();
        
        // First line: ID and cost
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID:", GUILayout.Width(25));
        string newId = EditorGUILayout.TextField(itemIdProperty.stringValue, GUILayout.Width(80));
        if (newId != itemIdProperty.stringValue)
        {
            itemIdProperty.stringValue = newId;
        }
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.LabelField("Cost:", GUILayout.Width(35));
        int newCost = EditorGUILayout.IntField(costProperty.intValue, GUILayout.Width(60));
        if (newCost != costProperty.intValue)
        {
            costProperty.intValue = newCost;
        }
        EditorGUILayout.EndHorizontal();
        
        // Second line: Name
        EditorGUILayout.BeginHorizontal();
        string newName = EditorGUILayout.TextField(itemNameProperty.stringValue);
        if (newName != itemNameProperty.stringValue)
        {
            itemNameProperty.stringValue = newName;
        }
        EditorGUILayout.EndHorizontal();
        
        // Third line: Buildings count + purchased status
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Buildings: {buildingsProperty.arraySize}", GUILayout.Width(90));
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.LabelField("Purchased:", GUILayout.Width(70));
        bool newPurchased = EditorGUILayout.Toggle(isPurchasedProperty.boolValue, GUILayout.Width(20));
        if (newPurchased != isPurchasedProperty.boolValue)
        {
            isPurchasedProperty.boolValue = newPurchased;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // Actions
        EditorGUILayout.BeginVertical(GUILayout.Width(60));
        GUILayout.FlexibleSpace();
        
        // Edit button
        if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(55), GUILayout.Height(20)))
        {
            selectedItem = actualIndex;
        }
        
        // Delete button
        if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(55), GUILayout.Height(20)))
        {
            if (EditorUtility.DisplayDialog("Delete Item",
                    $"Are you sure you want to delete '{itemNameProperty.stringValue}'?",
                    "Delete", "Cancel"))
            {
                shopItemsProperty.DeleteArrayElementAtIndex(actualIndex);
                
                if (selectedItem == actualIndex)
                {
                    selectedItem = -1;
                }
                else if (selectedItem > actualIndex)
                {
                    selectedItem--;
                }
                
                FilterItems();
                GUIUtility.ExitGUI(); // To prevent issues with collection change
            }
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        // Make row clickable
        if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0) // Left click
            {
                selectedItem = selectedItem == actualIndex ? -1 : actualIndex;
                Repaint();
                Event.current.Use();
            }
            else if (Event.current.button == 1) // Right click
            {
                ShowItemContextMenu(actualIndex);
                Event.current.Use();
            }
        }
    }
    
    private void DrawItemDetails()
    {
        EditorGUILayout.BeginVertical(boxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        if (selectedItem >= 0 && shopItemsProperty != null && selectedItem < shopItemsProperty.arraySize)
        {
            SerializedProperty itemProperty = shopItemsProperty.GetArrayElementAtIndex(selectedItem);
            SerializedProperty itemIdProperty = itemProperty.FindPropertyRelative("itemId");
            SerializedProperty itemNameProperty = itemProperty.FindPropertyRelative("itemName");
            SerializedProperty costProperty = itemProperty.FindPropertyRelative("cost");
            SerializedProperty isPurchasedProperty = itemProperty.FindPropertyRelative("isPurchased");
            SerializedProperty buildingsProperty = itemProperty.FindPropertyRelative("buildingsToUnlock");
            
            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Editing Item: {itemNameProperty.stringValue}", subHeaderStyle);
            
            GUILayout.FlexibleSpace();
            
            // Button with text instead of icon
            if (GUILayout.Button("Duplicate", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                DuplicateItem(selectedItem);
            }
            
            // Button with text instead of icon
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(70)))
            {
                if (EditorUtility.DisplayDialog("Delete Item",
                        $"Are you sure you want to delete '{itemNameProperty.stringValue}'?",
                        "Delete", "Cancel"))
                {
                    shopItemsProperty.DeleteArrayElementAtIndex(selectedItem);
                    selectedItem = -1;
                    FilterItems();
                    GUIUtility.ExitGUI();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Item details section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Item Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item ID:", GUILayout.Width(100));
            itemIdProperty.stringValue = EditorGUILayout.TextField(itemIdProperty.stringValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Name:", GUILayout.Width(100));
            itemNameProperty.stringValue = EditorGUILayout.TextField(itemNameProperty.stringValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cost:", GUILayout.Width(100));
            costProperty.intValue = EditorGUILayout.IntField(costProperty.intValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Purchased:", GUILayout.Width(100));
            isPurchasedProperty.boolValue = EditorGUILayout.Toggle(isPurchasedProperty.boolValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Buildings section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Buildings To Unlock", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField($"Count: {buildingsProperty.arraySize}");
            
            if (GUILayout.Button("Add Building", GUILayout.Width(100)))
            {
                buildingsProperty.arraySize++;
                buildingsProperty.GetArrayElementAtIndex(buildingsProperty.arraySize - 1).objectReferenceValue = null;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Buildings list
            buildingsScrollPosition = EditorGUILayout.BeginScrollView(buildingsScrollPosition);
            
            if (buildingsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No buildings assigned to this item. Add buildings to be unlocked when this item is purchased.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < buildingsProperty.arraySize; i++)
                {
                    DrawBuildingRow(buildingsProperty, i);
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Select an item to edit its details", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBuildingRow(SerializedProperty buildingsProperty, int index)
    {
        Rect rowRect = EditorGUILayout.BeginHorizontal(
            index % 2 == 0 ? EditorStyles.helpBox : itemStyle,
            GUILayout.Height(40)
        );
        
        EditorGUILayout.LabelField($"{index + 1}.", GUILayout.Width(30));
        
        EditorGUI.BeginChangeCheck();
        Object newValue = EditorGUILayout.ObjectField(
            buildingsProperty.GetArrayElementAtIndex(index).objectReferenceValue,
            typeof(Building),
            true
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            buildingsProperty.GetArrayElementAtIndex(index).objectReferenceValue = newValue;
        }
        
        // Text button instead of icon
        if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            buildingsProperty.DeleteArrayElementAtIndex(index);
            GUIUtility.ExitGUI();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Preview tooltip on hover
        if (Event.current.type == EventType.Repaint && rowRect.Contains(Event.current.mousePosition))
        {
            Building building = buildingsProperty.GetArrayElementAtIndex(index).objectReferenceValue as Building;
            if (building != null)
            {
                // Display info about the building
                GUIContent tooltip = new GUIContent("", $"ID: {building.name}\nStatus: {(building.isUnlocked ? "Unlocked" : "Locked")}");
                EditorGUI.LabelField(rowRect, tooltip);
            }
        }
    }
    
    private void DrawBottomPanel()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Apply Changes", GUILayout.Height(30), GUILayout.Width(150)))
        {
            if (serializedManager != null)
            {
                serializedManager.ApplyModifiedProperties();
                EditorUtility.SetDirty(selectedManager);
                AssetDatabase.SaveAssets();
                Debug.Log("Changes saved to Building Shop Manager successfully!");
            }
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Revert Changes", GUILayout.Height(30), GUILayout.Width(150)))
        {
            if (serializedManager != null)
            {
                serializedManager.Update();
                FilterItems();
            }
        }
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void InitializeSerializedProperties()
    {
        if (selectedManager == null)
            return;
        
        serializedManager = new SerializedObject(selectedManager);
        shopItemsProperty = serializedManager.FindProperty("shopItems");
        maxItemsProperty = serializedManager.FindProperty("maxItemsInShop");
        selectedItem = -1;
        
        FilterItems();
    }
    
    private void FilterItems()
    {
        filteredItems.Clear();
        
        if (shopItemsProperty == null || shopItemsProperty.arraySize == 0)
            return;
        
        for (int i = 0; i < shopItemsProperty.arraySize; i++)
        {
            SerializedProperty item = shopItemsProperty.GetArrayElementAtIndex(i);
            
            if (string.IsNullOrEmpty(searchString))
            {
                filteredItems.Add(item);
                continue;
            }
            
            string search = searchString.ToLowerInvariant();
            string id = item.FindPropertyRelative("itemId").stringValue.ToLowerInvariant();
            string name = item.FindPropertyRelative("itemName").stringValue.ToLowerInvariant();
            
            if (id.Contains(search) || name.Contains(search))
            {
                filteredItems.Add(item);
            }
        }
    }
    
    private void FindBuildingShopManagers()
    {
        var managers = FindObjectsOfType<BuildingShopManager>();
        
        if (managers.Length == 0)
        {
            EditorUtility.DisplayDialog("No Managers Found", 
                "No Building Shop Managers found in the current scene.", "OK");
            return;
        }
        
        GenericMenu menu = new GenericMenu();
        
        foreach (var manager in managers)
        {
            menu.AddItem(new GUIContent(manager.name), selectedManager == manager, 
                () => {
                    selectedManager = manager;
                    InitializeSerializedProperties();
                    Repaint();
                });
        }
        
        menu.ShowAsContext();
    }
    
    private void AddNewShopItem()
    {
        if (shopItemsProperty == null)
            return;
            
        int newIndex = shopItemsProperty.arraySize;
        shopItemsProperty.arraySize++;
        
        SerializedProperty newItem = shopItemsProperty.GetArrayElementAtIndex(newIndex);
        newItem.FindPropertyRelative("itemId").stringValue = "item_" + (newIndex + 1);
        newItem.FindPropertyRelative("itemName").stringValue = "New Item " + (newIndex + 1);
        newItem.FindPropertyRelative("cost").intValue = 100;
        newItem.FindPropertyRelative("isPurchased").boolValue = false;
        
        SerializedProperty buildingsProperty = newItem.FindPropertyRelative("buildingsToUnlock");
        buildingsProperty.arraySize = 0;
        
        selectedItem = newIndex;
        FilterItems();
    }
    
    private void DuplicateItem(int index)
    {
        if (shopItemsProperty == null || index < 0 || index >= shopItemsProperty.arraySize)
            return;
        
        int newIndex = shopItemsProperty.arraySize;
        shopItemsProperty.arraySize++;
        
        // Get source and destination items
        SerializedProperty sourceItem = shopItemsProperty.GetArrayElementAtIndex(index);
        SerializedProperty newItem = shopItemsProperty.GetArrayElementAtIndex(newIndex);
        
        // Copy basic properties
        newItem.FindPropertyRelative("itemId").stringValue = sourceItem.FindPropertyRelative("itemId").stringValue + "_copy";
        newItem.FindPropertyRelative("itemName").stringValue = sourceItem.FindPropertyRelative("itemName").stringValue + " (Copy)";
        newItem.FindPropertyRelative("cost").intValue = sourceItem.FindPropertyRelative("cost").intValue;
        newItem.FindPropertyRelative("isPurchased").boolValue = sourceItem.FindPropertyRelative("isPurchased").boolValue;
        
        // Copy buildings array
        SerializedProperty sourceBuildings = sourceItem.FindPropertyRelative("buildingsToUnlock");
        SerializedProperty newBuildings = newItem.FindPropertyRelative("buildingsToUnlock");
        
        newBuildings.arraySize = sourceBuildings.arraySize;
        for (int i = 0; i < sourceBuildings.arraySize; i++)
        {
            newBuildings.GetArrayElementAtIndex(i).objectReferenceValue = 
                sourceBuildings.GetArrayElementAtIndex(i).objectReferenceValue;
        }
        
        selectedItem = newIndex;
        FilterItems();
    }
    
    private void ShowItemContextMenu(int index)
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Edit Item"), false, () => {
            selectedItem = index;
            Repaint();
        });
        
        menu.AddItem(new GUIContent("Duplicate Item"), false, () => {
            DuplicateItem(index);
            Repaint();
        });
        
        menu.AddSeparator("");
        
        menu.AddItem(new GUIContent("Delete Item"), false, () => {
            if (EditorUtility.DisplayDialog("Delete Item",
                    "Are you sure you want to delete this shop item?",
                    "Delete", "Cancel"))
            {
                shopItemsProperty.DeleteArrayElementAtIndex(index);
                if (selectedItem == index)
                {
                    selectedItem = -1;
                }
                FilterItems();
                Repaint();
            }
        });
        
        menu.ShowAsContext();
    }
    
    private int GetActualIndex(SerializedProperty itemProperty)
    {
        if (shopItemsProperty == null)
            return -1;
            
        // Find the actual index of the item in the original array
        for (int i = 0; i < shopItemsProperty.arraySize; i++)
        {
            if (shopItemsProperty.GetArrayElementAtIndex(i).propertyPath == itemProperty.propertyPath)
            {
                return i;
            }
        }
        return -1;
    }
    
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}