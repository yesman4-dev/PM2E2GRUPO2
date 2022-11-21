using Nancy.Json;
using Plugin.Media;
using PM2E2GRUPO2.Controllers;
using PM2E2GRUPO2.Models;
using SignaturePad.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PM2E2GRUPO2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModificarUbicacion : ContentPage
    {
        Sitio _sitio;
        Plugin.Media.Abstractions.MediaFile FileFoto = null;
        byte[] FileFotoBytes = null;
        public ModificarUbicacion(Sitio sitio)
        {
            InitializeComponent();

            /*try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var strokesSignature = serializer.DeserializeObject(sitio.firma);
                var a = strokesSignature.GetType();
                PadView.Strokes = (IEnumerable<IEnumerable<Point>>)strokesSignature;
            }
            catch (Exception error)
            {
                DisplayAlert("Aviso", "" + error, "OK");
            }*/
            _sitio = sitio;
            latitud.Text = sitio.Latitud;
            longitud.Text = sitio.Longitud;
            descripcion.Text = sitio.Descripcion;
            imgpersona.Source = ImageSource.FromStream(() => new System.IO.MemoryStream(sitio.FirmaDigital));
        }

        private void cleandescripcion_Clicked(object sender, EventArgs e)
        {
            descripcion.Text = null;
        }

        private async void btnactualizar_Clicked(object sender, EventArgs e)
        {
            bool flag1 = false;

            if (descripcion.Text == null || descripcion.Text == "")
            {
                flag1 = true;
                await DisplayAlert("Operación Fallida", "Se necesita una breve descripción de la ubicación.", "OK");
            }

            if (!flag1)
            {
                
                try
                {

                    Sitio sitio = new Sitio
                    {
                        Id = _sitio.Id,
                        Descripcion = descripcion.Text,
                        Latitud = latitud.Text,
                        Longitud = longitud.Text,
                        FirmaDigital = FileFotoBytes,
                        firma = "nulo"
                    };

                    await SitioController.UpdateSitio(sitio);
                    await DisplayAlert("Aviso", "Sitio modificado con éxito", "Aceptar");
                    await Navigation.PopAsync();
                }
                catch (Exception error)
                {
                    await DisplayAlert("Aviso", "" + error, "OK");
                }


            }
        }

        private async void btneliminar_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Aviso", "¿Seguro que desea eliminar?", "Confirmar", "Volver");
            if (answer)
            {
                await SitioController.DeleteSite("" + _sitio.Id);
                await Navigation.PopAsync();
            }
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