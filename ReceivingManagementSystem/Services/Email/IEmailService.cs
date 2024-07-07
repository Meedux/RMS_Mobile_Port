using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string emailTo, string emailCC, string subject, string content);
    }
}
