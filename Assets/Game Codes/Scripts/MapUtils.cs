using System.Collections.Generic;
using UnityEngine;

public class MapUtils : MonoBehaviour
{
    public static readonly Dictionary<string, List<string>> Neighbors = new()
    {
        { "SA1", new List<string> { "SA2", "SA3", "SA4" } },
        { "SA2", new List<string> { "SA1", "SA3", "NA1" } },
        { "SA3", new List<string> { "SA2", "SA1", "SA4", "AF5" } },
        { "SA4", new List<string> { "SA1", "SA3" } },
        
        { "NA1", new List<string> { "SA2", "NA2", "NA3" } },
        { "NA2", new List<string> { "NA3", "NA1", "NA4", "NA7" } },
        { "NA3", new List<string> { "NA1", "NA2", "NA5" } },
        { "NA4", new List<string> { "NA5", "NA7", "NA9", "NA2" } },
        { "NA5", new List<string> { "NA3", "NA4", "NA6", "NA9" } },
        { "NA6", new List<string> { "NA5", "NA9", "AS4" } },
        { "NA7", new List<string> { "NA2", "NA4", "NA8", "NA9" } },
        { "NA8", new List<string> { "NA7", "NA9", "EU2" } },
        { "NA9", new List<string> { "NA4", "NA5", "NA6", "NA8" } },

        { "AF1", new List<string> { "AF2", "AF3", "AF6" } },
        { "AF2", new List<string> { "AF1", "AF3", "AF5" } },
        { "AF3", new List<string> { "AF1", "AF2", "AF4", "AF6" } },
        { "AF4", new List<string> { "AF3", "AF5", "AS1" } },
        { "AF5", new List<string> { "AF2", "AF3", "AF4", "SA3", "EU4" } },
        { "AF6", new List<string> { "AF1", "AF3" } },

        { "EU1", new List<string> { "EU2", "EU3", "EU5", "EU7", "EU8" } },
        { "EU2", new List<string> { "EU1", "EU3", "EU7", "NA8" } },
        { "EU3", new List<string> { "EU1", "EU2", "EU4", "EU7" } },
        { "EU4", new List<string> { "EU3", "EU6", "EU7", "AF5" } },
        { "EU5", new List<string> { "EU1", "EU8", "AS4", "AS5" } },
        { "EU6", new List<string> { "EU4", "EU7", "EU8", "AS1" } },
        { "EU7", new List<string> { "EU1", "EU3", "EU4", "EU6", "EU8" } },
        { "EU8", new List<string> { "EU1", "EU5", "EU6", "EU7" } },

        { "AS1", new List<string> { "AS3", "AS5", "EU5", "EU6", "AF4" } },
        { "AS2", new List<string> { "AS3", "AS6", "AU1" } },
        { "AS3", new List<string> { "AS1", "AS2", "AS5", "AS6" } },
        { "AS4", new List<string> { "AS5", "AS6", "AS7", "EU5", "NA6", "AS8" } },
        { "AS5", new List<string> { "AS1", "AS3", "AS4", "AS6", "EU5" } },
        { "AS6", new List<string> { "AS2", "AS3", "AS4", "AS7", "AS8" } },
        { "AS7", new List<string> { "AS4", "AS5", "AS6" } },
        { "AS8", new List<string> { "AS4", "AS6" } },

        { "AU1", new List<string> { "AU2", "AU4", "AS2" } },
        { "AU2", new List<string> { "AU1", "AU3" } },
        { "AU3", new List<string> { "AU2", "AU4" } },
        { "AU4", new List<string> { "AU1", "AU3" } }
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
