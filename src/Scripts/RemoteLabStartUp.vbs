'RemoteLab Startup script - this script records the startup event for RemoteLab

on error resume next
dim con, sql , Network

set con = createobject("adodb.connection")
set Network = createobject("wscript.network")

sql = "exec dbo.P_remotelabdb_startup '" & ucase(network.ComputerName) & "', '" & poolName & "'"
con.connectionstring = connstr

con.open
con.execute sql
con.Close 

set con = nothing
set network = nothing