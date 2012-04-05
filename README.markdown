DeNSo DB
================================================
DensoDB is a new NoSQL document database. Written for .Net environment in c# language.  
It's simple, fast and reliable.

You can use it in three different ways:

1. InProcess: No need of service installation and communication protocol. The fastest way to use it. You have direct access to the DataBase memory and you can manipulate objects and data in a very fast way. 

2. As a Service: Installed as Windows Service, it can be used as a network document store.You can use rest service or wcf service to access it. It's not different from the previuos way to use it but you have a networking protocol and so it's not fast as the previous one.

3. On a Mesh: mixing the previous two usage mode with a P2P mesh network, it can be easily syncronizable with other mesh nodes. It gives you the power of a distributed scalable fast database, in a server or server-less environment.

You can use it as a database for a stand alone application or in a mesh to share data in a social application. 
The P2P protocol for your application and sincronization rules will be transparet for you, and you'll be able to develop all yor application as it's stand-alone and connected only to a local DB. 

Features
------------------------------------------------

1. Journaled 
2. Built using Command Query Responsibility Segregation pattern in mind. 
3. Store data as Bson-like Documents. 
4. Accessible via Rest 
5. Accessible via WCF 
6. Peer to peer syncronization and event propagation enabled. 
7. Pluggable via Server Plugin to manipulate different commands. 

The Theory
------------------------------------------------

A document database is a Database where data are stored as they are. Usually there is no need to normalize or change their structure. 

Used Patterns
------------------------------------------------
  
	

Practice
------------------------------------------------



Why should i use it ?
------------------------------------------------
