# Remote Lab

## Overview

Remote Lab in a technology which allows you easily create and manage *virtual labs*. Our definition of *virtual lab* means a user can connect to the desktop of a computer from anywhere without the requrement of being in a specific physical place.

The underlying technology which makes Remote Lab "virtual" is Windows Remote desktop, and fundamentally it is just a system (website, database, and scripts) to manage the complexities associated with a group of Remote Desktop enabled workstations.

## Features

Remote Lab:

1. Allows administrators to group workstations into "pools" for ease of management.
1. Authenticates users and determines the pools they have access to. 
1. Tracks the status of workstations, noting which ones are available, in use, rebooting, not responding to RDP packets, etc.
1. Issues reservations to users requesting access to a workstation, and automatically expires that reservation after a specified amount of time.
1. Monitors resource utilitization so that one user cannot occupy more than one computer within a pool.
1. Only allows a user to connect/logon via RDP when they are issued a resevation via the website. 
1. Logs all activity and events (logon, logoff, startup, shutdown, errors, reservations, etc.) for analysis / reporting.
1. Will automatically reboot computers stuck in an unusable state.
1. Lets administrators view comptuer utilitzation / activity in real-time from the website. 
1. Allows users to release their reservations and select another computer.
1. Integrates with Active Directory for authentication and authorization via AD groups.
1. Is customizable without requiring any knowledge of HTML or programming.

## Requirements

To use Remote Lab within your organization you will need 3 things. Windows Active Directory, a server for running the Remote Lab website, and a bunch of computers designated as remote lab workstations. Specifically, you'll need:

+ A **Windows Active Directory Domain**. The domain is used to 
  1. Manage access to the computers running Windows Remote Desktop, which are part of Remote Lab
  2. Manage user accounts and control access within the applicaiton  
+ A **Windows Server (2008 R2 or Higher)** with the following installed
   1. Internet Information Services to host the Remote Lab web site
   2. SQL Server to host the Remote Lab Database
   3. The .Net Framework v4.5
+ **Computer workstations (Running Windows 7 or higher)**. These workstations can be physical or virtual and are licensed in the same manner you'd license your physical lab. Furthermore each computer must:
   1. Be Joined to the Active Directory Domain
   2. Have Windows Remote Desktop enabled.

## Components

There are several components to the Remote Lab system:

+ **Remote Lab Website**. At the most basic level, the website serves as a Windows Remote Desktop Broker. Users log-on to the website with their Active Directory credentials, and then are issued a reservation for an available workstation. Once the reservation is reserved, the user is issued an RDP file they can use to connect to their reservation.
+ **Remote Lab Control Database**. The database maintains the state of the computers in Remote Lab and records all of the events for analytical purposes.
+ **Remote Lab Logon/Logoff/Startup/Shutdown Scripts** These scripts are responsible for reporting workstation status to the control database. Administrators typically deploy these scripts via Windows Group Policy.
+ **Windows Active Directory** is used for managing authentication and authroization (via AD Groups) in Remote Lab. It also manages authentication to the workstations.
+ **Workstations** are similar to your typical lab workstations, except they are only accessible via Windows Remote Desktop. Workstations can be placed into Pools so that they can be controlled and governed by different groups.

## Installation 