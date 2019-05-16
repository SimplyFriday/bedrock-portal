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