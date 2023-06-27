using Microsoft.AspNetCore.Mvc;
using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace DressStore.Areas.Identity.Pages.Account
{
    public class LoginModelOTP 
    {

        public LoginModelOTP()
        {
        }

        public InputModelOTP Input { get; set; }


        public class InputModelOTP
        {
            public string PhoneNumber { get; set; }

            
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            

            
        }


    }
}
