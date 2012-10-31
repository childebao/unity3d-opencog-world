using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/**
 * Persistence utility for saving object into file in binary format.
 */
public static class Persist
{
    public static void Save(object obj, string fileName)
    {
        FileStream stream = new FileStream(fileName, FileMode.Create);
 
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            //formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
            formatter.Serialize(stream, obj);
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to serialize. Exception: " + e.Message);
            throw;
        }
        finally
        {
            stream.Close();
        }
    }

    public static object Load(string fileName)
    {
        if (!File.Exists(fileName)) return null;

        FileStream stream = new FileStream(fileName, FileMode.Open);
        object obj = null;
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new AllowAllVersionDeserializationBinder();
            obj = (object)formatter.Deserialize(stream);
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to deserialize. Exception: " + e.Message);
            throw;
        }
        finally
        {
            stream.Close();
        }
        return obj;
    }
}