
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

CREATE TABLE IF NOT EXISTS `Autopurge` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `Timespan` text CHARACTER SET utf8 NOT NULL,
  `Mode` int(11) NOT NULL,
  PRIMARY KEY (`GuildId`,`ChannelId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `ChannelMirroring` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `FromChannelId` bigint(20) unsigned NOT NULL,
  `ToChannelId` bigint(20) unsigned NOT NULL,
  `WebhookId` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`FromChannelId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Core` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `Prefix` text NOT NULL,
  `EnableCommands` bit(1) NOT NULL,
  `ExcludedChannels` mediumtext NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `InactiveRole` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `RoleId` bigint(20) unsigned NOT NULL,
  `ImmuneRoleId` bigint(20) NOT NULL,
  `Threshold` text CHARACTER SET utf8 NOT NULL,
  `Inverse` bit(1) NOT NULL,
  `DefaultLastAction` datetime NOT NULL,
  `LastUpdate` datetime NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `InactiveRoleUsers` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `UserId` bigint(20) unsigned NOT NULL,
  `LastAction` datetime NOT NULL,
  PRIMARY KEY (`GuildId`,`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `JoinMessage` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `Enabled` bit(1) NOT NULL,
  `Direct` bit(1) NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `Title` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Footer` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Content` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Text` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Image` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Thumbnail` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Icon` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Colour` int(10) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `MessageFilter` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `Mode` int(11) NOT NULL,
  `Complex` text CHARACTER SET utf8 NOT NULL,
  PRIMARY KEY (`GuildId`,`ChannelId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `MessageLogs` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `DeletedChannelId` bigint(20) unsigned NOT NULL,
  `EditedChannelId` bigint(20) unsigned NOT NULL,
  `ExcludedChannels` mediumtext CHARACTER SET utf8 NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `MessageLogsMessages` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `MessageId` bigint(20) unsigned NOT NULL,
  `UserId` bigint(20) unsigned NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Content` mediumtext CHARACTER SET utf8 NOT NULL DEFAULT '',
  PRIMARY KEY (`GuildId`,`ChannelId`,`MessageId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `MessagePinning` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `PinChannelId` bigint(20) unsigned NOT NULL,
  `WebhookIds` text NOT NULL DEFAULT '',
  `Pin` bit(1) NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Misc` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `Type` varchar(1000) CHARACTER SET utf8 NOT NULL DEFAULT '0',
  `Value` text CHARACTER SET utf8 NOT NULL,
  PRIMARY KEY (`GuildId`,`Type`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Notices` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `MessageId` bigint(20) unsigned NOT NULL,
  `Enabled` bit(1) NOT NULL,
  `Delay` text CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Title` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Footer` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Content` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Text` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Image` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Thumbnail` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Icon` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `Colour` int(10) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`ChannelId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Premium` (
  `SlotId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) unsigned NOT NULL,
  `GuildId` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`SlotId`)
) ENGINE=InnoDB AUTO_INCREMENT=127 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Reputation` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `Emotes` mediumtext CHARACTER SET utf8 NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `ReputationUsers` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `UserId` bigint(20) unsigned NOT NULL,
  `Reputation` bigint(20) NOT NULL,
  PRIMARY KEY (`GuildId`,`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Roles` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `RolePersist` bit(1) NOT NULL,
  `JoinRoles` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`GuildId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `RolesPersistantRoles` (
  `Id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `GuildId` bigint(20) unsigned NOT NULL,
  `UserId` bigint(20) unsigned NOT NULL,
  `Roles` mediumtext CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Sharding` (
  `Id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `Shards` int(11) unsigned NOT NULL,
  `LowerShardId` int(11) unsigned DEFAULT NULL,
  `Heartbeat` datetime DEFAULT NULL,
  `Guilds` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Subscriptions` (
  `SubscriptionId` varchar(256) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL DEFAULT '',
  `UserId` bigint(20) NOT NULL,
  `EndsAt` datetime NOT NULL,
  `Slots` int(11) NOT NULL,
  PRIMARY KEY (`SubscriptionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Test` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Num` int(11) NOT NULL DEFAULT 0,
  `Dat` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=99093 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `Users` (
  `UserId` bigint(20) unsigned NOT NULL,
  `Email` text CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `LastVisit` datetime NOT NULL,
  `Visits` int(10) NOT NULL DEFAULT 0,
  `CustomerId` text CHARACTER SET utf8 COLLATE utf8_unicode_ci DEFAULT '',
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `VoiceLink` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `Enabled` bit(1) NOT NULL,
  `DeleteChannels` bit(1) NOT NULL,
  `Prefix` text CHARACTER SET utf8 NOT NULL,
  `ExcludedChannels` mediumtext CHARACTER SET utf8 NOT NULL,
  PRIMARY KEY (`GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `VoiceLinkChannels` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `TextChannelId` bigint(20) unsigned NOT NULL,
  `VoiceChannelId` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`VoiceChannelId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `VoiceRoles` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `RoleId` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`GuildId`,`ChannelId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `VoteChannels` (
  `GuildId` bigint(20) unsigned NOT NULL,
  `ChannelId` bigint(20) unsigned NOT NULL,
  `Mode` int(10) unsigned NOT NULL,
  `Emotes` text CHARACTER SET utf8 NOT NULL DEFAULT '',
  PRIMARY KEY (`GuildId`,`ChannelId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
