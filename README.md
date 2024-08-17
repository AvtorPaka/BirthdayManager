# Telegram bot for birthday management and birthday reminders.

## The goal
The bot was developed for everyone who forgets to congratulate their friends on their birthday or doesn't know what they want as a gift.

**Link to the bot**: [@BirthdayManagementBot](https://t.me/BirthdayManagementBot) (Deprecated)

## How to use?
1. Create an account
    - Fill date of birth
    - Fill birthday wishes
2. Create or join a group
3. Invite your friends
4. Receive reminders of friend's birthdays and their birthday wishes

## Quick start / Local deployment

* In order to run telegram bot's with webhook you need to obtain web server with SSL certifficats, where it will be set. [Full guide](https://core.telegram.org/bots/webhooks)


* For local deployment your best choice is Ngrok, which provides temporary subdomain, binded to your local port and accessable from the global web.

### 1. Ngrok

* Install Homebrew if you don't already have it

```shell
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

* Install Ngrok, [create account](https://dashboard.ngrok.com/signup), claim and add authtoken

* Start tunnel. You can choose your own port, besides 8443, by changing BirthdayNotificationsBot/Properties/launchSettings.json

```json
"applicationUrl": "http://localhost:8443"
```

```shell
brew install --cask ngrok
ngrok config add-authtoken <token>
ngrok http 8443
```

* Claim forwarding url for your webhoock

```shell
Forwarding {NGROK_URL} -> http://localhost:8443
```

### 2. MySQL

* Run MySQL in docker container

```shell
docker run \
   --name birthday-bot-mysql \
   -e MYSQL_ROOT_PASSWORD= {MYSQL_ROOT_PASSWORD} \
   -v mysql-volume:/var/lib/mysql \
   -p 3306:3306 \
   -d mysql:8.4
```

### 3. Configuration

* Create secrets.json and claim UserSecretsId

```shell
cd BirthdayNotificationsBot
dotnet user-secrets init
grep "UserSecretsId" BirthdayNotificationsBot.csproj
    <UserSecretsId>{YOUR_USER_SECRETS_ID}</UserSecretsId>
```

* Fill data

```shell
echo '{
    "ConnectionStrings": {
        "MySQLConnection": "server=127.0.0.1;port=3306;user=root;password={MYSQL_ROOT_PASSWORD};database=birthday_bot_data;"
    },
    "BotConfiguration": {
        "BotToken": "{YOUR_BOT_TOKEN}",
        "HostAddress": {NGROK_URL},
        "Route": "/bot",
        "SecretToken": "{YOUR_SECRET_TOKEN}"
    }
}' > ~/.microsoft/usersecrets/{YOUR_USER_SECRETS_ID}/secrets.json
```

### 4.  Running

* Native

```shell
cd BirthdayNotificationsBot
dotnet run
```

* Or via docker

```shell
docker buildx build -t telegram-bot-image:local .
docker run -d -p 8443:8080 --name telegram-bot-contaier telegram-bot-image:local
```

## Server delpoyment

* In case you have server with domain name and SSL certs to deploy to, you can try deployment with docker-compose.

* Full [guide](https://core.telegram.org/bots/webhooks) for server configuration

### 1. Configuration

* Create secrets.json and claim UserSecretsId

```shell
cd BirthdayNotificationsBot
dotnet user-secrets init
grep "UserSecretsId" BirthdayNotificationsBot.csproj
    <UserSecretsId>{YOUR_USER_SECRETS_ID}</UserSecretsId>
```


* Fill data

```shell
echo '{
    "ConnectionStrings": {
        "MySQLConnection": "server=localhost;port={Y_MYSQL_PORT};user={Y_MYSQL_USER};password={Y_MYSQL_PASSWORD};database={Y_MYSQL_DATABASE};"
    },
    "BotConfiguration": {
        "BotToken": "{YOUR_BOT_TOKEN}",
        "HostAddress": {YOUR_HOST_ADDRESS},
        "Route": "/bot",
        "SecretToken": "{YOUR_SECRET_TOKEN}"
    }
}' > ~/.microsoft/usersecrets/{YOUR_USER_SECRETS_ID}/secrets.json
```

* Localhost is set as server host for MySQL connection string to provide containers communication within docker-network.

* Create .txt files with MYSQL_PASSWORD and MYSQL_ROOT_PASSWORD in them.

```shell
touch <your_path>/db_root_password.txt
echo "root_password" > <your_path>/db_root_password.txt

touch <your_path>/db_password.txt
echo "password" > <your_path>/db_password.txt
```

* Create and fill .env file

```shell
touch .env
echo "MYSQL_PORT={YOUR_MYSQL_PORT}
MYSQL_DATABASE={YOUR_DB_NAME}
MYSQL_USER={YOUR_MYSQL_USER}
MYSQL_ROOT_PASSWORD_PATH={YOUR_PATH}
MYSQL_PASSWORD_PATH={YOUR_PATH}" > .env
```

### 2. Nginx

* **Put your ssl certs and keys in .PEM format into nginx/ssl folder.**

    **Specifically provide:** 

    1. **certificate.crt**
    2. **certificate.key**
    3. **certificate_ca.crt**

* **In order to set your own Ssl certs data, server_name, resolver and proxy_pass, change nginx.conf in nginx directory**

### 3. Running

* Change platform configuration in docker-compose.yaml depending on your server platform.

```yaml
platform: linux/amd64 #linux/arm64
```

* Run with docker-compose

```shell
docker-compose up -d
```

## Technology stack

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![ASP.NET](https://img.shields.io/badge/ASP.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![EF.Core](https://img.shields.io/badge/EF.Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

![MySQL](https://img.shields.io/badge/MySQL-005C84?style=for-the-badge&logo=mysql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=for-the-badge&logo=docker&logoColor=white)
![Nginx](https://img.shields.io/badge/Nginx-009639?style=for-the-badge&logo=nginx&logoColor=white)