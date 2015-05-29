using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPSmelter
{
    /// <summary>
    /// This class is pretty much built on what KAS uses to pull parts out of containers
    /// All credit to https://github.com/KospY/KAS
    /// https://github.com/KospY/KAS/blob/master/LICENSE.md
    /// </summary>
    public class VesselSpawner
    {
        const string PluginPath = "\\GameData\\KSPSmelter\\PluginData\\";

        private HideConfigNode _OldVabShip;
        private string _craftFile = "";
        private Part _srcPart = null;
        private Vector3 _spawnOffset = new Vector3(0,0,0);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_craftFile">Craft file name as it appears in the PluginPath folder</param>
        /// <param name="_srcPart">Source part to spawn relative to</param>
        /// <param name="_spawnOffset">Offset spawn from Source part position</param>
        public VesselSpawner(string _craftFile, Part _srcPart, Vector3 _spawnOffset)
        {
            //Store paths
            this._craftFile = _craftFile;
            this._srcPart = _srcPart;
            this._spawnOffset = _spawnOffset;

            if (this._srcPart == null)
                Debug.Log("Relative source part can't be null");
            if ((_craftFile != String.Empty))
                Debug.Log("Source part path can't be null");

        }
        /// <summary>
        /// Spawns the vessel
        /// </summary>
        public void SpawnVessel()
        {
            //Load craft file
            ShipConstruct _ship = LoadVessel(this._craftFile);
            if (_ship != null)
                SpawnVessel(_ship, this._srcPart, this._spawnOffset);
            else
                Debug.Log("Failed to load the vessel");
        }
        /// <summary>
        /// Attempt vessel load
        /// </summary>
        /// <param name="_craftFile"></param>
        /// <returns></returns>
        private ShipConstruct LoadVessel(string _craftFile)
        {
            //Get path to vessel
            string BasePath = Environment.CurrentDirectory + PluginPath;
            string path = BasePath + _craftFile;

            //Save old ship for later, else player will see it in the VAB
            _OldVabShip = new HideConfigNode(ShipConstruction.ShipConfig);

            //Load craft file
            ShipConstruct _shipConstruct = ShipConstruction.LoadShip(path);

            //Check load
            if (_shipConstruct == null)
            {
                // Restore ShipConstruction ship, otherwise player sees loaded craft in VAB
                ShipConstruction.ShipConfig = _OldVabShip.GetConfigNode();
                return null; //Fail
            }
            return _shipConstruct;
        }
        /// <summary>
        /// Spawn ship construct
        /// https://github.com/KospY/KAS/blob/master/Plugin/KAS_Shared.cs
        /// </summary>
        /// <param name="_shipConstruct">Shipconstruct to spawn</param>
        /// <param name="_srcPart">Source part to spawn relative to</param>
        /// <param name="_spawnOffset">Offset spawn from Source part position</param>
        private void SpawnVessel(ShipConstruct _shipConstruct, Part _srcPart, Vector3 _spawnOffset)
        {
            //Store construct root
            Part _newConstructRootPart = _shipConstruct.parts[0];

            //Center rootpart
            Vector3 offset = _newConstructRootPart.transform.localPosition;
            _newConstructRootPart.transform.Translate(-offset);

            //Get launch spawn point, relative to part
            Transform t = _srcPart.transform;
            GameObject launchPos = new GameObject();
            launchPos.transform.parent = _srcPart.transform;
            launchPos.transform.position = t.position;
            launchPos.transform.position += t.TransformDirection(_spawnOffset);
            launchPos.transform.rotation = t.rotation;
            //Store our launch / spawn position
            Transform launchTransform = launchPos.transform;
            //Kill original object
            launchPos.DestroyGameObject();
            //Set rootpart origin
            _shipConstruct.Parts[0].localRoot.transform.Translate(launchPos.transform.position, Space.World);
            //Position
            float angle;
            Vector3 axis;
            //Extract ToAngleAxis data from selected spawning location
            launchTransform.rotation.ToAngleAxis(out angle, out axis);
            //TRANSFORM Rotate localRootPart in relation to root
            _shipConstruct.Parts[0].localRoot.transform.RotateAround(launchTransform.position, axis, angle);

            //Create vessel object
            Vessel _newVessel = _newConstructRootPart.localRoot.gameObject.AddComponent<Vessel>();
            //Attach vessel information
            _newVessel.id = Guid.NewGuid();
            _newVessel.vesselName = _srcPart.vessel.vesselName + " - " + _shipConstruct.shipName;
            _newVessel.landedAt = _srcPart.vessel.vesselName;

            //Store backup
            ShipConstruction.CreateBackup(_shipConstruct);

            //Init from VAB
            _newVessel.Initialize(true);
            //Set Landed
            _newVessel.Landed = true;

            //Set Orbit
            InitiateOrbit(launchTransform.position, _srcPart.vessel, _newVessel);

            //Set Mission info
            uint missionId = (uint)Guid.NewGuid().GetHashCode();
            string flagUrl = _srcPart.flagURL;
            uint launchId = HighLogic.CurrentGame.launchID++;

            //Set part mission info
            for (int i = 0; i < _newVessel.parts.Count; i++)
            {
                Part part = _newVessel.parts[i];
                part.flightID = ShipConstruction.GetUniqueFlightID(FlightDriver.FlightStateCache.flightState);
                part.flagURL = flagUrl;
                part.launchID = launchId;
                part.missionID = missionId;
            }

            //Generate staging
            Staging.beginFlight();
            _newConstructRootPart.vessel.ResumeStaging();
            Staging.GenerateStagingSequence(_newConstructRootPart.localRoot);
            Staging.RecalculateVesselStaging(_newConstructRootPart.vessel);

            //Set position, again
            _newVessel.SetPosition(launchTransform.position);
            _newVessel.SetRotation(launchTransform.rotation);

            ////Get allll the parts, does it even matter since we launch from VAB?
            //for (int i = 0; i < _newVessel.parts.Count; i++)
            //{
            //    // Solar panels from containers (we dont have no bloody containers here tho) don't work otherwise
            //    for (int j = 0; j < _newVessel.parts[i].Modules.Count; j++)
            //    {
            //        ConfigNode node = new ConfigNode();
            //        node.AddValue("name", _newVessel.parts[i].Modules[j].moduleName);
            //        _newVessel.parts[i].LoadModule(node, ref j);
            //    }
            //}


            //Save Protovessel
            ProtoVessel _newProto = new ProtoVessel(_newVessel);

            //Kill and remove spawned vessel, had some serious problems with spawn position warping/glitching
            _newVessel.Die();

            //Set the protovessels position to the relative one we found, maybe redundant
            _newProto.position = launchPos.transform.position;

            //If you check this value, you will see the height change from launch scene to resume scene, extra dafuq
            float height = _newProto.height;
            
            if (FlightDriver.StartupBehaviour == FlightDriver.StartupBehaviours.RESUME_SAVED_FILE ||
                FlightDriver.StartupBehaviour == FlightDriver.StartupBehaviours.RESUME_SAVED_CACHE)
            {
                //Odd behaviour with positioning during different flight scenes, workaround awaaaay
                _newProto.height = TrueAlt(launchTransform.position,_srcPart.vessel);
            }

            //Load Protovessel
            _newProto.Load(HighLogic.CurrentGame.flightState);

            // Restore ShipConstruction ship, otherwise player sees loaded craft in VAB
            ShipConstruction.ShipConfig = _OldVabShip.GetConfigNode();

            //Fix Control Lock
            FlightInputHandler.ResumeVesselCtrlState(FlightGlobals.ActiveVessel);
            //Fix active vessel staging
            FlightGlobals.ActiveVessel.ResumeStaging();

        }
        /// <summary>
        /// http://forum.kerbalspaceprogram.com/threads/111116-KSP-Altitude-Calculation-Inquiry
        /// </summary>
        /// <returns></returns>
        private float TrueAlt(Vector3 _LauncPos, Vessel _srcVessel)
        {
            //Vector3 pos = _srcPart.transform.position; //or this.vessel.GetWorldPos3D()
            float ASL = FlightGlobals.getAltitudeAtPos(_LauncPos);
            if (_srcVessel.mainBody.pqsController == null) { return ASL; }
            float terrainAlt = Convert.ToSingle(_srcVessel.pqsAltitude);
            if (_srcVessel.mainBody.ocean && _srcVessel.heightFromTerrain <= 0) { return ASL; } //Checks for oceans
            return ASL - terrainAlt;
        }
        /// <summary>
        /// https://github.com/taniwha-qf/Extraplanetary-Launchpads/blob/master/Source/BuildControl.cs
        /// https://github.com/taniwha-qf/Extraplanetary-Launchpads/blob/master/License.txt
        /// </summary>
        /// <param name="_newVessel"></param>
        /// <param name="_srcVessel"></param>
        private void InitiateOrbit(Vector3 _spawnPoint, Vessel _srcVessel, Vessel _newVessel)
        {
            var mode = OrbitDriver.UpdateMode.UPDATE;
            _newVessel.orbitDriver.SetOrbitMode(mode);

            var craftCoM = GetVesselWorldCoM(_newVessel);
            var vesselCoM = _spawnPoint;
            var offset = (Vector3d.zero + craftCoM - vesselCoM).xzy;

            var corb = _newVessel.orbit;
            var orb = _srcVessel.orbit;
            var UT = Planetarium.GetUniversalTime();
            var body = orb.referenceBody;
            corb.UpdateFromStateVectors(orb.pos + offset, orb.vel, body, UT);

            Debug.Log(String.Format("[EL] {0} {1}", "orb", orb.pos));
            Debug.Log(String.Format("[EL] {0} {1}", "corb", corb.pos));

        }
        public Vector3 GetVesselWorldCoM(Vessel v)
        {
            var com = v.findLocalCenterOfMass();
            return v.rootPart.partTransform.TransformPoint(com);
        }
    }
}
