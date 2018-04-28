# Looper

Description:

Works on windows XP/7/10
To be used within a company's PCs fleet for purposes of intra-office communications e.g. news and other information from the marketing / internal communications team. As opposed to using Campaign Monitor or just emails to send out news and other updates.

Three components to the project. The looper - which is the client side XML/RSS type reader that polls the server component, the server component is simply an xml file that holds the articles that can be added by the executable Loop Article Adder. Lastly, the 'link' that is added can be any html page with pictures etc placed on a server internal or external that is accessible from the client side. 

1) Move the 'LoopArticles.xml' inside of the 'Server' directory onto your server share. Read only for all AD domain users to this location. No write/modify permissions. 

This xml file holds the articles added by the administrator. 

2) Modify 'MainWindow.xaml.cs' inside of 'LoopArticleAdder' directory - line 30 to reflect where you placed xml file in step 1.

3) On the client PC on which you will be deploying the client executable, create C:\Temp folder. This will hold the personalcache files that store articles as added to the server module by the administrator. 

4) Still on the client PC, copy the files: 'loop.ico' into C:\Temp folder

5) Modify 'MainWindow.xaml.cs' inside of 'looper' directory - line 24 to reflect the same location as per step 1. i.e. the 'ArticleSource' variable 

6) Still in 'MainWindow.xaml.cs', modify line 43 to whatever your company name is. 

7) Still in 'MainWindow.xaml.cs', modify line 300 to reflect the same location as per step 1. for the first argument.

8) Lastly, in the on line 58, modify the timer.interval as required - source is set to 12 seconds, change to say 5 minutes to have the client poll the server every 5 minutes for new articles. 

9) Compile looper.sln. move the executables to the client PC and place them in c:\Temp\ - run the executable on the PC. Perhaps put it as a startup process so each time a user logs in it runs the app and places the loop icon in the system tray. 

10) next to add articles, compile 'LoopArticleAdder.sln' and run the executable. Add the title, description and link (MUST USE http:// or https:// otherwise this app will run an exception - this is a bug) click Add... this will add a new xml item to the LoopArticles.xml file you specified on the server. After this on the client, after 5 ninutes or whatever time you set at step 8) has elapsed - the new article will be alerted. 

