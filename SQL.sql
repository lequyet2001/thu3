USE [master]
GO
/****** Object:  Database [Thu3]    Script Date: 14/01/2024 23:44:40 ******/
CREATE DATABASE [Thu3]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Thu3', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER01\MSSQL\DATA\Thu3.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Thu3_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER01\MSSQL\DATA\Thu3_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Thu3] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Thu3].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Thu3] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Thu3] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Thu3] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Thu3] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Thu3] SET ARITHABORT OFF 
GO
ALTER DATABASE [Thu3] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Thu3] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Thu3] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Thu3] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Thu3] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Thu3] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Thu3] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Thu3] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Thu3] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Thu3] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Thu3] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Thu3] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Thu3] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Thu3] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Thu3] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Thu3] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Thu3] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Thu3] SET RECOVERY FULL 
GO
ALTER DATABASE [Thu3] SET  MULTI_USER 
GO
ALTER DATABASE [Thu3] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Thu3] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Thu3] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Thu3] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Thu3] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Thu3] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'Thu3', N'ON'
GO
ALTER DATABASE [Thu3] SET QUERY_STORE = ON
GO
ALTER DATABASE [Thu3] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Thu3]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[id] [nvarchar](10) NULL,
	[name] [nvarchar](50) NULL,
	[has_name] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[comment]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[comment](
	[id] [nvarchar](10) NULL,
	[content] [nvarchar](50) NULL,
	[created] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feel]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feel](
	[id_post] [nvarchar](10) NULL,
	[id_user] [nvarchar](10) NULL,
	[type] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Image]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Image](
	[id] [nvarchar](10) NOT NULL,
	[image] [nvarchar](max) NULL,
	[id_post] [nvarchar](10) NULL,
	[index] [int] NULL,
 CONSTRAINT [PK_Image] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[List_ban]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[List_ban](
	[id_user] [nvarchar](10) NOT NULL,
	[id_user_was_banned] [nvarchar](10) NULL,
 CONSTRAINT [PK_List_ban] PRIMARY KEY CLUSTERED 
(
	[id_user] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[List_friend]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[List_friend](
	[id_user] [nvarchar](10) NULL,
	[id_friend] [nvarchar](10) NULL,
	[accept] [nvarchar](10) NULL,
	[created] [datetime] NULL,
	[modified] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mark]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mark](
	[id] [nvarchar](10) NULL,
	[type_mark] [nvarchar](10) NULL,
	[mark_content] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[mark_comment]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mark_comment](
	[id] [nvarchar](10) NULL,
	[id_user] [nvarchar](10) NULL,
	[id_post] [nvarchar](10) NULL,
	[id_mark] [nvarchar](10) NULL,
	[id_comment] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Posts]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Posts](
	[id_user] [nvarchar](10) NULL,
	[id] [nvarchar](10) NOT NULL,
	[id_modified] [nvarchar](10) NOT NULL,
	[described] [text] NULL,
	[status] [text] NULL,
	[created] [datetime] NULL,
	[modified] [datetime] NULL,
	[name] [nvarchar](50) NULL,
	[is_marked] [int] NULL,
	[state] [nvarchar](50) NULL,
	[banned] [int] NULL,
 CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[id_modified] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Report]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Report](
	[id] [nvarchar](10) NULL,
	[id_user_report] [nvarchar](10) NULL,
	[id_post] [nvarchar](10) NULL,
	[subject] [nvarchar](50) NULL,
	[details] [text] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Searchs]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Searchs](
	[id] [nvarchar](10) NULL,
	[keyword] [nvarchar](100) NULL,
	[created] [datetime] NULL,
	[id_user] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[id] [nvarchar](10) NULL,
	[usename] [nvarchar](30) NULL,
	[password] [nvarchar](30) NULL,
	[email] [nvarchar](50) NULL,
	[active] [int] NULL,
	[link_avata] [nvarchar](max) NULL,
	[token] [nvarchar](100) NULL,
	[session] [nvarchar](100) NULL,
	[coins] [int] NULL,
	[created] [datetime] NULL,
	[banned] [nvarchar](2) NULL,
	[description] [nvarchar](100) NULL,
	[address] [nvarchar](100) NULL,
	[country] [nvarchar](100) NULL,
	[city] [nvarchar](100) NULL,
	[link] [nvarchar](100) NULL,
	[cover_image] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Video]    Script Date: 14/01/2024 23:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Video](
	[id] [nvarchar](10) NOT NULL,
	[video] [nvarchar](max) NULL,
	[id_post] [nvarchar](10) NULL,
	[thumbnail] [nvarchar](max) NULL,
 CONSTRAINT [PK_Video] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [Thu3] SET  READ_WRITE 
GO
