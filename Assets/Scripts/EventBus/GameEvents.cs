using UnityEngine;

public record StartDungeon(GameObject Player);

public record LeaveDungeon();

public record OpenShop();

public record CloseShop();

public record OpenInventory(InventoryController Source);

public record CloseInventory();

public record StartGambling();

public record EndGambling();

public record BattleStateChanged(BattleState FromState, BattleState ToState);