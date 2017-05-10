using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UM4SN
{
    public static class Bundles
    {
        public static AssetBundleRequest ReadLocalBundleFile(string path)
        {
            AssetBundle assets = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + path);
            return assets.LoadAllAssetsAsync(typeof(GameObject));
        }
        public static GameObject ReadAsset(AssetBundleRequest request, string gameObjectName)
        {
            for (var i = 0; i < request.allAssets.Length; i++)
            {
                if (request.allAssets[i].name == gameObjectName)
                {
                    return request.allAssets[i] as GameObject;
                }
            }
            return null;
        }
        public static void DeleteAllOfScript(GameObject gameObject, Type type)
        {
            Component[] components = gameObject.GetComponents(type);
            foreach (Component component in components) { UnityEngine.Object.Destroy(component); }
        }
    }
}
