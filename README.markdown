DeNSo DB
================================================
DensoDB is a new NoSQL document database. Written for .Net environment in c# language.  
It's simple, fast and reliable.

You can use it in three different ways:

1. InProcess: No need of service installation and communication protocol. The fastest way to use it. You have direct access to the DataBase memory and you can manipulate objects and data in a very fast way. 

2. As a Service: Installed as Windows Service, it can be used as a network document store.You can use rest service or wcf service to access it. It's not different from the previuos way to use it but you have a networking protocol and so it's not fast as the previous one.

3. On a Mesh: mixing the previous two usage mode with a P2P mesh network, it can be easily syncronizable with other mesh nodes. It gives you the power of a distributed scalable fast database, in a server or server-less environment.

You can use it as a database for a stand alone application or in a mesh to share data in a social application. 
The P2P protocol for your application and syncronization rules will be transparet for you, and you'll be able to develop all yor application as it's stand-alone and connected only to a local DB. 

Features
------------------------------------------------

1. Journaled 
2. Built using Command Query Responsibility Segregation pattern in mind. 
3. Store data as Bson-like Documents. 
4. Accessible via Rest 
5. Accessible via WCF 
6. Peer to peer syncronization and event propagation enabled. 
7. Pluggable via Server Plugin.
8. Use Linq syntax for queries. 

The Theory
------------------------------------------------

A document database is a Database where data are stored as they are. 
Usually there is no need to normalize or change their structure, so it's optimal for domain driven design, because you'll not need to think about how to persist your domain. 

A document is stored in BSon-like format
A document is self contained, and all you needed to understand it's information are in the document itself. 
So you will not need additional query to understand foreign key or lookup table datas. 

You can have collections of document. Collection are not tables, you can think to collections as sets of documents, categorized in the same way.
A collection is schema-free and you can store different kind of documents in the same collection. 

Every document in every collection is classified using a Unique Identifier. 
The DB will use the UID to store and to access every document directly. 

You can store your data in DeNSo using a Bson document or, more easly you can store your entities in DeNSo. 

DeNSo has a Bson De/Serializer able to traduce virtually every entity in a BSon Document and vice-versa. 

Used Patterns
------------------------------------------------
 
DensoDB use Command Query Responsibility Segregation Pattern, because it give a clear easy to manage separation between components in the DataBase. 
CQRS make internal implementation very simple to understand and easy to maintain. It's also efficient and extremely scalable. 

CQRS make easy to manage extensibility, too. 
Extensibility is managed at event dispatcher level. Events can be dispatched in multiple ways and to multiple handlers. ex: P2P support is implemented as DensoDb extension. 

Practice
------------------------------------------------

(coming soon)


Why should i use it ?
------------------------------------------------

It's a different approach to data management, and can be used in a more "social" environment. 
Imagine you need a social application that must work in a partially connected environment, densoDb can help in this kind of approach. 
Some examples (coming soon) show how to use densoDB in this scenario. 


What i'm working on now
------------------------------------------------

More detailed documentation. 
Samples.. so you can start working more easly. 
 