using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UM4SN
{
    public class UnityPlugin : IUnityPlugin
    {
        public virtual string Title { get; private set; }
        public virtual string Desc { get; private set; }

        public virtual void OnEnable()
        {
            Title = GetType().Assembly.GetName().Name;
            Desc = "You has no descwipshen :(";
        }

        public virtual void OnDisable()
        {

        }

        public virtual void OnGameLoad()
        {

        }

        public virtual void OnGameStart()
        {

        }

        public virtual void LargeWorldEntityLoaded(GameObject obj)
        {

        }

        public virtual void GlobalLargeWorldEntityLoaded(GameObject obj)
        {

        }

        public virtual void GameObjectLoaded(GameObject obj)
        {

        }

        public virtual void AfterCraftCreated()
        {

        }

        public virtual void GameObjectSpawned()
        {

        }

        public virtual void FixPrefab()
        {

        }
    }
}
