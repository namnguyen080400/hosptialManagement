using Microsoft.AspNetCore.SignalR;

namespace HospitalServerHub
{
    public class SupplierContact : Hub
    {
        public async Task SendMessage(string supplierName, string supplierPhone, string itemName, int quantity)
        {
            // It takes the name of the method to be invoked on the client-side
            await Clients.All.SendAsync("ReceiveMessage", supplierName, supplierPhone, itemName, quantity);
        }
    }
}
