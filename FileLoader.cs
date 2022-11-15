using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace Budget
{
    public class FileLoader
    {
        public delegate void LoadingHandler();
        public event LoadingHandler OnLoaded;

        // Состояние загрузчика
        public enum LoaderState
        {
            Started = 0,
            Running,
            Finished
        }

        private string _lastLoadedVersion;
        private string _loadingVersion;

        private Uri _uri;
        private string _filePath;

        private FileStream _fileStream;
        private int _nextRange;
        private int _size;

        private LoaderState _loaderState;

        public FileLoader(string loadingVersion, string filePath, string link)
        {
            _uri = new Uri(link);
            _filePath = filePath;
            _loadingVersion = loadingVersion;
            _lastLoadedVersion = Properties.Settings.Default.LastLoadedVersion;

            _fileStream = null;
            _nextRange = 0;
            _size = 10000;

            var loadingThread = new Thread(new ThreadStart(StartLoad));
            loadingThread.IsBackground = false;
            loadingThread.Start();
        }

        public void StartLoad()
        {
            bool start = false;

            if (String.IsNullOrEmpty(_lastLoadedVersion))
                start = true;
            else if (Utils.CompareVersions(_loadingVersion, _lastLoadedVersion) > 0)
                start = true;
            
            if (start)
            {
                _loaderState = LoaderState.Started;
                Properties.Settings.Default.LastVersionSetupLoaded = false;
                _fileStream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                GetNextChunk();
            }
        }

        public LoaderState State
        {
            get { return _loaderState; }
        }

        private void GetNextChunk()
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(_uri);

            request.Method = "GET";
            request.Timeout = 30 * 1000;  //таймаут - 30 секунд
            request.AddRange(_nextRange, _nextRange + _size);
            _nextRange += (_size + 1);
            request.BeginGetResponse(new AsyncCallback(webRequest_Callback), request);
            _loaderState = LoaderState.Running;
        }

        private void webRequest_Callback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                SaveChunk(response.GetResponseStream());
                response.Close();
                GetNextChunk();
            }
            catch (WebException wEx)
            {
                if (wEx.Message.Contains("416"))
                {  //файл докачался до конца
                    _fileStream.Close();
                    _loaderState = LoaderState.Finished;
                    Properties.Settings.Default.LastLoadedVersion = _loadingVersion;
                    Properties.Settings.Default.LastVersionSetupLoaded = true;
                    Properties.Settings.Default.Save();

                    OnLoaded();
                }
            }
            catch { }
        }

        private void SaveChunk(Stream incomingStream)
        {
            int READ_CHUNK = 1024 * 1024;
            int WRITE_CHUNK = 1000 * 1024;
            byte[] buffer = new byte[READ_CHUNK];
            Stream stream = incomingStream;
            while (true)
            {
                int read = stream.Read(buffer, 0, READ_CHUNK);
                if (read <= 0)
                    break;
                int to_write = read;

                while (to_write > 0)
                {
                    _fileStream.Write(buffer, 0, Math.Min(to_write, WRITE_CHUNK));
                    to_write -= Math.Min(to_write, WRITE_CHUNK);
                }
            }
        }
    }
}