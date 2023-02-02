# Self Hosting

This guide will help you to run your own instance of Utili. It assumes very little technical knowledge, so feel free to use the contents page to skip to the sections which are relevant for you.

The guide assumes that you are on a Windows machine, and that you will host Utili on a Debian virtual machine. This setup method will vary slightly for different Linux distros, but it should work on all of them (thanks Docker). I even tested it on a Raspberry Pi 3b, which has a completely different CPU architecture!


## Contents

1. [Domain Name](#domain-name)

1. [Creating your Discord Application](#creating-your-discord-application)

2. [Provisioning and Configuring a Virtual Server](#provisioning-and-configuring-a-virtual-server)


## Domain Name

1. You need a domain name which the website will be accessible from. A subdomain will suffice, such as `utili.example.com`. Free domain names are available from some sites, but most require an annual subscription. You could also ask your nerdy friend nicely to see if they will lend you a subdomain, as these are free to create.


## Creating your Discord Application

1. Head over to https://discord.com/developers/applications and click "New Application". Enter a suitable name to create the application.

2. Add a cool avatar.

3. Note down the Application ID displayed.

4. On the OAuth2 tab, click Reset Secret and note down the Client Secret displayed.

5. Still on the OAuth2 tab, click "Add Redirect". Enter `https://example.com/signin-discord`, replacing `example.com` with your domain name.

6. Add a second redirect for `https://example.com/return`, again replacing `example.com` with your domain name. Remember to save your changes.

7. On the Bot tab, click "Add Bot".

8. Under the Priviliged Gateway Intents section, enable the Server Members Intent and Message Content Intent.

9. Click "Reset Token" and note down the token displayed.


## Provisioning and Configuring a Virtual Server

1. You need a Linux server to run Utili on. The easiest option is renting a virtual server from a cloud provider. Utili will run with 1 CPU core and 2GB of RAM. Expect to pay around £5 per month for this virtual server.

2. When configuring your server, choose the Debian Linux distribution. If this isn't available, choose something close to it such as Ubuntu. Any Linux distribution should work, but for ease of following this guide I recommend you stick with Debian.

4. Once you have purchased your virtual server, your cloud provider will tell you its IPv4 address. Note this down as you will need it throughout the setup process.

5. Connect to your server over SSH. Again, you cloud provider should provide a guide on how to do this. After this stage you should have remote terminal access to your virtual server.

6. Install sudo by running `apt update && apt install sudo -y`

7. Create a new user by running `useradd -m utili -s /bin/bash`. You can change the username "utili" to anything you want. Add the new user to the sudoers group with `usermod -aG sudo utili`.

8. On your PC, if you do not already have an SSH key, run `ssh-keygen; Get-Contents ~/.ssh/id_rsa.pub` in a PowerShell window to create one. Command prompt is not the same as PowerShell, this will not work in Command Prompt! Copy the public key which is printed, it starts with `ssh-rsa`, and ends with `rsa-key-00000000`.

9. On your virtual server, switch to the newly created user with `su utili`. Create the SSH configuration directory with `mkdir ~/.ssh`. Run `nano ~/.ssh/authorized_keys` to open a text editor. Paste your public key into this file, you might need to right-click or use CTRL+SHIFT+V instead of the usual CTRL+V. Exit the text editor with `CTRL+X`, `y`, then `Enter`.

10. On your PC, confirm that you can log into the new user over SSH. For example, `ssh utili@12.34.56.78`. This will not require a password, instead your computer will automatically authenticate using the key you generated earlier.

11. On your virtual server, return to the root user with `exit`. Open a text editor with `nano /etc/ssh/sshd_config`. Scroll down until you find the `PermitRootLogin` line. Remove the `#` at the start of the line, and change the value to `no`. Remove the `#` in front of `PubkeyAuthentication` and change its value to `yes`. Finally, remove the `#` in front of `PasswordAuthentication` and change its value to `no`. The changed lines should look like this once you have completed this step:

```
PermitRootLogin no
PubkeyAuthentication yes
PasswordAuthentication no
```

12. Restart the SSH service with `systemctl restart sshd`. Confirm that you can still log in to the new user over SSH. Exit the old SSH terminal, and use the direct connection to the user account for the remainder of the setup.
