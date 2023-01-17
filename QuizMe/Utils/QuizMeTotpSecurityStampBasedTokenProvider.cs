using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using OtpNet;
using System;
using System.Globalization;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace QuizMe.Utils
{
    public class QuizMeTotpSecurityStampBasedTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
    {
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(false);
        }

        public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            var email = await manager.GetEmailAsync(user);
            return "CustomTotpTokenProvider:" + purpose + ":" + email;
        }
        public override async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var secretKey = await manager.CreateSecurityTokenAsync(user);
            var totp = new Totp(secretKey, mode: OtpHashMode.Sha512, step: 180, totpSize: 10);
            var totpCode = totp.ComputeTotp(DateTime.UtcNow);
            return totpCode;
        }
        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            int code;
            var usedTimeSteps = new HashSet<long>();
            if (!int.TryParse(token, out code))
            {
                return false;
            }
            var secretKey = await manager.CreateSecurityTokenAsync(user);
            var totp = new Totp(secretKey, mode: OtpHashMode.Sha512, step: 180, totpSize: 10);
            long timeWindowUsed;
            var check = totp.VerifyTotp(token, out timeWindowUsed);
            check &= !usedTimeSteps.Contains(timeWindowUsed);
            usedTimeSteps.Add(timeWindowUsed);
            return check;
        }
    }
}
