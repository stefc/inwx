## Accessing INWX API 


For a private project I want to use Let's Encrypt together with the DNS Verification method 
(in short create or update a nameserver TXT record) on my DNS Provider I use here in Germany named INWX. 

For doing this automatic I need a C# client for the API they provide to me 
(API)[https://www.inwx.de/de/help/apidoc]

I don't find any C# adaption on their provided client bindings so I decide to write it by my own. 
But I only implement the following API methods they provide. That's at the moment all I need for getting my work done: 

- account.login
- account.logout
- nameserver.info
- nameserver.createRecord
- nameserver.updateRecord


For explain how it is used have a look on the `/sample` folder. I think it's selfdecriped for any C# developer. 

If you need further methods please put in Pull-Request on that or leave me a short note here.

Stefc