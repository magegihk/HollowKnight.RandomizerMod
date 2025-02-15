﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Components;
using RandomizerMod.FsmStateActions;
using SeanprCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using static RandomizerMod.LogHelper;
using System.Collections;
using RandomizerMod.SceneChanges;

namespace RandomizerMod
{
    internal static class SceneEditor
    {
        private static readonly Random Rnd = new Random();
        private static int _rndNum;

        public static void Hook()
        {
            UnHook();

            ModHooks.Instance.ObjectPoolSpawnHook += FixExplosionPogo;
            On.EnemyHitEffectsArmoured.RecieveHitEffect += FalseKnightNoises;
            On.PlayMakerFSM.OnEnable += ModifyFSM;
        }

        public static void UnHook()
        {
            ModHooks.Instance.ObjectPoolSpawnHook -= FixExplosionPogo;
            On.EnemyHitEffectsArmoured.RecieveHitEffect -= FalseKnightNoises;
            On.PlayMakerFSM.OnEnable -= ModifyFSM;
        }

        public static void SceneChanged(Scene newScene)
        {
            RecalculateRandom();

            // Critical changes for randomizer functionality
            {
                RandomizerChanges.ApplyRandomizerChanges(newScene);
                RandomizerChanges.FixSoftlocks(newScene);
                RandomizerChanges.EditStagStations(newScene);
            }

            // Transition fixes: critical changes for transition randomizer functionality, protected by bool checks so they can also be used for item randomizer if necessary
            // Control fixes: npc quests that could otherwise be broken with out-of-sequence rooms
            {
                TransitionFixes.ApplyTransitionFixes(newScene);
                TransitionFixes.ApplyControlFixes(newScene);
            }

            // QoL Fixes - hints of all types, lemm sell all, fast grubfather, fast dream nail cutscene, etc
            {
                QoLFixes.MiscQoLChanges(newScene);
                QoLFixes.ApplyHintChanges(newScene);
            }

            // Mainly restores pogos, etc., that were removed by TC
            {
                SkipFixes.FixMiscSkips(newScene);
            }

            {
                DreamPlantEdits.ReplaceDreamPlantOrbs(newScene);
            }

            // Restores all lever skips which were possible on patch 1221
            if (RandomizerMod.Instance.Settings.LeverSkips)
            {
                SkipFixes.FixLeverSkips(newScene);
            }
        }



        // Everything below here is ancient, and can't really be moved elsewhere

        // No idea
        private static void RecalculateRandom()
        {
            _rndNum = Rnd.Next(25);
        }

        // No idea
        private static void FalseKnightNoises(On.EnemyHitEffectsArmoured.orig_RecieveHitEffect orig,
            EnemyHitEffectsArmoured self, float dir)
        {
            orig(self, dir);

            if (_rndNum != 17 || self.gameObject.name != "False Knight New")
            {
                return;
            }

            AudioPlayerOneShot hitPlayer = FSMUtility.LocateFSM(self.gameObject, "FalseyControl").GetState("Hit")
                .GetActionsOfType<AudioPlayerOneShot>()[0];
            AudioClip clip = hitPlayer.audioClips[Rnd.Next(hitPlayer.audioClips.Length)];

            AudioClip temp = self.enemyDamage.Clip;
            self.enemyDamage.Clip = clip;
            self.enemyDamage.SpawnAndPlayOneShot(self.audioPlayerPrefab, self.transform.position);
            self.enemyDamage.Clip = temp;
        }

        // Has side effect of making some things pogoable which originally were not; i.e. enemy death explosions
        private static GameObject FixExplosionPogo(GameObject go)
        {
            if (!go.name.StartsWith("Gas Explosion Recycle M"))
            {
                return go;
            }

            go.layer = (int) PhysLayers.ENEMIES;
            NonBouncer noFun = go.GetComponent<NonBouncer>();
            if (noFun)
            {
                noFun.active = false;
            }

            return go;
        }

        // Seems to remove Zote death triggers and also affect dream nail storage? Not entirely sure.
        private static void ModifyFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (self.Fsm.FsmComponent.FsmName == "Check Zote Death")
            {
                Object.Destroy(self);
                return;
            }

            orig(self);

            if (self.gameObject.name != "Knight" || self.FsmName != "Dream Nail")
            {
                return;
            }

            self.GetState("Cancelable").GetActionsOfType<ListenForDreamNail>()[0].activeBool = true;
            self.GetState("Cancelable Dash").GetActionsOfType<ListenForDreamNail>()[0].activeBool = true;
            self.GetState("Queuing").GetActionsOfType<ListenForDreamNail>()[0].activeBool = true;
            self.GetState("Queuing").RemoveActionsOfType<BoolTest>();
        }
    }
}
