# SolrTest

This is a console application to test out the functionality or SolrNet

## Setup

1. Start Solr
```
> c:\solr\bin\solr start  -c
```
2. Create Solr Index
```
> c:\solr\bin\solr create -c YeticodeCollection -d .\Solr\YeticodeConfigSet\conf -n YeticodeConfigSet
```
3. Run App
```
> SolrTest\bin\debug\SolrTest.exe
```