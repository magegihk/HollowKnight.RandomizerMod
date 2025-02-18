﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Modding;
using RandomizerMod.Actions;
using RandomizerMod.Randomization;
using SeanprCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RandomizerMod.LogHelper;
using static RandomizerMod.GiveItemActions;
using RandomizerMod.SceneChanges;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod
    {
        private static Dictionary<string, Sprite> _sprites;
        private static Dictionary<string, string> _secondaryBools;

        private static Thread _logicParseThread;

        public static RandomizerMod Instance { get; private set; }

        public SaveSettings Settings { get; set; } = new SaveSettings();

        public override ModSettings SaveSettings
        {
            get => Settings = Settings ?? new SaveSettings();
            set => Settings = value is SaveSettings saveSettings ? saveSettings : Settings;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloaded)
        {
            if (Instance != null)
            {
                LogWarn("Attempting to make multiple instances of mod, ignoring");
                return;
            }

            // Set instance for outside use
            Instance = this;

            // Make sure the play mode screen is always unlocked
            Ref.GM.EnablePermadeathMode();

            // Unlock godseeker too because idk why not
            Ref.GM.SetStatusRecordInt("RecBossRushMode", 1);

            Assembly randoDLL = GetType().Assembly;

            // Load embedded resources
            _sprites = ResourceHelper.GetSprites("RandomizerMod.Resources.");
            
            try
            {
                LanguageStringManager.LoadLanguageXML(
                    randoDLL.GetManifestResourceStream("RandomizerMod.Resources.language.xml"));
            }
            catch (Exception e)
            {
                LogError("Could not process language xml:\n" + e);
            }

            _logicParseThread = new Thread(() =>
            LogicManager.ParseXML(randoDLL));
            _logicParseThread.Start();

            // Add hooks
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.LanguageGetHook += LanguageStringManager.GetLanguageString;
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;
            On.PlayMakerFSM.OnEnable += FixVoidHeart;
            On.GameManager.BeginSceneTransition += EditTransition;

            RandomizerAction.Hook();
            BenchHandler.Hook();
            SceneEditor.Hook();

            // Setup preloaded objects
            ObjectCache.GetPrefabs(preloaded);

            // Some items have two bools for no reason, gotta deal with that
            _secondaryBools = new Dictionary<string, string>
            {
                {nameof(PlayerData.hasDash), nameof(PlayerData.canDash)},
                {nameof(PlayerData.hasShadowDash), nameof(PlayerData.canShadowDash)},
                {nameof(PlayerData.hasSuperDash), nameof(PlayerData.canSuperDash)},
                {nameof(PlayerData.hasWalljump), nameof(PlayerData.canWallJump)},
                {nameof(PlayerData.gotCharm_23), nameof(PlayerData.fragileHealth_unbreakable)},
                {nameof(PlayerData.gotCharm_24), nameof(PlayerData.fragileGreed_unbreakable)},
                {nameof(PlayerData.gotCharm_25), nameof(PlayerData.fragileStrength_unbreakable)}
            };

            _logicParseThread.Join(); // new update -- logic manager is needed to supply start locations to menu
            Log(LogicManager.StartLocations == null);
            MenuChanger.EditUI();
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                (SceneNames.Tutorial_01, "_Props/Chest/Item/Shiny Item (1)"),
                (SceneNames.Tutorial_01, "_Enemies/Crawler 1"),
                (SceneNames.Tutorial_01, "_Props/Cave Spikes (1)"),
                (SceneNames.Tutorial_01, "_Markers/Death Respawn Marker"),
                (SceneNames.Tutorial_01, "_Scenery/plat_float_17"),
                (SceneNames.Ruins_House_01, "Grub Bottle/Grub")
            };
        }

        public static Sprite GetSprite(string name)
        {
            if (_sprites != null && _sprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            return null;
        }

        public static bool LoadComplete()
        {
            return _logicParseThread == null || !_logicParseThread.IsAlive;
        }

        public void StartNewGame()
        {
            // Charm tutorial popup is annoying, get rid of it
            Ref.PD.hasCharm = true;

            //Lantern start for easy mode
            if (RandomizerMod.Instance.Settings.FreeLantern)
            {
                PlayerData.instance.hasLantern = true;
            }

            if (RandomizerMod.Instance.Settings.EarlyGeo)
            {
                PlayerData.instance.AddGeo(300);
            }

            // Fast boss intros
            Ref.PD.unchainedHollowKnight = true;
            Ref.PD.encounteredMimicSpider = true;
            Ref.PD.infectedKnightEncountered = true;
            Ref.PD.mageLordEncountered = true;
            Ref.PD.mageLordEncountered_2 = true;

            if (!Settings.Randomizer)
            {
                return;
            }

            if (!LoadComplete())
            {
                _logicParseThread.Join();
            }

            RandoLogger.InitializeTracker();
            RandoLogger.InitializeSpoiler();

            try
            {
                Randomizer.Randomize();

                RandoLogger.UpdateHelperLog(forceUpdate: true);
            }
            catch (Exception e)
            {
                LogError("Error in randomization:\n" + e);
            }
        }

        public override string GetVersion()
        {
            string ver = "3.03";
            int minAPI = 51;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            if (apiTooLow)
            {
                return ver + " (Update API)";
            }

            return ver;
        }

        private void UpdateCharmNotches(PlayerData pd)
        {
            // Update charm notches
            if (Settings.CharmNotch)
            {
                if (pd == null)
                {
                    return;
                }

                pd.CountCharms();
                int charms = pd.charmsOwned;
                int notches = pd.charmSlots;

                if (!pd.salubraNotch1 && charms >= 5)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch1), true);
                    notches++;
                }

                if (!pd.salubraNotch2 && charms >= 10)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch2), true);
                    notches++;
                }

                if (!pd.salubraNotch3 && charms >= 18)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch3), true);
                    notches++;
                }

                if (!pd.salubraNotch4 && charms >= 25)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch4), true);
                    notches++;
                }

                pd.SetInt(nameof(PlayerData.charmSlots), notches);
                Ref.GM.RefreshOvercharm();
            }
        }

        private bool BoolGetOverride(string boolName)
        {
            // Fake spell bools
            if (boolName == "hasVengefulSpirit")
            {
                return Ref.PD.fireballLevel > 0;
            }

            if (boolName == "hasShadeSoul")
            {
                return Ref.PD.fireballLevel > 1;
            }

            if (boolName == "hasDesolateDive")
            {
                return Ref.PD.quakeLevel > 0;
            }

            if (boolName == "hasDescendingDark")
            {
                return Ref.PD.quakeLevel > 1;
            }

            if (boolName == "hasHowlingWraiths")
            {
                return Ref.PD.screamLevel > 0;
            }

            if (boolName == "hasAbyssShriek")
            {
                return Ref.PD.screamLevel > 1;
            }

            // This variable is incredibly stubborn, not worth the effort to make it cooperate
            // Just override it completely
            if (boolName == nameof(PlayerData.gotSlyCharm) && Settings.Randomizer)
            {
                return Settings.SlyCharm;
            }

            if (boolName == nameof(PlayerData.spiderCapture))
            {
                return false;
            }

            if (boolName == nameof(PlayerData.nailsmithSheo))
            {
                return false;
            }

            if (boolName == nameof(PlayerData.corniferAtHome))
            {
                return PlayerData.instance.GetBoolInternal(boolName) || RandomizerMod.Instance.Settings.RandomizeMaps;
            }

            if (boolName == nameof(PlayerData.instance.openedMapperShop))
            {
                // prevent Iselda from being locked out when maps are not randomized
                return PlayerData.instance.GetBoolInternal(boolName) ||
                    (!RandomizerMod.Instance.Settings.RandomizeMaps &&
                    (
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_cityLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_abyssLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_cliffsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_crossroadsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_deepnestLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_fogCanyonLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_fungalWastesLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_greenpathLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_minesLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_outskirtsLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_royalGardensLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.corn_waterwaysLeft)) ||
                    PlayerData.instance.GetBoolInternal(nameof(PlayerData.openedRestingGrounds))
                    ));
            }

            if (boolName.StartsWith("RandomizerMod."))
            {
                // format is RandomizerMod.GiveAction.ItemName.LocationName for shop bools. Only the item name is used for savesettings bools
                return Settings.GetBool(false, boolName.Split('.')[2]);
            }
            
            if (RandomizerMod.Instance.Settings.RandomizeRooms && (boolName == "troupeInTown" || boolName == "divineInTown")) return false;
            if (boolName == "crossroadsInfected" && RandomizerMod.Instance.Settings.RandomizeRooms
                && new List<string> { SceneNames.Crossroads_03, SceneNames.Crossroads_06, SceneNames.Crossroads_10, SceneNames.Crossroads_19 }.Contains(GameManager.instance.sceneName)) return false;

            return Ref.PD.GetBoolInternal(boolName);
        }

        private void BoolSetOverride(string boolName, bool value)
        {
            PlayerData pd = Ref.PD;

            // It's just way easier if I can treat spells as bools
            if (boolName == "hasVengefulSpirit" && value && pd.fireballLevel <= 0)
            {
                pd.SetInt("fireballLevel", 1);
            }
            else if (boolName == "hasVengefulSpirit" && !value)
            {
                pd.SetInt("fireballLevel", 0);
            }
            else if (boolName == "hasShadeSoul" && value)
            {
                pd.SetInt("fireballLevel", 2);
            }
            else if (boolName == "hasShadeSoul" && !value && pd.fireballLevel >= 2)
            {
                pd.SetInt("fireballLevel", 1);
            }
            else if (boolName == "hasDesolateDive" && value && pd.quakeLevel <= 0)
            {
                pd.SetInt("quakeLevel", 1);
            }
            else if (boolName == "hasDesolateDive" && !value)
            {
                pd.SetInt("quakeLevel", 0);
            }
            else if (boolName == "hasDescendingDark" && value)
            {
                pd.SetInt("quakeLevel", 2);
            }
            else if (boolName == "hasDescendingDark" && !value && pd.quakeLevel >= 2)
            {
                pd.SetInt("quakeLevel", 1);
            }
            else if (boolName == "hasHowlingWraiths" && value && pd.screamLevel <= 0)
            {
                pd.SetInt("screamLevel", 1);
            }
            else if (boolName == "hasHowlingWraiths" && !value)
            {
                pd.SetInt("screamLevel", 0);
            }
            else if (boolName == "hasAbyssShriek" && value)
            {
                pd.SetInt("screamLevel", 2);
            }
            else if (boolName == "hasAbyssShriek" && !value && pd.screamLevel >= 2)
            {
                pd.SetInt("screamLevel", 1);
            }
            else if (boolName.StartsWith("RandomizerMod."))
            {
                // format is RandomizerMod.GiveAction.ItemName.LocationName for shop bools. Only the item name is used for savesettings bools

                string[] pieces = boolName.Split('.');
                pieces[1].TryToEnum(out GiveAction giveAction);
                string item = pieces[2];
                string location = pieces[3];

                GiveItem(giveAction, item, location);
                return;
            }
            // Send the set through to the actual set
            pd.SetBoolInternal(boolName, value);

            // Check if there is a secondary bool for this item
            if (_secondaryBools.TryGetValue(boolName, out string secondaryBoolName))
            {
                pd.SetBool(secondaryBoolName, value);
            }

            if (boolName == nameof(PlayerData.hasCyclone) || boolName == nameof(PlayerData.hasUpwardSlash) ||
                boolName == nameof(PlayerData.hasDashSlash))
            {
                // Make nail arts work
                bool hasCyclone = pd.GetBool(nameof(PlayerData.hasCyclone));
                bool hasUpwardSlash = pd.GetBool(nameof(PlayerData.hasUpwardSlash));
                bool hasDashSlash = pd.GetBool(nameof(PlayerData.hasDashSlash));

                pd.SetBool(nameof(PlayerData.hasNailArt), hasCyclone || hasUpwardSlash || hasDashSlash);
                pd.SetBool(nameof(PlayerData.hasAllNailArts), hasCyclone && hasUpwardSlash && hasDashSlash);
            }
            else if (boolName == nameof(PlayerData.hasDreamGate) && value)
            {
                // Make sure the player can actually use dream gate after getting it
                FSMUtility.LocateFSM(Ref.Hero.gameObject, "Dream Nail").FsmVariables
                    .GetFsmBool("Dream Warp Allowed").Value = true;
            }
            else if (boolName == nameof(PlayerData.hasAcidArmour) && value)
            {
                // Gotta update the acid pools after getting this
                PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
            }
            else if (boolName.StartsWith("gotCharm_"))
            {
                // Check for Salubra notches if it's a charm
                UpdateCharmNotches(pd);
            }
        }

        private int IntOverride(string intName)
        {
            if (intName == "RandomizerMod.Zero")
            {
                return 0;
            }

            return Ref.PD.GetIntInternal(intName);
        }

        private void FixVoidHeart(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            // Normal shade and sibling AI
            if ((self.FsmName == "Control" && self.gameObject.name.StartsWith("Shade Sibling")) || (self.FsmName == "Shade Control" && self.gameObject.name.StartsWith("Hollow Shade")))
            {
                self.FsmVariables.FindFsmBool("Friendly").Value = false;
                self.GetState("Pause").ClearTransitions();
                self.GetState("Pause").AddTransition("FINISHED", "Init");
            }
            // Make Void Heart equippable
            else if (self.FsmName == "UI Charms" && self.gameObject.name == "Charms")
            {
                self.GetState("Equipped?").RemoveTransitionsTo("Black Charm? 2");
                self.GetState("Equipped?").AddTransition("EQUIPPED", "Return Points");
                self.GetState("Set Current Item Num").RemoveTransitionsTo("Black Charm?");
                self.GetState("Set Current Item Num").AddTransition("FINISHED", "Return Points");
            }
        }

        // Will be moved out of RandomizerMod in the future
        private static void EditTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (PlayerData.instance.bossRushMode && info.SceneName == "GG_Entrance_Cutscene")
            {
                OpenMode.OpenModeDataChanges();
                info.SceneName = PlayerData.instance.respawnScene;
                orig(self, info);
                return;
            }
            if (string.IsNullOrEmpty(info.EntryGateName) || string.IsNullOrEmpty(info.SceneName))
            {
                orig(self, info);
                return;
            }
            else if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                TransitionPoint tp = Object.FindObjectsOfType<TransitionPoint>().FirstOrDefault(x => x.entryPoint == info.EntryGateName && x.targetScene == info.SceneName);
                string transitionName = string.Empty;

                if (tp == null)
                {
                    if (self.sceneName == SceneNames.Fungus3_44 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Crossroads_02 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Crossroads_06 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Deepnest_10 && info.EntryGateName == "left1") transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_shop) transitionName = self.sceneName + "[door_sly]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_Town_Stag_Station) transitionName = self.sceneName + "[door_station]";
                    else if (self.sceneName == SceneNames.Town && info.SceneName == SceneNames.Room_Bretta) transitionName = self.sceneName + "[door_bretta]";
                    else if (self.sceneName == SceneNames.Crossroads_04 && info.SceneName == SceneNames.Room_Charm_Shop) transitionName = self.sceneName + "[door_charmshop]";
                    else if (self.sceneName == SceneNames.Crossroads_04 && info.SceneName == SceneNames.Room_Mender_House) transitionName = self.sceneName + "[door_Mender_House]";
                    else if (self.sceneName == SceneNames.Ruins1_04 && info.SceneName == SceneNames.Room_nailsmith) transitionName = self.sceneName + "[door1]";
                    else if (self.sceneName == SceneNames.Fungus3_48 && info.SceneName == SceneNames.Room_Queen) transitionName = self.sceneName + "[door1]";
                    else
                    {
                        orig(self, info);
                        return;
                    }
                }
                else
                {
                    string name = tp.name.Split(null).First(); // some transitions have duplicates named left1 (1) and so on

                    if (RandomizerMod.Instance.Settings.RandomizeRooms)
                    {
                        // It's simplest to treat the three transitions connecting Mantis Lords and Mantis Village as one
                        if (self.sceneName == SceneNames.Fungus2_14 && name.StartsWith("bot")) name = "bot3";
                        else if (self.sceneName == SceneNames.Fungus2_15 && name.StartsWith("top")) name = "top3";
                    }

                    transitionName = self.sceneName + "[" + name + "]";
                }

                if (Instance.Settings._transitionPlacements.TryGetValue(transitionName, out string destination))
                {
                    try
                    {
                        if (!RandomizerMod.Instance.Settings.GetBool(false, transitionName))
                        {
                            RandomizerMod.Instance.Settings.SetBool(true, transitionName);
                            RandomizerMod.Instance.Settings.SetBool(true, destination);
                            RandoLogger.LogTransitionToTracker(transitionName, destination);
                            RandoLogger.UpdateHelperLog(transitionName, gotTransition: true);
                        }
                    }
                    catch (Exception e)
                    {
                        RandomizerMod.Instance.LogError("Error in logging new transition: " + transitionName + "\n" + e);
                    }
                    info.SceneName = LogicManager.GetTransitionDef(destination).sceneName.Split('-').First();
                    info.EntryGateName = LogicManager.GetTransitionDef(destination).doorName;
                }
            }
            TransitionFixes.ApplySaveDataChanges(info.SceneName, info.EntryGateName);
            orig(self, info);
        }

        

        private void HandleSceneChanges(Scene from, Scene to)
        {
            if (Ref.GM.GetSceneNameString() == SceneNames.Menu_Title)
            {
                // Reset settings on menu load
                Settings = new SaveSettings();
                RandomizerAction.ClearActions();

                try
                {
                    MenuChanger.EditUI();
                }
                catch (Exception e)
                {
                    LogError("Error editing menu:\n" + e);
                }
            }

            if (Ref.GM.IsGameplayScene())
            {
                try
                {
                    // In rare cases, this is called before the previous scene has unloaded
                    // Deleting old randomizer shinies to prevent issues
                    GameObject oldShiny = GameObject.Find("Randomizer Shiny");
                    if (oldShiny != null)
                    {
                        Object.DestroyImmediate(oldShiny);
                    }

                    RandomizerAction.EditShinies();
                }
                catch (Exception e)
                {
                    LogError($"Error applying RandomizerActions to scene {to.name}:\n" + e);
                }
            }

            try
            {
                RestrictionManager.SceneChanged(to);
                SceneEditor.SceneChanged(to);
                OpenMode.OpenModeSceneChanges(to);
            }
            catch (Exception e)
            {
                LogError($"Error applying changes to scene {to.name}:\n" + e);
            }
        }
    }
}
