using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Application.EncryptionServiceInterface;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OnlineSchool.Presentation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncryptionService _encryptionService;

        public ChatHub(AppDbContext context, IHttpContextAccessor httpContextAccessor, IEncryptionService encryptionService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _encryptionService = encryptionService;
        }

        public void LoadMoreMessages(int subjectId, int skip)
        {
            Clients.Caller.SendAsync("GetMessages", GetMessages(skip, 5, subjectId));
        }

        public List<SubjectMessage> GetMessages(int skip, int amount, int subjectId)
        {
            var baseQuery = _context.SubjectMessages.Include(u => u.User).Where(x => x.SubjectId == subjectId).OrderByDescending(x => x.Created);
            var takenMessages = baseQuery.Skip(skip).Take(amount).ToList();
            foreach (var m in takenMessages)
                m.Text = _encryptionService.DecryptMessage(Convert.FromBase64String(m.Text));

            return takenMessages;
        }

        public async Task SendMessage(string message, int subjectId)
        {
            string userEmail = _httpContextAccessor.HttpContext.User.Identity.Name;
            var messageDto = new SubjectMessage
            {
                UserId = (await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail)).Id,
                User = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail),
                SubjectId = subjectId,
                Text = message,
                Created = DateTime.Now
            };

            await Clients.Groups(subjectId.ToString()).SendAsync("ReceiveMessage", messageDto);

            //encryption and convert to string
            messageDto.Text = Convert.ToBase64String(_encryptionService.EncryptMessage(message));

            await _context.SubjectMessages.AddAsync(messageDto);
            await _context.SaveChangesAsync();
        }

        public async Task OnConnectedAsync(int subjectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, subjectId.ToString());
        }
    }
}
