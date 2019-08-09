using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using Language;
using static RandomizerMod.LogHelper;
using RandomizerMod.Randomization;

namespace RandomizerMod
{
    internal static class LanguageStringManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LanguageStrings =
            new Dictionary<string, Dictionary<string, string>>();

        public static void LoadLanguageXML(Stream xmlStream)
        {
            // Load XmlDocument from resource stream
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlStream);
            xmlStream.Dispose();

            XmlNodeList nodes = xml.SelectNodes("Language/entry");
            if (nodes == null)
            {
                LogWarn("Malformatted language xml, no nodes that match Language/entry");
                return;
            }

            foreach (XmlNode node in nodes)
            {
                string sheet = node.Attributes?["sheet"]?.Value;
                string key = node.Attributes?["key"]?.Value;

                if (sheet == null || key == null)
                {
                    LogWarn("Malformatted language xml, missing sheet or key on node");
                    continue;
                }

                SetString(sheet, key, node.InnerText.Replace("\\n", "\n"));
            }

            Log("Language xml processed");
        }

        public static void SetString(string sheetName, string key, string text)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key) || text == null)
            {
                return;
            }

            if (!LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet))
            {
                sheet = new Dictionary<string, string>();
                LanguageStrings.Add(sheetName, sheet);
            }

            sheet[key] = text;
        }

        public static void ResetString(string sheetName, string key)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet) && sheet.ContainsKey(key))
            {
                sheet.Remove(key);
            }
        }

        public static string GetLanguageString(string key, string sheetTitle)
        {
            if (sheetTitle == "Jiji" && key == "HIVE" && RandomizerMod.Instance.Settings.Jiji)
            {
                return NextJijiHint();
            }
            if (sheetTitle == "Quirrel" && RandomizerMod.Instance.Settings.Quirrel && RandomizerMod.Instance.Settings.QuirrerHintCounter < 3 &&
                new List<string> { "QUIRREL_MEET_TEMPLE_C", "QUIRREL_GREENPATH_1", "QUIRREL_QUEENSTATION_01", "QUIRREL_MANTIS_01", "QUIRREL_RUINS_1", "QUIRREL_SPA", "QUIRREL_MINES_2", "QUIRREL_FOGCANYON_A", "QUIRREL_EPILOGUE_A" }.Contains(key))
            {
                return GetQuirrelHint(key);
            }
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sheetTitle))
            {
                return string.Empty;
            }

            if (LanguageStrings.ContainsKey(sheetTitle) && LanguageStrings[sheetTitle].ContainsKey(key))
            {
                return LanguageStrings[sheetTitle][key];
            }

            return Language.Language.GetInternal(key, sheetTitle);
        }
        public static string NextJijiHint()
        {
            int hintMax = RandomizerMod.Instance.Settings.Hints.Length;
            string hintItemName = string.Empty;
            string hintItemSpot = string.Empty;
            string hint = string.Empty;
            while (RandomizerMod.Instance.Settings.JijiHintCounter < hintMax - 1)
            {
                string item = RandomizerMod.Instance.Settings.Hints[RandomizerMod.Instance.Settings.JijiHintCounter].Item1;
                string location = RandomizerMod.Instance.Settings.Hints[RandomizerMod.Instance.Settings.JijiHintCounter].Item2;
                hint = CreateJijiHint(item, location);
                RandoLogger.LogHintToTracker(hint);

                if (Actions.RandomizerAction.AdditiveBoolNames.TryGetValue(item, out string additiveBoolName))
                {
                    if (!RandomizerMod.Instance.Settings.GetBool(false, additiveBoolName))
                    {
                        hintItemName = item;
                        hintItemSpot = location;
                        RandomizerMod.Instance.Settings.JijiHintCounter++;
                        break;
                    }
                }
                else if (!PlayerData.instance.GetBool(LogicManager.GetItemDef(item).boolName))
                {
                    hintItemName = item;
                    hintItemSpot = location;
                    RandomizerMod.Instance.Settings.JijiHintCounter++;
                    break;
                }
                RandomizerMod.Instance.Settings.JijiHintCounter++;
            }
            if (hintItemName == string.Empty || hintItemSpot == string.Empty || hint == string.Empty) return "Oh! I guess I couldn't find any items you left behind. Since you're doing so well, though, I think I'll be keeping this meal.";

            return hint;
        }

        public static string CreateJijiHint(string hintItemName, string hintItemSpot)
        {
            ReqDef hintItem = LogicManager.GetItemDef(hintItemName);
            ReqDef hintSpot = LogicManager.GetItemDef(hintItemSpot);
            bool good = false;
            int useful = 0;
            foreach ((string, string) p in RandomizerMod.Instance.Settings.Hints)
            {
                ReqDef item = LogicManager.GetItemDef(p.Item1);
                ReqDef location = LogicManager.GetItemDef(p.Item2);
                if (location.areaName == hintSpot.areaName)
                {
                    if (item.isGoodItem) good = true;
                    if (item.progression) useful++;
                }
            }
            string secondMessage;
            if (good) secondMessage = " 那边的东西想想就让我流口水。";
            else if (useful > 2) secondMessage = " 那里有点东西的啊。";
            else if (useful == 1) secondMessage = " 值不值得去一趟那里呢？";
            else secondMessage = " 纯捡垃圾吃的，呵呵。";

            hintItemName = GetLanguageString(hintItem.nameKey, "UI");
            string hintItemArea = hintSpot.areaName;
            string firstMessage;

            if (hintItemArea == "苍绿之径") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在一片草木繁盛的翠绿土地。";
            else if (hintItemArea == "真菌荒地") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 坐落于长相奇异的真菌与冒泡的湖水里。";
            else if (hintItemArea == "水晶山峰") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 几乎被周围微微闪烁的水晶所掩埋。";
            else if (hintItemArea == "深渊") firstMessage = "天灵灵，地灵灵，丢的装备快显形... 虽然很微弱... " + hintItemName + " 在世界极深处，黑暗笼罩着，几乎被吞噬...";
            else if (hintItemArea == "皇家水道") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 周围是水流在管道中流淌，不过它不会被冲走...";
            else if (hintItemArea == "安息之地") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在一个神圣的沉眠场所。";
            else if (hintItemArea == "祖先山丘") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在一个古老的山丘里…一个奇特的祭祀场。";
            else if (hintItemArea == "泪水之城") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在王国首都的中心，雨水也不能将其打湿...";
            else if (hintItemArea == "雾之峡谷") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在异土的迷雾中走失。";
            else if (hintItemArea == "呼啸悬崖") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在离我们很高的地方，被狂风所包围。";
            else if (hintItemArea == "王国边缘") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在遥远的世界边际。";
            else if (hintItemArea == "遗忘十字路") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 就在我们的下方，王国通路的曲折蜿蜒使其逐渐被遗忘。";
            else if (hintItemArea == "国王山道") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 就在王国的入口附近。";
            else if (hintItemArea == "深邃巢穴") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在伸手不见五指的巢穴，位于王国深处。";
            else if (hintItemArea == "德特茅斯") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 就在外面，一个宁静的、逐渐消逝的村庄。";
            else if (hintItemArea == "蜂巢") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 被金色的光辉环绕，在遥远蜂巢中。";
            else if (hintItemArea == "王后花园") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在叛乱花园的美人那。";
            else if (hintItemArea == "愚人斗兽场") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 边上满是战士与愚人。";
            else if (hintItemArea == "古老盆地") firstMessage = "天灵灵，地灵灵，丢的装备快显形... " + hintItemName + " 在国王宫殿的废墟外。";
            else firstMessage = hintItemName + " 在 " + hintItemArea + ".";

            return firstMessage + secondMessage;
        }

        public static string GetQuirrelHint(string key)
        {
            RandomizerMod.Instance.Settings.QuirrerHintCounter++;
            string hint = string.Empty;
            List<string> areas = new List<string>();
            List<string> locations = new List<string>();

            switch (key)
            {
                case "QUIRREL_MEET_TEMPLE_C":
                    locations = RandomizerMod.Instance.Settings.ItemPlacements.Where(pair => new List<string> { "梦之钉", "梦之门", "苏醒的梦之钉" }.Contains(pair.Item1)).Select(pair => pair.Item2).ToList();
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_',' '));
                        }
                    }
                    hint = "巨大的石卵，静卧在古国的尸骸之中。这卵……有温度吗？它散发出独特的气息。" +
                        "<page>" + "我们能打开它吗？上面都是奇怪的花纹..." + 
                        "<page>" + "也许...我听说在这王国存在能够启迪思想、发现黑暗秘密的遗物。" +
                        "<page>" + "第一个据说被藏在了 " + areas[0] + "。" +
                        "<page>" + "第二个安全地待在 " + areas[1] + "。" +
                        "<page>" + "而最后一个...在" + areas[2] + "。" +
                        "<page>" + "或许你将凭借其中一个解开这座神殿的谜题...";
                    break;
                case "QUIRREL_GREENPATH_1":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == "螳螂爪" || pair.Item1 == "帝王之翼").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "你好啊！看来我们都远离大道了。" +
                        "<page>" + "真不敢相信这些灰尘扑扑的旧公路能带我来到这么绿意盎然的地方！" +
                        "<page>" + "这座建筑和崇拜偶像有关系，不过显然这个偶像早已被人遗忘。在这里喘口气也挺不错。"
                        + "<page>" + "哦，我又给你找到了一个不错的小提示。如果你想抵达更高的地方，试着找找 " + areas[0] + "."
                        +"<page>" + "你应该能在那里找到些有用的东西。";
                    break;
                case "QUIRREL_QUEENSTATION_01":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == "蛾翼披风" || pair.Item1 == "暗影披风").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "是不是很了不起。我从没想到从迷雾中下来之后还能找到这么大的鹿角虫站。" +
                        "<page>" + "圣巢里肯定虫山虫海，才能在这么深入荒原的地方修建这些巨大的建筑。" +
                        "<page>" + "看来危险的动物都还没能来到这里。这是最适合休息的地方了。" +
                        "<page>" + "就从现在开始，你需要加快点儿速度了。我听说过一种能赋予如此能力的特殊披风。" +
                        "<page>" + "我自己是找不到的，但或许你可以。试着看看 " + areas[0] + ".";
                    break;
                case "QUIRREL_MANTIS_01":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == "光蝇灯笼").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "又见面了！你肯定已经见过村子里的部落了，对吧？含蓄点说……他们不太相信陌生人。" +
                        "<page>"+ "但他们也不是粗人。空气里的瘟疫蒙蔽了弱者的心智……但他们抵抗住了疾病。他们依然身怀睿智和荣耀，当然也保存着致命的传统。" +
                        "<page>"+ "朋友，我要给你一点建议。如果你想向下走，途经部落的首领，你会发现没有工具为你照亮道路会更加困难。" +
                        "<page>"+ "我在路过" + areas[0] + "的时候见过额外的一个。";
                    break;
                case "QUIRREL_RUINS_1":
                    locations = RandomizerMod.Instance.Settings.ItemPlacements.Where(pair => new List<string> { "荒芜俯冲", "黑暗降临" }.Contains(pair.Item1)).Select(pair => pair.Item2).ToList();
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "朋友，首都就在我们面前。多阴郁的地方啊，无数迷题的答案也掌握在它手里。" +
                        "<page>"+ "我也感觉到了这里的吸引力，但现在我坐在它面前，却迟迟不敢往前。" +
                        "<page>"+ "我不知道是恐惧，还是别的东西让我不敢继续？" +
                        "<page>"+ "在我们面前的圣所里发生过的那些关于恐怖研究的故事太令人厌恶，没必要再重提一遍。不过我恐怕他们已经研究出了一种足以击穿大地的魔法，但如此的艺术已经随着城市陷落而失落。" +
                        "<page>"+ "如果你还需要这种力量，试着探索一下 " + areas[0] + "。那似乎是最可能找到它的地方。或许你同样能在 " + areas[1] + " 发现它，但那恐怕不太容易发掘。";
                    break;
                case "QUIRREL_SPA":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.Last(pair => pair.Item1 == "蛾翼披风" || pair.Item1 == "暗影披风").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "你好！真让人激动啊，能在野兽的巢穴里找到这么舒服温暖的地方。" +
                        "<page>"+ "真是个残暴的地方。这片混乱的深处可能有个村子。它的居民从没承认过圣巢之王（可可树的老婆）。" +
                        "<page>"+ "顺便问一下，你注意到那些四处散布的奇怪黑色障碍了吗？" +
                        "<page>"+ "我猜能穿过它们的手段被封藏在 " + areas[0] + "，但谁能说有把握呢？";
                    break;
                case "QUIRREL_MINES_2":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.Last(pair => pair.Item1 == "水晶之心").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "看到下面的矿工还不能摆脱无尽的苦工，这会让你悲伤吗？" +
                        "<page>" + "即使已经死去，强烈的使命感依然在驱动他们的躯壳。" +
                        "<page>" + "据说水晶矿里蕴含着某种能量，没有城市中市民所驾驭的灵魂那么强大，而且温和得多。" +
                        "<page>" + "他们用来为盾构机供能的核心必然曾拥有惊人的能量，但如今已经不剩多少。我知道的唯一一个已经被搬去了 " + areas[0] + "。";
                    break;
                case "QUIRREL_FOGCANYON_A":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.Last(pair => pair.Item1 == "伊思玛的眼泪").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "这个王国是不是到处都是惊喜？酸湖的顶端居然有建筑。" +
                        "<page>" + "说到这儿，我对你能走到这一步很有感触。在没有适当保护的情况下，这些酸对我们有极大的威胁。" +
                        "<page>" + "假如你仍然需要这个能力，我建议搜索一下 " + areas[0] +
                        "<page>"+ "。为什么我不禁觉得...这些景象很眼熟？虽然我说不上来它们是什么..." +
                        "<page>"+ "我以为是发现的欲望引领我来到这里，但现在看来似乎并不是这样。" +
                        "<page>"+ "是这座建筑在召唤我。";
                    break;
                case "QUIRREL_EPILOGUE_A":
                    locations.Add(RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == "螳螂爪" || pair.Item1 == "帝王之翼").Item2);
                    foreach (string location in locations)
                    {
                        if (LogicManager.ShopNames.Contains(location))
                        {
                            areas.Add("a shop");
                        }
                        else
                        {
                            areas.Add(LogicManager.GetItemDef(location).areaName.Replace('_', ' '));
                        }
                    }
                    hint = "我们又见面了，小个子朋友。我在这终于找到了心中的平静。" +
                        "<page>"+ "我想你的旅程还远未结束。若你仍想探寻更高处，去 " + areas[0] + "。你应在那里找到它，我是如此希望的..." +
                        "<page>"+ "我见过这个世界两次，但我的使命可能让我忘记了第一次的感觉。真高兴能再一次见证这美丽的世界。" +
                        "<page>"+ "圣巢广阔又奇妙，但在它呈现的诸多奇迹中，没有哪一项能像你这么吸引人。" +
                        "<page>"+ "哈，你用斯多葛学派的沉默来回应我的奉承。" +
                        "<page>"+ "我喜欢。我就喜欢你这样。";
                    break;
                default:
                    LogWarn("Unknown key passed to GetQuirrelHint");
                    break;
            }
            RandoLogger.LogHintToTracker(hint, jiji: false, quirrel: true);
            return hint;
        }
    }
}