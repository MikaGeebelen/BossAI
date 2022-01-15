using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    public static readonly string SaveFolder = Application.dataPath + "/Rules/";
    public static readonly string BossFolder = Application.dataPath + "/Bosses/";
    public static void Init()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        if (!Directory.Exists(BossFolder))
        {
            Directory.CreateDirectory(BossFolder);
        }

        if (!Directory.Exists(BossFolder + "/Structure/"))
        {
            Directory.CreateDirectory(BossFolder + "/Structure/");
        }

        if (!Directory.Exists(BossFolder + "/Info/"))
        {
            Directory.CreateDirectory(BossFolder + "/Info/");
        }

        if (!Directory.Exists(BossFolder + "/Tree/"))
        {
            Directory.CreateDirectory(BossFolder + "/Tree/");
        }
    }
    public static void SaveRule(string saveText)
    {
        File.WriteAllText(SaveFolder + "Rules.json", saveText);
    }

    public static string LoadRule()
    {
        return File.ReadAllText(SaveFolder + "Rules.json");
    }

    public static void SaveBossGraph(string bossGraph, string bossJson)
    {
        if (bossGraph == "")
        {
            File.WriteAllText(BossFolder + "temp" + ".json", bossJson);
        }
        else
        {
            File.WriteAllText(BossFolder + bossGraph + ".json", bossJson);
        }
    }

    public static string LoadBossGraph(string bossGraph)
    {
       return File.ReadAllText(BossFolder + bossGraph +".json");
    }

    public static void SaveBossStructure(string bossGraph, string bossJson)
    {
        if (bossGraph == "")
        {
            File.WriteAllText(BossFolder + "/Structure/" + "temp" + ".json", bossJson);
        }
        else
        {
            File.WriteAllText(BossFolder + "/Structure/" + bossGraph + ".json", bossJson);
        }
    }

    public static string LoadBossStructure(string bossGraph)
    {
        return File.ReadAllText(BossFolder + "/Structure/" + bossGraph + ".json");
    }

    public static void SaveBossInfo(string bossGraph, string bossJson)
    {
        if (bossGraph == "")
        {
            File.WriteAllText(BossFolder + "/Info/" + "temp" + ".json", bossJson);
        }
        else
        {
            File.WriteAllText(BossFolder + "/Info/" + bossGraph + ".json", bossJson);
        }
    }
    public static string LoadBossInfo(string bossGraph)
    {
        return File.ReadAllText(BossFolder + "/Info/" + bossGraph + ".json");
    }

    public static void SaveBossTree(string bossGraph, string bossJson)
    {
        if (bossGraph == "")
        {
            File.WriteAllText(BossFolder + "/Tree/" + "temp" + ".json", bossJson);
        }
        else
        {
            File.WriteAllText(BossFolder + "/Tree/" + bossGraph + ".json", bossJson);
        }
    }

    public static string LoadBossTree(string bossGraph)
    {
        return File.ReadAllText(BossFolder + "/Tree/" + bossGraph + ".json");
    }

}
