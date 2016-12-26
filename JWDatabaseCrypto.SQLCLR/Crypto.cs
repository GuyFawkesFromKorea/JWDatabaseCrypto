using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;

public partial class Crypto
{
    /// <summary>
    /// 암호화
    /// </summary>
    /// <param name="arg1">암호화 방식</param>
    /// <param name="arg2">문자열</param>
    /// <returns></returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString EnCryption(string arg1, string arg2, string arg3, string arg4)
    {
        if (string.IsNullOrEmpty(arg1)) throw new Exception("Secure type is null");
        //if (string.IsNullOrEmpty(KeyHandler.Instace.Key)) throw new Exception("encrypt key is null.");
        if (string.IsNullOrEmpty(arg2)) return null;
        if (string.IsNullOrEmpty(arg3)) throw new Exception("Identify parameter is null");
        if (string.IsNullOrEmpty(arg4)) throw new Exception("Identify parameter is null");

        var key = KeyHandler.Instace.GetKey(arg3, arg4);

        if (string.IsNullOrEmpty(key)) throw new Exception("encrypt key is null.");

        // 여기에 코드를 입력합니다.
        string encryptoString = string.Empty;

        if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.PSWD.ToString())
        {
            encryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineSHA256>.Encrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.PSWD2.ToString())
        {
            encryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineSHA512>.Encrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN.ToString())
        {
            encryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineAES128>.Encrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN2.ToString())
        {
            encryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineAES256>.Encrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN3.ToString())
        {
            encryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineMD5>.Encrypt(arg2, key);
        }

        return new SqlString(encryptoString);
    }

    /// <summary>
    /// 복호화, PSWD, PSWD2는 복화화 기능 없음.
    /// </summary>
    /// <param name="arg1">암호화 방식</param>
    /// <param name="arg2">암화화된 문자열</param>
    /// <returns></returns>
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString DeCryption(string arg1, string arg2, string arg3, string arg4)
    {
        if (string.IsNullOrEmpty(arg1)) throw new Exception("Secure type is null");
        //if (string.IsNullOrEmpty(KeyHandler.Instace.Key)) throw new Exception("encrypt key is null.");
        if (string.IsNullOrEmpty(arg2)) return null;
        if (string.IsNullOrEmpty(arg3)) throw new Exception("Identify parameter is null");
        if (string.IsNullOrEmpty(arg4)) throw new Exception("Identify parameter is null");

        var key = KeyHandler.Instace.GetKey(arg3, arg4);

        if (string.IsNullOrEmpty(key)) throw new Exception("encrypt key is null.");

        // 여기에 코드를 입력합니다.
        string decryptoString = string.Empty;

        if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.PSWD.ToString())
        {
            decryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineSHA256>.Decrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.PSWD2.ToString())
        {
            decryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineSHA512>.Decrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN.ToString())
        {
            decryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineAES128>.Decrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN2.ToString())
        {
            decryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineAES256>.Decrypt(arg2, key);
        }
        else if (arg1.ToUpper() == GSCoreLibrary.Crypto.SecurityType.SSN3.ToString())
        {
            decryptoString = StringCryptoFactory<GSCoreLibrary.Crypto.CryptorEngineMD5>.Decrypt(arg2, key);
        }

        return new SqlString(decryptoString);
    }

    //[Microsoft.SqlServer.Server.SqlFunction]
    //public static SqlString GetKey(string arg1, string arg2) {
    //	// 여기에 코드를 입력합니다.		
    //	return new SqlString(KeyHandler.Instace.GetKey(arg1, arg2));
    //}

    class KeyHandler
    {
        private static KeyHandler _instance;
        public static KeyHandler Instace
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KeyHandler();
                }

                return _instance;
            }
        }
        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
        }

        private string _id = "2"; //important!!!
        private string _uri = "[ip:port]";
        private string _decryptKey = "stonecircle--skmns.co.kr";
        private string _userid;
        private string _password;

        public KeyHandler()
        {
            //_key = GetKey();
        }

        public string GetKey(string userid, string password)
        {
            bool isRefresh = false;

            if (_userid != userid || _password != password)
            {
                isRefresh = true;
            }

            _userid = userid;
            _password = password;

            if (isRefresh)
            {
                GSCoreLibrary.HttpWebClient.HttpWebClientHandler handler =
                    new GSCoreLibrary.HttpWebClient.HttpWebClientHandler();

                var tokenString = handler.GetToken("[ip:port/serviceUrl]", userid, password, "USER_ID", "USER_PASSWORD");

                if (tokenString != null)
                {
                    var status = JsonConvert.DeserializeObject<Status>(tokenString);

                    if (status.Successeded)
                    {
                        var result = handler.GetResponse("[ip:port/serviceUrl]", "GET", status.Token);

                        if (result != null)
                        {
                            string key = (string)JsonConvert.DeserializeObject(result);
                            return _key = key;
                        }
                    }
                }
            }

            return _key;
        }

        private string GetKey()
        {
            HttpWebRequest wReq;
            HttpWebResponse wRes;

            string uri = _uri + "/api/KeyStore/" + _id;
            string cookie = string.Empty;

            wReq = (HttpWebRequest)WebRequest.Create(uri); // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
            wReq.Method = "GET"; // 전송 방법 "GET" or "POST"
            wReq.ServicePoint.Expect100Continue = false;
            //wReq.CookieContainer = new CookieContainer();
            //wReq.CookieContainer.SetCookies(new Uri(uri), cookie); // 넘겨줄 쿠키가 있을때 CookiContainer 에 저장

            string resResult = string.Empty;

            using (wRes = (HttpWebResponse)wReq.GetResponse())
            {
                Stream respPostStream = wRes.GetResponseStream();
                StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);

                resResult = readerPost.ReadToEnd();
            }

            var keyStore = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyStore>(resResult);

            if (keyStore == null) throw new Exception("Not found dbKey1.");

            return keyStore.KEY_VALUE;
        }

        public class KeyStore
        {
            public int ID { get; set; }
            public string KEY_VALUE { get; set; }
            public string KEY_SETUP { get; set; }
            public string AUDIT_ID { get; set; }
            public DateTime AUDIT_DTM { get; set; }
        }

        public class Status
        {
            public bool Successeded { get; set; }
            public string Token { get; set; }
            public string Message { get; set; }
        }
    }
}
