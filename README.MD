# Big Stonks Telegram Bot

## Credentials
- Place Telegram bot key in single line in "bigtelegram.txt" in the visual studio project folder
- Place IEX Cloud secret key in single line in "iex.txt" similarly

## Docker Script

Use rundocker.sh to build and start the application in Docker.
This will delete your old container upon running and create
a new one copying the old database file.

Running this will save the copy of the database to the a backups folder.

You will be prompted for the name of the Docker container. Pick the running
instance of "stonk-dev" and type its name. This will ensure the DB is copied.

## Commands

- /buy SYMBOL SHARES
- /sell SYMBOL SHARES
- /me
- /leaderboard
- /price SYMBOL
