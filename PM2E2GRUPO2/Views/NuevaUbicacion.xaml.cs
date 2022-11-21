using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using PM2E2GRUPO2.Controllers;
using PM2E2GRUPO2.Models;
using SignaturePad.Forms;
using System.IO;
using Plugin.AudioRecorder;
using Nancy.Json;
using Plugin.Media;

namespace PM2E2GRUPO2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NuevaUbicacion : ContentPage
    {

        Plugin.Media.Abstractions.MediaFile FileFoto = null;
        byte[] FileFotoBytes = null;

        bool flag2 = false;

        private readonly AudioRecorderService audioRecorderService = new AudioRecorderService();

        SitioController sitiosApi;
        List<Sitio> ListaSitios;

        public NuevaUbicacion()
        {
            InitializeComponent();
            checkInternet();
            getLocation();

            sitiosApi = new SitioController();
            ListaSitios = new List<Sitio>();

            flag2 = false;
        }

        private async void grabarvoz_Clicked(object sender, EventArgs e)
        {
            
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            var status2 = await Permissions.RequestAsync<Permissions.StorageRead>();
            var status3 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted && status2 != PermissionStatus.Granted && status3 != PermissionStatus.Granted)
            {
                return; // si no tiene los permisos no avanza
            }

            onda1.IsVisible = true;
            onda2.IsVisible = true;
            ondaespacio.IsVisible = false;
            imgmicro.Source = "voice.png";
            btnsalvar.IsEnabled = false;
            grabarvoz.IsVisible = false;
            detenervoz.IsVisible = true;

            await audioRecorderService.StartRecording();

            flag2 = true;
        }

        private async void detenervoz_Clicked(object sender, EventArgs e)
        {
            onda1.IsVisible = false;
            onda2.IsVisible = false;
            ondaespacio.IsVisible = true;
            ondaespacio.Text = "¡Guardado Exitosamente!";
            imgmicro.Source = "voiceoff.png";
            btnsalvar.IsEnabled = true;
            grabarvoz.IsVisible = true;
            detenervoz.IsVisible = false;

            await audioRecorderService.StopRecording();

        }


        #region Location
        private void cleanLocation()
        {
            latitud.Text = null;
            longitud.Text = null;
        }

        public async void getLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    latitud.Text = "" + location.Latitude;
                    longitud.Text = "" + location.Longitude;
                    //await DisplayAlert("Aviso", "Si se leyó la ubicacion: "+location.Latitude +", "+location.Longitude, "OK");
                }
                else
                {
                    await DisplayAlert("Aviso", "El GPS está desactivado o no puede reconocerse", "OK");
                    cleanLocation();
                }

            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                await DisplayAlert("Aviso", "Este dispositivo no soporta la versión de GPS utilizada", "OK");
                cleanLocation();
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                //await DisplayAlert("Aviso", "Handle not enabled on device exception: "+fneEx, "OK");
                await DisplayAlert("Aviso", "La ubicación está desactivada", "OK");
                cleanLocation();

            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                await DisplayAlert("Aviso", "La aplicación no puede acceder a su ubicación.\n\n" +
                    "Habilite los permisos de ubicación en los ajustes del dispositivo", "OK");
                cleanLocation();
            }
            catch (Exception ex)
            {
                // Unable to get location
                await DisplayAlert("Aviso", "No se ha podido obtener la localización (error de gps)", "OK");
                cleanLocation();
            }
        }
        #endregion
        #region Internet
        public async void checkInternet()
        {
            //await DisplayAlert("Aviso", "si", "OK");
            var current = Connectivity.NetworkAccess;

            if (current != NetworkAccess.Internet)
            {
                // Connection to internet is available
                await DisplayAlert("Aviso", "Usted no tiene acceso a Internet.\nEl acceso a Internet es requerido para el buen funcionamiento de la aplicación.", "OK");
            }

        }
        #endregion

        private async void btnsalvar_Clicked(object sender, EventArgs e)
        {
            bool flag1 = false;
            if (latitud.Text == null || longitud.Text == null)
            {
                flag1 = true;
                await DisplayAlert("Operación Fallida", "Se necesitan las coordenadas de su ubicación para guardar.", "OK");
            }

            if(descripcion.Text == null || descripcion.Text == "")
            {
                flag1 = true;
                await DisplayAlert("Operación Fallida", "Se necesita una breve descripción de la ubicación.", "OK");
            }

            if (!flag1)
            {
                byte[] AudioBytes = null;

                try
                {

                    if (FileFotoBytes == null)
                    {
                        bool resp = await DisplayAlert("Aviso", "Tomarse una fotografía es requerido para poder aperturar su cuenta de usuario", "Tomar Foto", "OK");
                        if (resp) { tomarfoto(); }
                        return;
                    }

                }
                catch (Exception error)
                {
                    await DisplayAlert("Aviso", "No has escrito tu firma", "OK");
                    return;
                }

                //obtenemos el audio
                try
                {
                    var audio = audioRecorderService.GetAudioFileStream();

                    //Pasamos el audio a imagen a base 64
                    /*var mStream2 = (FileStream)audio;
                    MemoryStream a = new MemoryStream();
                    await mStream2.CopyToAsync(a);
                    byte[] data2 = a.ToArray();
                    string base64Val2 = Convert.ToBase64String(data2);
                    AudioBytes = Convert.FromBase64String(base64Val2);*/

                    AudioBytes = File.ReadAllBytes(audioRecorderService.GetAudioFilePath());
                }
                catch (Exception error)
                {
                    if (flag2)
                    {
                        await DisplayAlert("Aviso", "No has hablado fuerte al grabar tu nota de voz", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Aviso", "No has grabado una nota de voz", "OK");
                    }
                    
                    return;
                }

                try
                {
                    

                    Sitio sitio = new Sitio
                    {
                        Descripcion = descripcion.Text,
                        Latitud = latitud.Text,
                        Longitud = longitud.Text,
                        FirmaDigital = FileFotoBytes,
                        AudioFile = AudioBytes,
                        firma = "nulo"
                    };

                    await SitioController.CreateSite(sitio);
                    await DisplayAlert("Aviso", "Sitio adicionado con éxito", "OK");
                    descripcion.Text = null;

                }
                catch (Exception error)
                {
                    await DisplayAlert("Aviso", ""+error, "OK");
                }

                
            }
        }

        private async void btnubicaciones_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UbicacionesSalvadas());
        }

        private void cleandescripcion_Clicked(object sender, EventArgs e)
        {
            descripcion.Text = null;
        }


        private async void imgpersona_Tapped(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Obtener fotografía", "Cancelar", null, "Seleccionar de galería", "Tomar foto");

            if (action == "Seleccionar de galería") { seleccionarfoto(); }
            if (action == "Tomar foto") { tomarfoto(); }
        }

        private async void tomarfoto()
        {
            FileFoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Fotos_starbank",
                Name = "fotografia.jpg",
                SaveToAlbum = true,
                CompressionQuality = 10
            });
            // await DisplayAlert("Path directorio", FileFoto.Path, "OK");


            if (FileFoto != null)
            {
                imgpersona.Source = ImageSource.FromStream(() =>
                {
                    return FileFoto.GetStream();
                });

                //Pasamos la foto a imagen a byte[] almacenandola en FileFotoBytes
                using (System.IO.MemoryStream memory = new MemoryStream())
                {
                    Stream stream = FileFoto.GetStream();
                    stream.CopyTo(memory);
                    FileFotoBytes = memory.ToArray();
                    /*string base64Val = Convert.ToBase64String(FileFotoBytes);
                    FileFotoBytes = Convert.FromBase64String(base64Val);*/
                }
            }
        }

        private async void seleccionarfoto()
        {
            /*if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }*/

            FileFoto = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Custom,
                CustomPhotoSize = 10
            });


            if (FileFoto == null)
                return;

            imgpersona.Source = ImageSource.FromStream(() =>
            {
                return FileFoto.GetStream();
            });

            //Pasamos la foto a imagen a byte[] almacenandola en FileFotoBytes
            using (System.IO.MemoryStream memory = new MemoryStream())
            {
                Stream stream = FileFoto.GetStream();
                stream.CopyTo(memory);
                FileFotoBytes = memory.ToArray();
                /*string base64Val = Convert.ToBase64String(FileFotoBytes);
                FileFotoBytes = Convert.FromBase64String(base64Val);*/
            }

            /*Imagen.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });*/
        }
    }
}