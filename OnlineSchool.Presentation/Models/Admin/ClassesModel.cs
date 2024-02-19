
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using OnlineSchool.Domain.Entities;

namespace OnlineSchool.Presentation.Models.Admin
{
    public class ClassesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AppUser Teacher { get; set; }
    }
}
