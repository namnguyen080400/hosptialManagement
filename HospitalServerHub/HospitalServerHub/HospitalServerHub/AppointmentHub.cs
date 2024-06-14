
using Microsoft.AspNetCore.SignalR;

namespace HospitalServerHub
{
    public class AppointmentHub : Hub
    {

        public async Task SendAppointmentUpdate(string userId,string message)
        {
            await Clients.User(userId).SendAsync("RecieveAppointmentUpdate", message);
        }
    }
}
