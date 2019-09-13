using Newtonsoft.Json.Linq;
using System.IO;

public static class FileUtility
{
    public static string ReadTextFile(string path)
    {
        StreamReader stream_reader = new StreamReader(path);
        string text = stream_reader.ReadToEnd();
        stream_reader.Close();

        return text;
    }

    public static void WriteTextFile(string path, string text, bool append = false)
    {
        StreamWriter stream_writer = new StreamWriter(path, append);
        stream_writer.WriteLine(text);
        stream_writer.Close();
    }

    public static void OutputText(string text, string name, bool append = false)
    {
        WriteTextFile("Output/" + name + ".txt", text, append);
    }

    public static void Save(Encodable encodable, string path)
    {
        WriteTextFile(path, encodable.EncodeJson().ToString());
    }

    public static T Load<T>(string path) where T : Encodable, new()
    {
        T obj = new T();
        obj.DecodeJson(JObject.Parse(ReadTextFile(path)));

        return obj;
    }

    public static void AddToGameSave(Encodable encodable, string save_name, string folder, string object_name)
    {
        Save(encodable, save_name + "/" + folder + "/" + object_name);
    }

    public static T LoadFromGameSave<T>(string save_name, string folder, string object_name) where T : Encodable, new()
    {
        return Load<T>(save_name + "/" + folder + "/" + object_name);
    }
}
