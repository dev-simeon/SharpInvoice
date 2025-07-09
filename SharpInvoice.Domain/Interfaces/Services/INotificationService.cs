using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpInvoice.Core.Interfaces.Services
{
    public interface INotificationService
    {
        string GenerateEmailConfirmationToken();
        Task SendEmailConfirmationAsync(string email, string token);
    }

}
