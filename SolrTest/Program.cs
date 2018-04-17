using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonServiceLocator;
using SolrNet;
using SolrTest.Models;

namespace SolrTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Startup.Init<FileIndex>("http://localhost:8983/solr/FilesCollection");
            Startup.Init<EmailIndex>("http://localhost:8983/solr/EmailsCollection");
            var fileIndexSolr = ServiceLocator.Current.GetInstance<ISolrOperations<FileIndex>>();
            var emailIndexSolr = ServiceLocator.Current.GetInstance<ISolrOperations<EmailIndex>>();

            string action;
            do
            {
                Console.WriteLine("Availabe Actions:");
                Console.WriteLine("\t[0] Exit");
                Console.WriteLine("\t[1] Delete");
                Console.WriteLine("\t[2] Index");
                Console.WriteLine("\t[3] Update");
                Console.WriteLine("\t[4] Search");
                Console.WriteLine("\t[5] Set Programming Files Delete Flag True");
                Console.WriteLine("");
                Console.Write("Action: ");
                action = Console.ReadLine();
                switch (action)
                {
                    case "1":
                    {
                        DeleteIndex(fileIndexSolr);
                        DeleteIndex(emailIndexSolr);
                            break;
                    }
                    case "2":
                    {
                        BuildIndex(fileIndexSolr, emailIndexSolr);
                        break;
                    }
                    case "3":
                    {
                        UpdateData(fileIndexSolr);
                        break;
                    }
                    case "4":
                    {
                        Search(fileIndexSolr);
                        break;
                    }
                    case "5":
                    {
                        SetProgrammingFilestoDeleteTrue(fileIndexSolr);
                        break;
                    }
                    default:
                        Console.WriteLine($"Action not found: {action}");
                        break;
                }

                Console.WriteLine("");
            } while (action != "0");

            Console.WriteLine("Done. Press [Enter] to exit.");
            Console.ReadLine();
        }

        public static void Search(ISolrOperations<FileIndex> solr)
        {
            Console.Write("Enter Search Term: ");
            var searchTerm = Console.ReadLine();

            try
            {
                var solrQueryResults = solr.Query(new SolrQuery(searchTerm));
                Console.WriteLine($"Count: {solrQueryResults.Count}");

                foreach (var solrQueryResult in solrQueryResults)
                {
                    Console.WriteLine($"File Path: {solrQueryResult.FilePath}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error performing search");
                Console.WriteLine(e);
            }
        }

        private static void DeleteIndex<T>(ISolrOperations<T> solr)
        {
            Console.WriteLine("Clearing Index");
            var solrQueryResults = solr.Query(new SolrQuery("*"));
            solr.Delete(solrQueryResults);
            solr.Commit();
        }

        private static void SetProgrammingFilestoDeleteTrue(ISolrOperations<FileIndex> solr)
        {
            var solrQueryResults = solr.Query(new SolrQuery("matched_functions:\"Programming Files\""));
            foreach (var solrQueryResult in solrQueryResults)
            {
                var atomicUpdateSpecs = new List<AtomicUpdateSpec>
                {
                    new AtomicUpdateSpec(FileIndex.Keys.Delete, AtomicUpdateType.Set, "true")
                };
                solr.AtomicUpdate(solrQueryResult, atomicUpdateSpecs);
            }

            solr.Commit();
        }

        private static void UpdateData(ISolrOperations<FileIndex> solr)
        {
            Console.WriteLine("Updating Index");
            var dictionary = GetDictionary();
            var solrQueryResults = solr.Query(new SolrQuery("*"));
            foreach (var key in dictionary.Keys)
            {
                foreach (var solrQueryResult in solrQueryResults)
                {
                    var atomicUpdateSpecs = new List<AtomicUpdateSpec>();
                    if (dictionary[key].IsMatch(solrQueryResult.FilePath))
                    {
                        if (!solrQueryResult.MatchedFunctions.Contains(key))
                        {
                            atomicUpdateSpecs.Add(new AtomicUpdateSpec(FileIndex.Keys.MatchedFunctions, AtomicUpdateType.Add, new[] { key }));
                            atomicUpdateSpecs.Add(new AtomicUpdateSpec(FileIndex.Keys.UnmatchedFunctions, AtomicUpdateType.Remove, new[] { key }));
                        }
                    }
                    else
                    {
                        if (!solrQueryResult.UnmatchedFunctions.Contains(key))
                        {
                            atomicUpdateSpecs.Add(new AtomicUpdateSpec(FileIndex.Keys.UnmatchedFunctions,
                                AtomicUpdateType.Add, new[] {key}));
                            atomicUpdateSpecs.Add(new AtomicUpdateSpec(FileIndex.Keys.MatchedFunctions,
                                AtomicUpdateType.Remove, new[] {key}));
                        }
                    }

                    // don't do the update if there is nothing to do as it will
                    // wipe out all of the data
                    if (atomicUpdateSpecs.Any())
                    {
                        solr.AtomicUpdate(solrQueryResult, atomicUpdateSpecs);
                    }
                }

                solr.Commit();
            }
        }

        private static void BuildIndex(ISolrOperations<FileIndex> fileIndexSolr, ISolrOperations<EmailIndex> emailIndexSolr)
        {
            foreach (var file in Directory.GetFiles(@"C:\temp"))
            {
                AddFileToIndex(fileIndexSolr, file);
            }

            AddEmailToIndex(emailIndexSolr, new DateTime(2011, 7,12, 10, 33, 0),  "John Smith", "Jane Doe", "Secret Squirrel Stuff",
                "here is the top secret contents");

            AddEmailToIndex(emailIndexSolr, new DateTime(2011, 7, 12, 10, 35, 0), "Jane Doe", "John Smith", "RE: Secret Squirrel Stuff",
                "thanks for this super secret stuff");
        }

        private static void AddEmailToIndex(ISolrOperations<EmailIndex> solr, DateTime dateSent, string to, string from, string subject,
            string messageBody)
        {
            solr.Add(new EmailIndex
            {
                DateSent = dateSent,
                To = to,
                From = from,
                Subject = subject,
                MessageBody = messageBody
            });
        }

        private static void AddFileToIndex(ISolrOperations<FileIndex> solr, string file)
        {
            Console.WriteLine($"Adding to index: {file}");

            using (FileStream fileStream = File.OpenRead(file))
            {
                    solr.Extract(
                    new ExtractParameters(fileStream, Path.GetFileName(file))
                    {
                        ExtractFormat = ExtractFormat.Text,
                        ExtractOnly = false,
                        Fields = new List<ExtractField>
                        {
                            new ExtractField(FileIndex.Keys.FilePath, file),
                            new ExtractField(FileIndex.Keys.Delete, "false")
                        }
                    });

                solr.Commit();
            }
        }

        public static Dictionary<string, Regex> GetDictionary()
        {
            var dictionary = new Dictionary<string, Regex>
            {
                {"Office Document", new Regex(@"^*(\.pdf)$")},
                {"Garmin Files", new Regex(@"^*(\.fit)$")},
                {"C#", new Regex(@"^*(\.cs)$")},
                {"Typescript", new Regex(@"^*(\.ts)$")},
                {"Programming Files", new Regex(@"^*(\.ts)|(\.cs)$")}
            };

            return dictionary;
        }

        static string CreateId(string file)
        {
            return GetSha256Hash(SHA256.Create(), $"{Environment.MachineName}{file}");
        }

        static string GetSha256Hash(SHA256 shaHash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
