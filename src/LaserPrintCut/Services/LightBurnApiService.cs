using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;

namespace LaserPrintCutAddin.Services
{
    public class LightBurnApiService
    {
        private readonly HttpClient _httpClient;
        private string _serverUrl = "http://localhost:7430";
        private bool _isConnected = false;

        public LightBurnApiService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                _serverUrl = value;
                _httpClient.BaseAddress = new Uri(value);
            }
        }

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/version");
                _isConnected = response.IsSuccessStatusCode;
                return _isConnected;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> SendFileToLightBurnAsync(string filePath, string projectName)
        {
            if (!_isConnected)
                await ConnectAsync();

            try
            {
                var formData = new MultipartFormDataContent();
                
                var fileBytes = File.ReadAllBytes(filePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                formData.Add(fileContent, "file", Path.GetFileName(filePath));

                formData.Add(new StringContent(projectName), "projectName");

                var response = await _httpClient.PostAsync("/api/v1/projects/import", formData);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StartPrintAndCutAsync()
        {
            if (!_isConnected)
                await ConnectAsync();

            try
            {
                var response = await _httpClient.PostAsync("/api/v1/print-and-cut/start", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LightBurnStatus> GetStatusAsync()
        {
            if (!_isConnected)
                await ConnectAsync();

            try
            {
                var response = await _httpClient.GetAsync("/api/v1/status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LightBurnStatus>(json);
                }
            }
            catch { }

            return new LightBurnStatus { IsConnected = false };
        }

        public async Task<List<LightBurnDevice>> GetDevicesAsync()
        {
            if (!_isConnected)
                await ConnectAsync();

            try
            {
                var response = await _httpClient.GetAsync("/api/v1/devices");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<LightBurnDevice>>(json);
                }
            }
            catch { }

            return new List<LightBurnDevice>();
        }

        public async Task<bool> SendCutJobAsync(CutJobParameters jobParams)
        {
            if (!_isConnected)
                await ConnectAsync();

            try
            {
                var json = JsonSerializer.Serialize(jobParams);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/v1/jobs/cut", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task DisposeAsync()
        {
            _httpClient?.Dispose();
        }
    }

    public class LightBurnStatus
    {
        public bool IsConnected { get; set; }
        public string Version { get; set; }
        public string CurrentState { get; set; }
        public double Progress { get; set; }
        public string ActiveProject { get; set; }
    }

    public class LightBurnDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public string Type { get; set; }
    }

    public class CutJobParameters
    {
        public string ProjectName { get; set; }
        public double Power { get; set; }
        public double Speed { get; set; }
        public int Passes { get; set; }
        public string CutMode { get; set; }
        public RegistrationMarkSettings RegistrationMarks { get; set; }
    }

    public class RegistrationMarkSettings
    {
        public double MarkDiameter { get; set; } = 10.0;
        public string MarkColor { get; set; } = "Red";
        public int MarkCount { get; set; } = 2;
        public RegistrationMarkPosition Position { get; set; } = RegistrationMarkPosition.Corners;
    }

    public enum RegistrationMarkPosition
    {
        Corners,
        Centers,
        Custom
    }
}