-- MySQL dump 10.13  Distrib 5.1.46, for suse-linux-gnu (x86_64)
--
-- Host: localhost    Database: cydin-md
-- ------------------------------------------------------
-- Server version	5.1.46-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `AddinRoot`
--

DROP TABLE IF EXISTS `AddinRoot`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AddinRoot` (
  `RootId` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AppRelease`
--

DROP TABLE IF EXISTS `AppRelease`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AppRelease` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AppVersion` varchar(50) DEFAULT NULL,
  `AddinRootVersion` varchar(50) DEFAULT NULL,
  `LastUpdateTime` datetime NOT NULL,
  `ApplicationId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `new_fk_AppRelease_Application` (`ApplicationId`),
  CONSTRAINT `new_fk_AppRelease_Application` FOREIGN KEY (`ApplicationId`) REFERENCES `Application` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Application`
--

DROP TABLE IF EXISTS `Application`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Application` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(150) COLLATE utf8_unicode_ci NOT NULL,
  `Platforms` varchar(150) COLLATE utf8_unicode_ci DEFAULT NULL,
  `Description` text COLLATE utf8_unicode_ci,
  `Subdomain` varchar(150) COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Configuration`
--

DROP TABLE IF EXISTS `Configuration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Configuration` (
  `Key` varchar(100) COLLATE utf8_unicode_ci NOT NULL,
  `Value` varchar(1000) COLLATE utf8_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Project`
--

DROP TABLE IF EXISTS `Project`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Project` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(150) DEFAULT NULL,
  `Description` text,
  `Trusted` tinyint(1) NOT NULL,
  `AddinId` varchar(500) DEFAULT NULL,
  `ApplicationId` int(11) NOT NULL,
  `IsPublic` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `new_fk_Project_Application` (`ApplicationId`),
  CONSTRAINT `new_fk_Project_Application` FOREIGN KEY (`ApplicationId`) REFERENCES `Application` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Release`
--

DROP TABLE IF EXISTS `Release`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Release` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProjectId` int(11) NOT NULL,
  `Version` varchar(50) DEFAULT NULL,
  `TargetAppVersion` varchar(50) DEFAULT NULL,
  `SourceTagId` int(11) DEFAULT NULL,
  `Platforms` varchar(50) DEFAULT NULL,
  `Status` varchar(50) DEFAULT NULL,
  `LastChangeTime` datetime NOT NULL,
  `DevStatus` int(11) DEFAULT '0',
  `AddinId` varchar(300) DEFAULT NULL,
  `AddinName` varchar(300) DEFAULT NULL,
  `AddinDescription` varchar(600) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `new_fk_constraint_project` (`ProjectId`),
  KEY `new_fk_constraint_source` (`SourceTagId`),
  CONSTRAINT `new_fk_constraint_project` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`Id`),
  CONSTRAINT `new_fk_constraint_source` FOREIGN KEY (`SourceTagId`) REFERENCES `SourceTag` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=94 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReleasePackage`
--

DROP TABLE IF EXISTS `ReleasePackage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReleasePackage` (
  `ReleaseId` int(11) DEFAULT NULL,
  `FileId` varchar(250) COLLATE utf8_unicode_ci NOT NULL,
  `Downloads` int(11) NOT NULL,
  `Platform` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `TargetAppVersion` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`FileId`),
  KEY `fk_ReleasePackage_Release` (`ReleaseId`),
  CONSTRAINT `fk_ReleasePackage_Release` FOREIGN KEY (`ReleaseId`) REFERENCES `Release` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SourceTag`
--

DROP TABLE IF EXISTS `SourceTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SourceTag` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProjectId` int(11) NOT NULL,
  `SourceId` int(11) NOT NULL,
  `Name` varchar(250) DEFAULT NULL,
  `AddinVersion` varchar(50) DEFAULT NULL,
  `TargetAppVersion` varchar(50) DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL,
  `Platforms` varchar(50) DEFAULT NULL,
  `Url` varchar(500) DEFAULT NULL,
  `LastRevision` varchar(100) DEFAULT NULL,
  `DevStatus` int(11) NOT NULL DEFAULT '0',
  `AddinId` varchar(300) DEFAULT NULL,
  `BuildDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `new_fk_constraint_st_project` (`ProjectId`),
  KEY `new_fk_constraint_st_source` (`SourceId`),
  CONSTRAINT `new_fk_constraint_st_project` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`Id`),
  CONSTRAINT `new_fk_constraint_st_source` FOREIGN KEY (`SourceId`) REFERENCES `VcsSource` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=59 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `User` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Login` varchar(50) DEFAULT NULL,
  `Name` varchar(100) DEFAULT NULL,
  `IsAdmin` tinyint(1) NOT NULL,
  `Email` varchar(200) DEFAULT NULL,
  `OpenId` varchar(200) DEFAULT NULL,
  `SiteNotifications` int(11) NOT NULL DEFAULT '0',
  `Password` varchar(100) DEFAULT NULL,
  `PasswordSalt` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserApplication`
--

DROP TABLE IF EXISTS `UserApplication`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserApplication` (
  `UserId` int(11) NOT NULL,
  `ApplicationId` int(11) NOT NULL,
  `Notifications` int(11) NOT NULL,
  `Permissions` int(11) NOT NULL DEFAULT '0',
  `ProjectNotifications` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`UserId`,`ApplicationId`),
  KEY `new_fk_UserApplication_Application` (`ApplicationId`),
  CONSTRAINT `new_fk_UserApplication_Application` FOREIGN KEY (`ApplicationId`) REFERENCES `Application` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `new_fk_UserApplication_User` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserProject`
--

DROP TABLE IF EXISTS `UserProject`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserProject` (
  `ProjectId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Permissions` int(11) NOT NULL DEFAULT '0',
  `Notifications` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ProjectId`,`UserId`),
  KEY `new_fk_constraint_up_user` (`UserId`),
  CONSTRAINT `new_fk_constraint_project2` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `new_fk_constraint_up_user` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `VcsSource`
--

DROP TABLE IF EXISTS `VcsSource`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `VcsSource` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProjectId` int(11) NOT NULL,
  `Url` varchar(250) DEFAULT NULL,
  `Type` varchar(50) DEFAULT NULL,
  `AutoPublish` tinyint(1) NOT NULL,
  `Status` varchar(50) DEFAULT NULL,
  `LastFetchTime` datetime NOT NULL,
  `ErrorMessage` varchar(1000) DEFAULT NULL,
  `FetchFailureCount` int(11) NOT NULL,
  `Tags` varchar(1000) DEFAULT NULL,
  `Branches` varchar(1000) DEFAULT NULL,
  `Directory` varchar(300) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `new_fk_constraint_vcs_project` (`ProjectId`),
  CONSTRAINT `new_fk_constraint_vcs_project` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sysdiagrams`
--

DROP TABLE IF EXISTS `sysdiagrams`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `sysdiagrams` (
  `name` varchar(128) NOT NULL,
  `principal_id` int(11) NOT NULL,
  `diagram_id` int(11) NOT NULL AUTO_INCREMENT,
  `version` int(11) DEFAULT NULL,
  `definition` blob,
  PRIMARY KEY (`diagram_id`),
  UNIQUE KEY `UK_principal_name` (`principal_id`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2010-10-20 12:19:10
