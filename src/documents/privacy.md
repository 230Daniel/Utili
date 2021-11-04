# Privacy Policy

## Overview

This Privacy Policy describes how your personal information is collected, used, and shared when you visit or make a purchase from https://utili.xyz (the "Site"), and how any sensitive information is processed and stored by the associated Discord bot Utili#5921 (the "Bot").

## Sensitive Information we collect

When you log in to the Site via Discord, we store your unique Discord account ID and its associated email address. We refer to this information as "Account Information".

The Site will also retrieve information about the Discord guilds which you are a member of, however this data will not be stored permanently.

When you add the Bot to a Discord guild it will begin retrieving information about that guild and its members in order to function properly. By default, no data is stored for a guild which the Bot is added to.

Some features of the Bot require the storage of sensitive information to function. When these features are enabled via the Site, the Bot will begin collecting and storing the information described in the table below. If the feature can be enabled within a smaller scope than the entire guild, only the information within the smaller scope will be stored. If the feature is disabled again via the Site, no more data will be collected and stored. We refer to this infromation as "Sensitive Discord Information".

| Feature       | Scope                               | Sensitive information stored if this feature is enabled within the scope                                         | Why this information is required for the feature to function                                                                                                                                                                              |
|---------------|-------------------------------------|------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Autopurge     | Per-Channel                         | The metadata of messages sent within the last 14 days (timestamp, is message pinned, is sender a bot)            | To avoid requesting information on a channel's messages from Discord every time the Bot checks for messages which are due for deletion, metadata about messages is fetched once and then updated whenever necessary                       |
| Inactive Role | Per-Guild                           | A timestamp for each member in the guild which describes when they last sent a message or joined a voice channel | To determine which users haven't been active for long enough to be marked as inactive                                                                                                                                                     |
| Message Logs  | Per-Guild with Per-Channel override | The content and metadata of messages sent within the last 30 days (author id, timestamp, content)                | Once a message has been edited or deleted its previous state is not accessible, so information must be stored about messages before they are edited or deleted in order for it to be possible to retrieve the previous state of a message |
| Role Persist  | Per-Guild with Per-Role override    | The IDs of the roles that a member had when they left the guild                                                  | To add the roles back once the member rejoins                                                                                                                                                                                             |

When we talk about "Sensitive Information" in this Privacy Policy, we are talking both about Account Information and Sensitive Discord Information.

## How we use your Sensitive Information

We use the Account Information that we collect to communicate with you if necessary. We do not use your Account Information for marketing purposes.

Sensitive Discord Information is used autonomously by the Bot to provide our features to applicable Discord guilds.

## Sharing your Sensitive Information

Your Account Information is shared with our billing partner Stripe so that they can power our payments and subscriptions. You can read more about how Stripe uses your Account Information here: https://stripe.com/privacy.

We may also share your Sensitive Information to comply with applicable laws and regulations, to respond to a subpoena, search warrant or other lawful request for information we receive, or to otherwise protect our rights.

## Do not track

Please note that we do not alter our Site’s data collection and use practices when we see a Do Not Track signal from your browser.

## Your Rights

If you are a European resident, you have the right to access personal information we hold about you and to ask that your personal information be corrected, updated, or deleted. If you would like to exercise this right, please contact us through the contact information below.

Additionally, if you are a European resident we note that we are processing your information in order to fulfill contracts we might have with you (for example if you make an order through the Site), or otherwise to pursue our legitimate business interests listed above. Additionally, please note that your information will be transferred outside of Europe, including to the United States.

## Data Retention

All Sensitive Information is retained unless and until you ask us to delete the information, or if the particular piece of Sensitive Information is only to be stored for a specific time period (as described in "Sensitive Information we collect") it will be deleted as soon as that time period has allotted.

## Minors

Persons under the age of 13 are not permitted to use the Site or the Bot.

Utili does not knowingly collect any Personal Identifiable Information from children under the age of 13. If you think that your child provided this kind of information on our website, we strongly encourage you to contact us immediately and we will remove such information from our records as promptly as possible.

## Changes

We may update this privacy policy at any time in order to reflect, for example, changes to our practices or for other operational, legal or regulatory reasons.

## Contact Us

For more information about our privacy practices, if you have questions, if you would like to make a complaint, or if you would like to request access to or the deletion of your data; please contact us by email at dpo@utili.xyz.
