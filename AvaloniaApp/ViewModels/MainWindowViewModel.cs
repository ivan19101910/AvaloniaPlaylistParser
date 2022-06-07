using AvaloniaApp.Models;
using HtmlAgilityPack;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace AvaloniaApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<Song> _songs;

        private string imageUrl;
        private string _playlistName;
        private string _parseUrl;
        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                this.RaiseAndSetIfChanged(ref _errorMessage, value);
            }
        }

        public string ImageUrl
        {
            get => imageUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref imageUrl, value);
                DownloadImage(ImageUrl);
            }
        }

        private Avalonia.Media.Imaging.Bitmap _img = null;
        public Avalonia.Media.Imaging.Bitmap Img
        {
            get => _img;
            set => this.RaiseAndSetIfChanged(ref _img, value);
        }

        public string PlaylistName {
            get
            {
                return _playlistName;
            }
            set
            {
                _playlistName = value;
                this.RaisePropertyChanged();
            }
        }
        public string ParseUrl {
            get
            {
                return _parseUrl;
            }
            set
            {
                _parseUrl = value;
                this.RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<Song> Songs { get { return _songs; } 
            set 
            {
                _songs = value;
                this.RaisePropertyChanged(); 
            } 
        }
        public ICommand ReceivePlaylistsCommand { get; private set; }
        public MainWindowViewModel()
        {

            ReceivePlaylistsCommand = ReactiveCommand.Create( () => 
            {
                var imgFilter = "//img[@class='o-pwa-image__img c-pwa-image-viewer__img js-pwa-faceout-image']";
                var titleFilter = "//h1[@class='c-pwa-product-meta-heading']";
                var detailsFilter = "//div[@class='o-pwa-accordion c-pwa-product-details__details is-expanded--medium is-expanded--large']";
                var headingFilter = "//h1[@class='c-pwa-product-meta-heading']";

                HtmlWeb web = new HtmlWeb();
                web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36";
                web.OverrideEncoding = Encoding.UTF8;
                var document = web.Load(ParseUrl);
                
                var img = document.DocumentNode.SelectNodes(imgFilter);
                var h1title = document.DocumentNode.SelectNodes(titleFilter);

                string imgSrc = img[0].Attributes["src"].Value;
                
                string title = h1title[0].InnerText;
                
                var details = document.DocumentNode.SelectSingleNode(detailsFilter).SelectNodes("//p");
                
                HtmlNode pnode = null;
                foreach (HtmlNode el in details)
                {
                    if (el.InnerText.Contains("Tracklisting"))
                    {
                        pnode = el;
                    }
                }
                if (pnode != null)
                {
                    var heading = document.DocumentNode.SelectSingleNode(headingFilter);
                    var splitted = heading.InnerText.Split('-');
                    string artist = splitted[0].Trim();
                    string album = splitted[1].Trim();

                    var rawSongsString = pnode.InnerHtml;
                    List<string> rawSongs = rawSongsString.Split("<br>").ToList();
                    for (int i = 0; i < rawSongs.Count; ++i)
                    {
                        if (!rawSongs[i].Any(char.IsDigit))
                        {
                            rawSongs.RemoveAt(i);
                        }
                        if (rawSongs[i].Contains("Side"))
                        {
                            rawSongs.RemoveAt(i);
                        }
                        rawSongs[i] = Regex.Replace(rawSongs[i], "[0-9].", "", RegexOptions.IgnoreCase).Replace(".", "").Trim();
                    }
                    List<Song> songs = new List<Song>();
                    foreach(string s in rawSongs)
                    {
                        songs.Add(new Song { Name = s, AlbumName = album, ArtistName = artist });
                    }
                    Playlist playlist = new Playlist { Title = album, Songs = songs, AvatarImageUrl = imgSrc };
                    Songs = new ObservableCollection<Song>(songs);
                    PlaylistName = album;
                    ImageUrl = imgSrc;
                }
                else
                {
                    ErrorMessage = "Error: No tracks found";
                }
                
                
            });
        }

        public void DownloadImage(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadDataAsync(new Uri(url));
                client.DownloadDataCompleted += DownloadComplete;
            }
        }
        private void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                byte[] bytes = e.Result;

                Stream stream = new MemoryStream(bytes);

                var image = new Avalonia.Media.Imaging.Bitmap(stream);
                Img = image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Img = null; // Could not download...
            }

        }

    }
}
