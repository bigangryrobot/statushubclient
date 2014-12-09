statushubclient
===============

All,

Statushub just got a bit friendlier. After brushing up on my regex tonight, I have coded in friendlier body messages and cleansed titles. This another step towards getting this to the general users. The verbiage right now is static, though eventually I’ll be a good programmer and move it to the config file… so for now if you want to change the verbiage let me know.

Here is an example event before cleaning

Here is after cleaning, yes it reads like a bad foreign translation but it’s a bit better than the spam before

Also as a side note the Merced vpn has been down since 11:12 today…

Clark

From: Clark Beverlin 
Sent: Monday, December 1, 2014 6:05 PM
Subject: Statushubapi v6

All,

I have deployed a fix for the statushub client which contains the following:
•	Better string matching for the events that are down and need to be brought back up
•	Massive code cleanup and refactoring (goodbye spaghetti monster)
•	Default of page timing to GMT + 8
•	Added in match rules for “Solarguard ADO user experience” to set solarguard to degraded state
•	Fixed the clear the board sequence which looped this morning and caused quite a bit of spam

Clark


From: Clark Beverlin 
Sent: Sunday, November 30, 2014 8:26 PM
Subject: RE: Statushubapi v5

All,

I have fixed a rather big issue with the statushub client in version 5. This major issue was that the application could not update statuses that it created, which caused the dashboard to show down states even after resolution which filled the UI with useless info (see below image). I have added in functionality where the system can now update resolution statuses on open issues automatically based on text matching of ipmon emails. Additionally, if it cannot find the matching open issue, it will close all open issues for the service indicated. Lastly, I added a feature where you can email statusupdates@somecompany.com with the subject containing “clean the board please” and it will close all open issues and reset status to green.

Side note, I need to pay for statushub as the trial has expired.

Clark


From: Clark Beverlin 
Sent: Tuesday, November 25, 2014 2:01 PM
To: DL-TECH-IT-INFRA
Subject: Statushubapi v4

All,

I have another update for statushub. It now uses an external file for processing match rules. Look for a file called “matchrules.js” in the install directory of \\someserver\c$\statushubclient. As a bonus, I’ve added in the ability to override the message title and body with a more friendly one of your choosing. It also forces the icon green after resolution as requested by John and Stephen.

Clark

From: Clark Beverlin 
Sent: Friday, November 21, 2014 8:14 PM
Subject: Statushubapi v3

All,

I’ve published another update to the statushub client to address some issues that Kouri brought up. You can now send loosely worded emails to it so as to flag things up or down instead of the confusing and un friendly csv format that I previously setup. Its logic is based on the levenshtein distance (fuzzy match) between what you are sending and what it can look up on the dashboard (I added in dynamic lookup functionality too! No more huge case statements!). This fuzzy match means that it may not mark the flag that you are intending, like for example “GNB Phones” still marks the “Phones” category, but I’ll work on that. Just simply have somewhere in the subject the following:

1.	Service name as it is on the dashboard
2.	Service state such as UP/DOWN/DEGRADED

Here is a working example:

The email I sent

Dashboard result
 
Clark

From: Clark Beverlin 
Sent: Thursday, November 20, 2014 4:37 PM
Subject: RE: Statushubapi v2

All,

I’ve added in support for ipmon alerts for emails sent from ipmon@somecompany.com.

Here is the email that it ingested:
 
Here is the resulting alert on the dashboard:
 
Here is the email it sent out to the subscribers:

I’m doing regex matches for Zipline, Launchpad, Mysolarcity, Nearme, Solarbid, Login, API, AZR (anything azure), VPN, SLC0GPL0 (greatplains) and setting status to down or up accordingly. EVERYTHING NOT MATCHING IS IGNORED. If you have an alert that I should be monitoring, send it to me with a description of what it means.

Clark


From: Clark Beverlin 
Sent: Wednesday, November 19, 2014 3:27 PM
Subject: Statushubapi v1

This application is now housed on slc0utl00 and is working well. I still have some work to do on it. Im guessing that ill have to work on injesting our current email alerts to determine outages and status, so that will be next.
 

From: Clark Beverlin 
Sent: Wednesday, November 19, 2014 3:23 PM
To: statusupdates
Subject: zipline,down,this is a test of the automated system,monitoring

Oh no, zipline is down again… good thing this is only a test of the email alert system…

Clark

From: Clark Beverlin 
Sent: Saturday, November 15, 2014 8:29 PM
Subject: Statushubapi v1

All,

I have created an api for the statushub based statuspage that we are testing. Right now its hosted on my laptop and I need to find a final location for it. Here is how it works:

Simply send an email to statusupdates@somecompany.com. A service (that is off for now) will look for emails in the inbox every 30 seconds and will then process them. To be processed the subject must contain the following and be separated by commas: {#1},{#2},{#3},{#4}
1.	the name of the service as it is on the dashboard example from above is “LAS Phones”
2.	status to display down/up/degradation of service
3.	the title of the event example from above is : ”this is a test of the automated system”
4.	the incident type identified/investigating/resolved/monitoring
5.	body of the email will be the body of the event message

Example: 

From: Clark Beverlin 
Sent: Saturday, November 15, 2014 8:09 PM
To: statusupdates
Subject: LAS Phones,down,this is a test of the automated system,investigating

This is a test and only a test of the automated system in is v1 form
This generates the following on the dashboard
 
