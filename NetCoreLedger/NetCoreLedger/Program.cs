using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using NetCoreLedger.Business;
using NetCoreLedger.Domain;
using NetCoreLedger.Extensions;

namespace NetCoreLedger
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //TestBlock();

            TestChain();

            Console.ReadKey();
        }

        private static void TestBlock()
        {
            var block = new Block();
            //block.Data = "iuHWEILTUHWETIluhwe twiouethg o8972ht o8wieugyt LETI o8927tge wlikutyO*& TG@LTiug wo8t7gw et" +
            //             "wiyet we8ot7y wet" +
            //             "we t" +
            //             "9weu tp97wyet ;iwueyt " +
            //             "2t 2p97ty weiuth we8o7tg wt;uk hwe t" +
            //             "wet[98e ywtw'it we" +
            //             "t8wet";

            var store = new Store("Test");

            foreach (var item in store.EnumerateFile())
            {

            }

            store.Append(block.Header.GetHash(), block);
        }

        private static void TestFill()
        {
            var store = new Store("Test");

            var genesis = new Block(0, DateTime.UtcNow.ToUnixTimeSeconds());
            //store.Append(block.Header.GetHash(), block);
        }

        private static void TestChain()
        {
            var genesis = new Block(0, DateTime.UtcNow.ToUnixTimeSeconds());

            // save genesis to blockStore
            // save genesis to Chain

            var chain = new Chain(genesis.Header);
            var newBlock = new Block(chain.Last.Header.GetHash(), chain.Count, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data!"
            };
            chain.AddLast(newBlock.Header);
            var newBlock2 = new Block(chain.Last.Header.GetHash(), chain.Count, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "More test data!"
            };
            chain.AddLast(newBlock2.Header);
            var newBlock3 = new Block(chain.Last.Header.GetHash(), chain.Count, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "Even more test data!"
            };
            chain.AddLast(newBlock3.Header);


            foreach (var blockHeader in chain.Enumerate())
            {
                
            }

        }

        //public static void Main(string[] args)
        //{
        //    var host = new WebHostBuilder()
        //        .UseKestrel()
        //        .UseContentRoot(Directory.GetCurrentDirectory())
        //        .UseIISIntegration()
        //        .UseStartup<Startup>()
        //        .UseApplicationInsights()
        //        .Build();

        //    host.Run();
        //}
    }
}
