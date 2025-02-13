using System.Collections.Generic;
using TaleWorlds.Library;
using YiGu.ThreeKingdoms.Modules.SpecialitySystem.CampaignBehaviors;
using System;
using TaleWorlds.CampaignSystem;
using YiGuCommand;
using System.Linq;
using YiGu.ThreeKingdoms.Core;

internal static class ConsoleCommands
{

    [CommandLineFunctionality.CommandLineArgumentFunction("add_speciality_to_player", "yuef")]
    public static string AddSpecialityToPlayer(List<string> args)
    {
        // 检查是否提供了有效的特性ID
        if (args == null || args.Count == 0)
        {
            return "Error: No speciality IDs provided. Correct format: yuef.add_speciality_to_player specialityID1 specialityID2 ...";
        }

        // 用于记录失败的特性ID
        List<string> failedSpecialities = new List<string>();

        // 遍历所有特性ID并为玩家添加
        foreach (var specialityId in args)
        {
            // 调用AddSpeciality方法将特性添加到玩家
            try
            {
                Campaign.Current.SpecialitySystem().AddSpeciality(Hero.MainHero, specialityId, 10);
            }
            catch (Exception ex)
            {
                failedSpecialities.Add(specialityId); // 记录添加失败的特性ID
            }
        }

        // 根据添加结果返回信息
        if (failedSpecialities.Count == 0)
        {
            return "Speciality additions completed.";
        }
        else
        {
            return $"Error adding speciality(s): {string.Join(", ", failedSpecialities)}. Correct format: yuef.add_speciality_to_player specialityID1 specialityID2 ...";
        }
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("add_speciality_to_hero", "yuef")]
    public static string AddSpecialityToHero(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count < 2 || !args.Contains("|"))
        {
            return "Error: Invalid input. Correct format: yuef.add_speciality_to_hero HeroName1 HeroName2 | specialityId1 specialityId2 | MaxLevel, The MaxLevel is optional";
        }

        // 使用 SplitStringByDelimiter 分割参数
        string heroesPart = string.Join(" ", args.TakeWhile(arg => arg != "|").ToArray());
        string specialitiesPart = string.Join(" ", args.SkipWhile(arg => arg != "|").Skip(1).ToArray());

        // 分割英雄名称和特性ID
        List<string> heroNames = SplitStringByDelimiter(heroesPart, " ");
        List<string> specialityIdsAndLevel = SplitStringByDelimiter(specialitiesPart, " ");

        // 获取 MaxLevel，默认 10
        int maxLevel = 10;
        if (specialityIdsAndLevel.Count > 0 && Int32.TryParse(specialityIdsAndLevel.Last(), out int tempMaxLevel))
        {
            maxLevel = tempMaxLevel;
            specialityIdsAndLevel.RemoveAt(specialityIdsAndLevel.Count - 1);
        }

        // 用于记录成功和失败的英雄
        List<string> errorMessages = new List<string>();

        // 遍历所有英雄名称并查找对应英雄
        foreach (var heroName in heroNames)
        {
            // 查找 Hero 对象
            Hero hero = Hero.AllAliveHeroes.Find(h => string.Equals(h.StringId, heroName, StringComparison.OrdinalIgnoreCase));

            if (hero == null)
            {
                errorMessages.Add($"Error: Hero '{heroName}' not found.");
                continue; // 继续处理下一个英雄
            }

            // 为找到的英雄添加所有特性
            foreach (var specialityId in specialityIdsAndLevel)
            {
                try
                {
                    Campaign.Current.SpecialitySystem().AddSpeciality(hero, specialityId, maxLevel);
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Error adding speciality '{specialityId}' to hero '{heroName}': {ex.Message}");
                }
            }
        }

        // 根据是否有错误信息返回结果
        if (errorMessages.Count > 0)
        {
            return string.Join("\n", errorMessages); // 将所有错误信息连接成字符串返回
        }

