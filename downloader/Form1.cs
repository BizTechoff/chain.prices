using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
//https://www.npgsql.org/ef6/index.html
namespace downloader
{
    public partial class main : Form
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
         string url,
         string cookieName,
         StringBuilder cookieData,
         ref int size,
         Int32 dwFlags,
         IntPtr lpReserved);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        private const Int32 InternetCookieHttponly = 0x2000;

        string loginUrl = "https://url.publishedprices.co.il/login";
        string logoutUrl = "https://url.publishedprices.co.il/logout";
        bool logedin = false;
        string rootPath = @"D:\documents\הצינור\קבצים";

        bool enableScheduleTask = true;
        chainers selectedChain = new chainers();

        class chainers
        {
            public string link = "";
            public string name = "";
            public string username = "";
            public bool auth = false;
            public bool online = false;
            public bool folders = false;
            public bool pages = false;
            public string code = "";
            public string code2 = "7575757575757";
            public string root = "Root";
            public string subChainRoot = "SubChain";
            public string storeRoot = "Store";
            public string rootPrice = "Root";
            public string rootPriceItem = "Item";
            public string rootPromo = "Root";
            public string rootPromoItem = "Promotion";
        };

        List<chainers> chainersInfo = new List<chainers>();
        int currentIndex = -1;
        string cftpSID = "";

        public main()
        {
            Console.WriteLine("main");
            rootPath = ConfigurationManager.AppSettings.Get("rootPath") ?? @"D:\documents\הצינור\קבצים";
            Console.WriteLine("rootPath: {0}", rootPath);
            InitializeComponent();
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            this.Load += this_Load;
            chains.Items.Clear();
            chains.Items.Add(new ListViewItem() { Text = "רמי לוי", Tag = "RamiLevi" });
            chains.Items.Add(new ListViewItem() { Text = "אושר עד", Tag = "osherad" });
            chains.Items.Add(new ListViewItem() { Text = "יוחננוף", Tag = "yohananof" });
            chains.Items.Add(new ListViewItem() { Text = "חצי חינם", Tag = "HaziHinam" });
            //chains.SelectedIndexChanged += Chains_SelectedIndexChanged;

            chainersInfo.Add(new chainers() { name = "חצי חינם", /*root = "Store",*/ username = "HaziHinam", auth = true, online = true });
            chainersInfo.Add(new chainers() { name = "יוחננוף", username = "yohananof", auth = true });
            chainersInfo.Add(new chainers() { name = "רמי לוי", username = "RamiLevi", auth = true, online = true });
            chainersInfo.Add(new chainers() { name = "אושר עד", username = "osherad", auth = true });

            chainersInfo.Add(new chainers() { name = "שופרסל דיל", root = "{http://www.sap.com/abapxml}abap", subChainRoot = "STORE", storeRoot = "STORE", rootPrice = "root", username = "shufersal", pages = true, online = true, link = "http://prices.shufersal.co.il/" });//http://prices.shufersal.co.il/?page=97
            chainersInfo.Add(new chainers() { name = "יינות ביתן", username = "bitan", online = true, folders = true, link = string.Format("http://publishprice.ybitan.co.il/{0}/", DateTime.Today.ToString("yyyyMMdd")) });
            chainersInfo.Add(new chainers() { name = "ויקטורי", rootPromo = "Promos", rootPromoItem = "Sale", rootPriceItem = "Product", rootPrice = "Prices", root = "Store", subChainRoot = "Branch", storeRoot = "Branch", code = "7290696200003", /*code2 = "7290633800006",*/ username = "victory", online = true, link = "http://matrixcatalog.co.il/NBCompetitionRegulations.aspx" });
            chainersInfo.Add(new chainers() { name = "מחסני השוק", rootPromo = "Promos", rootPromoItem = "Sale", rootPriceItem = "Product", rootPrice = "Prices", root = "Store", subChainRoot = "Branch", storeRoot = "Branch", code = "7290661400001", /*code2 = "7290633800006",*/ username = "shuk", online = true, link = "http://matrixcatalog.co.il/NBCompetitionRegulations.aspx" });


            //browser.Url = new Uri(loginUrl + "?username=osherad");
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.DocumentCompleted += this.browser_DocumentCompleted;
            this.browser.Navigated += this.browser_Navigated;
        }

        private async void this_Load(object sender, EventArgs e)
        {
            await insert_1();
            return;
            Console.WriteLine("this_Load");
            timer.Start();
            if (enableScheduleTask)
            {
                Console.WriteLine(1);
                await Task.Factory.StartNew(async () =>
                {
                    Application.DoEvents();
                    await start_1();
                });
            }
        }

        void exit()
        {
            try { Environment.Exit(7575); }
            catch { };
        }

        async Task start_1()
        {
            Console.WriteLine("start_1");
            await next();
        }

        async Task next()
        {
            Console.WriteLine("next");
            if (currentIndex == chainersInfo.Count - 1)// || (onlyInsert))
            {
                //if (onlyInsert)
                //{
                //    selectedChain = chainersInfo[0];
                //}
                Console.WriteLine("Finished all chainers");
                await insert_1();
                exit();
            }
            else
            {
                ++currentIndex;
                selectedChain = chainersInfo[currentIndex];
                //currentChain = selectedChain.username;
                if (selectedChain.auth)
                {
                    Console.WriteLine("lanching {0} at {1}..", chainersInfo[currentIndex].name, loginUrl + "?username=" + selectedChain.username);
                    browser.Navigate(loginUrl + "?username=" + selectedChain.username);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
                else if (selectedChain.folders)
                {
                    //await next();
                    //browser.Navigate("http://publishprice.ybitan.co.il/");
                    browser.Navigate(selectedChain.link);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
                else if (selectedChain.pages)
                {
                    var pages = 97;
                    for (int i = 1; i <= pages; ++i)
                    {
                        browser.Navigate(selectedChain.link + "?page=" + i);
                        Application.DoEvents();
                        Thread.Sleep(1000);
                        Application.DoEvents();
                        Thread.Sleep(1000);
                        Application.DoEvents();
                        break;
                    }
                }
                else if (selectedChain.link.Contains("matrix"))
                {
                    browser.Navigate(selectedChain.link);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
            }
        }

        async Task download_1()
        {
            Console.WriteLine("download_1");
            cftpSID = getCst();

            string path = rootPath + "\\" + selectedChain.username + "\\" + DateTime.Today.ToString("yyyy.MM.dd");//.AddDays(-1)
            Console.WriteLine("path: {0}", path);
            await downloadFounded(path);
            await Decompress(path);
            await clean(path);
        }

        async Task unzip_1()
        {
            Console.WriteLine("unzip_1");
            await Decompress(null);
        }

        async Task insert_1()
        {
            Console.WriteLine("insert_1");
            string connString = ConfigurationManager.
                ConnectionStrings["salA1000-db"].ConnectionString;
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();
            Stopwatch watch = Stopwatch.StartNew();

            /* STORES */
            Console.WriteLine("retrieving stores..");
            int effectedStores = await importStores(conn);
            Console.WriteLine("retrieved {0} stores. finished at: {1}", effectedStores, watch.Elapsed);

            /* PRODUCTS */
            Console.WriteLine("retrieving products bulk..");
            int effectedProducts = await importProductsBulk(conn);
            Console.WriteLine("retrieved {0} products bulk. finished at: {1}", effectedProducts, watch.Elapsed);

            watch.Stop();
            conn.Close();
            Console.WriteLine("insert Elapsed: {0}", watch.Elapsed);
        }

        string getFileTypeName(string fileName)
        {
            string result = "";
            if (fileName != null)
            {
                List<char> digits = new List<char>("0123456789".ToCharArray());
                fileName = fileName.Trim();
                foreach (var c in fileName)
                {
                    if (digits.Contains(c))
                    {
                        break;
                    }
                    result += c;
                }
            }
            return result;
        }

        string getFileChainCode(string fileName)
        {
            string result = "";
            if (fileName != null)
            {
                List<char> digits = new List<char>("0123456789".ToCharArray());
                fileName = fileName.Trim();

                string[] split = fileName.Split('-');
                if (split.Length > 0)
                {
                    foreach (var c in split[0])
                    {
                        if (digits.Contains(c))
                        {
                            result += c;
                        }
                    }
                }
            }
            return result;
        }

        string getFileStoreCode(string fileName)
        {
            string result = "";
            if (fileName != null)
            {
                var start = fileName.IndexOf('-');
                var end = fileName.LastIndexOf('-');
                if (start == end)
                {
                    Console.WriteLine("{0} contains only one '-'", fileName);
                }
                else
                {
                    result = fileName.Substring(start, end - start).Replace("-", "");
                }
            }
            return result;
        }

        DateTime getFileUpdated(string fileName)
        {
            //PromoFull0000000000000-00-1-202209260603
            DateTime result = DateTime.MinValue;
            if (fileName != null)
            {
                List<char> digits = new List<char>("0123456789".ToCharArray());
                var date = "";
                for (int i = fileName.Length - 1; i >= 0; --i)
                {
                    var c = fileName[i];
                    if (digits.Contains(c))
                    {
                        date += c;
                    }
                    else { break; }
                }
                date = string.Join("", date.Reverse());
                if (date.Length == 12)
                {
                    string m = date.Substring(10, 2);
                    string hh = date.Substring(8, 2);
                    string d = date.Substring(6, 2);
                    string MM = date.Substring(4, 2);
                    string yyyy = date.Substring(0, 4);
                    DateTime.TryParse(string.Format("{0}-{1}-{2} {3}:{4}", yyyy, MM, d, hh, m), out result);
                }
            }
            return result;
        }

        string getStoreType(string type)
        {
            string result = "physical";
            if (type == "2")
            {
                result = "virtual";
            }
            return result;
        }

        async Task<int> importStores(NpgsqlConnection conn)
        {
            Dictionary<string /*code*/, string /*chainId*/> chainsCodes = new Dictionary<string, string>();
            var chains = conn.CreateCommand();
            chains.CommandText = string.Format("SELECT * FROM chains");
            var reader = await chains.ExecuteReaderAsync();
            chains.Dispose();

            // retrieve exists codes from table
            Stopwatch watch = Stopwatch.StartNew();
            while (await reader.ReadAsync())
            {
                string id = reader.GetString(reader.GetOrdinal("id"));
                string chainCode = reader.GetString(reader.GetOrdinal("code"));
                if (!chainsCodes.ContainsKey(chainCode))
                {
                    chainsCodes[chainCode] = id;
                }
            }
            //await reader.CloseAsync();
            await reader.DisposeAsync();
            watch.Stop();
            Console.WriteLine("retrieved {0} exists chain-codes. finished at: {1}", chainsCodes.Count, watch.Elapsed);

            Dictionary<string /*chainId-subChainCode*/, string /*subChainId*/> subChainsCodes = new Dictionary<string, string>();
            var subchains = conn.CreateCommand();
            subchains.CommandText = string.Format("SELECT * FROM subchains");
            reader = await subchains.ExecuteReaderAsync();
            subchains.Dispose();

            // retrieve exists codes from table
            watch = Stopwatch.StartNew();
            while (await reader.ReadAsync())
            {
                string id = reader.GetString(reader.GetOrdinal("id"));
                int subChainCode = reader.GetInt32(reader.GetOrdinal("code"));
                string chainId = reader.GetString(reader.GetOrdinal("chain"));
                string key = string.Format("{0}={1}", chainId, subChainCode);
                if (!subChainsCodes.ContainsKey(key))
                {
                    subChainsCodes[key] = id;
                }
            }
            //reader.Close();
            await reader.DisposeAsync();
            watch.Stop();
            Console.WriteLine("retrieved {0} exists sub-chains-codes. finished at: {1}", subChainsCodes.Count, watch.Elapsed);

            Dictionary<string /*subChainId-storeCode*/, string /*storeId*/> storesCodes = new Dictionary<string, string>();
            var stores = conn.CreateCommand();
            stores.CommandText = string.Format("SELECT * FROM stores");
            reader = await stores.ExecuteReaderAsync();
            stores.Dispose();

            // retrieve exists codes from table
            watch = Stopwatch.StartNew();
            while (await reader.ReadAsync())
            {
                string id = reader.GetString(reader.GetOrdinal("id"));
                int storeCode = reader.GetInt32(reader.GetOrdinal("code"));
                string subChainId = reader.GetString(reader.GetOrdinal("subchain"));
                string key = string.Format("{0}={1}", subChainId, storeCode);
                if (!storesCodes.ContainsKey(key))
                {
                    storesCodes[key] = id;
                }
            }
            //reader.Close();
            await reader.DisposeAsync();
            watch.Stop();
            Console.WriteLine("retrieved {0} exists stores-codes. finished at: {1}", storesCodes.Count, watch.Elapsed);

            // upsert codes table
            int effectedStores = 0;
            watch = Stopwatch.StartNew();
            foreach (chainers c in chainersInfo)
            {
                selectedChain = c;

                string path = rootPath + "\\" + c.username + "\\" + DateTime.Today.ToString("yyyy.MM.dd");
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("directory: '{0}' NOT EXISTS", path);
                    continue;
                }
                foreach (var file in Directory.GetFiles(path, "Stores*.xml", SearchOption.TopDirectoryOnly))
                {
                    Console.WriteLine("{0}", file);
                    if (file.Contains("Full") || file.Contains("full"))
                    {

                    }
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        //foreach(var e in doc.Elements())
                        //{
                        //    Console.WriteLine(e.Name.ToString());
                        //    foreach (var ee in e.Elements())
                        //    {
                        //        Console.WriteLine(ee.Name.ToString());
                        //        foreach (var eee in ee.Elements())
                        //        {
                        //            Console.WriteLine(eee.Name.ToString());
                        //        }
                        //    }
                        //}
                        var root = doc.Element(selectedChain.root);
                        if (selectedChain.username.Contains("shufersal"))
                        {
                            root = root.Element("{http://www.sap.com/abapxml}values");
                        }
                        //else if (selectedChain.username.Contains("shuk"))
                        //{
                        //    root = root.Element("Store");
                        //}
                        if (root != null)
                        {
                            string chainCode = root.Elements().FirstOrDefault(e => e.Name.LocalName.ToLower() == "chainid")?.Value;
                            if (chainCode == null || chainCode.Length == 0)
                            {
                                chainCode = selectedChain.code;
                            }
                            if (!chainsCodes.ContainsKey(chainCode))
                            {
                                var chainId = await upsertChain(conn, root);
                                chainsCodes[chainCode] = chainId;
                            }


                            foreach (var sChain in root.Descendants(selectedChain.subChainRoot))
                            {
                                int subChainCode = int.Parse(sChain.Elements().First(e => e.Name.LocalName.ToLower() == "subchainid").Value);
                                string subChainKey = string.Format("{0}={1}", chainsCodes[chainCode], subChainCode);
                                if (!subChainsCodes.ContainsKey(subChainKey))
                                {
                                    var subChainId = await insertSubChain(conn, sChain, chainsCodes[chainCode]);
                                    subChainsCodes[subChainKey] = subChainId;
                                }

                                if (selectedChain.subChainRoot == selectedChain.storeRoot)
                                {
                                    int storeCode = int.Parse(sChain.Elements().First(e => e.Name.LocalName.ToLower() == "storeid").Value);
                                    string key = string.Format("{0}={1}", subChainsCodes[subChainKey], storeCode);
                                    if (!storesCodes.ContainsKey(key))
                                    {
                                        var storeId = await insertStore(conn, sChain, subChainsCodes[subChainKey]);
                                        storesCodes[key] = storeId;
                                    }
                                }
                                else
                                {
                                    foreach (var store in root.Descendants(selectedChain.storeRoot))
                                    {
                                        int storeCode = int.Parse(store.Elements().First(e => e.Name.LocalName.ToLower() == "storeid").Value);
                                        string key = string.Format("{0}={1}", subChainsCodes[subChainKey], storeCode);
                                        if (!storesCodes.ContainsKey(key))
                                        {
                                            var storeId = await insertStore(conn, store, subChainsCodes[subChainKey]);
                                            storesCodes[key] = storeId;
                                        }
                                    }
                                }
                            }

                            //foreach (var sChain in root.Descendants("SubChain"))
                            //{
                            //    string subChainCode = sChain.Element("SubChainId").Value;
                            //    string subChainKey = string.Format("{0}={1}", chainsCodes[chainCode], subChainCode);
                            //    if (!subChainsCodes.ContainsKey(subChainKey))
                            //    {
                            //        var subChainId = await insertSubChain(conn, sChain, chainsCodes[chainCode]);
                            //        subChainsCodes[subChainKey] = subChainId;
                            //    }

                            //    foreach (var store in root.Descendants(selectedChain.subChainRoot))
                            //    {
                            //        int storeCode = int.Parse(store.Element("StoreId").Value);
                            //        string key = string.Format("{0}={1}", subChainsCodes[subChainKey], storeCode);
                            //        if (!storesCodes.ContainsKey(key))
                            //        {
                            //            var storeId = await insertStore(conn, store, subChainsCodes[subChainKey]);
                            //            storesCodes[key] = storeId;
                            //        }
                            //    }
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error2: file: {0} ex: {1}", file, ex.ToString());
                        continue;
                    }
                }
            }
            return effectedStores;
        }

        async Task<string> upsertChain(NpgsqlConnection conn, XElement root)
        {
            var result = "";
            // Update chain date&time&version if newer
            string chainCode = root.Elements().FirstOrDefault(e => e.Name.LocalName.ToLower() == "chainid")?.Value;
            if (chainCode == null || chainCode.Length == 0)
            {
                chainCode = selectedChain.code;
            }
            var chainCommand = conn.CreateCommand();
            chainCommand.CommandText = string.Format("SELECT * FROM chains WHERE code = '{0}'", chainCode);
            var reader = await chainCommand.ExecuteReaderAsync();
            chainCommand.Dispose();

            if (!reader.HasRows)
            {
                // insert new chain
                await reader.CloseAsync();
                result = Guid.NewGuid().ToString();
                var update = conn.CreateCommand();
                update.CommandText = string.Format("INSERT INTO chains (id,name,code,version,date,time) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')",
                            result,
                            root.Elements().FirstOrDefault(e => e.Name.LocalName.ToLower() == "chainname")?.Value ?? selectedChain.name,
                            chainCode,
                            root.Element("XmlDocVersion")?.Value,
                            root.Element("LastUpdateDate")?.Value ?? root.Attribute("Date")?.Value ?? DateTime.MinValue.ToString("yyyy-MM-dd"),
                            root.Element("LastUpdateTime")?.Value ?? root.Attribute("Time")?.Value ?? "00:00");
                try
                {
                    var effected = await update.ExecuteNonQueryAsync();
                    update.Dispose();
                    //Console.WriteLine("{0} rows effected", effected);
                    //result = effected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error update: CommandText: {0} ex: {1}", update.CommandText, ex.ToString());
                }
            }
            else
            {
                result = reader.GetString(reader.GetOrdinal("id"));
                // update exists chain if newer
                DateTime date;
                DateTime.TryParse(string.Format("{0} {1}",
                    reader.GetDate(reader.GetOrdinal("date")),
                    reader.GetString(reader.GetOrdinal("time"))),
                    out date);

                DateTime rootDate;
                DateTime.TryParse(string.Format("{0} {1}",
                    root.Element("LastUpdateDate")?.Value ?? root.Attribute("Date")?.Value ?? DateTime.MinValue.ToString("yyyy-MM-dd"),
                    root.Element("LastUpdateTime")?.Value ?? root.Attribute("Time")?.Value ?? "00:00"),
                    out rootDate);

                if (rootDate > date)
                {
                    var update = conn.CreateCommand();
                    update.CommandText = string.Format("UPDATE chains SET (date='{0}',time='{1}',version='{2}') WHERE (code='{3}')",
                    root.Element("LastUpdateDate")?.Value ?? root.Attribute("Date")?.Value ?? DateTime.MinValue.ToString("yyyy-MM-dd"),
                    root.Element("LastUpdateTime")?.Value ?? root.Attribute("Time")?.Value ?? "00:00",
                    root.Element("XmlDocVersion")?.Value ?? "");

                    try
                    {
                        var effected = await update.ExecuteNonQueryAsync();
                        //Console.WriteLine("{0} rows effected", effected);
                        //result = effected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error update: CommandText: {0} ex: {1}", update.CommandText, ex.ToString());
                    }
                    update.Dispose();
                }
            }
            await reader.DisposeAsync();
            return result;
        }

        async Task<string> insertSubChain(NpgsqlConnection conn, XElement root, string chain)
        {
            var result = "";
            // Update chain date&time&version if newer
            int subChainCode = int.Parse(root.Elements().First(e => e.Name.LocalName.ToLower() == "subchainid").Value);
            var chainCommand = conn.CreateCommand();
            chainCommand.CommandText = string.Format("SELECT * FROM subchains WHERE chain = '{0}' AND code = '{1}'", chain, subChainCode);
            var reader = await chainCommand.ExecuteReaderAsync();
            chainCommand.Dispose();

            if (!reader.HasRows)
            {
                // insert new chain
                await reader.CloseAsync();
                result = Guid.NewGuid().ToString();
                var update = conn.CreateCommand();
                update.CommandText = string.Format("INSERT INTO subchains (id,name,code,chain) VALUES ('{0}','{1}',{2},'{3}')",
                            result,
                            root.Elements().First(e => e.Name.LocalName.ToLower() == "subchainname").Value,
                            subChainCode,
                            chain);
                try
                {
                    var effected = await update.ExecuteNonQueryAsync();
                    update.Dispose();
                    //Console.WriteLine("{0} rows effected", effected);
                    //result = effected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error update: CommandText: {0} ex: {1}", update.CommandText, ex.ToString());
                }
            }
            else
            {
                result = reader.GetString(reader.GetOrdinal("id"));
            }
            await reader.DisposeAsync();
            return result;
        }

        async Task<string> insertStore(NpgsqlConnection conn, XElement root, string subChain)
        {
            var result = "";
            // Update chain date&time&version if newer
            int storeCode = int.Parse(root.Elements().First(e => e.Name.LocalName.ToLower() == "storeid").Value);
            var chainCommand = conn.CreateCommand();
            chainCommand.CommandText = string.Format("SELECT * FROM stores WHERE subChain = '{0}' AND code = {1}", subChain, storeCode);
            var reader = await chainCommand.ExecuteReaderAsync();
            chainCommand.Dispose();

            if (!reader.HasRows)
            {
                var bikoretNo = root.Elements().First(e => e.Name.LocalName.ToLower() == "bikoretno").Value;
                if (bikoretNo.Length == 0)
                {
                    bikoretNo = "0";
                }
                result = Guid.NewGuid().ToString();
                // insert new chain
                await reader.CloseAsync();
                var update = conn.CreateCommand();
                update.CommandText = string.Format("INSERT INTO stores (id,name,code,bikoret,type,address,city,zip,subchain) VALUES ('{0}','{1}',{2} , {3},'{4}','{5}','{6}','{7}','{8}')",
                            result,
                            root.Elements().First(e => e.Name.LocalName.ToLower() == "storename").Value.Replace("'", "''"),
                            storeCode,
                            int.Parse(bikoretNo),
                            getStoreType(root.Elements().First(e => e.Name.LocalName.ToLower() == "storetype").Value),
                            root.Elements().First(e => e.Name.LocalName.ToLower() == "address").Value.Replace("'", "''"),
                            root.Elements().First(e => e.Name.LocalName.ToLower() == "city").Value,
                            root.Elements().First(e => e.Name.LocalName.ToLower() == "zipcode").Value,
                            subChain);
                try
                {
                    var effected = await update.ExecuteNonQueryAsync();
                    update.Dispose();
                    //Console.WriteLine("{0} rows effected", effected);
                    //result = effected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error update: CommandText: {0} ex: {1}", update.CommandText, ex.ToString());
                }
            }
            else
            {
                result = reader.GetString(reader.GetOrdinal("id"));
                //result = true;
            }
            await reader.DisposeAsync();
            return result;
        }

        class product
        {
            public string id = "";
            public string code = "";
            public DateTime? updated = null;
        }

        class storeProduct
        {
            public string storeId = "";
            public string productId = "";
            public string id = "";
            public string productCode = "";
            public DateTime? updated = null;
        }

        async Task<int> importProductsBulk(NpgsqlConnection conn)
        {
            Console.WriteLine("retrieving products..");
            Dictionary<string /*code*/, product> productsCodes = new Dictionary<string, product>();
            Dictionary<string /*id*/, string /*code*/> productsIds = new Dictionary<string, string>();
            var existsProducts = conn.CreateCommand();
            existsProducts.CommandText = string.Format("SELECT * FROM products");
            var reader = await existsProducts.ExecuteReaderAsync();
            existsProducts.Dispose();

            // retrieve exists codes from table
            Stopwatch watch = Stopwatch.StartNew();
            while (await reader.ReadAsync())
            {
                string id = reader.GetString(reader.GetOrdinal("id"));
                string code = reader.GetString(reader.GetOrdinal("code"));
                DateTime updated = reader.GetDateTime(reader.GetOrdinal("updated"));
                if (!productsCodes.ContainsKey(code))
                {
                    productsCodes[code] = new product { id = id, code = code, updated = updated };
                    productsIds[id] = code;
                }
            }
            reader.Close();
            await reader.DisposeAsync();
            watch.Stop();
            Console.WriteLine("retrieved {0} products exists codes. finished at: {1}", productsCodes.Count, watch.Elapsed);

            watch.Restart();
            Dictionary<string /*storeId*/, Dictionary<string /*productCode*/, storeProduct>> storeProductsCodes = new Dictionary<string, Dictionary<string, storeProduct>>();

            Console.WriteLine("retrieving stores-products..");
            var existsStoreProducts = conn.CreateCommand();
            existsStoreProducts.CommandText = string.Format("SELECT * FROM storesproducts");
            //await existsStoreProducts.PrepareAsync();
            var readerStoreProducts = await existsStoreProducts.ExecuteReaderAsync();
            existsStoreProducts.Dispose();

            // retrieve exists store-products from table
            while (await readerStoreProducts.ReadAsync())
            {
                string storeId = readerStoreProducts.GetString(readerStoreProducts.GetOrdinal("store"));
                string productId = readerStoreProducts.GetString(readerStoreProducts.GetOrdinal("product"));
                string id = readerStoreProducts.GetString(readerStoreProducts.GetOrdinal("id"));
                DateTime updated = readerStoreProducts.GetDateTime(readerStoreProducts.GetOrdinal("updated"));
                string productCode = productsIds[productId];

                if (!storeProductsCodes.ContainsKey(storeId))
                {
                    storeProductsCodes[storeId] = new Dictionary<string, storeProduct>();
                }
                if (!storeProductsCodes[storeId].ContainsKey(productCode))
                {
                    storeProductsCodes[storeId][productCode] = new storeProduct()
                    {
                        storeId = storeId,
                        productId = productId,
                        id = id,
                        productCode = productCode,
                        updated = updated
                    };
                }
            }
            readerStoreProducts.Close();
            await readerStoreProducts.DisposeAsync();

            Console.WriteLine("Loading storesproducts took: {0}", watch.Elapsed);


            // upsert codes table
            int effectedProducts = 0;
            watch = Stopwatch.StartNew();
            foreach (chainers c in chainersInfo)
            {
                selectedChain = c;

                string path = rootPath + "\\" + selectedChain.username + "\\" + DateTime.Today.ToString("yyyy.MM.dd");
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("directory: '{0}' NOT EXISTS", path);
                    continue;
                }
                int counter = 0;
                string fileType = "Price*.xml";
                List<string> files = new List<string>(
                    Directory.GetFiles(path, fileType, SearchOption.TopDirectoryOnly));
                foreach (var file in files)
                {
                    ++counter;
                    Console.WriteLine("{0}= {1}/{2}: {3}", selectedChain.username, counter, files.Count, file);
                    try
                    {
                        var watcher = Stopwatch.StartNew();
                        XDocument doc = XDocument.Load(file);
                        Console.WriteLine("Loading file {0} took: {1}", file, watch.Elapsed);
                        //watch.Restart();
                        var root = doc.Element(selectedChain.rootPrice);
                        if (root != null)
                        {
                            string chain = root.Elements().First(e => e.Name.LocalName.ToLower() == "chainid").Value;
                            int subChain = int.Parse(root.Elements().First(e => e.Name.LocalName.ToLower() == "subchainid").Value);
                            int store = int.Parse(root.Elements().First(e => e.Name.LocalName.ToLower() == "storeid").Value);
                            Console.WriteLine("ChainId: {0}, SubChainId: {1}, StoreId: {2},", chain, subChain, store);//, root.Value);

                            string q = string.Format(
                                "SELECT stores.id FROM stores WHERE stores.code = {0} AND stores.subchain = " +
                                "(SELECT subchains.id FROM subchains WHERE subchains.code = {1} AND subchains.chain = " +
                                "(SELECT chains.id FROM chains WHERE chains.code = '{2}'))", store, subChain, chain);
                            var select = conn.CreateCommand();
                            select.CommandText = q;
                            var storeId = await select.ExecuteScalarAsync() as string;
                            select.Dispose();

                            if (storeId == null)
                            {
                                // found xml with store that NOT in storeslist.xml, so create her with 'unknown name'
                                //storeId = await insertStoreUnknoen(conn,)
                                Console.WriteLine("FOUND UnKnown STORE at file: {0}", file);
                                continue;
                            }

                            // add to products if not exists
                            if (!storeProductsCodes.ContainsKey(storeId))
                            {
                                storeProductsCodes[storeId] = new Dictionary<string, storeProduct>();
                            }

                            var total = root.Descendants(selectedChain.rootPriceItem).Count();
                            var productsCounter = 0;
                            var counterContains = 0;

                            string queryProducts = "INSERT INTO products (id,code,type,manufacturerName,manufactureCountry,manufacturerDesc,unitQty,quantity,unitOfMeasure,isWeighted,qtyInPackage,minPrice,maxPrice,updated) VALUES {0}";
                            string parmsProducts = "";
                            string queryStoresProducts = "INSERT INTO storesproducts (id,store,product,price,unitOfMeasurePrice,allowDiscount,status,sku,updated,name) VALUES {0}";
                            string parmsStoresProducts = "";

                            // update params
                            var priceCases = new List<string>();
                            var unitOfMeasurePriceCases = new List<string>();
                            var allowDiscountCases = new List<string>();
                            var updatedCases = new List<string>();
                            var statusCases = new List<string>();
                            var skuCases = new List<string>();
                            //var storesWhere = new List<string>();
                            var productsWhere = new List<string>();

                            foreach (var item in root.Descendants(selectedChain.rootPriceItem))
                            {
                                ++productsCounter;
                                if (productsCounter % 500 == 0)
                                {
                                    Console.WriteLine("Product {0} of {1}", productsCounter, total);
                                }

                                float price;
                                float.TryParse(item.Element("ItemPrice").Value, out price);
                                DateTime spUpdated;
                                DateTime.TryParse(item.Element("PriceUpdateDate").Value, out spUpdated);
                                string pCode = item.Element("ItemCode").Value;

                                if (!productsCodes.ContainsKey(pCode))
                                {
                                    // insert products
                                    var productId = Guid.NewGuid().ToString();
                                    productsCodes[pCode] = new product { id = productId, code = pCode, updated = spUpdated };

                                    if (parmsProducts.Length > 0)
                                    {
                                        parmsProducts += ",";
                                    }
                                    parmsProducts += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                                        productId,
                                        item.Element("ItemCode").Value,
                                        item.Element("ItemType").Value,
                                        (item.Element("ManufacturerName")?.Value ?? item.Element("ManufactureName").Value).Replace("'", "''"),//root.Element("ItemName").Value
                                        item.Element("ManufactureCountry").Value,
                                        (item.Element("ManufacturerItemDescription")?.Value ?? item.Element("ManufactureItemDescription").Value).Replace("'", "''"),
                                        item.Element("UnitQty").Value.Replace("'", "''"),
                                        item.Element("Quantity").Value,
                                        (item.Element("UnitOfMeasure")?.Value ?? item.Element("UnitMeasure").Value).Replace("'", "''"),
                                        (item.Element("bIsWeighted")?.Value ?? item.Element("BisWeighted").Value),
                                        item.Element("QtyInPackage").Value,
                                        0.0,
                                        0.0,
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                                }

                                if (!storeProductsCodes[storeId].ContainsKey(pCode))
                                {
                                    // insert stores-products
                                    storeProductsCodes[storeId][pCode] = new storeProduct() { storeId = storeId, productId = productsCodes[pCode].id, id = Guid.NewGuid().ToString(), productCode = pCode, updated = spUpdated };

                                    if (parmsStoresProducts.Length > 0)
                                    {
                                        parmsStoresProducts += ",";
                                    }
                                    parmsStoresProducts += string.Format("('{0}','{1}','{2}', {3}, {4},'{5}','{6}','{7}','{8}','{9}')",
                                        storeProductsCodes[storeId][pCode].id,
                                        storeId,
                                        storeProductsCodes[storeId][pCode].productId,
                                        price,
                                        item.Element("UnitOfMeasurePrice").Value,
                                        item.Element("AllowDiscount").Value == "1",
                                        (item.Element("ItemStatus")?.Value ?? item.Element("itemStatus").Value) == "1",
                                        item.Element("ItemId")?.Value ?? "",
                                        spUpdated.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
                                        item.Element("ItemName").Value.Replace("'", "''"));
                                }

                                else if (spUpdated > storeProductsCodes[storeId][pCode].updated)
                                {
                                    // update stores-products
                                    storeProductsCodes[storeId][pCode].updated = spUpdated;
                                    var productId = storeProductsCodes[storeId][pCode].productId;

                                    ++counterContains;
                                    float unitOfMeasurePrice = 0;
                                    float.TryParse(item.Element("UnitOfMeasurePrice").Value, out unitOfMeasurePrice);
                                    bool allowDiscount = item.Element("AllowDiscount").Value == "1";
                                    bool status = (item.Element("ItemStatus")?.Value ?? item.Element("itemStatus").Value) == "1";
                                    string sku = item.Element("ItemId")?.Value ?? "";

                                    priceCases.Add(string.Format("WHEN '{0}' THEN {1}", productId, price));
                                    unitOfMeasurePriceCases.Add(string.Format("WHEN '{0}' THEN {1}", productId, unitOfMeasurePrice));
                                    allowDiscountCases.Add(string.Format("WHEN '{0}' THEN '{1}'", productId, allowDiscount));
                                    updatedCases.Add(string.Format("WHEN '{0}' THEN '{1}'", productId, spUpdated));
                                    statusCases.Add(string.Format("WHEN '{0}' THEN '{1}'", productId, status));
                                    skuCases.Add(string.Format("WHEN '{0}' THEN '{1}'", productId, sku));
                                    productsWhere.Add(string.Format("'{0}'", productId));
                                }
                            }

                            // insert products
                            if (parmsProducts.Length > 0)
                            {
                                var insert = conn.CreateCommand();
                                insert.CommandText = string.Format(queryProducts, parmsProducts);
                                try
                                {
                                    var effected = await insert.ExecuteNonQueryAsync();
                                    Console.WriteLine("Products: {0} rows effected of total {1} rows-in-command, {2} already contains", effected, counter, counterContains);
                                    if (effected != counter)
                                    {

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed insert command: {0}, error: {1}", insert.CommandText, ex.ToString());
                                }
                                insert.Dispose();
                            }

                            // insert stores-products
                            if (parmsStoresProducts.Length > 0)
                            {
                                var insert = conn.CreateCommand();
                                insert.CommandText = string.Format(queryStoresProducts, parmsStoresProducts);
                                try
                                {
                                    var effected = await insert.ExecuteNonQueryAsync();
                                    Console.WriteLine("StoresProducts: {0} rows effected", effected);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed insert command: {0}, error: {1}", insert.CommandText, ex.ToString());
                                }
                                insert.Dispose();
                            }

                            // update stores-products
                            if (productsWhere.Count > 0)
                            {
                                var queryStoresProductsUpdate = "" +
                                    "UPDATE storesproducts SET " +
                                    "price = (" +
                                    "   CASE product " +
                                            string.Join(" ", priceCases) +
                                    "       ELSE price" +
                                    "   END" +
                                    " )," +
                                    "unitOfMeasurePrice = (" +
                                    "   CASE product " +
                                            string.Join(" ", unitOfMeasurePriceCases) +
                                    "       ELSE unitOfMeasurePrice" +
                                    "   END" +
                                    " )," +
                                    "allowDiscount = (" +
                                    "   CASE product " +
                                            string.Join(" ", allowDiscountCases) +
                                    "       ELSE allowDiscount" +
                                    "   END" +
                                    " )," +
                                    "updated = (" +
                                    "   CASE product " +
                                            string.Join(" ", updatedCases) +
                                    "       ELSE updated" +
                                    "   END" +
                                    " )," +
                                    "status = (" +
                                    "   CASE product " +
                                            string.Join(" ", statusCases) +
                                    "       ELSE status" +
                                    "   END" +
                                    " )," +
                                    "sku = (" +
                                    "   CASE product " +
                                            string.Join(" ", skuCases) +
                                    "       ELSE sku" +
                                    "   END" +
                                    " )" +
                                    string.Format(" WHERE store = '{0}' AND product IN ({1})", storeId, string.Join(",", productsWhere));

                                var update = conn.CreateCommand();
                                update.CommandText = queryStoresProductsUpdate;
                                try
                                {
                                    var effected = await update.ExecuteNonQueryAsync();
                                    Console.WriteLine("update StoresProducts: {0} rows effected", effected);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed update command: {0}, error: {1}", update.CommandText, ex.ToString());
                                }
                                update.Dispose();
                            }
                        }

                        //Console.WriteLine("Loading file {0} took: {1}", file, watch.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error2: file: {0} ex: {1}", file, ex.ToString());
                        continue;
                    }
                }
            }
            return effectedProducts;
        }

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        CookieContainer GetUriCookieContainer(Uri uri)
        {
            Console.WriteLine("GetUriCookieContainer");
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        string getCst()
        {
            string result = "";
            var c = GetUriCookieContainer(browser.Url);
            if (c != null)
            {
                var vv = c.GetCookieHeader(browser.Url);
                Console.WriteLine(vv);
                //cftpSID=2nSQ7IbxMaS4lwFyL21ImRNqrJWJ8wGvFQa0l0m65Rc
                var s = vv.Split('=');
                if (s.Length > 1)
                {
                    result = s[1];//#####
                }
            }
            return result;
        }

        class chainFile
        {
            public string chain = "";
            public string store = "";
            public string type = "";
            public DateTime updated = DateTime.MinValue;
            public string link = "";

            public override string ToString()
            {
                string result = "";
                result += "chain: " + chain;
                result += " | ";
                result += "store: " + store;
                result += " | ";
                result += "type: " + type;
                result += " | ";
                result += "updated: " + updated.ToString("yyyy-MM-dd HH:mm");
                result += " | ";
                result += "link: " + link;
                return result;
            }
        }

        async Task<List<string>> downloadFounded(string path)
        {
            Console.WriteLine("downloadFounded");
            List<string> result = new List<string>();
            string elemTag = path.Contains("bitan") ? "files" : path.Contains("shufersal") ? "gridContainer" : path.Contains("victory") || path.Contains("shuk") ? "download_content" : "fileList";
            var files = await getReleventFiles(path, elemTag);
            foreach (var f in files) // ordered by store-filetype
            {
                if (f.link.ToLower().Contains("stores"))
                {

                }
                if (selectedChain.code.Length == 0 || f.link.Contains(selectedChain.code + "-") || f.link.Contains(selectedChain.code2 + "-"))
                {
                    Console.WriteLine("Download: {0}", f);

                    using (var client = new WebClient())
                    {
                        client.Headers.Add("Accept: application / json, text / javascript, */*; q=0.01");
                        client.Headers.Add("Accept-Encoding: gzip, deflate, br");
                        client.Headers.Add("Accept-Language: en-US,en;q=0.9");
                        client.Headers.Add("Content-Type: application/x-www-form-urlencoded; charset=UTF-8");
                        if (cftpSID.Length > 0 && !path.Contains("shufersal") && !path.Contains("victory") && !path.Contains("shuk"))
                        {
                            client.Headers.Add("Cookie: cftpSID=" + cftpSID);//??
                            //client.Headers.Add("Host: url.publishedprices.co.il");
                            //client.Headers.Add("Origin: https://url.publishedprices.co.il");
                            //client.Headers.Add("Referer: https://url.publishedprices.co.il/file");
                        }
                        client.Headers.Add("Sec-Fetch-Dest: empty");
                        client.Headers.Add("Sec-Fetch-Mode: cors");
                        client.Headers.Add("Sec-Fetch-Site: same-origin");
                        client.Headers.Add("User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1");
                        client.Headers.Add("X-Requested-With: XMLHttpRequest");

                        try
                        {
                            var link = f.link.Split('?')[0];
                            var fileName = Path.GetFileName(link);
                            if (fileName.EndsWith(".gz"))
                            {
                                if (fileName.Contains(".xml"))//matrix
                                {
                                    fileName = fileName.Replace("-000.xml", "").Replace("-001.xml", "").Replace(".xml", "");
                                }
                            }

                            await client.DownloadFileTaskAsync(
                                f.link,
                                Path.Combine(path, fileName));
                            //Decompress("");
                        }
                        catch (Exception ex) { Console.WriteLine(ex?.ToString()); }
                    }
                }
            }
            return result;
        }


        public async Task Decompress(string path)
        {
            Console.WriteLine("Decompress");
            FileInfo[] files = new FileInfo[] { };
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                files = di.GetFiles();
            }

            int counter = 0;
            foreach (FileInfo f in files)
            {
                if (f.Extension.Contains("gz") && f.Length > 0)
                {
                    using (FileStream originalFileStream = f.OpenRead())
                    {
                        string currentFileName = f.FullName;
                        string newFileName = currentFileName.Remove(currentFileName.Length - f.Extension.Length) + ".xml";

                        using (FileStream decompressedFileStream = File.Create(newFileName))
                        {
                            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                            {
                                try
                                {
                                    await decompressionStream.CopyToAsync(decompressedFileStream);
                                    Console.WriteLine("Decompressed: {0}", f.Name);
                                    ++counter;
                                }
                                catch (Exception ex) { Console.WriteLine("{0}.error: {1}", f.Name, ex.ToString()); }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Decompress: {0} files", counter);
        }

        private async void start_Click(object sender, EventArgs e)
        {
            Console.WriteLine("start_Click");
            await Task.Factory.StartNew(() => { start_1(); });
        }

        private async void unzip_Click(object sender, EventArgs e)
        {
            Console.WriteLine("unzip_Click");
            await unzip_1();
        }

        private async void download_Click(object sender, EventArgs e)
        {
            Console.WriteLine("download_Click");
            await Task.Factory.StartNew(() => { download_1(); });
        }

        void login()
        {
            Console.WriteLine("login");
            var v = browser.Document.GetElementById("login-button").InvokeMember("Click");
            Application.DoEvents();
            logedin = true;
        }

        void logout()
        {
            Console.WriteLine("logout");
            browser.Navigate(logoutUrl);
            Application.DoEvents();
            logedin = false;
        }

        async void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Console.WriteLine("browser_DocumentCompleted: ( url: {0} )", e.Url);
            Application.DoEvents();
            Application.DoEvents();

            var uri = e?.Url.AbsoluteUri;
            if (uri.Contains("login?m="))
            {
                Application.DoEvents();
                await next();
            }
            else if (uri.Contains("login"))
            {
                Console.WriteLine("login");
                if (!logedin)
                {
                    Application.DoEvents();
                    Thread.Sleep(5 * 1000);
                    Application.DoEvents();
                    Thread.Sleep(5 * 1000);
                    Application.DoEvents();
                    login();
                }
            }
            else if (uri.Contains("file") || uri.Contains("bitan") || uri.Contains("matrix"))
            {
                Console.WriteLine("file downloading..");
                Application.DoEvents();
                Application.DoEvents();
                Thread.Sleep(5 * 1000);
                Application.DoEvents();
                Application.DoEvents();
                Thread.Sleep(5 * 1000);
                Application.DoEvents();
                Application.DoEvents();
                await download_1();
                if (logedin)
                {
                    Application.DoEvents();
                    Thread.Sleep(5 * 1000);
                    Application.DoEvents();
                    Thread.Sleep(5 * 1000);
                    Application.DoEvents();
                    logout();
                }
                else
                {
                    Application.DoEvents();
                    await next();
                }
            }
            else if (uri.Contains("shufersal"))
            {
                Console.WriteLine("file downloading..");
                Application.DoEvents();
                Application.DoEvents();
                Thread.Sleep(5 * 1000);
                Application.DoEvents();
                Application.DoEvents();
                Thread.Sleep(5 * 1000);
                Application.DoEvents();
                Application.DoEvents();
                await download_1();
                Application.DoEvents();
                await next();
                //else if (uri.Contains("bitan"))
                //{
                //    Console.WriteLine("bitan");
                //    //var files = await getReleventFiles(path);
                //    string path = rootPath + "\\" + currentChain + "\\" + DateTime.Today.ToString("yyyy.MM.dd");//.AddDays(-1)

                //    var files = await getReleventFiles(path, );
                //    Console.WriteLine(string.Join("\n", files));
                //}
            }
        }


        /// <summary>
        /// Download files from current browser.document
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task<List<chainFile>> getReleventFiles(string path, string rootFiles = "fileList")
        {
            //ValueTuple<string /*Store*/, string /*FileTypeName*/> r = ValueTuple.Create<string, string>();

            List<chainFile> result = new List<chainFile>();

            Console.WriteLine("downloadFiles");
            //Dictionary<string /*Store*/, Dictionary<string /*FileTypeName*/, Dictionary<DateTime /*updated*/, string /*link*/>>> result = new Dictionary<string, Dictionary<string, Dictionary<DateTime, string>>>();
            //Dictionary<string /*Store*/, Dictionary<string /*FileTypeName*/, DateTime /*updated*/>> result = new Dictionary<string, Dictionary<string, DateTime>>();

            var dirName = path;
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            string lastChain = "";
            var files = browser.Document.GetElementById(rootFiles);

            int pages = 1;
            HtmlElementCollection elem = null;
            if (rootFiles == "fileList")
            {
                elem = files.Children;
            }
            else if (rootFiles == "files" || rootFiles == "gridContainer" || rootFiles == "download_content")
            {
                foreach (HtmlElement body in files.Children)
                {
                    if (body.TagName == "TABLE")
                    {
                        elem = body.Children;

                        foreach (HtmlElement tf in elem)
                        {
                            if (tf.TagName == "TFOOT")
                            {
                                foreach (HtmlElement tr in tf.Children)
                                {
                                    if (tr.TagName == "TR")
                                    {
                                        foreach (HtmlElement td in tr.Children)
                                        {
                                            if (td.TagName == "TD")
                                            {
                                                int lastPage = 0;
                                                foreach (HtmlElement a in td.Children)
                                                {
                                                    if (a.TagName == "A")
                                                    {
                                                        var href = a.GetAttribute("href");
                                                        if (href?.Trim().Length > 0)
                                                        {
                                                            string[] split = href.Split('=');
                                                            int page = 0;
                                                            int.TryParse(split[split.Length - 1], out page);
                                                            if (page > lastPage)
                                                            {
                                                                lastPage = page;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (lastPage > 0)
                                                {
                                                    pages = lastPage;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (body.TagName == "TABLE")
                    {

                    }
                    else
                    {
                        continue;
                    }
                }
            }



            foreach (HtmlElement table in elem)
            {
                if (table.TagName == "TBODY")
                {
                    foreach (HtmlElement tr in table.Children)
                    {
                        if (tr.TagName == "TR")
                        {
                            //if (counter == 20) break;
                            foreach (HtmlElement td in tr.Children)
                            {
                                if (td.TagName == "TD")
                                {
                                    foreach (HtmlElement a in td.Children)
                                    {
                                        if (a.TagName == "A")
                                        {
                                            var href = a.GetAttribute("href");
                                            if (href?.Trim().Length > 0)
                                            {
                                                Console.WriteLine(href);
                                                try
                                                {
                                                    string fileName = Path.GetFileName(href);
                                                    if (fileName.Trim().Length > 0)
                                                    {
                                                        fileName = fileName.Split('.')[0];//remove ext
                                                        if (fileName.ToLower().Contains("stores"))
                                                        {

                                                        }

                                                        string type = getFileTypeName(fileName);
                                                        if (type.ToLower().StartsWith("null", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            continue;
                                                        }
                                                        DateTime updated = getFileUpdated(fileName);
                                                        string store = getFileStoreCode(fileName);
                                                        string chain = getFileChainCode(fileName);
                                                        int lastChainLen = 0;
                                                        int chainLen = 0;
                                                        try
                                                        {
                                                            lastChainLen = lastChain.Trim().Trim('0').Trim().Length;
                                                            chainLen = chain.Trim().Trim('0').Trim().Length;
                                                        }
                                                        catch { }
                                                        if (lastChainLen == 0 && selectedChain.code.Length == 0)// without 'matrix'
                                                        {
                                                            lastChain = chain;
                                                        }
                                                        else if (chainLen > 0)
                                                        {
                                                            lastChain = chain;
                                                        }

                                                        var cf = new chainFile()
                                                        {
                                                            chain = lastChain,
                                                            store = store,
                                                            type = type,
                                                            updated = updated,
                                                            link = href
                                                        };

                                                        var found = result.FirstOrDefault(
                                                            itm => itm.chain == lastChain && itm.store == store && itm.type == type);
                                                        if (found == null)
                                                        {
                                                            result.Add(cf);
                                                        }
                                                        else if (updated > found.updated)
                                                        {
                                                            found.updated = updated;
                                                            found.link = href;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex) { Console.WriteLine("{0}.erro: {1}", href, ex.ToString()); }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //nullPrice
            //nullPromo
            /*
                store - price       - updated
                store - priceFull   - updated
                store - promo       - updated
                store - promoFull   - updated
                store - stores      - updated
            */

            //result = result.OrderBy(itm => itm.Key).ToDictionary(itm => itm.Key, itm => itm.Value);
            //foreach (var store in result.Keys)
            //{
            //    result[store] = result[store].OrderBy(itm => itm.Key).ToDictionary(itm => itm.Key, itm => itm.Value);
            //    foreach (var updated in result[store].Keys)
            //    {
            //        result[store][updated] = result[store][updated].OrderBy(itm => itm.Key).ToDictionary(itm => itm.Key, itm => itm.Value);
            //    }
            //}

            Console.WriteLine("Found: {0} files", result.Count);
            return result;
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Console.WriteLine("browser_Navigated: ( url: {0} )", e.Url);
            Application.DoEvents();
            Application.DoEvents();
        }

        async Task clean(string path)
        {
            Console.WriteLine("clean");
            if (Directory.Exists(path))
            {
                List<string> files = new List<string>(Directory.GetFiles(path, "*.gz", SearchOption.TopDirectoryOnly));
                foreach (var f in files)
                {
                    try { File.Delete(f); }
                    catch (Exception ex) { Console.WriteLine("error delete file {0}: {1}", f, ex.ToString()); }
                }
            }
        }

        private void Chains_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Chains_SelectedIndexChanged: {0}", chains.SelectedIndex);
            //currentChain = "";
            if (chains.SelectedIndex >= 0)
            {
                //var item = chains.Items[chains.SelectedIndex] as ListViewItem;

                //if (signedin)
                //{
                //    signout();
                //}
                //else
                //{
                //    selectedChain.username = item.Tag?.ToString() ?? "";
                //    if (selectedChain.username.Length > 0)
                //    {
                //        var elem = browser.Document.GetElementById("Username");
                //        elem?.SetAttribute("value", selectedChain.username);

                //        signin();
                //    }
                //}
            }
        }

        private void Main_Click(object sender, HtmlElementEventArgs e)
        {
            Console.WriteLine("login-button clicked!");
            Console.WriteLine("sender: {0}", ((HtmlElement)sender).Name);
            Console.WriteLine("e: {0}", ((HtmlElement)e.FromElement).Name);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("File downloaded");
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("ValidateServerCertificate");
            return true;
        }

        public static void LeftClick(int x, int y)
        {
            Console.WriteLine("LeftClick");

            Cursor.Position = new System.Drawing.Point(x, y);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine(1);
            this.Text = MousePosition.ToString();
        }

        private void insert_Click(object sender, EventArgs e)
        {
            try { insert_1(); }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

    }
}
