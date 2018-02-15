using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Engine;
using Engine.classes;
using System.IO;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player player;
        private Monster currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";
        public SuperAdventure()
        {
            InitializeComponent();
            if (File.Exists(PLAYER_DATA_FILE_NAME)) {
                player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            } else{
                player = Player.CreateDefaultPlayer();
            }
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            updatePlayerStats();
        }
        private void btnNorth_Click(object sender, EventArgs e)
        {
            clearOutput();
            MoveTo(player.CurrentLocation.LocationToNorth);
        }
        private void btnWest_Click(object sender, EventArgs e)
        {
            clearOutput();
            MoveTo(player.CurrentLocation.LocationToWest);

        }
        private void btnEast_Click(object sender, EventArgs e)
        {
            clearOutput();
            MoveTo(player.CurrentLocation.LocationToEast);

        }
        private void btnSouth_Click(object sender, EventArgs e)
        {
            clearOutput();
            MoveTo(player.CurrentLocation.LocationToSouth);

        }
        private void btnUseWeapon_Click(object sender, EventArgs e) {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            // Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);
            // Apply the damage to the monster's CurrentHitPoints
            currentMonster.CurrentHitPoints -= damageToMonster;
            // Display message
            ScrollToBottomOfMessages();
            rtbMessages.Text += "You hit the " + currentMonster.Name + " for " +damageToMonster.ToString() + " points." + Environment.NewLine;
            // Check if the monster is dead
            if (currentMonster.CurrentHitPoints <= 0){
                // Monster is dead
                ScrollToBottomOfMessages();
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + currentMonster.Name + Environment.NewLine;
                // Give player experience points for killing the monster
                player.AddExperiencePoints(currentMonster.RewardExperiencePoints);
                updatePlayerStats();
                rtbMessages.Text += "You receive " + currentMonster.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                // Give player gold for killing the monster
                player.Gold += currentMonster.RewardGold;
                ScrollToBottomOfMessages();
                rtbMessages.Text += "You receive " +
                currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine;
                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();
                // Add items to the lootedItems list, comparing a random number to the droppercentage
                foreach (LootItem lootItem in currentMonster.LootTable) {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage){
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }
                // If no items were randomly selected, then add the default loot item(s).
                if (lootedItems.Count == 0) {
                    foreach (LootItem lootItem in currentMonster.LootTable) {
                        if (lootItem.IsDefaultItem) {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }
                // Add the looted items to the player's inventory
                foreach (InventoryItem inventoryItem in lootedItems) {
                    player.AddItemToInventory(inventoryItem.Details);
                    if (inventoryItem.Quantity == 1) {
                        ScrollToBottomOfMessages();
                        rtbMessages.Text += "You loot " +
                        inventoryItem.Quantity.ToString() + " " +
                        inventoryItem.Details.Name + Environment.NewLine;
                    } else{
                        ScrollToBottomOfMessages();
                        rtbMessages.Text += "You loot " +
                        inventoryItem.Quantity.ToString() + " " +
                        inventoryItem.Details.NamePlural + Environment.NewLine;
                    }
                }
                // Refresh player information and inventory controls
                updatePlayerStats();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();
                // Add a blank line to the messages box, just for appearance.
                ScrollToBottomOfMessages();
                rtbMessages.Text += Environment.NewLine;
                // Move player to current location (to heal player and create a new monsterto fight)
                MoveTo(player.CurrentLocation);
            } else {
                // Monster is still alive
                // Determine the amount of damage the monster does to the player
                int damageToPlayer =
                RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);
                // Display message
                ScrollToBottomOfMessages();
                rtbMessages.Text += "The " + currentMonster.Name + " did " +
                damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
                // Subtract damage from player
                player.CurrentHitPoints -= damageToPlayer;
                // Refresh player data in UI
                updatePlayerStats();
                if (player.CurrentHitPoints <= 0) {
                    // Display message
                    ScrollToBottomOfMessages();
                    rtbMessages.Text += "The " + currentMonster.Name + " killed you." +
                    Environment.NewLine;
                    // Move player to "Home"
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }
        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;
            // Add healing amount to the player's current hit points
            player.CurrentHitPoints = (player.CurrentHitPoints + potion.AmountToHeal);
            // CurrentHitPoints cannot exceed player's MaximumHitPoints
            if (player.CurrentHitPoints > player.MaximumHitPoints) {
                player.CurrentHitPoints = player.MaximumHitPoints;
            }
            // Remove the potion from the player's inventory
            foreach (InventoryItem ii in player.Inventory) {
                if (ii.Details.ID == potion.ID) {
                    ii.Quantity--;
                    break;
                }
            }
            // Display message
            ScrollToBottomOfMessages();
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;
            // Monster gets their turn to attack
            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);
            // Display message
            ScrollToBottomOfMessages();
            rtbMessages.Text += "The " + currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
            // Subtract damage from player
            player.CurrentHitPoints -= damageToPlayer;
            if (player.CurrentHitPoints <= 0)  {
                // Display message
                ScrollToBottomOfMessages();
                rtbMessages.Text += "The " + currentMonster.Name + " killed you." + Environment.NewLine;
                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }
            // Refresh player data in UI
            updatePlayerStats();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }
        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                ScrollToBottomOfMessages();
                rtbMessages.Text += "You must have a " +
                newLocation.ItemRequiredToEnter.Name +
                " to enter this location." + Environment.NewLine;
                return;
            }
            // Update the player's current location
            player.CurrentLocation = newLocation;
            // Show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);
            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;
            // Completely heal the player
            player.CurrentHitPoints = player.MaximumHitPoints;
            // Update Hit Points in UI
            updatePlayerStats();
            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null){
                bool playerAlreadyHasQuest = player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest =player.CompletedThisQuest(newLocation.QuestAvailableHere);
                // See if the player already has the quest
                if (playerAlreadyHasQuest) {
                    // If the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest) {
                        // See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);
                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)  {
                            // Display message
                            ScrollToBottomOfMessages();
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the "+newLocation.QuestAvailableHere.Name +" quest." + Environment.NewLine;
                            // Remove quest items from inventory
                            player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);
                            // Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                                rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() +" experience points" + Environment.NewLine;
                                rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                                rtbMessages.Text +=newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                                rtbMessages.Text += Environment.NewLine;
                            player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            updatePlayerStats();
                            player.Gold += newLocation.QuestAvailableHere.RewardGold;
                            // Add the reward item to the player's inventory
                            player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);
                            // Mark the quest as completed
                            // Find the quest in the player's quest list
                            player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                             }
                        }
                    } else{
                    // The player does not already have the quest
                    // Display the messages
                    ScrollToBottomOfMessages();
                    rtbMessages.Text += "You receive the " +newLocation.QuestAvailableHere.Name +" quest." + Environment.NewLine;
                        rtbMessages.Text += newLocation.QuestAvailableHere.Description +  Environment.NewLine;
                        rtbMessages.Text += "To complete it, return with:" +Environment.NewLine;
                        foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)  {
                            if (qci.Quantity == 1) {
                                rtbMessages.Text += qci.Quantity.ToString() + " " +
                                qci.Details.Name + Environment.NewLine;
                            } else {
                                rtbMessages.Text += qci.Quantity.ToString() + " " +
                                qci.Details.NamePlural + Environment.NewLine;
                            }
                        }
                    ScrollToBottomOfMessages();
                    rtbMessages.Text += Environment.NewLine;
                        // Add the quest to the player's quest list
                        player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                    }
                }
                // Does the location have a monster?
                if (newLocation.MonsterLivingHere != null){
                ScrollToBottomOfMessages();
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name +
                    Environment.NewLine;
                    // Make a new monster, using the values from the standard monsterin the World.Monster list
                    Monster standardMonster = World.MonsterByID(
                    newLocation.MonsterLivingHere.ID);
                    currentMonster = new Monster(standardMonster.ID, standardMonster.Name,
                    standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints,
                    standardMonster.MaximumHitPoints);
                    foreach (LootItem lootItem in standardMonster.LootTable){
                        currentMonster.LootTable.Add(lootItem);
                    }
                    cboWeapons.Visible = true;
                    cboPotions.Visible = true;
                    btnUseWeapon.Visible = true;
                    btnUsePotion.Visible = true;
                }else{
                    currentMonster = null;
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
            // Refresh player's inventory list
            UpdateInventoryListInUI();
            // Refresh player's quest list
            UpdateQuestListInUI();
            // Refresh player's weapons combobox
            UpdateWeaponListInUI();
            // Refresh player's potions combobox
            UpdatePotionListInUI();

        }
        private void UpdateInventoryListInUI(){
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";
            dgvInventory.Rows.Clear();
            foreach (InventoryItem inventoryItem in player.Inventory){
                if (inventoryItem.Quantity > 0) {
                    dgvInventory.Rows.Add(new[] {inventoryItem.Details.Name,inventoryItem.Quantity.ToString() });
                }
            }
        }
        private void UpdateQuestListInUI() {
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";
            dgvQuests.Rows.Clear();
            foreach (PlayerQuest playerQuest in player.Quests){
                dgvQuests.Rows.Add(new[] {playerQuest.Details.Name,playerQuest.IsCompleted.ToString() });
            }
        }
        private void UpdateWeaponListInUI() {
            List<Weapon> weapons = new List<Weapon>();
            foreach (InventoryItem inventoryItem in player.Inventory){
                if (inventoryItem.Details is Weapon) {
                    if (inventoryItem.Quantity > 0) {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }
            if (weapons.Count == 0) {
                // The player doesn't have any weapons, so hide the weapon combobox and "Use" button
            cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            } else{
                cboWeapons.SelectedIndexChanged -=
                cboWeapons_SelectedIndexChanged;
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged +=
                cboWeapons_SelectedIndexChanged;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";
                if (player.CurrentWeapon != null) {
                    cboWeapons.SelectedItem = player.CurrentWeapon;
                } else{
                    cboWeapons.SelectedIndex = 0;
                }
            }
        }
        private void UpdatePotionListInUI(){
            List<HealingPotion> healingPotions = new List<HealingPotion>();
            foreach (InventoryItem inventoryItem in player.Inventory) {
                if (inventoryItem.Details is HealingPotion){
                    if (inventoryItem.Quantity > 0) {
                        healingPotions.Add(
                        (HealingPotion)inventoryItem.Details);
                    }
                }
            }
            if (healingPotions.Count == 0) {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            } else {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
            }
        }
        private void clearOutput() {
            rtbMessages.Clear();
        }
        private void ScrollToBottomOfMessages() {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }
        private void updatePlayerStats() {
            // Refresh player information and inventory controls
            lblHP.Text = player.CurrentHitPoints.ToString();
            lblGold.Text = player.Gold.ToString();
            lblExp.Text = player.ExperiencePoints.ToString();
            lblLvl.Text = player.Level.ToString();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e) {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.toXmlString());
        }
        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}





