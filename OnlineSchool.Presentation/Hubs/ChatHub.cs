
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LoadMoreMessages( int subjectId, int skip)
        {
             Clients.Caller.SendAsync("GetMessages", GetMessages(skip,5,subjectId));
        }

        public List<SubjectMessage> GetMessages(int skip, int amount, int subjectId)
        {
            var baseQuery = _context.SubjectMessages.Include(u => u.User).Where(x => x.SubjectId == subjectId).OrderByDescending(x => x.Created);
            return baseQuery.Skip(skip).Take(amount).ToList();
        }

        public async Task SendMessage(string message, int subjectId)
        {
            string userEmail = _httpContextAccessor.HttpContext.User.Identity.Name;
            var messageDto = new SubjectMessage
            {
                UserId =( await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail)).Id,
                User = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail),
                SubjectId = subjectId,
                Text = message,
                Created = DateTime.Now
            };

            await Clients.Groups(subjectId.ToString()).SendAsync("ReceiveMessage", messageDto);

            await _context.SubjectMessages.AddAsync(messageDto);
            await _context.SaveChangesAsync();
        }

        public async Task OnConnectedAsync(int subjectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, subjectId.ToString());
        }
    }
}
