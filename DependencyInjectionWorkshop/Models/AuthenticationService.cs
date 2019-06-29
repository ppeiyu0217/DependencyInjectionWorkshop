using Dapper;
using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class ProFileDao
    {
        public string GetCurrentPasswordFromDb(string accountId)
        {
            string currentPassword;
            using (var connection = new SqlConnection("datasource=db,password=abc"))
            {
                currentPassword = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return currentPassword;
        }
    }

    public class AuthenticationService
    {
        private readonly ProFileDao _proFileDao = new ProFileDao();

        public bool Verify(string accountId, string password, string otp)
        {
            //檢查帳號是否被封鎖
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = IsAccountLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            //取得密碼
            var currentPassword = _proFileDao.GetCurrentPasswordFromDb(accountId);

            var hashPassword = GetHashPassword(password);

            //取得Otp
            var currentOtp = GetCurrentOtp(accountId, httpClient);

            // 驗證密碼、Otp
            if (hashPassword == currentPassword && otp == currentOtp)
            {
                ResetFailCount(accountId, httpClient);
                return true;
            }
            else
            {
                //累計失敗次數
                AddFailCount(accountId, httpClient);

                //紀錄失敗次數
                LogFailCount(accountId, httpClient);

                //推播
                PushMessage();

                return false;
            }
        }

        private static void PushMessage()
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(responseMessage => { }, "my channel", "my message", "my bot name");
        }

        private static void LogFailCount(string accountId, HttpClient httpClient)
        {
            var failedCount = GetFailedCount(accountId, httpClient);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static bool IsAccountLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void ResetFailCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            string currentOtp;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return currentOtp;
        }

        private static string GetHashPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}