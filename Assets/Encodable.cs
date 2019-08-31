
using Newtonsoft.Json.Linq;

public interface Encodable
{
    string EncodeString();
    void DecodeString(string string_encoding);

    JObject EncodeJson();
    void DecodeJson(JObject json_object);
}