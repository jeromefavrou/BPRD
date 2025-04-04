using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;

public class RAMDisplay : MonoBehaviour
{
    private TMP_Text ramText; // Assigne ce champ dans l'inspecteur

    void Awake()
    {
        ramText = GetComponent<TMP_Text>();
    }
    void Start()
    {
    }

    void OnGUI()
    {
        string os = SystemInfo.operatingSystem.ToLower();

        if (os.Contains("windows"))
        {
            // Code pour Windows
            DisplayMemoryWindows();
        }
        else if (os.Contains("linux"))
        {
            // Code pour Linux
            DisplayMemoryLinux();
        }
        else
        {
            ramText.text = "Système non supporté";
        }
    }

    void DisplayMemoryLinux()
    {
        string[] lines = File.ReadAllLines("/proc/meminfo");
        ramText.text = "RAM NON Disponible: ";

        foreach (string line in lines)
        {
            if (line.StartsWith("MemAvailable"))
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && float.TryParse(parts[1], out float availableKB))
                {
                    float res = availableKB / 1024f; // Convertir en Mo
                    ramText.text = "RAM libre: " + res.ToString("F2") + " Mo";
                    break;
                }
            }
        }
    }

    void DisplayMemoryWindows()
    {
        try
        {
            // P/Invoke pour récupérer la mémoire libre sous Windows
            ulong freeMemory = GetFreePhysicalMemory();
            ramText.text = "RAM libre: " + (freeMemory / 1024f / 1024f).ToString("F2") + " Mo"; // Conversion en Mo
        }
        catch (Exception e)
        {
            ramText.text = "Erreur de récupération des données de RAM: " + e.Message;
        }
    }

    // P/Invoke pour obtenir la mémoire libre sous Windows
    [DllImport("kernel32.dll")]
    public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public ulong GetFreePhysicalMemory()
    {
        MEMORYSTATUSEX status = new MEMORYSTATUSEX();
        status.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

        if (GlobalMemoryStatusEx(ref status))
        {
            return status.ullAvailPhys; // Retourne la mémoire libre en octets
        }
        else
        {
            throw new Exception("Impossible de récupérer les informations sur la mémoire.");
        }
    }


}