        return "Specialities have been successfully added to the specified heroes.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("remove_speciality_of_player", "yuef")]
    public static string RemoveSpecialityOfPlayer(List<string> args)
    {
        // 检查是否提供了有效的特性ID
        if (args == null || args.Count == 0)
        {
            return "Error: No speciality IDs provided. Correct format: yuef.remove_speciality_of_player specialityID1 specialityID2 ...";
        }

        // 用于记录成功和失败的特性ID
        List<string> resultMessages = new List<string>();

        // 遍历所有特性ID并为玩家移除
        foreach (var specialityId in args)
        {
            try
            {
                string result = SpecialityHelper.RemoveHeroSpeciality(Hero.MainHero, specialityId);
                resultMessages.Add(result);
            }
            catch (Exception ex)
            {
                resultMessages.Add($"Error removing speciality '{specialityId}': {ex.Message}");
            }
        }

        return string.Join("\n", resultMessages); // 将所有结果信息连接成字符串返回

    }

    [CommandLineFunctionality.CommandLineArgumentFunction("remove_speciality_of_hero", "yuef")]
    public static string RemoveSpecialityOfHero(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count < 2 || !args.Contains("|"))
        {
            return "Error: Invalid input. Correct format: yuef.remove_speciality_of_hero HeroName1 HeroName2 | specialityId1 specialityId2.";
        }

        // 使用 SplitStringByDelimiter 分割参数
        string heroesPart = string.Join(" ", args.TakeWhile(arg => arg != "|").ToArray());
        string specialitiesPart = string.Join(" ", args.SkipWhile(arg => arg != "|").Skip(1).ToArray());

        // 分割英雄名称和特性ID
        List<string> heroNames = SplitStringByDelimiter(heroesPart, " ");
        List<string> specialityIdsAndLevel = SplitStringByDelimiter(specialitiesPart, " ");

        // 用于记录成功和失败的结果
        List<string> resultMessages = new List<string>();

        // 遍历所有英雄名称并查找对应英雄
        foreach (var heroID in heroNames)
        {
            // 查找 Hero 对象
            Hero hero = Hero.AllAliveHeroes.Find(h => string.Equals(h.StringId, heroID, StringComparison.OrdinalIgnoreCase));

            if (hero == null)
            {
                resultMessages.Add($"Error: Hero '{heroID}' not found.");
                continue; // 继续处理下一个英雄
            }

            // 为找到的英雄移除所有特性
            foreach (var specialityId in specialityIdsAndLevel)
            {
                try
                {
                    // 假设 RemoveHeroSpeciality 是一个方法，用来移除给定ID的特性
                    string result = SpecialityHelper.RemoveHeroSpeciality(hero, specialityId);
                    resultMessages.Add(result);
                }
                catch (Exception ex)
                {
                    resultMessages.Add($"Error removing speciality '{specialityId}' from hero '{heroID}': {ex.Message}");
                }
            }
        }

