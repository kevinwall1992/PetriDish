
using Newtonsoft.Json.Linq;

public interface Encodable
{
    JObject EncodeJson();
    void DecodeJson(JObject json_object);
}