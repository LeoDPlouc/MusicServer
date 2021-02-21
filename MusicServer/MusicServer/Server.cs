using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net;
using System.IO;
using System.Web;
using MusicServer.Beans;

namespace MusicServer
{
    class Server
    {
        static HttpListener listener;

        public static void Start()
        {
            if (listener != null) return;

            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8000/");
            listener.Start();

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                string url = context.Request.RawUrl;
                if (url.StartsWith("/song/"))
                {
                    if (context.Request.HttpMethod == "GET")
                        getSong(context);
                }
            }
        }
        private static async void postSong(HttpListenerContext context)
        {
            JsonDocument json = processBody(context.Request);
            Song song = await Song.Load(json.ToString());

            await song.Save();
        }

        private static void getSong(HttpListenerContext context)
        {
            string acousticId = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("acousticId");

            string songData = getJSONFile("SongData/" + acousticId);

            using (var stream = context.Response.OutputStream)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(songData);
                context.Response.ContentLength64 = buffer.Length;
                stream.Write(buffer, 0, buffer.Length);
            }
            context.Response.Close();

        }
        private static JsonDocument processBody(HttpListenerRequest req)
        {
            string json;
            using (var stream = new StreamReader(req.InputStream))
            {
                json = stream.ReadToEnd();
            }

            return JsonDocument.Parse(json);
        }

        private static string getJSONFile(string path)
        {
            using (var stream = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                return stream.ReadToEnd();
            }
        }
    }
}
