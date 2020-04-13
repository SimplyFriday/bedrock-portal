# Bedrock Portal
This project aims to create a fully featured and customizable web portal that can be used to host Minecraft Bedrock Edition dedicated servers. It is written in .Net Core which should allow it to run both on Windows and Linux systems, though it is developed and tested against windows.

# Features
* Multiple user tiers (regular user, moderator, admin)
* Dynamically updating whitelist
* A web store for buying in game items
  * In game items are created by issueing console commands 
  * The store is also used to buy membership time on the server whitelist
* Web access to the server console
  * Moderators can be given a limited subset of commands to use
  * Admins can use any command
* Discord integration
  * Send notifications to a webhook when players log in
  * Players can earn points by chatting (requires the Node.js bot in the DiscordReverseWebhook repo)
* Static pages can be easilly added via html files and can even be added to the menu by modifying the appsettings.json file

# Basic Setup (no store)

This software requires both .NET Core and an instance of Microsoft SQL Server Express. You'll need to create a database for the application to use.

## SQL Server

Create a new database using SQL Server Management Studio, then create a new login by running the following statements:

```SQL
CREATE LOGIN PortalUser
WITH PASSWORD = 'Strong Password Goes Here';

-- This is not a great idea as this user has all of the mojo. If you know how to
-- deal with more granular roles, this user needs read/write and ddladmin
ALTER SERVER ROLE sysadmin ADD MEMBER PortalUser;
```


## appsettings.json

Rename **appsettings_template.json** to **appsettings.json**. Open it in a text editor and make the following edits:
* ExePath - should be changed to your BDS exe location
* StartDirectory - should be changed to the BDS install folder
* SystemFromEmailAddress - is the From adress on system generated emails.
* WhiteListPath - should be changed to the whitelist.json location
* DiscordWebhookUrl - The portal will use this webhook to post when anyone logs in
* SendGridApiKey - You can create a free account in order to send system generated emails
* ApplicationTitle - Change to your realm name
* MembershipEnabled - false
* StoreEnabled - false

The following sections are a little more complicated.

### StaticMenuLinks

Anything in here will show up on the top nav menu. You can add custom pages here. Anything placed in ~/wwwroot/html_frag can be used here.

### CommandWhitelistByRole

Currently the only roles are "Admin" and "Moderator". Admins can use all console commands, Moderators can only use the commands listed here.

### ConnectionStrings

Set this to an empty MS SQL Server database. It should look like this:
```"DefaultConnection": "Server=localhost\\dev;Database=PortalTest;User ID=PortalUser;Password=Strong Password Goes Here"```

### Authentication

If you want to enable Google community login, you can use this section.
