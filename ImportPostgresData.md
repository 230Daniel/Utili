# Import Postgres Data

 - Create a backup of the old database with `pg_dump utili > data.bak`.
 - Move the backup to the repository root directory (where docker-compose commands work).
 - Import the backup with these commands:

```
pi@raspberrypi [~/utili]: docker compose stop
pi@raspberrypi [~/utili]: sudo rm -rf postgresql-data
pi@raspberrypi [~/utili]: docker compose start postgres
pi@raspberrypi [~/utili]: docker compose cp data.bak postgres:data.bak
pi@raspberrypi [~/utili]: docker compose exec postgres /bin/sh
# su postgres
postgres@89f9d9a5c10a:/$ psql utili -U utili < data.bak
postgres@89f9d9a5c10a:/$ exit
# exit
pi@raspberrypi [~/utili]: docker compose start
```
