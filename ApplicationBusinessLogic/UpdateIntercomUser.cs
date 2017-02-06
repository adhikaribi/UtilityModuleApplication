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

namespace ApplicationBusinessLogic
{
    public class UpdateIntercomUser : IModule
    {
        private static string _moduleName = "UpdateIntercomUser";
        private static string _description = "UpdateIntercomUser does all the updates";
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
        // TODO : Write logic for Run method here.
        public void Run()
        {
            Console.WriteLine("UpdateIntercomUser Started....");

            DataTable table = new DataTable();

            //String sConnect1 = "Datasource=ec2-107-21-222-226.compute-1.amazonaws.com;User Id=root;password=laser;Persist Security Info=True;charset=utf8";
            //String sConnect1 = "Datasource=ec2-174-129-106-240.compute-1.amazonaws.com;User Id=activtrak;password=laser123;Persist Security Info=True;charset=utf8";
            String sConnect1 = ConfigurationManager.ConnectionStrings["AccountsServer"].ConnectionString;
            //args = new[] {"0"};

            MySqlConnection con = new MySqlConnection(sConnect1);

            String strCmd = strCmd = String.Format(@"
use 1accountsdb; 
select creator, acount, gsize, zaccountinfo.acctid, 0 as uflag, licensecount from zaccountinfo
join accounts on zaccountinfo.acctid=accounts.acctid
where zaccountinfo.acctid='{0}'
order by zaccountinfo.acctid desc", _accountId);

            //if (args.Count() != 1 /*&& args.Count() != 2*/)
            //{
            //    Console.WriteLine("UpdateIntercomUser startacctid");
            //    return;
            //}

            //int limit = -1;
            //if (args.Count() == 2)
            //    limit = Convert.ToInt32(args[1]);

            try
            {
                MySqlCommand cmd = new MySqlCommand(strCmd, con);
                con.Open();
                MySqlDataReader myReader = cmd.ExecuteReader();
                table.Load(myReader);
                myReader.Close();
                con.Close();
                table.PrimaryKey = new DataColumn[] {table.Columns[0]};

                Console.WriteLine("Number of accounts = {0}", table.Rows.Count.ToString());

                //Console.WriteLine("Press Enter to continue.");
                //String line = Console.ReadLine();
                //if ( line == null )
                //    throw(new Exception("break"));

                //bool stop = false;
                //int count = 0;

                //API is immature, but may be useful in future.
                // var userResults = new List<JToken>();

                var scrollParam = "";
                int retrievedUsers = 0;

                while (true)
                {
                    var scrollurl = "https://api.intercom.io/users/scroll";

                    HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(scrollurl + scrollParam);
                    request.AllowWriteStreamBuffering = true;
                    request.Method = "Get";
                    request.Accept = "application/json";

                    string authInfo =
                        "d645d06179373abcfd63e2adf58402e05e558127:396f0e4d375ee5a4866aafe2dcbefa46605a739e";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    request.Headers.Add("Authorization", "Basic " + authInfo);
                    //request.Credentials = new NetworkCredential("d645d06179373abcfd63e2adf58402e05e558127", "396f0e4d375ee5a4866aafe2dcbefa46605a739e");

                    //Send Web-Request and receive a Web-Response
                    HttpWebResponse httpWebesponse = (HttpWebResponse) request.GetResponse();

                    //Translate data from the Web-Response to a string
                    Stream userStream = httpWebesponse.GetResponseStream();
                    StreamReader streamreader = new StreamReader(userStream, Encoding.UTF8);
                    string response = streamreader.ReadToEnd();
                    streamreader.Close();


                    JObject jsonresponse = (JObject) JsonConvert.DeserializeObject(response);
                    var users = jsonresponse["users"];
                    //userResults.Add(users);

                    foreach (var user in users)
                    {
                        DataRow row = table.Rows.Find(user["email"].ToString());
                        if (row == null)
                            Console.WriteLine(String.Format("No zaccountinfo for {0} {1}", user["name"].ToString(),
                                user["email"].ToString()));
                        else
                        {
                            row[4] = 1;
                            //Console.WriteLine(String.Format("Yes zaccountinfo for {0} {1}", user["name"].ToString(), user["email"].ToString()));
                        }
                    }


                    JToken scrollToken;
                    jsonresponse.TryGetValue("scroll_param", out scrollToken);
                    scrollParam = $"?scroll_param={scrollToken.ToString()}";

                    var count = users.Count();


                    retrievedUsers += count;
                    Console.WriteLine(
                        $"{users.Count()} ({retrievedUsers}) users retrieved, next scrolltoken: {scrollParam}");
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

                Console.WriteLine("Finished Processing User Results");

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


                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if ((long) table.Rows[i][4] == 0)
                        continue;

                    Console.WriteLine("{0}", table.Rows[i].Field<UInt32>(3));

                    String creator = table.Rows[i].Field<String>(0);
                    UInt32 acount = table.Rows[i].Field<Object>(1) == null ? 0 : table.Rows[i].Field<UInt32>(1);
                    ulong gsize = table.Rows[i].Field<Object>(2) == null ? 0 : table.Rows[i].Field<UInt64>(2);
                    // Get the licenseCount information from the database
                    var licenseCount = table.Rows[i].Field<Object>(5) == null ? 0 : table.Rows[i].Field<UInt32>(5);

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

                    JObject cu = new JObject();
                    cu.Add("acount", acount);
                    cu.Add("gsize", gsize);
                    cu.Add("licenseoverlimit", CheckLicenseOvercount(licenseCount, gsize));

                    JObject user = new JObject();
                    user.Add("email", creator);
                    user.Add("custom_data", cu);

                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] data = encoding.GetBytes(user.ToString());

                    retry:
                    HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create("https://api.intercom.io/users");
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

                    Stream newStream = request.GetRequestStream();
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();


                    //Send Web-Request and receive a Web-Response
                    try
                    {
                        HttpWebResponse httpWebResponse = (HttpWebResponse) request.GetResponse();

                        //Translate data from the Web-Response to a string
                        Stream outStream = httpWebResponse.GetResponseStream();
                        StreamReader streamreader = new StreamReader(outStream, Encoding.UTF8);
                        string response = streamreader.ReadToEnd();
                        streamreader.Close();
                    }
                    catch (WebException werr)
                    {
                        if (((HttpWebResponse) werr.Response).StatusCode.ToString() == "429")
                        {
                            long unixreset = Convert.ToInt64(werr.Response.Headers["X-RateLimit-Reset"]);
                            long waitsecs = unixreset - UnixTimeNow() + 10;
                            //Some dependency here on server clock accuracy
                            int waitms = (int) (waitsecs * 1000);
                            Console.WriteLine("Waiting {0}s for request rate reset", waitsecs);
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

            Console.WriteLine("UpdateIntercomUser Ended");
        }

        public string Description => _description;

        private bool CheckLicenseOvercount(UInt32 licenseCount, ulong size)
        {
            bool result = licenseCount <= 3 && size > 3 * 1024 * 1024;
            return result;
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
