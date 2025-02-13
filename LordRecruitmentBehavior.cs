using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace YiGuCommand
{
    internal class LordRecruitmentBehavior : CampaignBehaviorBase
    {
        private static Random _random = new Random();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {

            // 君主欲招募家族，若其家族尚无归属则可提议
            starter.AddPlayerLine("yuef_recruit_clan", "lord_talk_speak_diplomacy_2", "yuef_recruit_clan",
                "某欲邀在下家族共图大业，若家族尚无依附之地，愿与将军并肩作战，如何？",
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.IsNoble && Hero.OneToOneConversationHero.Clan.Kingdom == null,
                null,
                100,
                null,
                null);


            starter.AddDialogLine("yuef_recruit_clan_2", "yuef_recruit_clan", "close_window",
                "尔等背信弃义、薄情寡义，竟敢以此等恶言羞辱我家族！今日侮我，来日必有偿还之日！家族之事，岂容尔等轻挑，休得再提！",
                // 判断玩家对话对象是否是家族之主
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && Hero.OneToOneConversationHero.GetRelationWithPlayer() < -30,
                null,
                100,
                null);

            starter.AddDialogLine("yuef_recruit_clan_2", "yuef_recruit_clan", "start",
                "哼，原来将军如此关怀鄙人，实令某感激涕零。此般心系百姓，真令某人钦佩。然而，此等恩泽，某怕是受之不起也。",//与玩家关系较差，修改对话为拒绝加入并表示态度，注意称呼
                                                 // 判断玩家对话对象是否是家族之主
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && Hero.OneToOneConversationHero.GetRelationWithPlayer() >= -30 && Hero.OneToOneConversationHero.GetRelationWithPlayer() < -10,
                null,
                100,
                null);

            starter.AddDialogLine("yuef_recruit_clan_2", "yuef_recruit_clan", "start",
                "谢阁下恩惠，然与阁下素常交情不深，恐难以信任。家族加盟一事，难以作出决定...",//与玩家不熟，修改对话为拒绝加入并表示态度，注意称呼
                                                 // 判断玩家对话对象是否是家族之主
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && Hero.OneToOneConversationHero.GetRelationWithPlayer() >= -10 && Hero.OneToOneConversationHero.GetRelationWithPlayer() < 10,
                null,
                100,
                null);

            starter.AddDialogLine("yuef_recruit_clan_2", "yuef_recruit_clan", "start",
                "多谢阁下厚意，然我军方寸已乱，士气未能恢复，恐难以再兴大计。实在愧对阁下盛情厚意，恐怕此时未能为您效力。日后若有缘再共谋，必不负重托。",//与玩家不熟，修改对话为考虑加入,实际拒绝并表示态度，注意称呼
                                                 // 判断玩家对话对象是否是家族之主
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && Hero.OneToOneConversationHero.GetRelationWithPlayer() >= 10 && Hero.OneToOneConversationHero.GetRelationWithPlayer() < 30,
                null,
                100,
                null);

            // 对话回应，表明部队需金支持
            starter.AddDialogLine("yuef_recruit_clan_2", "yuef_recruit_clan", "yuef_recruit_clan_2",
                "多谢阁下厚恩，然我军士气低落，若无足够金帛，恐难以振作...",
                // 判断玩家对话对象是否是家族之主
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && Hero.OneToOneConversationHero.GetRelationWithPlayer() >= 30,
                null,
                100,
                null);

            // 如果对话对象不是家族之主，回应表示无法做主并结束对话
            starter.AddDialogLine("yuef_recruit_clan_2_non_leader", "yuef_recruit_clan", "start",
                "大人，谢您厚意，然我非家族之主，难以做主此事...",
                // 判断对话对象是否是家族之主，如果不是则返回此对话
                () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader != Hero.OneToOneConversationHero,
                null,
                100,
                null);

            // 如果对话对象为家族之主，玩家回应表示愿意提供金帛
            starter.AddPlayerLine("yuef_recruit_clan_3_1", "yuef_recruit_clan_2", "yuef_recruit_clan_3_1",
                "若将军所托，必不负所望，金帛自当供给，铠甲刀枪、粮草兵员，皆可调度，定不让将军有所缺乏。",
                null,
                delegate
                {
                    GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, Hero.OneToOneConversationHero, Hero.OneToOneConversationHero.Clan.Gold / 2, false);
                },
                100,
                delegate (out TextObject explanation)
                {
                    float baseAmount = 300000.0f;
                    float percentageBonus = 0.0f;

                    var relation = Hero.OneToOneConversationHero.GetRelationWithPlayer();

                    if (relation < 70.0f)
                    {
                        // 小于70时，增加百分比（例如每差1点增加5%）
                        percentageBonus = (70.0f - relation) * 0.05f;  // 每小于1的关系值增加2%
                    }
                    else
                    {
                        // 大于70时，减少百分比（例如每差1点减少1.5%）
                        percentageBonus = (relation - 70.0f) * -0.015f;  // 每大于1的关系值减少1.5%
                    }

                    float finalAmount = baseAmount * (1 + percentageBonus);

                    bool hasEnoughGold = Hero.MainHero.Gold >= Hero.OneToOneConversationHero.Clan.Gold + finalAmount;

                    if (!hasEnoughGold)
                    {
                        explanation = new TextObject(string.Format("尚缺{0}金帛，恳请将军再赐助", Hero.OneToOneConversationHero.Clan.Gold + finalAmount), null);
                        return false;
                    }
                    else
                    {
                        explanation = new TextObject(string.Format("须得{0}金帛，方可为将军所用", Hero.OneToOneConversationHero.Clan.Gold + finalAmount), null);
                        return true;
                    }
                },
                null);

            // 玩家考虑后，决定暂缓回答
            starter.AddPlayerLine("yuef_recruit_clan_3_2", "yuef_recruit_clan_2", "start",
                "请将军稍待片刻，容在下深思再作决断。",
                null,
                null,
                100,
                null,
                null);

            // 家族答谢，接受加入
            starter.AddDialogLine("yuef_recruit_clan_4", "yuef_recruit_clan_3_1", "yuef_recruit_clan_4",
                "多谢主公收留，家族定当效命，誓死不负！",
                null,
                null,
                100,
                null);

            // 玩家欢迎家族加入国度
            starter.AddPlayerLine("yuef_recruit_clan_5", "yuef_recruit_clan_4", "close_window",
                "欢迎贵族加盟，共图江山大业！",
                null,
                delegate
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(Hero.OneToOneConversationHero.Clan, Clan.PlayerClan.Kingdom, true);
                },
                100,
                null,
                null);
        }


        public override void SyncData(IDataStore dataStore)
        {
        }


    }
}


