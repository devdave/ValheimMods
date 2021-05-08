﻿using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using SkillType = Skills.SkillType;

namespace EpicLoot.MagicItemEffects
{
	[HarmonyPatch(typeof(Skills), "GetSkillFactor")]
	public static class AddSkillLevel_Skills_GetSkillFactor_Patch
	{
        [UsedImplicitly]
        private static void Postfix(Skills __instance, SkillType skillType, ref float __result)
        {
            __result = Math.Min(1, __result + SkillIncrease(__instance.m_player, skillType) / 100f);
        }

		public static int SkillIncrease(Player player, SkillType skillType)
		{
			var increase = 0;
			
			void check(string effect, params SkillType[] type)
            {
            	if (type.Contains(skillType))
            	{
            		increase += (int) player.GetMagicEquipmentWithEffect(effect).Sum(item => item.GetMagicItem().GetTotalEffectValue(effect));
            	}
            }

            check(MagicEffectType.AddSwordsSkill, SkillType.Swords);
            check(MagicEffectType.AddKnivesSkill, SkillType.Knives);
            check(MagicEffectType.AddClubsSkill, SkillType.Clubs);
            check(MagicEffectType.AddPolearmsSkill, SkillType.Polearms);
            check(MagicEffectType.AddSpearsSkill, SkillType.Spears);
            check(MagicEffectType.AddBlockingSkill, SkillType.Blocking);
            check(MagicEffectType.AddAxesSkill, SkillType.Axes);
            check(MagicEffectType.AddBowsSkill, SkillType.Bows);
            check(MagicEffectType.AddUnarmedSkill, SkillType.Unarmed);
            check(MagicEffectType.AddPickaxesSkill, SkillType.Pickaxes);
            check(MagicEffectType.AddMovementSkills, SkillType.Run, SkillType.Jump, SkillType.Swim, SkillType.Sneak);

            return increase;
		}
	}

	[HarmonyPatch(typeof(SkillsDialog), "Setup")]
	public static class DisplayExtraSkillLevels_SkillsDialog_Setup_Patch
	{
		[UsedImplicitly]
		private static void Postfix(SkillsDialog __instance, Player player)
		{
			var allSkills = player.m_skills.GetSkillList();
			foreach (var element in __instance.m_elements)
			{
				var skill = allSkills.Find(s => s.m_info.m_description == element.GetComponentInChildren<UITooltip>().m_text);
				var extraSkill = AddSkillLevel_Skills_GetSkillFactor_Patch.SkillIncrease(player, skill.m_info.m_skill);
				extraSkill = Math.Min(extraSkill, 100 - (int) skill.m_level);
				if (extraSkill > 0)
				{
					var levelbar = Utils.FindChild(element.transform, "bar");
					var extraLevelbar = Object.Instantiate(levelbar.gameObject, levelbar.parent);
					var rect = extraLevelbar.GetComponent<RectTransform>();
					rect.sizeDelta = new Vector2(Math.Min(160f, (skill.m_level + extraSkill) * 1.6f), rect.sizeDelta.y);
                    extraLevelbar.GetComponent<Image>().color = EpicLoot.GetRarityColorARGB(ItemRarity.Magic);
					extraLevelbar.transform.SetSiblingIndex(levelbar.GetSiblingIndex());
					var levelText = Utils.FindChild(element.transform, "leveltext");
					levelText.GetComponent<Text>().text += $" <color={EpicLoot.GetRarityColor(ItemRarity.Magic)}>+{extraSkill}</color>";
				}
			}
		}
	}
}