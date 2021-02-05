# Big Stonks Telegram Bot

## Credentials
- Place Telegram bot key in single line in "bigtelegram.txt" in the visual studio project folder
- Place IEX Cloud secret key in single line in "iex.txt" similarly

## Docker Script

Use rundocker.sh to build and start the application in Docker.
This will delete your old container upon running and create
a new one copying the old database file.

Running this will save the copy of the database to the a backups folder.
