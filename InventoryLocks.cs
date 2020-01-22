using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace InventoryLocks
{
	internal class InventoryLocks : Mod
	{
		private bool[] CanFavoriteAt => (bool[])typeof(ItemSlot).GetField("canFavoriteAt", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

		private MethodInfo MethodInfo_ItemSlot_OverrideLeftClick => typeof(ItemSlot).GetMethod("OverrideLeftClick", BindingFlags.NonPublic | BindingFlags.Static);

		private MethodInfo MethodInfo_ItemSlot_SellOrTrash => typeof(ItemSlot).GetMethod("SellOrTrash", BindingFlags.NonPublic | BindingFlags.Static);

		public override void Load()
		{
			On.Terraria.UI.ItemSlot.OverrideHover += ItemSlot_OverrideHover;
			On.Terraria.UI.ItemSlot.OverrideLeftClick += ItemSlot_OverrideLeftClick;
			On.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int += ItemSlot_LeftClick;
		}

		public override void Unload()
		{
			On.Terraria.UI.ItemSlot.OverrideHover -= ItemSlot_OverrideHover;
			On.Terraria.UI.ItemSlot.OverrideLeftClick -= ItemSlot_OverrideLeftClick;
			On.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int -= ItemSlot_LeftClick;
		}

		private void ItemSlot_LeftClick(On.Terraria.UI.ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			var OverrideLeftClick = MethodInfo_ItemSlot_OverrideLeftClick.Invoke(null, new object[3]
			{
				inv, context, slot
			});
			if ((bool)OverrideLeftClick)
			{
				return;
			}
			inv[slot].newAndShiny = false;
			Player player = Main.player[Main.myPlayer];
			bool flag = false;
			switch (context)
			{
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
					flag = player.chest == -1;
					break;
			}
			if (ItemSlot.ShiftInUse && flag)
			{
				MethodInfo_ItemSlot_SellOrTrash.Invoke(null, new object[3]
				{
					inv, context, slot
				});
				return;
			}
			if (player.itemAnimation == 0 && player.itemTime == 0)
			{
				int num = ItemSlot.PickItemMovementAction(inv, context, slot, Main.mouseItem);
				if (num == 0)
				{
					if (context == 6 && Main.mouseItem.type != 0)
					{
						inv[slot].SetDefaults(0, false);
					}
					if (Main.LocalPlayer.inventory[slot].type != ModContent.ItemType<ItemLock>())
					{
						Utils.Swap(ref inv[slot], ref Main.mouseItem);
					}
					if (inv[slot].stack > 0)
					{
						if (context != 0)
						{
							switch (context)
							{
								case 8:
								case 9:
								case 10:
								case 11:
								case 12:
								case 16:
								case 17:
									AchievementsHelper.HandleOnEquip(player, inv[slot], context);
									break;
							}
						}
						else
						{
							AchievementsHelper.NotifyItemPickup(player, inv[slot]);
						}
					}
					if (inv[slot].type == 0 || inv[slot].stack < 1)
					{
						inv[slot] = new Item();
					}
					if (Main.mouseItem.IsTheSameAs(inv[slot]))
					{
						Utils.Swap(ref inv[slot].favorited, ref Main.mouseItem.favorited);
						if (inv[slot].stack != inv[slot].maxStack && Main.mouseItem.stack != Main.mouseItem.maxStack)
						{
							if (Main.mouseItem.stack + inv[slot].stack <= Main.mouseItem.maxStack)
							{
								inv[slot].stack += Main.mouseItem.stack;
								Main.mouseItem.stack = 0;
							}
							else
							{
								int num2 = Main.mouseItem.maxStack - inv[slot].stack;
								inv[slot].stack += num2;
								Main.mouseItem.stack -= num2;
							}
						}
					}
					if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
					{
						Main.mouseItem = new Item();
					}
					if (Main.mouseItem.type > 0 || inv[slot].type > 0)
					{
						Recipe.FindRecipes();
						if (Main.LocalPlayer.inventory[slot].type != ModContent.ItemType<ItemLock>())
						{
							Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						}
					}
					if (context == 3 && Main.netMode == 1)
					{
						NetMessage.SendData(32, -1, -1, null, player.chest, (float)slot, 0f, 0f, 0, 0, 0);
					}
				}
				else if (num == 1)
				{
					if (Main.mouseItem.stack == 1 && Main.mouseItem.type > 0 && inv[slot].type > 0 && inv[slot].IsNotTheSameAs(Main.mouseItem))
					{
						Utils.Swap<Item>(ref inv[slot], ref Main.mouseItem);
						Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						if (inv[slot].stack > 0)
						{
							if (context != 0)
							{
								switch (context)
								{
									case 8:
									case 9:
									case 10:
									case 11:
									case 12:
									case 16:
									case 17:
										AchievementsHelper.HandleOnEquip(player, inv[slot], context);
										break;
								}
							}
							else
							{
								AchievementsHelper.NotifyItemPickup(player, inv[slot]);
							}
						}
					}
					else if (Main.mouseItem.type == 0 && inv[slot].type > 0)
					{
						Utils.Swap<Item>(ref inv[slot], ref Main.mouseItem);
						if (inv[slot].type == 0 || inv[slot].stack < 1)
						{
							inv[slot] = new Item();
						}
						if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
						{
							Main.mouseItem = new Item();
						}
						if (Main.mouseItem.type > 0 || inv[slot].type > 0)
						{
							Recipe.FindRecipes();
							Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						}
					}
					else if (Main.mouseItem.type > 0 && inv[slot].type == 0)
					{
						if (Main.mouseItem.stack == 1)
						{
							Utils.Swap<Item>(ref inv[slot], ref Main.mouseItem);
							if (inv[slot].type == 0 || inv[slot].stack < 1)
							{
								inv[slot] = new Item();
							}
							if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
							{
								Main.mouseItem = new Item();
							}
							if (Main.mouseItem.type > 0 || inv[slot].type > 0)
							{
								Recipe.FindRecipes();
								Main.PlaySound(7, -1, -1, 1, 1f, 0f);
							}
						}
						else
						{
							Main.mouseItem.stack--;
							inv[slot].SetDefaults(Main.mouseItem.type, false);
							Recipe.FindRecipes();
							Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						}
						if (inv[slot].stack > 0)
						{
							if (context != 0)
							{
								switch (context)
								{
									case 8:
									case 9:
									case 10:
									case 11:
									case 12:
									case 16:
									case 17:
										AchievementsHelper.HandleOnEquip(player, inv[slot], context);
										break;
								}
							}
							else
							{
								AchievementsHelper.NotifyItemPickup(player, inv[slot]);
							}
						}
					}
				}
				else if (num == 2)
				{
					if (Main.mouseItem.stack == 1 && Main.mouseItem.dye > 0 && inv[slot].type > 0 && inv[slot].type != Main.mouseItem.type)
					{
						Utils.Swap(ref inv[slot], ref Main.mouseItem);
						Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						if (inv[slot].stack > 0)
						{
							if (context != 0)
							{
								switch (context)
								{
									case 8:
									case 9:
									case 10:
									case 11:
									case 12:
									case 16:
									case 17:
										AchievementsHelper.HandleOnEquip(player, inv[slot], context);
										break;
								}
							}
							else
							{
								AchievementsHelper.NotifyItemPickup(player, inv[slot]);
							}
						}
					}
					else if (Main.mouseItem.type == 0 && inv[slot].type > 0)
					{
						Utils.Swap<Item>(ref inv[slot], ref Main.mouseItem);
						if (inv[slot].type == 0 || inv[slot].stack < 1)
						{
							inv[slot] = new Item();
						}
						if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
						{
							Main.mouseItem = new Item();
						}
						if (Main.mouseItem.type > 0 || inv[slot].type > 0)
						{
							Recipe.FindRecipes();
							Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						}
					}
					else if (Main.mouseItem.dye > 0 && inv[slot].type == 0)
					{
						if (Main.mouseItem.stack == 1)
						{
							Utils.Swap<Item>(ref inv[slot], ref Main.mouseItem);
							if (inv[slot].type == 0 || inv[slot].stack < 1)
							{
								inv[slot] = new Item();
							}
							if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
							{
								Main.mouseItem = new Item();
							}
							if (Main.mouseItem.type > 0 || inv[slot].type > 0)
							{
								Recipe.FindRecipes();
								Main.PlaySound(7, -1, -1, 1, 1f, 0f);
							}
						}
						else
						{
							Main.mouseItem.stack--;
							inv[slot].SetDefaults(Main.mouseItem.type, false);
							Recipe.FindRecipes();
							Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						}
						if (inv[slot].stack > 0)
						{
							if (context != 0)
							{
								switch (context)
								{
									case 8:
									case 9:
									case 10:
									case 11:
									case 12:
									case 16:
									case 17:
										AchievementsHelper.HandleOnEquip(player, inv[slot], context);
										break;
								}
							}
							else
							{
								AchievementsHelper.NotifyItemPickup(player, inv[slot]);
							}
						}
					}
				}
				else if (num == 3 && PlayerHooks.CanBuyItem(player, Main.npc[player.talkNPC], inv, inv[slot]))
				{
					Main.mouseItem = inv[slot].Clone();
					Main.mouseItem.stack = 1;
					if (inv[slot].buyOnce)
					{
						Main.mouseItem.value *= 5; // preserve item value for items sold to the shop
												   //Main.mouseItem.Prefix((int)inv[slot].prefix); // no prefix, preserved by cloning
					}
					else
					{
						Main.mouseItem.Prefix(-1);
					}
					Main.mouseItem.position = player.Center - new Vector2((float)Main.mouseItem.width, (float)Main.mouseItem.headSlot) / 2f;
					ItemText.NewText(Main.mouseItem, Main.mouseItem.stack, false, false);
					if (inv[slot].buyOnce && --inv[slot].stack <= 0)
					{
						inv[slot].SetDefaults(0, false);
					}
					if (inv[slot].value > 0)
					{
						Main.PlaySound(18, -1, -1, 1, 1f, 0f);
					}
					else
					{
						Main.PlaySound(7, -1, -1, 1, 1f, 0f);
					}
					PlayerHooks.PostBuyItem(player, Main.npc[player.talkNPC], inv, Main.mouseItem);
				}
				else if (num == 4 && PlayerHooks.CanSellItem(player, Main.npc[player.talkNPC], inv, Main.mouseItem))
				{
					Chest chest = Main.instance.shop[Main.npcShop];
					if (player.SellItem(Main.mouseItem.value, Main.mouseItem.stack))
					{
						int soldItemIndex = chest.AddShop(Main.mouseItem);
						Main.mouseItem.SetDefaults(0, false);
						Main.PlaySound(18, -1, -1, 1, 1f, 0f);
						PlayerHooks.PostSellItem(player, Main.npc[player.talkNPC], chest.item, chest.item[soldItemIndex]);
					}
					else if (Main.mouseItem.value == 0)
					{
						int soldItemIndex = chest.AddShop(Main.mouseItem);
						Main.mouseItem.SetDefaults(0, false);
						Main.PlaySound(7, -1, -1, 1, 1f, 0f);
						PlayerHooks.PostSellItem(player, Main.npc[player.talkNPC], chest.item, chest.item[soldItemIndex]);
					}
					Recipe.FindRecipes();
				}
				switch (context)
				{
					case 0:
					case 1:
					case 2:
					case 5:
						return;
					case 3:
					case 4:
					default:
						inv[slot].favorited = false;
						return;
				}
			}
		}

		private bool ItemSlot_OverrideLeftClick(On.Terraria.UI.ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot)
		{
			Item item = inv[slot];
			if (item.IsAir && Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				item.SetDefaults(ModContent.ItemType<ItemLock>());
                item.favorited = true;
                Main.PlaySound(SoundID.Unlock);
				return true;
			}

			if (Main.cursorOverride == 2)
			{
				if (ChatManager.AddChatText(Main.fontMouseText, ItemTagHandler.GenerateTag(item), Vector2.One))
				{
					Main.PlaySound(12, -1, -1, 1, 1f, 0f);
				}
				return true;
			}
			if (Main.cursorOverride == 3)
			{
				if (!CanFavoriteAt[context])
				{
					return false;
				}
				item.favorited = !item.favorited;
				Main.PlaySound(12, -1, -1, 1, 1f, 0f);
				return true;
			}
			else
			{
				if (Main.cursorOverride == 7)
				{
					inv[slot] = Main.player[Main.myPlayer].GetItem(Main.myPlayer, inv[slot], false, true);
					Main.PlaySound(12, -1, -1, 1, 1f, 0f);
					return true;
				}
				if (Main.cursorOverride == 8)
				{
					inv[slot] = Main.player[Main.myPlayer].GetItem(Main.myPlayer, inv[slot], false, true);
					if (Main.player[Main.myPlayer].chest > -1)
					{
						NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, slot, 0f, 0f, 0, 0, 0);
					}
					return true;
				}
				if (Main.cursorOverride == 9)
				{
					ChestUI.TryPlacingInChest(inv[slot], false);
					return true;
				}
				return false;
			}
		}

		private void ItemSlot_OverrideHover(On.Terraria.UI.ItemSlot.orig_OverrideHover orig, Item[] inv, int context, int slot)
		{
			Item item = inv[slot];
			if (ItemSlot.ShiftInUse && item.type > 0 && item.stack > 0 && !inv[slot].favorited)
			{
				switch (context)
				{
					case 0:
					case 1:
					case 2:
						if (Main.npcShop > 0 && !item.favorited)
						{
							Main.cursorOverride = 10;
						}
						else if (Main.player[Main.myPlayer].chest != -1)
						{
							if (ChestUI.TryPlacingInChest(item, true))
							{
								Main.cursorOverride = 9;
							}
						}
						else
						{
							Main.cursorOverride = 6;
						}
						break;
					case 3:
					case 4:
						if (Main.player[Main.myPlayer].ItemSpace(item))
						{
							Main.cursorOverride = 8;
						}
						break;
					case 5:
					case 8:
					case 9:
					case 10:
					case 11:
					case 12:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
						if (Main.player[Main.myPlayer].ItemSpace(inv[slot]))
						{
							Main.cursorOverride = 7;
						}
						break;
				}
			}
			if (item.IsAir && Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				Main.cursorOverride = 3;
			}
			if (Main.keyState.IsKeyDown(Main.FavoriteKey) && CanFavoriteAt[context])
			{
				if (item.type > 0 && item.stack > 0 && Main.drawingPlayerChat)
				{
					Main.cursorOverride = 2;
					return;
				}
				if (item.type > 0 && item.stack > 0)
				{
					Main.cursorOverride = 3;
				}
			}
		}
	}

	internal class ItemLock : ModItem
	{
		public override void UpdateInventory(Player player)
		{
			if (!item.favorited)
				item.TurnToAir();
		}
	}
}