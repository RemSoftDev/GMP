using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;

namespace GuessMyPhoto.Models
{
    public class HttpHelper
    {
        private string apiUrl = App.ApiUrl;
        public async Task<string> Get(string requestUrl, List<string> prms)
        {
            string fullUrl = apiUrl + requestUrl;
            Uri uri = new Uri(LogicHelper.CreateUrl(fullUrl, prms));
            

            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://www.contoso.com");

            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();
                var buffer = await httpResponse.Content.ReadAsBufferAsync();
                var byteArray = buffer.ToArray();
                httpResponseBody = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                //httpResponseBody = await httpResponse.Content.ReadAsStringAsync(UnicodeEncoding.Utf8);
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            return httpResponseBody;
        }

        private void ConvertBytes(ref byte[] arr)
        {
            var fixDictionary = new Dictionary<int, int>() { { 134, 198 }, { 152, 216 }, { 133, 197 }, { 145, 209 }, { 150, 214 }, { 132, 196 } };
            for (var i = 0; i < arr.Length - 1; i++)
            {
                if (arr[i] == 195 && fixDictionary.ContainsKey((int)arr[i + 1]))
                {
                    int a;
                    fixDictionary.TryGetValue(arr[i + 1], out a);
                    arr[i + 1] = (byte)a;
                    var arList = arr.ToList();
                    arList.RemoveAt(i);
                    arr = arList.ToArray();
                }
            }
        }

        public async Task<string> PostImage(string requestUrl, Dictionary<string, string> values, WriteableBitmap image)
        {
            string boundary = "----WebKitFormBoundaryNHdtnVZQupwXyW75";
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            string fullUrl = apiUrl + requestUrl;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(fullUrl);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = await wr.GetRequestStreamAsync();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (var key in values)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key.Key, key.Value);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                ConvertBytes(ref formitembytes);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "File", "upload_new.png", "image/png");
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            //Stream fileStream = image.PixelBuffer.AsStream();
            //byte[] buffer = new byte[4096];
            //int bytesRead = 0;
            //while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            //{
            //    rs.Write(buffer, 0, bytesRead);
            //}
            //fileStream.Close();
            //rs.Write(imageBytes, 0, imageBytes.Length);
            //using (Stream stream = image.PixelBuffer.AsStream())
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    stream.CopyTo(memoryStream);
            //    byte[] bytes = memoryStream.ToArray();
            //    rs.Write(bytes, 0, bytes.Length);
            //}

            StorageFile storageFile = await WriteableBitmapToStorageFile(image);

            var randomAccessStream = await storageFile.OpenReadAsync();
            Stream fileStream = randomAccessStream.AsStreamForRead();

            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            //fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            string response = "";

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                response = reader2.ReadToEnd();
                //log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                //log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    //wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
                await storageFile.DeleteAsync();
            }

            return response;
        }

        private async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB)
        {
            string FileName = "MyFile.";
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            
            FileName += "png";
            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder
                    .CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                          (uint)WB.PixelWidth,
                          (uint)WB.PixelHeight,
                          96.0,
                          96.0,
                          pixels);
                await encoder.FlushAsync();
            }

            return file;
        }
    }
}
