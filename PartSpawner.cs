using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPSmelter
{
    public class PartSpawner
    {
        /// <summary>
        /// Path of plugin
        /// </summary>
        const string PluginPath = "\\GameData\\KSPSmelter\\PluginData\\";
        /// <summary>
        /// Craftfile name
        /// </summary>
        private string _craftFile = null;
        /// <summary>
        /// Source part from which new part will spawn
        /// </summary>
        private Part _srcPart = null;
        /// <summary>
        /// Loaded vessel construct root part
        /// </summary>
        private Part _newConstructRootPart = null;
        private Vector3 _spawnOffset;

        private Vessel _newVessel = null;
        /// <summary>
        /// Stores active vessel when spawning
        /// </summary>
        private Vessel _activeVessel = null;
        /// <summary>
        /// Loaded vessel construct
        /// </summary>
        private ShipConstruct _shipConstruct = null;
        /// <summary>
        /// HideConfigNode is a normal ConfigNode, except we use this class to hide the struture from the debugger, 
        /// so it dosn't accidently an infinite
        /// </summary>
        private HideConfigNode _OldVabShip = null;

        public PartSpawner(string _craftFile, Part _srcPart, Vector3 _spawnOffset)
        {
            this._craftFile = _craftFile;
            this._srcPart = _srcPart;
            this._spawnOffset = _spawnOffset;
            
            if (this._srcPart == null)
                throw new Exception("Source part can't be null");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_craftFile"></param>
        private void PreLoad(string _craftFile)
        {
            //Save current VAB ship
            _OldVabShip = new HideConfigNode(ShipConstruction.ShipConfig);
            //Save Active Vessel
            _activeVessel = FlightGlobals.ActiveVessel;
        }
        /// <summary>
        /// Loads ship from craft file
        /// </summary>
        /// <param name="_craftFile"></param>
        /// <returns></returns>
        private bool Load(string _craftFile)
        {
            string BasePath = Environment.CurrentDirectory + PluginPath;
            string path = BasePath + _craftFile;

            //Load craft file
            _shipConstruct = ShipConstruction.LoadShip(path);

            //Check load
            if (_shipConstruct == null)
            {
                return false; //Fail
            }
            //Store construct root
            _newConstructRootPart = _shipConstruct.parts[0];

            return true;
        }
        /// <summary>
        /// Moves vessel to spawn point
        /// </summary>
        /// <returns></returns>
        private void LaunchTransform()
        {
            //Fields
            float angle;
            Vector3 axis;

            //Center on current vessel
            Vector3 flipOffset = _newConstructRootPart.transform.localPosition;
            //TRANSFORM
            _newConstructRootPart.transform.Translate(-flipOffset);

            //THESE ARE ALL DEBUG VALUES
            bool activeAndEnabled = _srcPart.isActiveAndEnabled;
            bool isPersistent = _srcPart.isPersistent;
            double terrainAltitude = _srcPart.vessel.terrainAltitude;
            bool vesselIsPersistent =_srcPart.vessel.isPersistent;
            Transform partTransform = _srcPart.FindModelTransform("ModuleSmelter");
            Vector3d position = _srcPart.vessel.findWorldCenterOfMass();

            //ERROR / BUG / FIXME
            //This function is inconsistent, input is the same every time, yet it returns 0 0 0 when resuming from cache/save, instead of 
            //returning correct value
            Vector3 calculatedPosWhenResume = _srcPart.vessel.mainBody.GetWorldSurfacePosition(_srcPart.vessel.latitude, _srcPart.vessel.longitude, _srcPart.vessel.altitude);
            //Same goes for
            Vector3 worldPos = _srcPart.vessel.GetWorldPos3D();
            //This results in incorrect placement

            //Get launch transform posistions
            GameObject launchPos = new GameObject();
            launchPos.transform.position = _srcPart.transform.position;
            launchPos.transform.rotation = _srcPart.transform.rotation;

            //Get Transform
            Transform launchTransform = launchPos.transform;

            //Destroy launchpos
            launchPos.DestroyGameObject();
      
            //Extract ToAngleAxis data from selected spawning location
            launchTransform.rotation.ToAngleAxis(out angle, out axis);

            //TRANSFORM localRootPart posistion and apply offset
            _newConstructRootPart.localRoot.transform.Translate(launchTransform.position + _spawnOffset, Space.World);

            //TRANSFORM Rotate localRootPart in relation to root
            _newConstructRootPart.localRoot.transform.RotateAround(launchTransform.position, axis, angle);

        }

        private void CreateDummyVessel()
        {
            //Create Vessel
            _newVessel = _newConstructRootPart.localRoot.gameObject.AddComponent<Vessel>();

            //Name Vessel
            _newVessel.id = Guid.NewGuid();
            _newVessel.vesselName = _srcPart.vessel.vesselName + " - " + _shipConstruct.shipName;
            _newVessel.landedAt = _srcPart.vessel.vesselName;

            //Create backup values, ???
            ShipConstruction.CreateBackup(_shipConstruct);

        }

        private void InitializeDummyVessel()
        {
            //Magic Initialize
            _newVessel.Initialize(true);//True? sure
        }

        private void SetupVessel()
        {
            //Set spawn height
            //_newVessel.heightFromTerrain = TrueAlt(_srcPart, _srcPart.vessel);
            
            //Set Landed
            _newVessel.Landed = true;

            //Set Mission
            uint missionId = (uint)Guid.NewGuid().GetHashCode();
            string flagUrl = _srcPart.flagURL;
            uint launchId = HighLogic.CurrentGame.launchID++;

            //Set Part data
            for (int i = 0; i < _newVessel.parts.Count; i++)
            {
                Part part = _newVessel.parts[i];
                part.flightID = ShipConstruction.GetUniqueFlightID(FlightDriver.FlightStateCache.flightState);
                part.flagURL = flagUrl;
                part.launchID = launchId;
                part.missionID = missionId;
            }
        }

        /// <summary>
        /// http://forum.kerbalspaceprogram.com/threads/111116-KSP-Altitude-Calculation-Inquiry
        /// </summary>
        /// <returns></returns>
        private float TrueAlt(Part _srcPart, Vessel _srcVessel)
        {
            Vector3 pos = _srcPart.transform.position; //or this.vessel.GetWorldPos3D()
            float ASL = FlightGlobals.getAltitudeAtPos(pos);
            if (_srcVessel.mainBody.pqsController == null) { return ASL; }
            float terrainAlt = Convert.ToSingle(_srcVessel.pqsAltitude);
            if (_srcVessel.mainBody.ocean && _srcVessel.heightFromTerrain <= 0) { return ASL; } //Checks for oceans
            return ASL - terrainAlt;
        }

        /// <summary>
        /// https://github.com/taniwha-qf/Extraplanetary-Launchpads/blob/a38adaf4131ccc97d192403f5303e937b967e821/Source/BuildControl.cs
        /// </summary>
        /// <param name="_newVessel"></param>
        /// <param name="_srcVessel"></param>
        private void InitiateOrbit()
        {         
            Vessel _srcVessel = _srcPart.vessel;

            var mode = OrbitDriver.UpdateMode.UPDATE;
            _newVessel.orbitDriver.SetOrbitMode(mode);
            var corb = _newVessel.orbit;
            var orb = _srcVessel.orbit;
            var UT = Planetarium.GetUniversalTime();
            var refBody = orb.referenceBody;
            corb.UpdateFromStateVectors(orb.pos, orb.vel, refBody, UT);
        }

        private void PostLoad()
        {

            // Restore ShipConstruction ship, otherwise player sees loaded craft in VAB
            ShipConstruction.ShipConfig = _OldVabShip.GetConfigNode();

            //Fix Control Lock
            FlightInputHandler.ResumeVesselCtrlState(FlightGlobals.ActiveVessel);

            //We should be fine
            FlightGlobals.SetActiveVessel(_activeVessel);

            //Dangerous
            InputLockManager.ClearControlLocks();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Spawn()
        {
            //Preload
            PreLoad(_craftFile);

            //Load Craft
            if (Load(_craftFile))
            {
                LaunchTransform();
                CreateDummyVessel();
                InitializeDummyVessel();
                SetupVessel();
                InitiateOrbit();
                PostLoad();
            }
        }
    }
}
