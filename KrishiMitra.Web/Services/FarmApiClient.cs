using System.Net.Http.Json;
using KrishiMitra.Core;

namespace KrishiMitra.Web.Services;

public sealed class FarmApiClient(HttpClient http)
{
    public async Task<List<Farm>> FarmsAsync(CancellationToken ct = default) => await http.GetFromJsonAsync<List<Farm>>("api/farms", ct) ?? [];
    public async Task<Farm?> FarmAsync(int id, CancellationToken ct = default) => await http.GetFromJsonAsync<Farm>($"api/farms/{id}", ct);
    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default) => await http.GetFromJsonAsync<T>(path, ct);
    public async Task<T?> PostAsync<T>(string path, object body, CancellationToken ct = default) { using var response = await http.PostAsJsonAsync(path, body, ct); response.EnsureSuccessStatusCode(); return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct); }
    public async Task<T?> PutAsync<T>(string path, object body, CancellationToken ct = default) { using var response = await http.PutAsJsonAsync(path, body, ct); response.EnsureSuccessStatusCode(); return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct); }
    public async Task DeleteAsync(string path, CancellationToken ct = default) { using var response = await http.DeleteAsync(path, ct); response.EnsureSuccessStatusCode(); }
}
