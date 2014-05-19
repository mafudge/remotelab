' Remote Lab Logon Script

on error resume next
dim connstr, remotepowershelluser, poolName

connstr = "Provider=sqloledb;Data Source=ist-s-sql2.ad.syr.edu;Initial Catalog=RemoteLabDbTest;Persist Security Info=True;User ID=RemoteLabDbTest;Password=70NorthIsAPinHead4490;Network Library=dbmssocn;Pooling=true"
remotepowershelluser = "w-ist-labsetup"
poolName = "Prod"
