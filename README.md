# SolrTest

This is a console application to test out the functionality or SolrNet

## Setup

1. Start Solr
```cmd
c:\solr\bin\solr start  -c
```
2. Create Solr Collections
```cmd
c:\solr\bin\solr create -c FilesCollection -d .\Solr\FileConfigSet\conf -n FileConfigSet
```
```cmd
c:\solr\bin\solr create -c EmailsCollection -d .\Solr\EmailConfigSet\conf -n EmailConfigSet
```
3. Run App
```cmd
.\SolrTest\bin\debug\SolrTest.exe
```


## Clean Up
Remove Collections
```Powershell
Invoke-WebRequest -Method GET - Uri "http://localhost:8983/solr/admin/collections?action=DELETE&name=FilesCollection"
```
```Powershell
Invoke-WebRequest -Method GET - Uri "http://localhost:8983/solr/admin/collections?action=DELETE&name=EmailsCollection"

```

Remove Config
```Powershell
Invoke-WebRequest -Method GET - Uri "http://localhost:8983/solr/admin/configs?action=DELETE&name=FileConfigSet"
```
```Powershell
Invoke-WebRequest -Method GET - Uri "http://localhost:8983/solr/admin/configs?action=DELETE&name=EmailConfigSet"
```