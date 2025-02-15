﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Randomization;
using static RandomizerMod.RandoLogger;
using static RandomizerMod.LogHelper;
using UnityEngine;

namespace RandomizerMod
{
    // WORK IN PROGRESS
    public static class GiveItemActions
    {
        public enum GiveAction
        {
            Bool = 0,
            Int,
            Charm,
            EquippedCharm,
            Additive,
            SpawnGeo,
            AddGeo,

            Map,
            Grub,
            Essence,
            Stag,

            MaskShard,
            VesselFragment,
            WanderersJournal,
            HallownestSeal,
            KingsIdol,
            ArcaneEgg,

            Dreamer,
            Kingsoul,
            Grimmchild,

            None
        }

        public static void GiveItem(GiveAction action, string item, string location, GameObject shiny = null)
        {
            LogItemToTracker(item, location);
            UpdateHelperLog(item, gotItem: true);
            RandomizerMod.Instance.Settings.SetBool(true, item);

            switch (action)
            {
                default:
                case GiveAction.Bool:
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).boolName, true);
                    break;

                case GiveAction.Int:
                    PlayerData.instance.IncrementInt(LogicManager.GetItemDef(item).intName);
                    break;

                case GiveAction.Charm:
                    PlayerData.instance.hasCharm = true;
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).boolName, true);
                    PlayerData.instance.charmsOwned++;
                    break;

                case GiveAction.EquippedCharm:
                    PlayerData.instance.hasCharm = true;
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).boolName, true);
                    PlayerData.instance.charmsOwned++;
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).equipBoolName, true);
                    PlayerData.instance.EquipCharm(LogicManager.GetItemDef(item).charmNum);

                    PlayerData.instance.CalculateNotchesUsed();
                    if (PlayerData.instance.charmSlotsFilled > PlayerData.instance.charmSlots)
                    {
                        PlayerData.instance.overcharmed = true;
                    }
                    break;

                case GiveAction.Additive:
                    string[] additiveItems = LogicManager.GetAdditiveItems(LogicManager.AdditiveItemNames.First(s => LogicManager.GetAdditiveItems(s).Contains(item)));
                    int i = additiveItems.Count(_item => RandomizerMod.Instance.Settings.GetBool(false, _item));
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(additiveItems[i-1]).boolName, true); // note that item has already been set true, so i starts at 1
                    break;

                case GiveAction.AddGeo:
                    PlayerData.instance.AddGeo(LogicManager.GetItemDef(item).geo);
                    break;

                // Disabled because it's more convenient to do this from the fsm. Use GiveAction.None for geo spawns.
                case GiveAction.SpawnGeo:
                    RandomizerMod.Instance.LogError("Tried to spawn geo from GiveItem.");
                    throw new NotImplementedException();

                case GiveAction.Map:
                    PlayerData.instance.hasMap = true;
                    PlayerData.instance.openedMapperShop = true;
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).boolName, true);
                    break;

                case GiveAction.Stag:
                    PlayerData.instance.SetBool(LogicManager.GetItemDef(item).boolName, true);
                    break;

                case GiveAction.Grub:
                    PlayerData.instance.grubsCollected++;
                    int clipIndex = new System.Random().Next(2);
                    /*AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex], 
                        new Vector3(
                            Camera.main.transform.position.x - 5,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 5
                        ));*/
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x - 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    AudioSource.PlayClipAtPoint(ObjectCache.GrubCry[clipIndex],
                        new Vector3(
                            Camera.main.transform.position.x + 2,
                            Camera.main.transform.position.y,
                            Camera.main.transform.position.z + 2
                        ));
                    break;

                case GiveAction.Essence:
                    PlayerData.instance.IntAdd(nameof(PlayerData.dreamOrbs), LogicManager.GetItemDef(item).geo);
                    EventRegister.SendEvent("DREAM ORB COLLECT");
                    break;

                case GiveAction.MaskShard:
                    PlayerData.instance.heartPieceCollected = true;
                    if (PlayerData.instance.heartPieces < 3)
                    {
                        PlayerData.instance.heartPieces++;
                    }
                    else
                    {
                        HeroController.instance.AddToMaxHealth(1);
                        PlayMakerFSM.BroadcastEvent("MAX HP UP");
                        PlayMakerFSM.BroadcastEvent("HERO HEALED FULL");
                        if (PlayerData.instance.maxHealthBase < PlayerData.instance.maxHealthCap)
                        {
                            PlayerData.instance.heartPieces = 0;
                        }
                    }
                    break;

                case GiveAction.VesselFragment:
                    PlayerData.instance.vesselFragmentCollected = true;
                    if (PlayerData.instance.vesselFragments < 2)
                    {
                        GameManager.instance.IncrementPlayerDataInt("vesselFragments");
                    }
                    else
                    {
                        HeroController.instance.AddToMaxMPReserve(33);
                        PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
                        if (PlayerData.instance.MPReserveMax < PlayerData.instance.MPReserveCap)
                        {
                            PlayerData.instance.vesselFragments = 0;
                        }
                    }
                    break;

                case GiveAction.WanderersJournal:
                    PlayerData.instance.foundTrinket1 = true;
                    PlayerData.instance.trinket1++;
                    break;

                case GiveAction.HallownestSeal:
                    PlayerData.instance.foundTrinket2 = true;
                    PlayerData.instance.trinket2++;
                    break;

                case GiveAction.KingsIdol:
                    PlayerData.instance.foundTrinket3 = true;
                    PlayerData.instance.trinket3++;
                    break;

                case GiveAction.ArcaneEgg:
                    PlayerData.instance.foundTrinket4 = true;
                    PlayerData.instance.trinket4++;
                    break;

                case GiveAction.Dreamer:
                    switch (item)
                    {
                        case "Lurien":
                            PlayerData.instance.lurienDefeated = true;
                            PlayerData.instance.maskBrokenLurien = true;
                            break;
                        case "Monomon":
                            PlayerData.instance.monomonDefeated = true;
                            PlayerData.instance.maskBrokenMonomon = true;
                            break;
                        case "Herrah":
                            PlayerData.instance.hegemolDefeated = true;
                            PlayerData.instance.maskBrokenHegemol = true;
                            break;
                    }
                    PlayerData.instance.guardiansDefeated++;
                    if (PlayerData.instance.guardiansDefeated == 1)
                    {
                        PlayerData.instance.hornetFountainEncounter = true;
                        PlayerData.instance.marmOutside = true;
                        PlayerData.instance.crossroadsInfected = true;
                    }
                    break;

                case GiveAction.Kingsoul:
                    PlayerData.instance.royalCharmState++;
                    switch (PlayerData.instance.royalCharmState)
                    {
                        case 1:
                            PlayerData.instance.gotCharm_36 = true;
                            break;
                        case 2:
                            PlayerData.instance.royalCharmState++;
                            break;
                        case 4:
                            PlayerData.instance.gotShadeCharm = true;
                            PlayerData.instance.charmCost_36 = 0;
                            PlayerData.instance.equippedCharm_36 = true;
                            break;
                    }
                    break;

                case GiveAction.Grimmchild:
                    PlayerData.instance.SetBool(nameof(PlayerData.instance.gotCharm_40), true);
                    // Skip first two collection quests
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternAppeared), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.nightmareLanternLit), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.troupeInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.divineInTown), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.metGrimm), true);
                    PlayerData.instance.SetInt(nameof(PlayerData.flamesRequired), 3);
                    PlayerData.instance.SetInt(nameof(PlayerData.flamesCollected), 3);
                    PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerSmall), true);
                    PlayerData.instance.SetBool(nameof(PlayerData.killedFlameBearerMed), true);
                    PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerSmall), 3);
                    PlayerData.instance.SetInt(nameof(PlayerData.killsFlameBearerMed), 3);
                    PlayerData.instance.SetInt(nameof(PlayerData.grimmChildLevel), 2);

                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Mines_10",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Ruins1_28",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Fungus1_10",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Tutorial_01",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "RestingGrounds_06",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    GameManager.instance.sceneData.SaveMyState(new PersistentBoolData
                    {
                        sceneName = "Deepnest_East_03",
                        id = "Flamebearer Spawn",
                        activated = true,
                        semiPersistent = false
                    });
                    break;
                case GiveAction.None:
                    break;
            }
        }
    }
}
