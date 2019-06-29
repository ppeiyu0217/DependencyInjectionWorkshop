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
    public class ProfileDao
    {
        public string GetPassword(string accountId)
        {
            string currentPassword;
            using (var connection = new SqlConnection("datasource=db,password=abc"))
            {
                currentPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return currentPassword;
        }
    }

    public class Sha256Adapter
    {
        public string Hash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }
    }

    public class NLogAdapter
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class FailedCounter
    {
        public void AddFailedCount(string accountId)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void ResetFailedCount(string accountId)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }
    }

    public class SlackAdaper
    {
        public void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(responseMessage => { }, "my channel", $"my message : {accountId}", "my bot name");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackAdaper _slackAdaper = new SlackAdaper();

        public bool Verify(string accountId, string password, string otp)
        {
            //檢查帳號是否被封鎖
            var isLocked = IsAccountLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            //取得密碼
            var currentPassword = _profileDao.GetPassword(accountId);

            var hashPassword = _sha256Adapter.Hash(password);

            //取得Otp
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            // 驗證密碼、Otp
            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                //累計失敗次數
                _failedCounter.AddFailedCount(accountId);

                //紀錄失敗次數
                var failedCount = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");

                //推播
                _slackAdaper.PushMessage(accountId);

                return false;
            }
        }

        private static bool IsAccountLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }

    public class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
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
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}