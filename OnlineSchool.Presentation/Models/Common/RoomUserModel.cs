using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Common
{
    public class RoomUserModel
    {
        public AppUser User { get; set; }
        public string ConnectionId { get; set; }
    }
}
