SELECT Id, QuantityOnHand, QuantityReserved, (QuantityOnHand - QuantityReserved) AS QuantityAvailable
FROM InventoryBatches
WHERE BatchNumber = 'LOADTEST-001';