        // 根据是否有结果信息返回最终消息
        return string.Join("\n", resultMessages);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("add_speciality_to_all_companions", "yuef")]
    public static string AddSpecialityToAllCompanions(List<string> args)
    {
        // 检查是否提供了有效的特性ID
        if (args == null || args.Count == 0)
        {
            return "Error: No speciality IDs provided. Correct format: yuef.add_speciality_to_all_companions specialityID1 specialityID2 ...";
        }

        // 用于记录失败的特性ID
        List<string> failedSpecialities = new List<string>();

        foreach (Hero hero in Clan.PlayerClan.Companions)
        {
            // 遍历所有特性ID并为玩家同伴添加
            foreach (var specialityId in args)
            {
                // 调用AddSpeciality方法将特性添加到玩家同伴
                try
                {
                    Campaign.Current.SpecialitySystem().AddSpeciality(hero, specialityId, 10);
                }
                catch (Exception ex)
                {
                    failedSpecialities.Add(specialityId); // 记录添加失败的特性ID
                }
            }

        }

        // 根据添加结果返回信息
        if (failedSpecialities.Count == 0)
        {
            return "Speciality additions completed.";
        }
        else
        {
            return $"Error adding speciality(s): {string.Join(", ", failedSpecialities)}. Correct format: yuef.add_speciality_to_player specialityID1 specialityID2 ...";
        }
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("remove_speciality_of_all_companions", "yuef")]
    public static string RemoveSpecialityOfAllCompanions(List<string> args)
    {
        if (args == null || args.Count == 0)
        {
            return "Error: No speciality IDs provided. Correct format: yuef.remove_speciality_of_all_companions specialityID1 specialityID2 ...";
        }

        List<string> resultMessages = new List<string>();

        foreach (Hero hero in Clan.PlayerClan.Companions)
        {
            // 遍历所有特性ID并为对象移除
            foreach (var specialityId in args)
            {
                try
                {
                    // 假设 RemoveHeroSpeciality 是一个方法，用来移除给定ID的特性
                    string result = SpecialityHelper.RemoveHeroSpeciality(hero, specialityId);
                    resultMessages.Add(result);
                }
                catch (Exception ex)
                {
                    resultMessages.Add($"Error removing speciality '{specialityId}' from hero '{hero.Name}': {ex.Message}");
                }
            }

        }

        return string.Join("\n", resultMessages);
    }


    [CommandLineFunctionality.CommandLineArgumentFunction("set_speciality_level_of_player", "yuef")]
    public static string SetSpecialityLevelOfPlayer(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count < 2 || !args.Contains("|"))
        {
            return "Error: Invalid input. Correct format: yuef.set_level_of_speciality_to_player specialityId1 specialityId2 | Level.";
        }

        // 使用 SplitStringByDelimiter 分割参数
        string specialitiesPart = string.Join(" ", args.TakeWhile(arg => arg != "|").ToArray());
        string levelPart = string.Join(" ", args.SkipWhile(arg => arg != "|").Skip(1).ToArray());

        // 分割特性ID
        List<string> specialityIds = SplitStringByDelimiter(specialitiesPart, " ");

        // 解析等级（假设为整数）
        if (!int.TryParse(levelPart.Trim(), out int level))
        {
            return "Error: Invalid level. Please provide a valid integer for the level.";
        }

        // 用于记录成功和失败的结果
        List<string> resultMessages = new List<string>();

        // 遍历所有特性ID并设置等级
        foreach (var specialityId in specialityIds)
        {
            try
            {
                // 设置英雄的特性等级
                string result = SpecialityHelper.SetHeroSpecialityLevel(Hero.MainHero, specialityId, level);
                resultMessages.Add(result);

            }
            catch (Exception ex)
            {
                resultMessages.Add($"Error setting speciality '{specialityId}' level to '{level}': {ex.Message}");
            }
        }

        // 根据是否有结果信息返回最终消息
        return string.Join("\n", resultMessages);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("set_speciality_level_of_hero", "yuef")]
    public static string SetSpecialityLevelOfHero(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count < 2 || !args.Contains("|"))
        {
            return "Error: Invalid input. Correct format: yuef.set_speciality_level_of_hero heroID1 heroID2 | specialityID1 specialityID2 | Level.";
        }

        // 使用 SplitStringByDelimiter 分割参数
        string heroesPart = string.Join(" ", args.TakeWhile(arg => arg != "|").ToArray());
        string specialitiesPart = string.Join(" ", args.SkipWhile(arg => arg != "|").Skip(1).ToArray());

        // 分割英雄名称和特性ID
        List<string> heroIds = SplitStringByDelimiter(heroesPart, " ");
        List<string> specialityIdsAndLevel = SplitStringByDelimiter(specialitiesPart, " ");

        // 获取 Level
        if (specialityIdsAndLevel.Count == 0 || !int.TryParse(specialityIdsAndLevel.Last(), out int level))
        {
            return "Error: Invalid level. Please provide a valid integer for the level.";
        }

        // 移除 Level 值
        specialityIdsAndLevel.RemoveAt(specialityIdsAndLevel.Count - 1);

        // 用于记录成功和失败的结果
        List<string> resultMessages = new List<string>();

        // 遍历所有英雄名称并查找对应英雄
        foreach (var heroID in heroIds)
        {
            // 查找 Hero 对象
            Hero hero = Hero.AllAliveHeroes.Find(h => string.Equals(h.StringId, heroID, StringComparison.OrdinalIgnoreCase));

            if (hero == null)
            {
                resultMessages.Add($"Error: Hero '{heroID}' not found.");
                continue; // 继续处理下一个英雄
            }

            // 为找到的英雄设置特性等级
            foreach (var specialityId in specialityIdsAndLevel)
            {
                try
                {
                    // 假设 SetHeroSpecialityLevel 是设置特性等级的方法
                    string result = SpecialityHelper.SetHeroSpecialityLevel(hero, specialityId, level);
                    resultMessages.Add(result);
                }
                catch (Exception ex)
                {
                    resultMessages.Add($"Error setting speciality '{specialityId}' level to '{level}' for hero '{heroID}': {ex.Message}");
                }
            }
        }

        // 根据是否有结果信息返回最终消息
        return string.Join("\n", resultMessages);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("set_speciality_level_of_all_companions", "yuef")]
    public static string SetSpecialityLevelOfAllCompanions(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count < 2 || !args.Contains("|"))
        {
            return "Error: Invalid input. Correct format: yuef.set_level_of_speciality_to_all_companions specialityId1 specialityId2 | Level.";
        }

        // 使用 SplitStringByDelimiter 分割参数
        string specialitiesPart = string.Join(" ", args.TakeWhile(arg => arg != "|").ToArray());
        string levelPart = string.Join(" ", args.SkipWhile(arg => arg != "|").Skip(1).ToArray());

        // 分割特性ID
        List<string> specialityIds = SplitStringByDelimiter(specialitiesPart, " ");

        // 解析等级（假设为整数）
        if (!int.TryParse(levelPart.Trim(), out int level))
        {
            return "Error: Invalid level. Please provide a valid integer for the level.";
        }

        // 用于记录成功和失败的结果
        List<string> resultMessages = new List<string>();

        foreach (Hero hero in Clan.PlayerClan.Companions)
        {
            // 遍历所有特性ID并设置等级
            foreach (var specialityId in specialityIds)
            {
                try
                {
                    // 设置英雄的特性等级
                    string result = SpecialityHelper.SetHeroSpecialityLevel(hero, specialityId, level);
                    resultMessages.Add(result);

                }
                catch (Exception ex)
                {
                    resultMessages.Add($"Error setting speciality '{specialityId}' level to '{level}': {ex.Message}");
                }
            }
        }

        // 根据是否有结果信息返回最终消息
        return string.Join("\n", resultMessages);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("apply_merit_change", "yuef")]
    public static string ApplyMeritChangeToPlayer(List<string> args)
    {
        // 检查输入是否符合格式
        if (args == null || args.Count != 1)
        {
            return "Error: Invalid input. Correct format: yuef.apply_merit_change <int>";
        }

        // 解析参数，确保它是一个有效的整数
        if (!int.TryParse(args[0], out int meritChange))
        {
            return "Error: Invalid merit change value. Please provide a valid integer.";
        }

        // 用于记录成功和失败的结果
        List<string> resultMessages = new List<string>();

        try
        {
            TKCampaign.Current.OfficialPositionSystem.ApplyMeritChange(Hero.MainHero, meritChange, true);
            resultMessages.Add($"Successfully applied merit change of {meritChange}.");
        }
        catch (Exception ex)
        {
            resultMessages.Add($"Error applying merit change to: {ex.Message}");
        }


        // 返回最终消息
        return string.Join("\n", resultMessages);
    }

    public static List<string> SplitStringByDelimiter(string A, string B)
    {
        // 检查输入是否为空或 null
        if (string.IsNullOrEmpty(A) || string.IsNullOrEmpty(B))
        {
            return new List<string> { A };
        }

        // 使用 Split 进行分割
        string[] splitResult = A.Split(new string[] { B }, StringSplitOptions.None);

        // 如果没有进行分割，则返回原始字符串
        return splitResult.Length == 1 && splitResult[0] == A
            ? new List<string> { A }
            : splitResult.ToList();
    }

}

