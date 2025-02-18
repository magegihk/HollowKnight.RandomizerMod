﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Randomization
{
    public class ProgressionManager
    {
        public int[] obtained;
        private Dictionary<string, int> grubLocations;
        private Dictionary<string, int> essenceLocations;
        private bool temp;
        private bool share = true;
        public HashSet<string> tempItems;

        public ProgressionManager(RandomizerState state, int[] progression = null, bool addSettings = true)
        {
            obtained = new int[LogicManager.bitMaskMax + 1];
            if (progression != null) progression.CopyTo(obtained, 0);

            FetchEssenceLocations(state);
            FetchGrubLocations(state);

            if (addSettings) ApplyDifficultySettings();
            RecalculateEssence();
            RecalculateGrubs();
        }

        public bool CanGet(string item)
        {
            return LogicManager.ParseProcessedLogic(item, obtained);
        }

        public void Add(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] |= a.Item1;
            if (temp)
            {
                tempItems.Add(item);
            }
            if (share)
            {
                Share(item);
            }
            RecalculateGrubs();
            RecalculateEssence();
            UpdateWaypoints();
        }

        public void Add(IEnumerable<string> items)
        {
            foreach (string item in items)
            {
                if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
                {
                    RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                    return;
                }
                obtained[a.Item2] |= a.Item1;
                if (temp)
                {
                    tempItems.Add(item);
                }
                if (share)
                {
                    Share(item);
                }
            }
            RecalculateGrubs();
            RecalculateEssence();
            UpdateWaypoints();
        }

        public void AddTemp(string item)
        {
            temp = true;
            if (tempItems == null)
            {
                tempItems = new HashSet<string>();
            }
            Add(item);
        }

        private void Share(string item)
        {
            if (ItemManager.recentProgression != null)
            {
                ItemManager.recentProgression.Add(item);
            }

            if (TransitionManager.recentProgression != null)
            {
                TransitionManager.recentProgression.Add(item);
            }
        }

        private void ToggleShare(bool value)
        {
            share = value;
        }

        public void Remove(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return;
            }
            obtained[a.Item2] &= ~a.Item1;
            if (LogicManager.grubProgression.Contains(item)) RecalculateGrubs();
            if (LogicManager.essenceProgression.Contains(item)) RecalculateEssence();
        }

        public void RemoveTempItems()
        {
            temp = false;
            foreach (string item in tempItems)
            {
                Remove(item);
            }
            tempItems = new HashSet<string>();
        }

        public void SaveTempItems()
        {
            temp = false;
            if (share)
            {
                foreach (string item in tempItems)
                {
                    Share(item);
                }
            }
            
            tempItems = new HashSet<string>();
        }

        public bool Has(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return false;
            }
            return (obtained[a.Item2] & a.Item1) == a.Item1;
        }

        public void UpdateWaypoints()
        {
            if (RandomizerMod.Instance.Settings.RandomizeRooms) return;

            foreach(string waypoint in LogicManager.Waypoints)
            {
                if (!Has(waypoint) && CanGet(waypoint))
                {
                    Add(waypoint);
                }
            }
        }

        private void ApplyDifficultySettings()
        {
            bool tempshare = share;
            share = false;

            if (RandomizerMod.Instance.Settings.ShadeSkips) Add("SHADESKIPS");
            if (RandomizerMod.Instance.Settings.AcidSkips) Add("ACIDSKIPS");
            if (RandomizerMod.Instance.Settings.SpikeTunnels) Add("SPIKETUNNELS");
            if (RandomizerMod.Instance.Settings.SpicySkips) Add("SPICYSKIPS");
            if (RandomizerMod.Instance.Settings.FireballSkips) Add("FIREBALLSKIPS");
            if (RandomizerMod.Instance.Settings.DarkRooms) Add("DARKROOMS");
            if (RandomizerMod.Instance.Settings.MildSkips) Add("MILDSKIPS");

            share = tempshare;
        }

        private void FetchGrubLocations(RandomizerState state)
        {
            switch (state)
            {
                default:
                    grubLocations = LogicManager.GetItemsByPool("Grub").ToDictionary(grub => grub, grub => 1);
                    break;

                case RandomizerState.InProgress when RandomizerMod.Instance.Settings.RandomizeGrubs:
                    grubLocations = new Dictionary<string, int>();
                    break;

                case RandomizerState.Validating when RandomizerMod.Instance.Settings.RandomizeGrubs:
                    grubLocations = ItemManager.nonShopItems.Where(kvp => LogicManager.GetItemDef(kvp.Value).pool == "Grub").ToDictionary(kvp => kvp.Value, kvp => 1);
                    foreach (var kvp in ItemManager.shopItems)
                    {
                        if (kvp.Value.Any(item => LogicManager.GetItemDef(item).pool == "Grub"))
                        {
                            grubLocations.Add(kvp.Key, kvp.Value.Count(item => LogicManager.GetItemDef(item).pool == "Grub"));
                        }
                    }
                    break;

                case RandomizerState.Completed when RandomizerMod.Instance.Settings.RandomizeGrubs:
                    grubLocations = RandomizerMod.Instance.Settings.ItemPlacements
                        .Where(pair => LogicManager.GetItemDef(pair.Item1).pool == "Grub" && !LogicManager.ShopNames.Contains(pair.Item2))
                        .ToDictionary(pair => pair.Item2, kvp => 1);
                    foreach (string shop in LogicManager.ShopNames)
                    {
                        if (RandomizerMod.Instance.Settings.ItemPlacements.Any(pair => pair.Item2 == shop && LogicManager.GetItemDef(pair.Item1).pool == "Grub"))
                        {
                            grubLocations.Add(shop, RandomizerMod.Instance.Settings.ItemPlacements.Count(pair => pair.Item2 == shop && LogicManager.GetItemDef(pair.Item1).pool == "Grub"));
                        }
                    }
                    break;
            }
        }

        private void FetchEssenceLocations(RandomizerState state)
        {
            essenceLocations = LogicManager.GetItemsByPool("Essence_Boss")
                .ToDictionary(item => item, item => LogicManager.GetItemDef(item).geo);

            switch (state)
            {
                default:
                    foreach (string root in LogicManager.GetItemsByPool("Root"))
                    {
                        essenceLocations.Add(root, LogicManager.GetItemDef(root).geo);
                    }
                    break;
                case RandomizerState.InProgress when RandomizerMod.Instance.Settings.RandomizeWhisperingRoots:
                    break;
                case RandomizerState.Validating when RandomizerMod.Instance.Settings.RandomizeWhisperingRoots:
                    foreach (var kvp in ItemManager.nonShopItems)
                    {
                        if (LogicManager.GetItemDef(kvp.Value).pool == "Root")
                        {
                            essenceLocations.Add(kvp.Key, LogicManager.GetItemDef(kvp.Value).geo);
                        }
                    }
                    foreach (var kvp in ItemManager.shopItems)
                    {
                        foreach (string item in kvp.Value)
                        {
                            if (LogicManager.GetItemDef(item).pool == "Root")
                            {
                                if (!essenceLocations.ContainsKey(kvp.Key))
                                {
                                    essenceLocations.Add(kvp.Key, 0);
                                }
                                essenceLocations[kvp.Key] += LogicManager.GetItemDef(item).geo;
                            }
                        }
                    }
                    break;
                case RandomizerState.Completed when RandomizerMod.Instance.Settings.RandomizeWhisperingRoots:
                    foreach (var pair in RandomizerMod.Instance.Settings.ItemPlacements)
                    {
                        if (LogicManager.GetItemDef(pair.Item1).pool == "Root" && !LogicManager.ShopNames.Contains(pair.Item2))
                        {
                            essenceLocations.Add(pair.Item2, LogicManager.GetItemDef(pair.Item1).geo);
                        }
                    }
                    foreach (string shop in LogicManager.ShopNames)
                    {
                        if (RandomizerMod.Instance.Settings.ItemPlacements.Any(pair => pair.Item2 == shop && LogicManager.GetItemDef(pair.Item1).pool == "Root"))
                        {
                            essenceLocations.Add(shop, 0);
                            foreach (var pair in RandomizerMod.Instance.Settings.ItemPlacements)
                            {
                                if (pair.Item2 == shop && LogicManager.GetItemDef(pair.Item1).pool == "Root")
                                {
                                    essenceLocations[shop] += LogicManager.GetItemDef(pair.Item1).geo;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void RecalculateEssence()
        {
            int essence = 0;

            foreach (string location in essenceLocations.Keys)
            {
                if (CanGet(location))
                {
                    essence += essenceLocations[location];
                }
                if (essence >= Randomizer.MAX_ESSENCE_COST + 25) break;
            }
            obtained[LogicManager.essenceIndex] = essence;
        }

        public void RecalculateGrubs()
        {
            int grubs = 0;

            foreach (string location in grubLocations.Keys)
            {
                if (CanGet(location))
                {
                    grubs += grubLocations[location];
                }
                if (grubs >= Randomizer.MAX_GRUB_COST + 1) break;
            }

            obtained[LogicManager.grubIndex] = grubs;
        }

        public void AddGrubLocation(string location)
        {
            if (!grubLocations.ContainsKey(location))
            {
                grubLocations.Add(location, 1);
            }
            else
            {
                grubLocations[location]++;
            }
        }

        public void AddEssenceLocation(string location, int essence)
        {
            RandoLogger.LogTracker("Adding " + location + " with " + essence);
            if (!essenceLocations.ContainsKey(location))
            {
                essenceLocations.Add(location, essence);
            }
            else
            {
                essenceLocations[location] += essence;
            }
        }

        // useful for debugging
        public string ListObtainedProgression()
        {
            string progression = string.Empty;
            foreach (string item in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(item).progression && Has(item)) progression += item + ", ";
            }

            if (RandomizerMod.Instance.Settings.RandomizeTransitions)
            {
                foreach (string transition in LogicManager.TransitionNames())
                {
                    if (Has(transition)) progression += transition + ", ";
                }
            }
            
            return progression;
        }
        public void SpeedTest()
        {
            Stopwatch watch = new Stopwatch();
            foreach (string item in LogicManager.ItemNames)
            {
                watch.Reset();
                watch.Start();
                string result = CanGet(item).ToString();
                double elapsed = watch.Elapsed.TotalSeconds;
                Log("Parsed logic for " + item + " with result " + result + " in " + watch.Elapsed.TotalSeconds);
            }
        }
    }
}
