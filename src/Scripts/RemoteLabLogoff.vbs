'Remote Lab Logoff Script

on error resume next
dim con, sql, Network

set con = createobject("adodb.connection")
set Network = createobject("wscript.network")

sql = "exec dbo.P_remotelabdb_logoff '" & ucase(network.ComputerName) & "'"
con.connectionstring = connstr

con.open
con.execute sql
con.Close 

set con = nothing
set network = nothing
