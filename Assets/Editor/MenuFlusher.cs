using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/// <summary>
/// Utility to list menu items from the Tools menu.
/// To remove items, comment out or delete the corresponding [MenuItem] method in the source script.
/// </summary>
public class MenuFlusher
{
    [MenuItem("Tools/Menu Flusher/Scan and List All Menu Items")]
    public static void ScanAllMenuItems()
    {
        var menuItemMethods = FindAllMenuItems();
        
        Debug.Log($"=== Found {menuItemMethods.Count} Menu Item(s) ===");
        foreach (var item in menuItemMethods.OrderBy(x => x.Key))
        {
            Debug.Log($"Path: {item.Key} | Class: {item.Value.DeclaringType?.Name} | Method: {item.Value.Name}");
        }

        EditorUtility.DisplayDialog("Menu Scanner", 
            $"Found {menuItemMethods.Count} menu items.\n\nSee Console for details.", 
            "OK");
    }

    [MenuItem("Tools/Menu Flusher/List Tools Menu Items")]
    public static void ListToolsMenuItems()
    {
        var menuItemMethods = FindAllMenuItems()
            .Where(x => x.Key.StartsWith("Tools/"))
            .OrderBy(x => x.Key)
            .ToList();

        Debug.Log($"=== Tools Menu Items ({menuItemMethods.Count}) ===");
        foreach (var item in menuItemMethods)
        {
            Debug.Log($"{item.Key} → {item.Value.DeclaringType?.Name}.{item.Value.Name}()");
        }

        EditorUtility.DisplayDialog("Tools Menu Scanner", 
            $"Found {menuItemMethods.Count} items in Tools menu.\n\nSee Console for details.", 
            "OK");
    }

    private static Dictionary<string, MethodInfo> FindAllMenuItems()
    {
        var result = new Dictionary<string, MethodInfo>();

        var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in allTypes)
        {
            var methods = type.GetMethods(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var menuItemAttr = method.GetCustomAttribute<MenuItem>();
                if (menuItemAttr != null)
                {
                    result[menuItemAttr.menuItem] = method;
                }
            }
        }

        return result;
    }
}
