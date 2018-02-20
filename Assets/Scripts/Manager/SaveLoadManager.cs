using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoadManager
{
    public static void SavePlayer(UserStatus us)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/save.juliett", FileMode.Create);

        bf.Serialize(stream, us);
        stream.Close();
    }

    public static UserStatus LoadPlayer()
    {
        if (File.Exists(Application.persistentDataPath + "/save.juliett"))
        {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream stream = new FileStream(Application.persistentDataPath + "/save.juliett", FileMode.Open);

            UserStatus dat = bf.Deserialize(stream) as UserStatus;

            stream.Close();
            return dat;
        }
        else
        {
            Debug.Log("No Save File");
            UserStatus dat = new UserStatus();
            return dat;
        }
    }
}

[Serializable]
public class UserStatus
{

}

