# Setup Powershell script 
$version ="1.0.1"

function Prompt-YesNo ($title)
{
    $ans = Read-Host $title
    return ($ans -eq 'Y') -or ($ans -eq 'y')
}

function Connect-SqlServer ($ConnectionString)
{
    $dbconn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $result = $true
    try
    {
        $dbconn.Open()
    }
    catch [Exception]
    {
        $result = $false
    }
    finally
    {
       $dbconn.Close()
    }
    return $result      
}

function Generate-PasswordKey()
{
    $i=0
    $buff = ""
    while ($i -lt 32) 
    {
        $rnd =  Get-Random -Minimum 0 -Maximum 255
        if ($i -eq 0 )
        {
            $buff = $rnd.ToString()
        }
        else 
        {
            $buff = $buff + ", " + $rnd.ToString()
        }
        $i++
    }
    $buff = $buff.Trim()
    return $buff
}

############### MAIN ######################
Clear-Host
$webConfig = "..\web.config"
$webConfigBackup = "web-backup-" + (get-date -Format "yyyyMMdd-hhmmss").ToString() + ".config"
Write-Host "`n`n***** Remote Lab Web Application Setup Script ver $version*****`n`n"

Write-Host "`nBacking up your $webConfig file to $webConfigBackup ..."
Copy-Item -Path $webConfig $webConfigBackup 
Write-Host "...Done`n"

# open the XML file and get the current values 
$doc = (Get-Content $webConfig) -as [xml]
$connStr = $doc.configuration.connectionStrings.add.connectionString
$settings = $doc.configuration.applicationSettings."RemoteLab.Properties.Settings".setting
$encKey = $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='EncryptionKeyForPasswords']/value")."#text"
$AdDNSDomain = $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDNSDomain']/value")."#text"
$AdDomain = $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDomain']/value")."#text"
$AdAdminGroup =$doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='AdministratorADGroup']/value")."#text"
$SmtpFromAddress =$doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='SmtpMessageFromAddress']/value")."#text"
$SmtpServer= $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='SmtpServer']/value")."#text"


# handle default values
if( $encKey -eq "EncryptionKeyForPasswords" ) 
{
    Write-Host "nGenerating Private Key For Password Storage..."
    $encKey = (Generate-PasswordKey).ToString()
    $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='EncryptionKeyForPasswords']/value")."#text" = $encKey
    Write-Host "...Done`n"
}
if ($AdDNSDomain -eq "ActiveDirectoryDNSDomain" )  
{ 
    $AdDNSDomain = $env:USERDNSDOMAIN.ToLowerInvariant() 
    $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDNSDomain']/value")."#text" = $AdDNSDomain 
}
if ($AdDomain -eq "ActiveDirectoryDomain" )  
{ 
    $AdDomain = $env:USERDOMAIN.ToLowerInvariant() 
    $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDomain']/value")."#text" = $AdDomain
}

# Datbase Settings - Connection String
if(Prompt-YesNo -Title "`nYour Connection String is `n`t$connStr `nChange It? [y,n]")
{
    do
    {
        $connStr = "Persist Security Info=True;Data Source=ServerName;Initial Catalog=DbName;User Id=DbUser;Password=DbPassword"
        $DbServer = (Read-Host "Enter Your SQL Server Hostname (eg. sqlserver.mydomain.com )`n:> ").ToString()
        $DbName = (Read-Host "Enter The SQL Server Database Name for the Remote Lab Database (eg. RemoteLabDb)`n:> ").ToString()
        $DbUser = (Read-Host "Enter The SQL Server User Account with Full rights to $DbName `n:> ").ToString()
        $DbPassword = (Read-Host "Enter the Password for $DbUser `n:> ").ToString()
        $connStr = $connStr -replace "ServerName", $DbServer
        $connStr = $connStr -replace "DbName", $DbName
        $connStr = $connStr -replace "DbUser", $DbUser
        $connStr = $connStr -replace "DbPassword", $DbPassword
        Write-Host "Testing Connection to Database..."
        $success =  (Connect-SqlServer -ConnectionString $connStr) 
        if ($Success) {
            Write-Host "...Success"
        } else  {
            Write-Host "...Failed."
        }
    }
    until ( $success)
    $doc.configuration.connectionStrings.add.connectionString = $connStr
}

#Active Directory Settings
if(Prompt-YesNo -Title "`nYour Active Directory Setup `n`tAD DNS Domain: $AdDNSDomain`n`tAD Domain: $AdDomain`n Change These Settings? [y,n]")
{
        $AdDNSDomain = (Read-Host "Enter Your Active Directory DNS Name (eg. ad.yourdomain.com )`n:> ").ToString()
        $AdDomain = (Read-Host "Enter Active Directory Domain Name (Lanman compatible) (eg. AD) `n:> ").ToString()
        $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDNSDomain']/value")."#text" = $AdDNSDomain 
        $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='ActiveDirectoryDomain']/value")."#text" = $AdDomain 
}

#SMTP Settings
if(Prompt-YesNo -Title "`nYour SMTP Server Settings `n`tSMTP Server: $SmtpServer`n`tSMTP FROM Email Address: $SmtpFromAddress`n Change These Settings? [y,n]")
{
        $SmtpServer = (Read-Host "Enter Your SMTP Server Name (eg. smtp-host.yourdomain.com )`n:> ").ToString()
        $SmtpFromAddress = (Read-Host "Enter Your SMTP From Email Address (eg. remotelab@yourdomain.com)`n:> ").ToString()
        $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='SmtpMessageFromAddress']/value")."#text" = $SmtpFromAddress 
        $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='SmtpServer']/value")."#text" = $SmtpServer
}

#Active Directory Admin Group
if(Prompt-YesNo -Title "`nYour Active Directory Admin Group for this Application is`n`t$AdAdminGroup `n Change It? [y,n]")
{
        $AdAdminGroup= (Read-Host "Enter the AD Group for administering this application`n:> ").ToString()
        $doc.SelectSingleNode("//configuration/applicationSettings/RemoteLab.Properties.Settings/setting[@name='AdministratorADGroup']/value")."#text" = $AdAdminGroup 
}


# Last Step, Save the web.config
Write-Host "Saving Your Changes to $webConfig ..."
$doc.Save($webConfig);
Write-Host "...Done"
