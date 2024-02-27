using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Hubs
{
    public class ConnectedUser
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
    }

    public class PrivateHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static List<ConnectedUser> connectedUsers = new List<ConnectedUser>();

        public PrivateHub(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task OnConnectedAsync(string recieverId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == _httpContextAccessor.HttpContext.User.Identity.Name);
            connectedUsers.Add(new ConnectedUser { UserId = user.Id, ConnectionId = Context.ConnectionId });
            await _context.PrivateMessages.Where(x => x.RecieverId == user.Id && x.IsRead == false).ForEachAsync(x => x.IsRead = true);
            await _context.SaveChangesAsync();
        }

        public async Task SendMessage(string text, string recieverId)
        {
            var sender = await _context.Users.FirstOrDefaultAsync(x => x.Email == _httpContextAccessor.HttpContext.User.Identity.Name);
            var recipientUser = connectedUsers.FirstOrDefault(x => x.UserId == recieverId);

            var message = new Domain.Entities.PrivateMessage { Created = DateTime.Now, Text = text, SenderId = sender.Id, RecieverId = recieverId, Sender = (await _context.Users.FirstOrDefaultAsync(x => x.Id == sender.Id)), IsRead = false };

            if (recipientUser != null)
            {
                message.IsRead = true;

                List<ConnectedUser> openConnections = connectedUsers.Where(x => x.UserId == recieverId).ToList();
                foreach (var item in openConnections)
                {
                    await Clients.Client(item.ConnectionId).SendAsync("GetPrivateMesssage", message);
                }
            }
            await Clients.Caller.SendAsync("GetPrivateMesssage", message);

            await _context.PrivateMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public void LoadMoreMessages(string recieverId, int skip)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == _httpContextAccessor.HttpContext.User.Identity.Name);
            Clients.Caller.SendAsync("GetMessages", GetMessages(skip, 5, user.Id, recieverId));
        }

        public List<PrivateMessage> GetMessages(int skip, int amount, string senderId, string recieverId)
        {
            var baseQuery = _context.PrivateMessages.Include(u => u.Sender).Where(x => (x.RecieverId == recieverId && x.SenderId == senderId) || (x.RecieverId == senderId && x.SenderId == recieverId)).OrderByDescending(x => x.Created);
            return baseQuery.Skip(skip).Take(amount).ToList();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == _httpContextAccessor.HttpContext.User.Identity.Name);

            Console.WriteLine(connectedUsers.Remove(connectedUsers.FirstOrDefault(x => x.UserId == user.Id)).ToString());

            return base.OnDisconnectedAsync(exception);
        }
    }
}
