using System;
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
        private List<string> grubItems;
        private List<string> essenceItems;

        public ProgressionManager(int[] progression = null, bool addSettings = true)
        {
            obtained = new int[LogicManager.bitMaskMax + 1];
            if (progression != null) progression.CopyTo(obtained, 0);
            if (addSettings) ApplyDifficultySettings();
            if (RandomizerMod.Instance.Settings.RandomizeRooms)
            {
                Add("梦之钉");
                Add("梦之门");
            }
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
            if (LogicManager.grubProgression.Contains(item)) RecalculateGrubs();
            if (LogicManager.essenceProgression.Contains(item)) RecalculateEssence();
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

        public bool Has(string item)
        {
            if (!LogicManager.progressionBitMask.TryGetValue(item, out (int, int) a))
            {
                RandomizerMod.Instance.LogWarn("Could not find progression value corresponding to: " + item);
                return false;
            }
            return (obtained[a.Item2] & a.Item1) == a.Item1;
        }

        public void ApplyDifficultySettings()
        {
            if (RandomizerMod.Instance.Settings.ShadeSkips) Add("SHADESKIPS");
            if (RandomizerMod.Instance.Settings.AcidSkips) Add("ACIDSKIPS");
            if (RandomizerMod.Instance.Settings.SpikeTunnels) Add("SPIKETUNNELS");
            if (RandomizerMod.Instance.Settings.SpicySkips) Add("SPICYSKIPS");
            if (RandomizerMod.Instance.Settings.FireballSkips) Add("FIREBALLSKIPS");
            if (RandomizerMod.Instance.Settings.DarkRooms) Add("DARKROOMS");
            if (RandomizerMod.Instance.Settings.MildSkips) Add("MILDSKIPS");
        }

        public void RecalculateEssence()
        {
            int essence = 0;
            if (essenceItems == null) essenceItems = LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).pool.StartsWith("Essence")).ToList();

            foreach (string item in essenceItems)
            {
                if (CanGet(item))
                {
                    essence += LogicManager.GetItemDef(item).geo;
                }
                if (essence >= 930) break;
            }
            obtained[LogicManager.essenceIndex] = essence;
        }

        public void RecalculateGrubs()
        {
            int grubs = 0;
            if (grubItems == null) grubItems = LogicManager.ItemNames.Where(item => LogicManager.GetItemDef(item).pool == "Grub").ToList();

            foreach (string item in grubItems)
            {
                if (CanGet(item))
                {
                    grubs++;
                }
                if (grubs >= 24) break;
            }
            obtained[LogicManager.grubIndex] = grubs;
        }

        // useful for debugging
        public string ListObtainedProgression()
        {
            string progression = string.Empty;
            foreach (string item in LogicManager.ItemNames)
            {
                if (LogicManager.GetItemDef(item).progression && Has(item)) progression += item + ", ";
            }
            foreach (string transition in LogicManager.TransitionNames())
            {
                if (Has(transition)) progression += transition + ", ";
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
