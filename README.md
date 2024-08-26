These files contain the frontend and backend code for this webapp. It's inspired by [EmailTrack](https://www.emailmeter.com/). You can check the deployed website [here](https://emailtrack.danyalakt.com/). 

You can also run the project locally using docker compose, but you have to create your own Google Api Client and add the client id and secret in ./backend/EmailStatsService/appsettings.Production.json.
You also need to add the gmail metadata scope to your google api client. 

Regarding emails, the way teams work is that when the user sends an invite to some user's email, it establishes a TCP connection to localhost:4222 instead of doing it from within the program itself. 
The reason I used this approach is because when I was deploying the website, sending emails from within EmailStatsService didn't work for some reason. To add on, I couldn't get EmailSender to work 
with docker compose (probably was an issue related to networking). What worked for me was running the frontend and backend service in docker compose, and running the EmailSender in a docker 
container on localhost. You need to enter email details in ./backend/EmailSender/appsettings.json for the service to work.

![](https://imgur.com/KvscrSY.png)

![](https://imgur.com/IlETg15.png)

TODO: 
- Fix any issues related gmail refetching
- Find and fix any other issues
- Improve UI
