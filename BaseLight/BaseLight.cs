using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM4SN;
using UnityEngine;

namespace BaseLight
{
    public class BaseLight : UnityPlugin
    {
        public override string Title
        {
            get
            {
                return "Base Light";
            }
        }
        public override string Desc
        {
            get
            {
                return "Toggles base light";
            }
        }

        public override void OnEnable()
        {
            new GameObject().AddComponent<OnLoadBaseLight>();
        }

        public override void AfterCraftCreated()
        {
            new GameObject().AddComponent<AfterCraftCreatedBaseLight>();
        }

        public override void OnGameStart()
        {
            KnownTech.Add(TechType.id_6289, true);
        }
    }

    public class OnLoadBaseLight : MonoBehaviour
    {
        public void Start()
        {
            TechTypeItems.TechTreeData[] techTreeData = new TechTypeItems.TechTreeData[2];
            techTreeData[0].count = 1;
            techTreeData[0].needs = TechType.Titanium;
            techTreeData[1].count = 2;
            techTreeData[1].needs = TechType.CopperWire;
            TechTypeItems.registerCustomTechNeed(TechType.id_6289, techTreeData, TechGroup.Miscellaneous, TechCategory.Misc);
        }
    }

    public class AfterCraftCreatedBaseLight : MonoBehaviour
    {
        public void Start()
        {
            string classID = Guid.NewGuid().ToString();
            GameObject StarshipMonitor = new GameObject();
            GameObject StarshipMonitor_ref = Resources.Load("Submarine/Build/StarshipMonitor") as GameObject;

            //copy start
            GameObject UNITYID_63300 = StarshipMonitor_ref.transform.GetChild(0).gameObject; //Starship_wall_monitor_01_03
            GameObject UNITYID_32295 = StarshipMonitor_ref.transform.GetChild(1).gameObject; //Cube
            GameObject UNITYID_35434 = StarshipMonitor_ref.transform.GetChild(2).gameObject; //Cube (1)
            //copy end

            PrefabIdentifier sc_prefabIdentifier = StarshipMonitor.AddComponent<PrefabIdentifier>();
            sc_prefabIdentifier.ClassId = classID;
            LargeWorldEntity sc_largeWorldEntity = StarshipMonitor.AddComponent<LargeWorldEntity>();
            sc_largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Far;
            Constructable sc_constructable = StarshipMonitor.AddComponent<Constructable>();
            sc_constructable.allowedOnWall = true;
            sc_constructable.allowedOnGround = false;
            sc_constructable.allowedOnCeiling = false;
            sc_constructable.allowedInSub = false;
            sc_constructable.forceUpright = false;
            sc_constructable.deconstructionAllowed = true;
            sc_constructable.rotationEnabled = false;
            sc_constructable.techType = TechType.id_6289;
            sc_constructable.model = null;
            StarshipMonitor.AddComponent<TechTag>().type = TechType.id_6289;
            ConstructableBounds sc_constructableBounds = new ConstructableBounds();
            sc_constructableBounds.bounds = new OrientedBounds(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f), new Vector3(1.5f, 0.5f, 1.5f));
            GameObject I_UNITYID_63300 = Instantiate(UNITYID_63300);
            GameObject I_UNITYID_32295 = Instantiate(UNITYID_32295);
            GameObject I_UNITYID_35434 = Instantiate(UNITYID_35434);
            I_UNITYID_63300.transform.parent = StarshipMonitor.transform;
            I_UNITYID_32295.transform.parent = StarshipMonitor.transform;
            I_UNITYID_35434.transform.parent = StarshipMonitor.transform;
            StarshipMonitor.AddComponent<BaseLightScript>();
            sc_constructable.model = I_UNITYID_63300;
            TechTypeItems.registerCustomTechType(6289, classID, "Light switch", "Turn the lights off and on!", EquipmentType.None, StarshipMonitor, "nesraksmods/baseswitch");
        }
    }

    public class BaseLightScript : HandTarget, IHandTarget
    {
        bool isOn = true;

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText("Use", string.Empty);
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (isOn)
            {
                GetComponentInParent<SubRoot>().ForceLightingState(false);
            } else
            {
                GetComponentInParent<SubRoot>().ForceLightingState(true);
            }
            isOn = !isOn;
        }
    }
}
