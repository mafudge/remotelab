' Remote Lab Logon Script

on error resume next
dim con, sql , Network, oShell, username, dbusername

set con = createobject("adodb.connection")
set rs = createobject("adodb.recordset")
set Network = createobject("wscript.network")
Set oShell = CreateObject("WScript.Shell") 

sql = "select username from Computers where computername = '" & ucase(network.ComputerName) & "'"
username = lcase(network.username) 
con.connectionstring = connstr
con.Open
Set rs = con.Execute(sql)
dbusername = lcase(rs(0))
If IsNull(dbusername) Then dbusername = ""

If Not (username = remotepowershelluser) Then
	If Not (dbusername = username) Then 
		oShell.Popup "You must use the web interface to access Remote Lab.", 5, "RemoteLab Error", 80000
		oShell.Run "%comspec% /c shutdown /l", , True
	Else
		sql2 = "exec dbo.P_remotelabdb_logon '" & ucase(network.ComputerName) & "','" & lcase(username) & "'"
		set rs = con.Execute(sql2) 
	End If	
End If

con.Close 
Set con = nothing
set network = Nothing



