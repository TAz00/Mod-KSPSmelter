using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPSmelter
{
        /// <summary>
        /// My first part!
        /// </summary>
    public class ModuleSmelter : ModuleCargoBay  //PartModule //ModuleCargoBay
    {

        /// <summary>
        /// Called when the part is started by Unity.
        /// </summary>
        public override void OnStart(StartState state)
        {
            // Add stuff to the log
            print("Hello, Kerbin!");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        [KSPAction("Spawn Fuel Canister")]
        public void ActionGroupSpawnFuel(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("FL-T400.craft");
        }

        [KSPEvent(name = "ContextMenuSpawnFuelCanister", active = true, guiActive = true, guiName = "Spawn Fuel Canister")]
        public void ContextMenuSpawnFuel()
        {
            SpawnCraftFromCraftFile("FL-T400.craft");
        }
        [KSPAction("Spawn Ore Canister")]
        public void ActionGroupSpawnOre(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("Ore.craft");
        }

        [KSPEvent(name = "ContextMenuSpawnOreCanister", active = true, guiActive = true, guiName = "Spawn Ore Canister")]
        public void ContextMenuSpawnOre()
        {
            SpawnCraftFromCraftFile("Ore.craft");
        }
        [KSPAction("Spawn Terrier Engine")]
        public void ActionGroupSpawnTerrier(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("Terrier.craft");
        }

        [KSPEvent(name = "ContextMenuSpawnTerrier", active = true, guiActive = true, guiName = "Spawn Terrier Engine")]
        public void ContextMenuSpawnTerrier()
        {
            SpawnCraftFromCraftFile("Terrier.craft");
        }
        [KSPAction("Spawn StructualFuselage")]
        public void ActionGroupSpawnFuselage(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("StructualFuselage.craft");
        }

        [KSPEvent(name = "ContextMenuSpawnStructualFuselage", active = true, guiActive = true, guiName = "Spawn StructualFuselage")]
        public void ContextMenuSpawnFuselage()
        {
            SpawnCraftFromCraftFile("StructualFuselage.craft");
        }
        [KSPAction("Spawn HubMax")]
        public void ActionGroupSpawnHubmax(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("HubMax.craft");
        }

        [KSPEvent(name = "ContextMenuSpawnHubMax", active = true, guiActive = true, guiName = "Spawn HubMax")]
        public void ContextMenuSpawnHubmax()
        {
            SpawnCraftFromCraftFile("HubMax.craft");
        }


        /// <summary>
        /// Now for the thing that WILL work
        /// </summary>
        private void SpawnCraftFromCraftFile(string filename)
        {
            float OffsetX = 03.0f;// 2.5f;
            float OffsetY = 00.0f;// 4.0f;
            float OffsetZ = 00.0f;
            Vector3 Offset = new Vector3(OffsetX, OffsetY, OffsetZ);

            PartSpawner partSpawner = new PartSpawner(filename, this.part, Offset);
            partSpawner.Spawn();

        }

        /// <summary>
        /// Load & Save Vessel Confignode when vessel loads
        /// </summary>
        /// <param name="configNode"></param>
        public override void OnLoad(ConfigNode configNode)
        {
            base.OnLoad(configNode);

        }

    }
}
