CREATE DATABASE IF NOT EXISTS test
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_0900_as_ci';


-- 
-- Disable foreign keys
-- 
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;

-- 
-- Set SQL mode
-- 
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- 
-- Set character set the client will use to send SQL statements to the server
--
SET NAMES 'utf8mb4';

--
-- Set default database
--
USE test;

--
-- Drop table `example`
--
DROP TABLE IF EXISTS example;

--
-- Drop table `role`
--
DROP TABLE IF EXISTS role;

--
-- Drop table `user`
--
DROP TABLE IF EXISTS user;

--
-- Drop table `user_role`
--
DROP TABLE IF EXISTS user_role;

--
-- Set default database
--
USE test;

--
-- Create table `user_role`
--
CREATE TABLE user_role (
  user_role_id CHAR(36) NOT NULL DEFAULT '',
  user_id CHAR(36) DEFAULT NULL,
  role_id CHAR(36) DEFAULT NULL,
  PRIMARY KEY (user_role_id)
)
ENGINE = INNODB,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_0900_as_ci;

--
-- Create table `user`
--
CREATE TABLE user (
  user_id CHAR(36) NOT NULL DEFAULT '',
  user_name VARCHAR(255) NOT NULL,
  salt VARCHAR(255) DEFAULT NULL,
  password VARCHAR(255) DEFAULT NULL,
  status INT DEFAULT NULL,
  PRIMARY KEY (user_id)
)
ENGINE = INNODB,
AVG_ROW_LENGTH = 16384,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_0900_as_ci;

--
-- Create table `role`
--
CREATE TABLE role (
  role_id CHAR(36) NOT NULL DEFAULT '',
  role_name VARCHAR(255) DEFAULT NULL,
  is_admin BIT(1) DEFAULT NULL,
  permissions LONGTEXT DEFAULT NULL,
  PRIMARY KEY (role_id)
)
ENGINE = INNODB,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_0900_as_ci;

--
-- Create table `example`
--
CREATE TABLE example (
  example_id CHAR(36) NOT NULL DEFAULT '',
  example_name VARCHAR(255) DEFAULT NULL,
  created_date DATETIME DEFAULT NULL,
  create_by VARCHAR(255) DEFAULT NULL,
  modified_date DATETIME DEFAULT NULL,
  modified_by VARCHAR(255) DEFAULT NULL,
  PRIMARY KEY (example_id)
)
ENGINE = INNODB,
CHARACTER SET utf8mb4,
COLLATE utf8mb4_0900_as_ci;

-- 
-- Dumping data for table user_role
--
-- Table test.user_role does not contain any data (it is empty)

-- 
-- Dumping data for table user
--
INSERT INTO user VALUES
('f12d7942-40f9-4d44-98f0-7a6ea1d05bcf', 'admin', '$2a$11$b.2wlRC3YRukAKCl9u678e', '$2a$11$b.2wlRC3YRukAKCl9u678e7yw//aZFcepPa629xNlmyPb4a4jgoOC', NULL);

-- 
-- Dumping data for table role
--
-- Table test.role does not contain any data (it is empty)

-- 
-- Dumping data for table example
--
INSERT INTO example VALUES
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'Tên ví dụ', '2023-10-03 14:32:27', 'admin', '2023-10-03 14:32:27', 'admin');

-- 
-- Restore previous SQL mode
-- 
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;

-- 
-- Enable foreign keys
-- 
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;