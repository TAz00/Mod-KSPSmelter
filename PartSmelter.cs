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


        [KSPAction("Spawn Fuel Canister")]
        public void ActionGroupSpawnFuel(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("FL-T400.craft");
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Spawn Fuel Canister", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuSpawnFuel()
        {
            SpawnCraftFromCraftFile("FL-T400.craft");
        }
        [KSPAction("Spawn Ore Canister")]
        public void ActionGroupSpawnOre(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("Ore.craft");
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Spawn Ore Canister", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuSpawnOre()
        {
            SpawnCraftFromCraftFile("Ore.craft");
        }
        [KSPAction("Spawn Terrier Engine")]
        public void ActionGroupSpawnTerrier(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("Terrier.craft");
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Spawn Terrier Engine", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuSpawnTerrier()
        {
            SpawnCraftFromCraftFile("Terrier.craft");
        }
        [KSPAction("Spawn StructualFuselage")]
        public void ActionGroupSpawnFuselage(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("StructualFuselage.craft");
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Spawn StructualFuselage", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuSpawnFuselage()
        {
            SpawnCraftFromCraftFile("StructualFuselage.craft");
        }
        [KSPAction("Spawn HubMax")]
        public void ActionGroupSpawnHubmax(KSPActionParam param)
        {
            SpawnCraftFromCraftFile("HubMax.craft");
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Spawn HubMax", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuSpawnHubmax()
        {
            SpawnCraftFromCraftFile("HubMax.craft");
        }

        [KSPEvent(active = false, guiActive = true, guiName = "Resume Print", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuResumePrint()
        {
            //Resume printing
            isPrinting = true;
            //Hide cancel/resume
            Events["ContextMenuResumePrint"].active = false;
            Events["ContextMenuCancelPrint"].active = false;
        }
        [KSPEvent(active = false, guiActive = true, guiName = "Cancel Print (lose ore)", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ContextMenuCancelPrint()
        {
            //Print cancelled, reset progress timers
            TimerEcho = 0;
            TimerFoxtrot = 0;
            //Set status
            Status = "Ready";
            //Hide cancel/resume
            Events["ContextMenuResumePrint"].active = false;
            Events["ContextMenuCancelPrint"].active = false;
            //Display print menus
            TogglePrintEvents(true);
        }

        [KSPField(isPersistant = false, guiActive = true, guiName = "Print")]
        public string Status = "Ready";

        /// <summary>
        /// Initate Printing Process
        /// </summary>
        private void SpawnCraftFromCraftFile(string filename)
        {
            if (!isPrinting) //Implicit, since we hide the menus
            {
                //No more printing menus
                TogglePrintEvents(false);
                //Store filename
                FilePrinted = filename;
                //Initiate printing
                isPrinting = true;
            }

        }
        /// <summary>
        /// Place to store the file being printed while we wait for completion.
        /// </summary>
        private string FilePrinted = "";

        /// <summary>
        /// Printing Process Complete
        /// </summary>
        private void PrintComplete()
        {
            //Setup spawn offset vector
            float OffsetX = 03.0f;
            float OffsetY = 00.0f;
            float OffsetZ = 00.0f;
            Vector3 Offset = new Vector3(OffsetX, OffsetY, OffsetZ);
            //Spawn part
            PartSpawner partSpawner = new PartSpawner(FilePrinted, this.part, Offset);
            partSpawner.Spawn();
        }

        /// <summary>
        /// Power consumption read form part cfg
        /// </summary>
        [KSPField(isPersistant = false)]
	    public float PowerConsumption;
        /// <summary>
        /// Ore consumption read form part cfg
        /// </summary>
        [KSPField(isPersistant = false)]
        public float OreConsumption;

        /// <summary>
        /// Store printing state
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool isPrinting = false;
        /// <summary>
        /// Store printing progress
        /// </summary>
        [KSPField(isPersistant = true)]
        private double TimerEcho;
        /// <summary>
        /// Store printing progress
        /// </summary>
        [KSPField(isPersistant = true)]
        private double TimerFoxtrot;

        /// <summary>
        /// Called every unity frame
        /// </summary>
        public void FixedUpdate()
        {
            //Print time is set to 30 seconds
            var electricThreshold = 30.0;
            var oreThreshold = 30.0;
            //Are we printing?
            if (isPrinting && this.vessel != null && this.vessel.gameObject.activeSelf)
            {
 
                //Use ElectricCharge
                var energyRequest = this.PowerConsumption * TimeWarp.fixedDeltaTime;
                var energyReceived = this.part.RequestResource("ElectricCharge", energyRequest);
                var energyRatio = energyReceived / energyRequest;
                TimerEcho += TimeWarp.deltaTime * energyRatio;

                //Use Ore in relation to energy (it will still print on low power)
                var oreRequest = this.OreConsumption * energyRatio * TimeWarp.fixedDeltaTime;
                var oreReceived = this.part.RequestResource("Ore", oreRequest);
                var oreRatio = oreReceived / oreRequest;
                TimerFoxtrot += TimeWarp.deltaTime * oreRatio;

                //Show a nice percentage
                var PercentDone = TimerEcho / electricThreshold;
                Status = "Busy (" + PercentDone.ToString("0%") + ")";

                //Did not receive enough ore in relation to energy, so we've run out of ore?
                if (oreRatio < 1)
                {
                    //Stop Print
                    isPrinting = false;

                    //Don't reset timers, as to pause print instead
                    //Print Paused
                    Status = "Paused, ran out of ore";
                    Events["ContextMenuResumePrint"].active = true;
                    Events["ContextMenuCancelPrint"].active = true;
                }

                //Does not check if ore and electricity is used in equal ratios, only that both are used
                if (TimerEcho >= electricThreshold && TimerFoxtrot >= oreThreshold)
                {
                    //Stop Print
                    isPrinting = false;
                    TimerEcho = 0;
                    TimerFoxtrot = 0;
                    //Spawn Part
                    PrintComplete();
                    //We can print again
                    TogglePrintEvents(true);
                    Status = "Ready";
                }
            }
            base.OnFixedUpdate();
        }

        /// <summary>
        /// Hide or display part printing events
        /// </summary>
        /// <param name="active"></param>
        private void TogglePrintEvents(bool active)
        {
            //Disable other prints while printing, enable when done
            Events["ContextMenuSpawnHubmax"].active = active;
            Events["ContextMenuSpawnFuselage"].active = active;
            Events["ContextMenuSpawnTerrier"].active = active;
            Events["ContextMenuSpawnOre"].active = active;
            Events["ContextMenuSpawnFuel"].active = active;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }


        public void Update()
        {

        }

    }
}
