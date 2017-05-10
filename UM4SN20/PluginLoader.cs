using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UM4SN
{
    public class PluginLoader
    {
        static bool doneOnce = false;
        public static ICollection<UnityPlugin> plugins;
        public static void StartModLoader()
        {
            if (!doneOnce)
            {
                Debug.Log("[UM4SN] Mod loader loaded (PEEPER)");
                Debug.Log("[UM4SN] Looking in: " + new FileInfo(@".\SNUnityMod\").FullName);
                ICollection<UnityPlugin> plugins = LoadPlugins(new FileInfo(@".\SNUnityMod\").FullName);
                doneOnce = true;
            }
        }

        public static void StartModLoaderLoading()
        {
            Debug.Log("[UM4SN] Game loading. Eventing mods.");
            foreach (var item in plugins)
            {
                item.OnGameLoad();
            }
        }

        public static void ModLoaderFinishedLoading()
        {
            Debug.Log("[UM4SN] Game started. Eventing mods.");
            foreach (var item in plugins)
            {
                item.OnGameStart();
            }
            //bud I don't think this is necessary
            foreach (KeyValuePair<string, GameObject> techGo in TechTypeItems.customTechGameObjects)
            {
                string gameObjectPath = techGo.Key;
                GameObject gameObject = techGo.Value;
                if (TechTypeItems.customTechTreeNeeds.ContainsKey(TechTypeItems.customPath2Tech.FirstOrDefault(x => x.Value == techGo.Key).Key))
                {
                    (gameObject.GetComponent<Constructable>() as Constructable).InitResourceMap();
                }
            }
        }

        public static void LargeWorldEntityUMLoad(GameObject reference)
        {
            foreach (var item in plugins)
            {
                item.GlobalLargeWorldEntityLoaded(reference);
            }
        }

        public static void LargeWorldEntityLoad(LargeWorldEntity reference)
        {
            foreach (var item in plugins)
            {
                item.LargeWorldEntityLoaded(reference.gameObject);
            }
        }

        public static void GameObjectEntityLoad(GameObject reference)
        {
            foreach (var item in plugins)
            {
                item.GameObjectLoaded(reference);
            }
        }

        public static void AfterCraftCreated()
        {
            foreach (var item in plugins)
            {
                item.AfterCraftCreated();
            }
        }

        public static void GameObjectSpawned(GameObject gameObject)
        {
            if (gameObject == null) { Debug.Log("[UM4SN] Spawned GameObject is null"); return; }
            if (gameObject.name == null) { Debug.Log("[UM4SN] Spawned GameObject has a null name"); return; }
            foreach (var item in plugins)
            {
                item.GameObjectSpawned();
            }
        }

        public static void DatabaseLoaded()
        {
            TechTypeItems.FixPrefabDB();
        }

        public static void TechTreeLoaded()
        {
            TechTypeItems.FixTechTree();
        }

        public static ICollection<UnityPlugin> LoadPlugins(string path)
        {
            string[] dllFileNames = null;
            Debug.Log("[UM4SN] Attempting to load " + path);
            if (Directory.Exists(path))
            {
                dllFileNames = Directory.GetFiles(path, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    Debug.Log("[UM4SN] " + dllFile + " was found!");
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(UnityPlugin);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.BaseType == pluginType)
                            {
                                pluginTypes.Add(type);
                            }
                        }
                    }
                }

                plugins = new List<UnityPlugin>(pluginTypes.Count);
                List<string> pluginNames = new List<string>();
                foreach (Type type in pluginTypes)
                {
                    UnityPlugin plugin = (UnityPlugin)Activator.CreateInstance(type);

                    if (!(pluginNames.Contains(plugin.Title)))
                    {
                        plugins.Add(plugin);
                        pluginNames.Add(plugin.GetType().Assembly.GetName().Name);
                        try
                        {
                            plugin.OnEnable();
                            Debug.Log("[UM4SN] Loaded " + plugin.Title);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("[UM4SN] Failed to load plugin: " + ex);
                        }
                    } else
                    {
                        Debug.Log("[UM4SN] Attempted to load " + plugin.Title + " twice, skipping.");
                    }
                }
                return plugins;
            }
            return null;
        }
    }
}
