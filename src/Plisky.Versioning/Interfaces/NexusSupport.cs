﻿namespace Plisky.CodeCraft;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Plisky.Diagnostics;

public record NexusConfig {
    public string? Username { get; init; }
    public string? Password { get; init; }
    public required string Url { get; init; }
    public required string Server { get; init; }
    public string BasePathUrl {
        get {
            return Url.Substring(0, Url.LastIndexOf('/') + 1);
        }
    }

    public required string FilenameUrl { get; init; }
    public string? SearchPath { get; init; } = null;
    public string? Repository { get; init; } = null;
}

public class MarkerPosition {
    public required int Position { get; set; }
    public required string Marker { get; set; }
    public string? Value { get; set; }

}
public class NexusSupport {
    protected Bilge b = new Bilge("plisky-nexus");
    public const string NEXUS_PREFIX = "[NEXUS]";
    private static readonly HttpClient client = new HttpClient();


    Action<byte[], string> SaveCreator(Action<byte[], string, string> saver, string path) {
        return (b, c) => saver(b, c, path);
    }

    public async Task CacheNexusFiles(NexusConfig nc, string identifier, Action<byte[], string, string> saveFile) {
        b.Info.Flow($"{identifier}");

        if (string.IsNullOrEmpty(nc.Repository)) {
            throw new InvalidOperationException($"Unable to connect to Nexus ({nc.Server}) correctly - missing repository.");
        }

        string assetsApi = $"{nc.Server}/service/rest/v1/assets?repository={nc.Repository}";

        var files = new List<Tuple<string, string>>();


        try {
            b.Verbose.Log($"Attepting to connect  to {assetsApi}");
            var request = new HttpRequestMessage(HttpMethod.Get, assetsApi);

            if (!string.IsNullOrEmpty(nc.Username)) {
                b.Verbose.Log($"Adding authentication - {nc.Username}");
                byte[] byteArray = new System.Text.UTF8Encoding().GetBytes($"{nc.Username}:{nc.Password}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            b.Verbose.Log("About to send.");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            b.Verbose.Log($"Content recieved {content.Length}");
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            foreach (var item in root.GetProperty("items").EnumerateArray()) {
                if (item.TryGetProperty("path", out var pathElement)) {
                    if (item.TryGetProperty("id", out var idElement)) {
                        b.Verbose.Log($"Found file {pathElement} {idElement}");

                        string? pt = pathElement.GetString();
                        string? id = idElement.GetString();
                        if (string.IsNullOrEmpty(pt) || string.IsNullOrEmpty(id)) {
                            b.Warning.Log($"Skipping file with empty path or id: {pt} {id}");
                        } else {
                            files.Add(new Tuple<string, string>(pt, id));
                        }
                    }
                }
            }
        } catch (HttpRequestException hrx) {
            throw new InvalidOperationException($"Unable to connect to Nexus ({nc.Server}) correctly. Status:{hrx.StatusCode}", hrx);
        }

        foreach (var l in files) {
            string downloadPath = $"{nc.Server}/repository/{nc.Repository}{l.Item1}";
            if (l.Item1.StartsWith(identifier)) {
                DownloadFileAsync(downloadPath, SaveCreator(saveFile, identifier), l.Item1, nc.Username, nc.Password).Wait();
            }
        }

    }

    public async Task UploadFileAsync(Stream fileContent, string repositoryPath, string? username, string? password) {
        try {


            using (var content = new StreamContent(fileContent)) {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var request = new HttpRequestMessage(HttpMethod.Put, repositoryPath) {
                    Content = content
                };

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
                    byte[] byteArray = new System.Text.UTF8Encoding().GetBytes($"{username}:{password}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        } catch (HttpRequestException) {
            throw;
        }
    }



    public async Task DownloadFileAsync(string downloadUrl, Action<byte[], string> saveFile, string fileName, string? username, string? password) {
        b.Info.Flow();
        try {
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
                byte[] byteArray = new System.Text.UTF8Encoding().GetBytes($"{username}:{password}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await client.SendAsync(request);
            b.Verbose.Log("Request Made, checking response", $"{downloadUrl}");
            response.EnsureSuccessStatusCode();

            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            //await File.WriteAllBytesAsync(filename, fileBytes);
            saveFile(fileBytes, fileName);
        } catch (HttpRequestException) {
            throw;
        }
    }

    public Dictionary<string, MarkerPosition> GetChunks(string nexusUrl, string[] markers) {

        var result = new Dictionary<string, MarkerPosition>();

        var mrks = new List<MarkerPosition>();
        foreach (string l in markers) {
            var m = new MarkerPosition() {
                Marker = l,
                Position = nexusUrl.IndexOf(l)

            };
            mrks.Add(m);
        }

        var mio = mrks.OrderBy(p => p.Position).ToList();
        for (int i = 0; i < mio.Count; i++) {

            if (mio[i].Position < 0) {
                continue;
            }

            if (i == mio.Count - 1) {
                mio[i].Value = nexusUrl.Substring(mio[i].Position + mio[i].Marker.Length);
            } else {
                mio[i].Value = nexusUrl.Substring(mio[i].Position + mio[i].Marker.Length, mio[i + 1].Position - mio[i].Position - mio[i].Marker.Length);
            }
        }

        foreach (var l in mrks) {
            result.Add(l.Marker, l);
        }
        return result;
    }

    [Pure]
    public NexusConfig? GetNexusSettings(string nexusToken) {
        if (!nexusToken.StartsWith(NEXUS_PREFIX)) {
            return null;
        }

        string nexusParse = nexusToken.Substring(NEXUS_PREFIX.Length);
        var chunks = GetChunks(nexusParse, new string[] { "[U::", "[P::", "[L::", "[R::", "[G::" });

        string? username = chunks["[U::"].Value;
        string? password = chunks["[P::"].Value;
        string? nexusUrl = chunks["[L::"].Value;

        if (string.IsNullOrEmpty(nexusUrl)) {
            return null;
        }

        int httpPos = nexusUrl.IndexOf("://");
        int afterHttp = nexusUrl.IndexOf('/', httpPos + 3);
        string server = nexusUrl.Substring(0, afterHttp);

        var result = new NexusConfig() {
            Username = username,
            Password = password,
            Url = nexusUrl,
            FilenameUrl = nexusUrl.Substring(nexusUrl.LastIndexOf('/') + 1),
            Server = server,
            Repository = chunks["[R::"]?.Value,
            SearchPath = chunks["[G::"]?.Value
        };
        return result;
    }

    public Tuple<string, string> GetVersionAndFilenameFromNexusUrl(string molsbaseMarker, string downloadUrl) {
        b.Info.Flow($"{downloadUrl}");
        if (downloadUrl.StartsWith(molsbaseMarker)) {
            string working = downloadUrl.Substring(molsbaseMarker.Length);
            int fileMarkerStart = working.LastIndexOf('/') + 1;
            string filename = working.Substring(fileMarkerStart).Replace("/", "");
            string version = working.Substring(0, fileMarkerStart - 1).Replace("/", "");

            return new Tuple<string, string>(version, filename);
        }
        b.Warning.Log($"download url did not start with marker ]{molsbaseMarker}[, returning empty");
        return new Tuple<string, string>(string.Empty, string.Empty);
    }

    public async Task<bool> FileExistsAsync(string fileUrl, string? username, string? password) {
        b.Info.Flow($"Checking existence of file at URL: {fileUrl}");
        var request = new HttpRequestMessage(HttpMethod.Head, fileUrl);

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) {
            byte[] byteArray = new System.Text.UTF8Encoding().GetBytes($"{username}:{password}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        try {
            var response = await client.SendAsync(request);
            b.Verbose.Log($"Received response: {(int)response.StatusCode} {response.StatusCode} for {fileUrl}");
            bool result = response.IsSuccessStatusCode;
            return result;
        } catch (HttpRequestException ex) {
            b.Warning.Log($"HTTP request exception when checking file existence at {fileUrl}: {ex.Message}");
            return false;
        }
    }
}
