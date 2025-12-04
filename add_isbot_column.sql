-- Add IsBot column to ChatMessages table
ALTER TABLE `ChatMessages` 
ADD COLUMN `IsBot` TINYINT(1) NOT NULL DEFAULT 0;

-- Update existing records: set IsBot = 0 for all (they are user messages)
UPDATE `ChatMessages` SET `IsBot` = 0;
