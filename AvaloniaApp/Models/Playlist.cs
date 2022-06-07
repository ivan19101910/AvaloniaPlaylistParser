using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApp.Models
{
    public class Playlist
    {
        public string AvatarImageUrl { get; set; }
        public string Title { get; set; }
        public List<Song> Songs { get; set; }
    }
}
