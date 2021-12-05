using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IAFinal
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<Photo> imageSources;
        private ObservableCollection<ModelOutput> predictions;
        private HttpClient client;
        public MainPage()
        {
            InitializeComponent();
            client = new HttpClient();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            GetFolder();

        }

        public async void GetFolder()
        {
            try
            {
                var results = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images
                });

                imageSources = new ObservableCollection<Photo>();

                foreach (var item in results)
                {
                    Photo photo = new Photo();
                    photo.file = item;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var stream = await item.OpenReadAsync();
                        photo.Image = ImageSource.FromStream(() => stream);
                        //streamBytes.CopyTo(ms);
					    //photo.data = ms.ToArray();
                    }

					imageSources.Add(photo);
                }

                this.CarouselView.ItemsSource = imageSources;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void SelectPhoto(object sender, EventArgs e)
        {
            try
            {
                Photo photo = this.CarouselView.CurrentItem as Photo;

                photo.IsSelected = !photo.IsSelected;

                if(photo.IsSelected)
                    this.chosed.BackgroundColor = Color.IndianRed;
                else this.chosed.BackgroundColor = Color.FromHex("#CF755A");

                using (MemoryStream ms = new MemoryStream())
                {
                    var stream = await photo.file.OpenReadAsync();
                    photo.Image = ImageSource.FromStream(() => stream);
                    //streamBytes.CopyTo(ms);
                    //photo.data = ms.ToArray();
                }

                this.CarouselView.ItemsSource = imageSources;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            try
            {
                Photo photo = this.CarouselView.CurrentItem as Photo;

                if (photo.IsSelected)
                    this.chosed.BackgroundColor = Color.IndianRed;
                else this.chosed.BackgroundColor = Color.FromHex("#CF755A");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void Choose_Clicked(object sender, EventArgs e)
        {
            try
            {
                this.predictions = new ObservableCollection<ModelOutput>();
                foreach (Photo image in imageSources)
                {
					if (image.IsSelected)
					{
                        Uri uri = new Uri("https://localhost:7122/Home/GetPredictions");

                        using (MemoryStream ms = new MemoryStream())
                        {
                            var stream = await image.file.OpenReadAsync();
							stream.CopyTo(ms);
							image.data = ms.ToArray();
						}

                        String jsonData = JsonConvert.SerializeObject(image.data);

                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(uri, content);
                        if (response.IsSuccessStatusCode)
                        {
                            string contento = await response.Content.ReadAsStringAsync();
                            var item = JsonConvert.DeserializeObject<ModelOutput>(contento);
                            item.Image = image.Image;

                            predictions.Add(item);
                        }
					}

                }
            }
            catch (Exception ex)
            {
                await this.DisplayAlert("Error", ex.Message, "Cerrar");
            }
            finally
            {
                this.lvPredictions.ItemsSource = predictions;
            }
        }

    }

    public class Photo
    {
        public ImageSource Image { get; set; }
        public bool IsSelected { get; set; }
        public byte[] data { get; set; }
        public FileResult file { get; set; }
        public Photo()
        {
            this.IsSelected = false;
        }

    }


    public class ModelOutput
    {
        public ImageSource Image { get; set; }

        public string Prediction { get; set; }

        public float[] Score { get; set; }
    }
}
