using ApplicationCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationBusinessLogic
{
    public class UpdateIntercomUser : IModule
    {
        private static string _moduleName = "UpdateIntercomUser";
        private static string _description = "UpdateIntercomUser dumps all the user information and updates the required parameters like gsize, account and licenseoverlimit";
        // e.g. 436540 
        private readonly int _accountId;
        public string Name => _moduleName;
        public UpdateIntercomUser(object accountId)
        {
            _accountId = Int32.Parse(accountId.ToString());
        }

        public UpdateIntercomUser()
        {
            _accountId = 0;
        }

        public void Run()
        {
            Console.WriteLine("UpdateIntercomUser Started....");

            var table = new DataTable();
            //String sConnect1 = "Datasource=ec2-107-21-222-226.compute-1.amazonaws.com;User Id=root;password=laser;Persist Security Info=True;charset=utf8";
            //String sConnect1 = "Datasource=ec2-174-129-106-240.compute-1.amazonaws.com;User Id=activtrak;password=laser123;Persist Security Info=True;charset=utf8";
            var connectionString = ConfigurationManager.ConnectionStrings["AccountsServer"].ConnectionString;
            var commandText = $"USE 1accountsdb; " +
                                     $"SELECT creator, acount, gsize, zaccountinfo.acctid, 0 as uflag, licensecount " +
                                     $"FROM zaccountinfo JOIN accounts " +
                                     $"ON zaccountinfo.acctid=accounts.acctid " +
                                     $"WHERE zaccountinfo.acctid >= {_accountId} ORDER BY zaccountinfo.acctid DESC";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    var cmd = new MySqlCommand(commandText, connection);
                    connection.Open();
                    var myReader = cmd.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                }

                table.PrimaryKey = new[] {table.Columns[0]};
                Console.WriteLine($"Number of accounts = {table.Rows.Count}");
                //Console.WriteLine("Press Enter to continue.");
                //String line = Console.ReadLine();
                //if ( line == null )
                //    throw(new Exception("break"));

                //bool stop = false;
                //int count = 0;

                //API is immature, but may be useful in future.
                // var userResults = new List<JToken>();
                var scrollParam = "";
                var retrievedUsers = 0;
                const string scrollurl = "https://api.intercom.io/users/scroll";
                while (true)
                {
                    var request = (HttpWebRequest) WebRequest.Create(scrollurl + scrollParam);
                    request.AllowWriteStreamBuffering = true;
                    request.Method = "Get";
                    request.Accept = "application/json";

                    string authInfo =
                        "d645d06179373abcfd63e2adf58402e05e558127:396f0e4d375ee5a4866aafe2dcbefa46605a739e";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    request.Headers.Add("Authorization", "Basic " + authInfo);
                    //request.Credentials = new NetworkCredential("d645d06179373abcfd63e2adf58402e05e558127", "396f0e4d375ee5a4866aafe2dcbefa46605a739e");

                    string response;
                    // Send Web-Request and receive a Web-Response
                    var httpWebesponse = (HttpWebResponse) request.GetResponse();
                    // Translate data from the Web-Response to a string
                    using (var userStream = httpWebesponse.GetResponseStream())
                    {
                        using (var streamreader = new StreamReader(userStream, Encoding.UTF8))
                        {
                            response = streamreader.ReadToEnd();
                        }
                    }

                    var jsonresponse = (JObject) JsonConvert.DeserializeObject(response);
                    var users = jsonresponse["users"];
                    //userResults.Add(users);

                    foreach (var user in users)
                    {
                        var row = table.Rows.Find(user["email"].ToString());
                        if (row == null)
                            Console.WriteLine($"No zaccountinfo for {user["name"]} {user["email"]}");
                        else
                        {
                            row[4] = 1;
                            //Console.WriteLine(String.Format("Yes zaccountinfo for {0} {1}", user["name"].ToString(), user["email"].ToString()));
                        }
                    }

                    JToken scrollToken;
                    jsonresponse.TryGetValue("scroll_param", out scrollToken);
                    scrollParam = $"?scroll_param={scrollToken?.ToString()}";

                    var count = users.Count();
                    retrievedUsers += count;

                    Console.WriteLine(
                        $"{count} ({retrievedUsers}) users retrieved, next scrolltoken: {scrollParam}");
                    if (count == 0) break;
                }

                //Console.WriteLine("Processing User Results");
                //foreach (JObject user in userResults.SelectMany(ur => ur))
                //{
                //    DataRow row = table.Rows.Find(user["email"].ToString());
                //    if (row == null)
                //        Console.WriteLine(String.Format("No zaccountinfo for {0} {1}", user["name"].ToString(), user["email"].ToString()));
                //    else
                //    {
                //        row[4] = 1;
                //        Console.WriteLine(String.Format("Yes zaccountinfo for {0} {1}", user["name"].ToString(), user["email"].ToString()));
                //    }
                //}

                //int created_since = 0;
                //string stop_id = "start";
                //string next_stop_id = "reset";

                //while (stop_id != next_stop_id)
                //{
                //    created_since += 1;

                //    string template = "https://api.intercom.io/users?page=1&per_page=50&sort=created_at&order=asc&created_since={0}";
                //    string next = template;

                //    stop_id = next_stop_id;
                //    next_stop_id = "reset";

                //    bool bstopEncountered = false;


                //    while (next != "" && !bstopEncountered)
                //    {
                //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(next, created_since));
                //        request.AllowWriteStreamBuffering = true;
                //        request.Method = "Get";
                //        request.Accept = "application/json";

                //        string authInfo = "d645d06179373abcfd63e2adf58402e05e558127:396f0e4d375ee5a4866aafe2dcbefa46605a739e";
                //        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                //        request.Headers.Add("Authorization", "Basic " + authInfo);
                //        //request.Credentials = new NetworkCredential("d645d06179373abcfd63e2adf58402e05e558127", "396f0e4d375ee5a4866aafe2dcbefa46605a739e");

                //        //Send Web-Request and receive a Web-Response
                //        HttpWebResponse httpWebresponse = (HttpWebResponse)request.GetResponse();

                //        //Translate data from the Web-Response to a string
                //        Stream userStream = httpWebresponse.GetResponseStream();
                //        StreamReader streamreader = new StreamReader(userStream, Encoding.UTF8);
                //        string response = streamreader.ReadToEnd();
                //        streamreader.Close();


                //        JObject jsonresponse = (JObject)JsonConvert.DeserializeObject(response);

                //        if (next_stop_id == "reset")
                //            next_stop_id = jsonresponse["users"][0]["id"].ToString();

                //        foreach (JObject jo in jsonresponse["users"])
                //        {
                //            DataRow row = table.Rows.Find(jo["email"].ToString());
                //            if (row == null)
                //                Console.WriteLine(String.Format("No zaccountinfo for {0} {1}", jo["name"].ToString(), jo["email"].ToString()));
                //            else
                //                row[4] = 1;

                //            string id = jo["id"].ToString();

                //            if (id == stop_id)
                //            {
                //                bstopEncountered = true;
                //                break;
                //            }
                //        }
                //                                //if (jsonresponse["pages"]["page"])
                //        next = jsonresponse["pages"]["next"].ToString();



                //        Console.WriteLine(created_since);

                //        if ((long)jsonresponse["pages"]["page"] == 200)
                //        {
                //            Console.WriteLine("Aborting reading period due to 10,000 record limit being reached");
                //            bstopEncountered = true;
                //        }
                //    }
                //}

                Console.WriteLine("Finished Processing User Results");

                for (var i = 0; i < table.Rows.Count; i++)
                {
                    if ((long) table.Rows[i][4] == 0)
                        continue;

                    Console.WriteLine("{0}", table.Rows[i].Field<UInt32>(3));

                    var creator = table.Rows[i].Field<String>(0);
                    var acount = table.Rows[i].Field<Object>(1) == null ? 0 : table.Rows[i].Field<UInt32>(1);
                    var gsize = table.Rows[i].Field<Object>(2) == null ? 0 : table.Rows[i].Field<UInt64>(2);
                    // Get the licenseCount information from the database
                    // Since the licensecount column in db is nullable type, use signed integer instead of unsigned (cast exception arises if UInt32 is used)
                    var licenseCount = table.Rows[i].Field<Object>(5) == null ? 0 : table.Rows[i].Field<Int32>(5);

                    //POST https://api.intercom.io/v1/users
                    //EXAMPLE REQUEST
                    //curl -s "https://api.intercom.io/v1/users" \
                    //  -X POST \
                    //  -u "pi3243fa:da39a3ee5e6b4b0d3255bfef95601890afd80709" \
                    //  -H "Content-Type: application/json" \
                    //  --data '{
                    //            "email" : "ben@intercom.io",
                    //            "user_id" : "7902",
                    //            "name" : "Ben McRedmond",
                    //            "created_at" : 1257553080,
                    //            "custom_data" : {"plan" : "pro"},
                    //            "last_seen_ip" : "1.2.3.4",
                    //            "last_seen_user_agent" : "ie6",
                    //            "companies" : [
                    //              {
                    //                "id" : 6,
                    //                "name" : "Intercom",
                    //                "created_at" : 103201,
                    //                "plan" : "Messaging",
                    //                "monthly_spend" : 50
                    //              }
                    //            ],
                    //            "last_request_at" : 1300000000
                    //          }'

                    var currentUser = new JObject
                    {
                        {"acount", acount},
                        {"gsize", gsize},
                        {"overstorage", CheckOverStorage(licenseCount, gsize)},
                        {"overagentlimit",  CheckOverAgentLimit(licenseCount, acount)}
                    };

                    var user = new JObject {{"email", creator}, {"custom_data", currentUser}};

                    var encoding = new UTF8Encoding();
                    var data = encoding.GetBytes(user.ToString());

                    retry:
                    var request = (HttpWebRequest) WebRequest.Create("https://api.intercom.io/users");
                    request.AllowWriteStreamBuffering = true;
                    request.Method = "POST";
                    request.ContentLength = data.Length;
                    request.ContentType = "application/json";
                    request.Accept = "application/json";

                    string authInfo =
                        "d645d06179373abcfd63e2adf58402e05e558127:396f0e4d375ee5a4866aafe2dcbefa46605a739e";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    request.Headers.Add("Authorization", "Basic " + authInfo);
                    //request.Credentials = new NetworkCredential("d645d06179373abcfd63e2adf58402e05e558127", "396f0e4d375ee5a4866aafe2dcbefa46605a739e");

                    using (var newStream = request.GetRequestStream())
                        newStream.Write(data, 0, data.Length);

                    //Send Web-Request and receive a Web-Response
                    try
                    {
                        var httpWebResponse = (HttpWebResponse) request.GetResponse();

                        //Translate data from the Web-Response to a string
                        using (var outStream = httpWebResponse.GetResponseStream())
                        {
                            var streamreader = new StreamReader(outStream, Encoding.UTF8);
                            streamreader.ReadToEnd();
                        }
                    }
                    catch (WebException werr)
                    {
                        if (((HttpWebResponse) werr.Response).StatusCode.ToString() == "429")
                        {
                            var unixreset = Convert.ToInt64(werr.Response.Headers["X-RateLimit-Reset"]);
                            var waitsecs = unixreset - UnixTimeNow() + 10;
                            //Some dependency here on server clock accuracy
                            var waitms = (int) (waitsecs * 1000);
                            Console.WriteLine($"Waiting {waitsecs}s for request rate reset");
                            Thread.Sleep(waitms);
                            goto retry;
                        }
                        else
                            Console.WriteLine(werr.Message);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }
                }

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            Console.WriteLine("UpdateIntercomUser Completed");
        }

        public string Description => _description;

        // If the licenseCount is less than or equal to 3 and the size is greater than 3 gigs, its overstorage
        private static bool CheckOverStorage(Int32 licenseCount, ulong size)
        {
            var result = licenseCount <= 3 && size > 3 * 1024 * 1024;
            return result;
        }

        // If the account is greater than licenseCount (> 3), its overagentlimit
        private static bool CheckOverAgentLimit(Int32 licenseCount, UInt32 account)
        {
            var allowedLicenses = Math.Max(licenseCount, 3);
            return account > allowedLicenses;
        }

        private static long UnixTimestampFromDateTime(DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            return unixTimestamp;
        }

        private static long UnixTimeNow()
        {
            long unixTimestamp = DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            return unixTimestamp;
        }

    }
}
