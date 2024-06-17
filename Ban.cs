using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable
namespace DisasterServer
{
  public class Ban
  {
    [JsonPropertyName("list")]
    public Dictionary<string, Dictionary<string, string>> List { get; set; } = new Dictionary<string, Dictionary<string, string>>();
  }
}
