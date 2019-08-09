using System;
using RandomizerMod.Extensions;
using SeanprCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static RandomizerMod.LogHelper;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace RandomizerMod
{
    internal static class MenuChanger
    {
        public static void EditUI()
        {
            // Reset settings
            RandomizerMod.Instance.Settings = new SaveSettings();

            // Fetch data from vanilla screen
            MenuScreen playScreen = Ref.UI.playModeMenuScreen;

            playScreen.title.gameObject.transform.localPosition = new Vector3(0, 520.56f);

            Object.Destroy(playScreen.topFleur.gameObject);

            MenuButton classic = (MenuButton) playScreen.defaultHighlight;
            MenuButton steel = (MenuButton) classic.FindSelectableOnDown();
            MenuButton back = (MenuButton) steel.FindSelectableOnDown();

            GameObject parent = steel.transform.parent.gameObject;

            Object.Destroy(parent.GetComponent<VerticalLayoutGroup>());

            // Create new buttons
            MenuButton startRandoBtn = classic.Clone("StartRando", MenuButton.MenuButtonType.Proceed,
                new Vector2(0, 0), "开始游戏", "随机", RandomizerMod.GetSprite("UI.logo"));
            /*
            MenuButton startNormalBtn = classic.Clone("StartNormal", MenuButton.MenuButtonType.Proceed,
                new Vector2(0, -200), "开始游戏", "非随机");
            MenuButton startSteelRandoBtn = steel.Clone("StartSteelRando", MenuButton.MenuButtonType.Proceed,
                new Vector2(10000, 10000), "钢魂", "随机", RandomizerMod.GetSprite("UI.logo2"));
            MenuButton startSteelNormalBtn = steel.Clone("StartSteelNormal", MenuButton.MenuButtonType.Proceed,
                new Vector2(10000, 10000), "钢魂", "非随机");
                
            startNormalBtn.transform.localScale = 
                startSteelNormalBtn.transform.localScale =
                    startSteelRandoBtn.transform.localScale = */
                    startRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);

            MenuButton backBtn = back.Clone("返回", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "返回");


            //RandoMenuItem<string> gameTypeBtn = new RandoMenuItem<string>(back, new Vector2(0, 600), "游戏类型", "常规", "钢魂");

            RandoMenuItem<string> presetPoolsBtn = new RandoMenuItem<string>(back, new Vector2(900, 1040), "预设", "循序渐进", "完美主义者", "垃圾堆", "自定义");
            RandoMenuItem<bool> RandoDreamersBtn = new RandoMenuItem<bool>(back, new Vector2(900, 960), "守梦者", true);
            RandoMenuItem<bool> RandoSkillsBtn = new RandoMenuItem<bool>(back, new Vector2(900, 880), "技能", true);
            RandoMenuItem<bool> RandoCharmsBtn = new RandoMenuItem<bool>(back, new Vector2(900, 800), "护符", true);
            RandoMenuItem<bool> RandoKeysBtn = new RandoMenuItem<bool>(back, new Vector2(900, 720), "钥匙", true);
            RandoMenuItem<bool> RandoGeoChestsBtn = new RandoMenuItem<bool>(back, new Vector2(900, 640), "钱箱", false, true);
            RandoMenuItem<bool> RandoMaskBtn = new RandoMenuItem<bool>(back, new Vector2(900, 560), "面具碎片", false, true);
            RandoMenuItem<bool> RandoVesselBtn = new RandoMenuItem<bool>(back, new Vector2(900, 480), "灵魂碎片", false, true);
            RandoMenuItem<bool> RandoOreBtn = new RandoMenuItem<bool>(back, new Vector2(900, 400), "苍白矿石", false, true);
            RandoMenuItem<bool> RandoNotchBtn = new RandoMenuItem<bool>(back, new Vector2(900, 320), "护符槽", false, true);
            RandoMenuItem<bool> RandoEggBtn = new RandoMenuItem<bool>(back, new Vector2(900, 240), "腐臭蛋", false, true);
            RandoMenuItem<bool> RandoRelicsBtn = new RandoMenuItem<bool>(back, new Vector2(900, 160), "古董", false, true);
            RandoMenuItem<bool> RandoSpoilerBtn = new RandoMenuItem<bool>(back, new Vector2(900, 80), "生成剧透日志", true, false);

            RandoMenuItem<string> presetSkipsBtn = new RandoMenuItem<string>(back, new Vector2(-900, 1040), "预设", "简单", "困难", "自定义");
            RandoMenuItem<bool> mildSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 960), "温和", false, true);
            RandoMenuItem<bool> shadeSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 880), "劈魂", false, true);
            RandoMenuItem<bool> fireballSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 800), "白波", false, true);
            RandoMenuItem<bool> acidSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 720), "酸冲", false, true);
            RandoMenuItem<bool> spikeTunnelsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 640), "钻刺", false, true);
            RandoMenuItem<bool> darkRoomsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 560), "摸黑", false, true);
            RandoMenuItem<bool> spicySkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 480), "刺激", false, true);

            RandoMenuItem<bool> charmNotchBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 280), "自动加护符槽", true, false);
            RandoMenuItem<bool> lemmBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 200), "一键甩卖", true, false);
            RandoMenuItem<bool> EarlyGeoBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 120), "开局钱", true, false);
            RandoMenuItem<bool> jijiBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 40), "吉吉提示", true, false);
            RandoMenuItem<bool> quirrelBtn = new RandoMenuItem<bool>(back, new Vector2(-900, -40), "奎若提示", true, false);
            

            RandoMenuItem<string> modeBtn = new RandoMenuItem<string>(back, new Vector2(0, 1040), "模式", "物品随机", "区域随机", "连接区域房间随机", "房间随机");

            // Create seed entry field
            GameObject seedGameObject = back.Clone("种子", MenuButton.MenuButtonType.Activate, new Vector2(0, 1130),
                "Click to type a custom seed").gameObject;
            Object.DestroyImmediate(seedGameObject.GetComponent<MenuButton>());
            Object.DestroyImmediate(seedGameObject.GetComponent<EventTrigger>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<FixVerticalAlign>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<ContentSizeFitter>());

            RectTransform seedRect = seedGameObject.transform.Find("Text").GetComponent<RectTransform>();
            seedRect.anchorMin = seedRect.anchorMax = new Vector2(0.5f, 0.5f);
            seedRect.sizeDelta = new Vector2(337, 63.2f);

            InputField customSeedInput = seedGameObject.AddComponent<InputField>();
            customSeedInput.transform.localPosition = new Vector3(0, 1240);
            customSeedInput.textComponent = seedGameObject.transform.Find("Text").GetComponent<Text>();

            RandomizerMod.Instance.Settings.Seed = new Random().Next(999999999);
            customSeedInput.text = RandomizerMod.Instance.Settings.Seed.ToString();

            customSeedInput.caretColor = Color.white;
            customSeedInput.contentType = InputField.ContentType.IntegerNumber;
            customSeedInput.onEndEdit.AddListener(ParseSeedInput);
            customSeedInput.navigation = Navigation.defaultNavigation;
            customSeedInput.caretWidth = 8;
            customSeedInput.characterLimit = 9;

            customSeedInput.colors = new ColorBlock
            {
                highlightedColor = Color.yellow,
                pressedColor = Color.red,
                disabledColor = Color.black,
                normalColor = Color.white,
                colorMultiplier = 2f
            };

            // Create some labels
            CreateLabel(back, new Vector2(-900, 1130), "需求的操作");
            CreateLabel(back, new Vector2(-900, 380), "便捷功能");
            CreateLabel(back, new Vector2(900, 1130), "随机程度");
            CreateLabel(back, new Vector2(0, 200), "汉化：德特茅斯汉化组 Q群：646558222\n\n\n原作者：Seanpr 升级3.0：Homothety\n\n\n区域/房间随机要求使用回椅子mod");
            CreateLabel(back, new Vector2(0, 1300), "种子：");

            // We don't need these old buttons anymore
            Object.Destroy(classic.gameObject);
            Object.Destroy(steel.gameObject);
            Object.Destroy(parent.FindGameObjectInChildren("GGButton"));
            Object.Destroy(back.gameObject);

            // Gotta put something here, we destroyed the old default
            playScreen.defaultHighlight = startRandoBtn;

            // Apply navigation info (up, right, down, left)
            //startNormalBtn.SetNavigation(gameTypeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            startRandoBtn.SetNavigation(modeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            //startSteelNormalBtn.SetNavigation(gameTypeBtn.Button, presetPoolsBtn.Button, backBtn, presetSkipsBtn.Button);
            //startSteelRandoBtn.SetNavigation(modeBtn.Button, presetPoolsBtn.Button, gameTypeBtn.Button, presetSkipsBtn.Button);
            modeBtn.Button.SetNavigation(backBtn, modeBtn.Button, startRandoBtn, modeBtn.Button);
            //gameTypeBtn.Button.SetNavigation(startRandoBtn, presetPoolsBtn.Button, startRandoBtn, presetSkipsBtn.Button);
            backBtn.SetNavigation(startRandoBtn, backBtn, modeBtn.Button, backBtn);

            presetSkipsBtn.Button.SetNavigation(EarlyGeoBtn.Button, startRandoBtn, shadeSkipsBtn.Button, presetSkipsBtn.Button);
            mildSkipsBtn.Button.SetNavigation(presetSkipsBtn.Button, startRandoBtn, mildSkipsBtn.Button, mildSkipsBtn.Button);
            shadeSkipsBtn.Button.SetNavigation(mildSkipsBtn.Button, startRandoBtn, fireballSkipsBtn.Button, shadeSkipsBtn.Button);
            fireballSkipsBtn.Button.SetNavigation(shadeSkipsBtn.Button, startRandoBtn, acidSkipsBtn.Button, fireballSkipsBtn.Button);
            acidSkipsBtn.Button.SetNavigation(fireballSkipsBtn.Button, startRandoBtn, spikeTunnelsBtn.Button, acidSkipsBtn.Button);
            spikeTunnelsBtn.Button.SetNavigation(acidSkipsBtn.Button, startRandoBtn, darkRoomsBtn.Button, spikeTunnelsBtn.Button);
            darkRoomsBtn.Button.SetNavigation(spikeTunnelsBtn.Button, startRandoBtn, spicySkipsBtn.Button, darkRoomsBtn.Button);
            spicySkipsBtn.Button.SetNavigation(darkRoomsBtn.Button, startRandoBtn, charmNotchBtn.Button, spicySkipsBtn.Button);

            charmNotchBtn.Button.SetNavigation(darkRoomsBtn.Button, startRandoBtn, lemmBtn.Button, charmNotchBtn.Button);
            lemmBtn.Button.SetNavigation(charmNotchBtn.Button, startRandoBtn, EarlyGeoBtn.Button, lemmBtn.Button);
            EarlyGeoBtn.Button.SetNavigation(lemmBtn.Button, startRandoBtn, jijiBtn.Button, EarlyGeoBtn.Button);
            jijiBtn.Button.SetNavigation(EarlyGeoBtn.Button, startRandoBtn, quirrelBtn.Button, jijiBtn.Button);
            quirrelBtn.Button.SetNavigation(jijiBtn.Button, startRandoBtn, presetSkipsBtn.Button, quirrelBtn.Button);
            

            presetPoolsBtn.Button.SetNavigation(RandoSpoilerBtn.Button, presetPoolsBtn.Button, RandoDreamersBtn.Button, startRandoBtn);
            RandoDreamersBtn.Button.SetNavigation(presetPoolsBtn.Button, RandoDreamersBtn.Button, RandoSkillsBtn.Button, startRandoBtn);
            RandoSkillsBtn.Button.SetNavigation(RandoDreamersBtn.Button, RandoSkillsBtn.Button, RandoCharmsBtn.Button, startRandoBtn);
            RandoCharmsBtn.Button.SetNavigation(RandoSkillsBtn.Button, RandoCharmsBtn.Button, RandoKeysBtn.Button, startRandoBtn);
            RandoKeysBtn.Button.SetNavigation(RandoCharmsBtn.Button, RandoKeysBtn.Button, RandoGeoChestsBtn.Button, startRandoBtn);
            RandoGeoChestsBtn.Button.SetNavigation(RandoKeysBtn.Button, RandoGeoChestsBtn.Button, RandoMaskBtn.Button, startRandoBtn);
            RandoMaskBtn.Button.SetNavigation(RandoGeoChestsBtn.Button, RandoMaskBtn.Button, RandoVesselBtn.Button, startRandoBtn);
            RandoVesselBtn.Button.SetNavigation(RandoMaskBtn.Button, RandoVesselBtn.Button, RandoOreBtn.Button, startRandoBtn);
            RandoOreBtn.Button.SetNavigation(RandoVesselBtn.Button, RandoOreBtn.Button, RandoNotchBtn.Button, startRandoBtn);
            RandoNotchBtn.Button.SetNavigation(RandoOreBtn.Button, RandoNotchBtn.Button, RandoEggBtn.Button, startRandoBtn);
            RandoEggBtn.Button.SetNavigation(RandoNotchBtn.Button, RandoEggBtn.Button, RandoRelicsBtn.Button, startRandoBtn);
            RandoRelicsBtn.Button.SetNavigation(RandoEggBtn.Button, RandoRelicsBtn.Button, RandoSpoilerBtn.Button, startRandoBtn);
            RandoSpoilerBtn.Button.SetNavigation(RandoRelicsBtn.Button, RandoSpoilerBtn.Button, presetPoolsBtn.Button, startRandoBtn);

            // Setup event for changing difficulty settings buttons
            void UpdateSkipsButtons(RandoMenuItem<string> item)
            {
                switch (item.CurrentSelection)
                {
                    case "简单":
                        SetShadeSkips(false);
                        mildSkipsBtn.SetSelection(false);
                        acidSkipsBtn.SetSelection(false);
                        spikeTunnelsBtn.SetSelection(false);
                        spicySkipsBtn.SetSelection(false);
                        fireballSkipsBtn.SetSelection(false);
                        darkRoomsBtn.SetSelection(false);
                        break;
                    case "困难":
                        SetShadeSkips(true);
                        mildSkipsBtn.SetSelection(true);
                        acidSkipsBtn.SetSelection(true);
                        spikeTunnelsBtn.SetSelection(true);
                        spicySkipsBtn.SetSelection(true);
                        fireballSkipsBtn.SetSelection(true);
                        darkRoomsBtn.SetSelection(true);
                        break;
                    case "自定义":
                        item.SetSelection("简单");
                        goto case "简单";

                    default:
                        LogWarn("Unknown value in preset button: " + item.CurrentSelection);
                        break;
                }
            }
            void UpdatePoolPreset(RandoMenuItem<string> item)
            {
                switch (item.CurrentSelection)
                {
                    case "循序渐进":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(false);
                        RandoMaskBtn.SetSelection(false);
                        RandoVesselBtn.SetSelection(false);
                        RandoOreBtn.SetSelection(false);
                        RandoNotchBtn.SetSelection(false);
                        RandoEggBtn.SetSelection(false);
                        RandoRelicsBtn.SetSelection(false);
                        break;
                    case "完美主义者":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(false);
                        RandoRelicsBtn.SetSelection(false);
                        break;
                    case "垃圾堆":
                        RandoDreamersBtn.SetSelection(true);
                        RandoSkillsBtn.SetSelection(true);
                        RandoCharmsBtn.SetSelection(true);
                        RandoKeysBtn.SetSelection(true);
                        RandoGeoChestsBtn.SetSelection(true);
                        RandoMaskBtn.SetSelection(true);
                        RandoVesselBtn.SetSelection(true);
                        RandoOreBtn.SetSelection(true);
                        RandoNotchBtn.SetSelection(true);
                        RandoEggBtn.SetSelection(true);
                        RandoRelicsBtn.SetSelection(true);
                        break;
                    case "自定义":
                        item.SetSelection("循序渐进");
                        goto case "循序渐进";
                }
            }

            void SetShadeSkips(bool enabled)
            {
                if (enabled)
                {
                    //gameTypeBtn.SetSelection("常规");
                    //SwitchGameType(false);
                }

                shadeSkipsBtn.SetSelection(enabled);
            }

            void SkipsSettingChanged(RandoMenuItem<bool> item)
            {
                presetSkipsBtn.SetSelection("自定义");

            }

            void PoolSettingChanged(RandoMenuItem<bool> item)
            {
                presetPoolsBtn.SetSelection("自定义");
            }

            presetSkipsBtn.Changed += UpdateSkipsButtons;
            presetPoolsBtn.Changed += UpdatePoolPreset;

            mildSkipsBtn.Changed += SkipsSettingChanged;
            shadeSkipsBtn.Changed += SkipsSettingChanged;
            shadeSkipsBtn.Changed += SaveShadeVal;
            acidSkipsBtn.Changed += SkipsSettingChanged;
            spikeTunnelsBtn.Changed += SkipsSettingChanged;
            spicySkipsBtn.Changed += SkipsSettingChanged;
            fireballSkipsBtn.Changed += SkipsSettingChanged;
            darkRoomsBtn.Changed += SkipsSettingChanged;

            RandoDreamersBtn.Changed += PoolSettingChanged;
            RandoSkillsBtn.Changed += PoolSettingChanged;
            RandoCharmsBtn.Changed += PoolSettingChanged;
            RandoKeysBtn.Changed += PoolSettingChanged;
            RandoGeoChestsBtn.Changed += PoolSettingChanged;
            RandoMaskBtn.Changed += PoolSettingChanged;
            RandoVesselBtn.Changed += PoolSettingChanged;
            RandoOreBtn.Changed += PoolSettingChanged;
            RandoNotchBtn.Changed += PoolSettingChanged;
            RandoEggBtn.Changed += PoolSettingChanged;
            RandoRelicsBtn.Changed += PoolSettingChanged;

            // Setup game type button changes
            void SaveShadeVal(RandoMenuItem<bool> item)
            {
                SetShadeSkips(shadeSkipsBtn.CurrentSelection);
            }

            /*void SwitchGameType(bool steelMode)
            {
                if (!steelMode)
                {
                    // Normal mode
                    startRandoBtn.transform.localPosition = new Vector2(0, 200);
                    startNormalBtn.transform.localPosition = new Vector2(0, -200);
                    startSteelRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelNormalBtn.transform.localPosition = new Vector2(10000, 10000);

                    //backBtn.SetNavigation(startNormalBtn, startNormalBtn, modeBtn.Button, startRandoBtn);
                   //magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startNormalBtn, lemmBtn.Button);
                    //lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startRandoBtn, allSkillsBtn.Button);
                }
                else
                {
                    // Steel Soul mode
                    startRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startNormalBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelRandoBtn.transform.localPosition = new Vector2(0, 200);
                    startSteelNormalBtn.transform.localPosition = new Vector2(0, -200);

                    SetShadeSkips(false);

                    //backBtn.SetNavigation(startSteelNormalBtn, startSteelNormalBtn, modeBtn.Button, startSteelRandoBtn);
                    //magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startSteelNormalBtn, lemmBtn.Button);
                    //lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startSteelRandoBtn, allSkillsBtn.Button);
                }
            }

            gameTypeBtn.Button.AddEvent(EventTriggerType.Submit,
                garbage => SwitchGameType(gameTypeBtn.CurrentSelection != "常规"));
                */

            // Setup start game button events
            void StartGame(bool rando)
            {
                RandomizerMod.Instance.Settings.CharmNotch = charmNotchBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.Lemm = lemmBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.EarlyGeo = EarlyGeoBtn.CurrentSelection;


                if (rando)
                {
                    RandomizerMod.Instance.Settings.Jiji = jijiBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.Quirrel = quirrelBtn.CurrentSelection;
                    

                    RandomizerMod.Instance.Settings.RandomizeDreamers = true;
                    RandomizerMod.Instance.Settings.RandomizeSkills = true;
                    RandomizerMod.Instance.Settings.RandomizeCharms = true;
                    RandomizerMod.Instance.Settings.RandomizeKeys = true;
                    RandomizerMod.Instance.Settings.RandomizeGeoChests = RandoGeoChestsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeMaskShards = RandoMaskBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeVesselFragments = RandoVesselBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizePaleOre = RandoOreBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeCharmNotches = RandoNotchBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeRancidEggs = RandoEggBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.RandomizeRelics = RandoRelicsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.CreateSpoilerLog = RandoSpoilerBtn.CurrentSelection;

                    RandomizerMod.Instance.Settings.Randomizer = rando;
                    RandomizerMod.Instance.Settings.RandomizeAreas = modeBtn.CurrentSelection == "区域随机";
                    RandomizerMod.Instance.Settings.RandomizeRooms = modeBtn.CurrentSelection.EndsWith("房间随机");
                    RandomizerMod.Instance.Settings.ConnectAreas = modeBtn.CurrentSelection.StartsWith("连接区域");

                    RandomizerMod.Instance.Settings.MildSkips = mildSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.ShadeSkips = shadeSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.FireballSkips = fireballSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.AcidSkips = acidSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.SpikeTunnels = spikeTunnelsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.DarkRooms = darkRoomsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.SpicySkips = spicySkipsBtn.CurrentSelection;
                }

                RandomizerMod.Instance.StartNewGame();
            }

            //startNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
            //startSteelNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            //startSteelRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
        }

        private static void ParseSeedInput(string input)
        {
            if (int.TryParse(input, out int newSeed))
            {
                RandomizerMod.Instance.Settings.Seed = newSeed;
            }
            else
            {
                LogWarn($"Seed input \"{input}\" could not be parsed to an integer");
            }
        }

        private static void CreateLabel(MenuButton baseObj, Vector2 position, string text)
        {
            GameObject label = baseObj.Clone(text + "Label", MenuButton.MenuButtonType.Activate, position, text)
                .gameObject;
            Object.Destroy(label.GetComponent<EventTrigger>());
            Object.Destroy(label.GetComponent<MenuButton>());
        }

        private class RandoMenuItem<T> where T : IEquatable<T>
        {
            public delegate void RandoMenuItemChanged(RandoMenuItem<T> item);

            private readonly FixVerticalAlign _align;
            private readonly T[] _selections;
            private readonly Text _text;
            private int _currentSelection;

            public RandoMenuItem(MenuButton baseObj, Vector2 position, string name, params T[] values)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                if (baseObj == null)
                {
                    throw new ArgumentNullException(nameof(baseObj));
                }

                if (values == null || values.Length == 0)
                {
                    throw new ArgumentNullException(nameof(values));
                }

                _selections = values;
                Name = name;

                Button = baseObj.Clone(name + "Button", MenuButton.MenuButtonType.Activate, position, string.Empty);

                _text = Button.transform.Find("Text").GetComponent<Text>();
                _text.fontSize = 36;
                _align = Button.gameObject.GetComponentInChildren<FixVerticalAlign>(true);

                Button.ClearEvents();
                Button.AddEvent(EventTriggerType.Submit, GotoNext);

                RefreshText();
            }

            public T CurrentSelection => _selections[_currentSelection];

            public MenuButton Button { get; }

            public string Name { get; }

            public event RandoMenuItemChanged Changed
            {
                add => ChangedInternal += value;
                remove => ChangedInternal -= value;
            }

            private event RandoMenuItemChanged ChangedInternal;

            public void SetSelection(T obj)
            {
                for (int i = 0; i < _selections.Length; i++)
                {
                    if (_selections[i].Equals(obj))
                    {
                        _currentSelection = i;
                        break;
                    }
                }

                RefreshText(false);
            }

            private void GotoNext(BaseEventData data = null)
            {
                _currentSelection++;
                if (_currentSelection >= _selections.Length)
                {
                    _currentSelection = 0;
                }

                RefreshText();
            }

            private void RefreshText(bool invokeEvent = true)
            {
                _text.text = Name + ": " + _selections[_currentSelection];
                _align.AlignText();

                if (invokeEvent)
                {
                    ChangedInternal?.Invoke(this);
                }
            }
        }
    }
}