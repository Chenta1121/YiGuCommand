using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using YiGu.ThreeKingdoms.Modules.SpecialitySystem.CampaignBehaviors;

using YiGu.ThreeKingdoms.Modules.SpecialitySystem.Models;

namespace YiGuCommand
{
    internal static class SpecialityHelper
    {
        static SpecialityCoreBehavior behavior = Campaign.Current.SpecialitySystem();

        public enum UpdateStatus
        {
            Success,               // 成功，特性已添加或更新
            HeroNotFound,          // 英雄未找到
            SpecialityNotFound,    // 特性未找到
            SpecialityAlreadyExists, // 特性已存在
            NoChange,              // 没有变化，特性未被更改
            AccessError,           // 访问错误，例如无法访问字典
            UnknownError           // 其他未知错误
        }


        public static string RemoveHeroSpeciality(Hero hero, string specialityID)
        {
            try
            {
                // 调用 UpdateHeroSpeciality 方法并获取状态
                UpdateStatus status = UpdateHeroSpeciality(behavior, hero, specialityID);

                // 根据返回的状态执行相应操作并返回提示信息
                switch (status)
                {
                    case UpdateStatus.Success:
                        return $"Success: Speciality '{specialityID}' removed from hero '{hero.StringId}'.";
                    case UpdateStatus.NoChange:
                        return $"No change: Speciality '{specialityID}' was not removed from hero '{hero.StringId}'.";
                    case UpdateStatus.SpecialityNotFound:
                        return $"Error: Speciality '{specialityID}' not found for hero '{hero.StringId}'.";
                    case UpdateStatus.AccessError:
                        return $"Error: Failed to access the speciality dictionary for hero '{hero.StringId}'.";
                    default:
                        return "Unknown error occurred while removing speciality.";
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并记录错误
                return $"Exception: Error removing speciality '{specialityID}' from hero '{hero.StringId}': {ex.Message}";
            }
        }


        public static string SetHeroSpecialityLevel(Hero hero, string specialityID, int level)
        {
            try
            {
                // 确保等级在合法范围内
                if (level > 10 || level < 0)
                {
                    level = 10;  // 如果等级不合法，设置为默认值 10
                }

                // 调用 UpdateHeroSpeciality 方法并获取返回状态
                UpdateStatus status = UpdateHeroSpeciality(behavior, hero, specialityID, specialityID, level, 10);

                // 根据返回的状态执行相应操作并返回提示信息
                switch (status)
                {
                    case UpdateStatus.Success:
                        return $"Success: Speciality '{specialityID}' level set to {level} for hero '{hero.StringId}'.";
                    case UpdateStatus.NoChange:
                        return $"No change: Speciality '{specialityID}' level not updated for hero '{hero.StringId}'.";
                    case UpdateStatus.SpecialityNotFound:
                        return $"Error: Speciality '{specialityID}' not found for hero '{hero.StringId}'.";
                    case UpdateStatus.AccessError:
                        return $"Error: Failed to access the speciality dictionary for hero '{hero.StringId}'.";
                    default:
                        return "Unknown error occurred while setting speciality level.";
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并记录错误
                return $"Exception: Error setting speciality '{specialityID}' level to '{level}' for hero '{hero.StringId}': {ex.Message}";
            }
        }



        // 修改函数返回类型为 string，根据不同的执行情况返回提示信息
        private static UpdateStatus UpdateHeroSpeciality(SpecialityCoreBehavior behavior, Hero hero, string oldSpecialityId = null, string newSpecialityId = null, int initLevel = 1, int maxLevel = 10)
        {
            // 获取 heroToSpecialityDict 字段信息
            FieldInfo fieldInfo = typeof(SpecialityCoreBehavior).GetField("heroToSpecialityDict", BindingFlags.NonPublic | BindingFlags.Instance);

            // 检查是否成功获取字典字段
            if (fieldInfo == null)
            {
                return UpdateStatus.AccessError; // 字典访问失败
            }

            // 获取字典实例
            var heroToSpecialityDict = fieldInfo.GetValue(behavior) as Dictionary<Hero, HashSet<HeroSpeciality>>;
            if (heroToSpecialityDict == null)
            {
                return UpdateStatus.AccessError; // 字典实例为空
            }

            // 如果字典中没有该 Hero，直接添加该 Hero 和新特性
            if (!heroToSpecialityDict.ContainsKey(hero))
            {
                heroToSpecialityDict[hero] = new HashSet<HeroSpeciality>();
            }

            // 获取该 Hero 的特性集合
            var specialitySet = heroToSpecialityDict[hero];

            // 获取 idToSpecialityDict 字段信息并通过反射获取字典实例
            FieldInfo specialityDictField = typeof(SpecialityCoreBehavior).GetField("idToSpecialityDict", BindingFlags.NonPublic | BindingFlags.Instance);
            if (specialityDictField == null)
            {
                return UpdateStatus.AccessError; // 字典访问失败
            }

            // 获取 idToSpecialityDict 字典实例
            var idToSpecialityDict = specialityDictField.GetValue(behavior) as Dictionary<string, Speciality>;
            if (idToSpecialityDict == null)
            {
                return UpdateStatus.AccessError; // 字典实例为空
            }

            // 获取 oldSpeciality 和 newSpeciality，确保字典中包含这些 ID
            Speciality oldSpeciality = null;
            Speciality newSpeciality = null;

            if (!string.IsNullOrWhiteSpace(oldSpecialityId) && idToSpecialityDict.ContainsKey(oldSpecialityId))
            {
                oldSpeciality = idToSpecialityDict[oldSpecialityId];
            }

            if (!string.IsNullOrWhiteSpace(newSpecialityId) && idToSpecialityDict.ContainsKey(newSpecialityId))
            {
                newSpeciality = idToSpecialityDict[newSpecialityId];
            }

            // 如果旧特性不为空，移除旧特性
            if (oldSpeciality != null)
            {
                var oldHeroSpeciality = specialitySet.FirstOrDefault(sp => sp.Speciality.ID == oldSpecialityId);
                if (oldHeroSpeciality != null)
                {
                    specialitySet.Remove(oldHeroSpeciality);
                    if (newSpeciality == null) return UpdateStatus.Success;
                }
            }

            // 如果新特性不为空，添加新特性
            if (newSpeciality != null)
            {
                if (!specialitySet.Any(sp => sp.Speciality.ID == newSpecialityId))
                {
                    HeroSpeciality newHeroSpeciality = new HeroSpeciality(hero, newSpeciality, maxLevel, initLevel, 0);
                    specialitySet.Add(newHeroSpeciality);
                    return UpdateStatus.Success; // 成功添加新特性
                }
                else
                {
                    return UpdateStatus.SpecialityAlreadyExists; // 特性已经存在
                }
            }

            return UpdateStatus.NoChange; // 没有变化
        }

    }
}
