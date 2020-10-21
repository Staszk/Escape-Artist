using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

//Written by John Imgrund

public class WebsiteScript
{
    public static event Action<int[], int[], int[]> ActionFinishedClickRequest = delegate { };
    public static event Action<string, int, int, Vector3> ActionGotPrice = delegate { };

    //Incoming variables
    string inComing;
    string[] inComingArray;
    string[,] itemPrices;

    private bool connect;

    //Website urls
    const string INCOMING_WEBSITE = "http://escapeartistgame.com/php/unitySender.php";
    const string OUTGOING_WEBSITE = "http://escapeartistgame.com/php/unityReceiver.php";
    const string LOCK_FACTION_WEBSITE = "http://escapeartistgame.com/php/unityLock.php";
    const string ITEM_PRICE_WEBSITE = "http://escapeartistgame.com/php/unityPriceSender.php";

    public WebsiteScript(bool connect)
    {
        Initialize(connect);
    }

    void Initialize(bool connect)
    {
        this.connect = connect;

        itemPrices = new string[8, 2]
            {
                {"Artifact", "450"},
                {"Gem", "650"},
                {"Jewelry", "600"},
                {"MainArtPiece", "20000"},
                {"Masks", "550"},
                {"Paintings", "350"},
                {"Statues", "400"},
                {"Vase", "500"},
            };
    }

    public IEnumerator GetRequest()
    {
        int[] timesClickedGood = new int[3];
        int[] timesClickedBad = new int[3];
        int[] audienceNumbers = new int[2];

        using (UnityWebRequest webRequest = UnityWebRequest.Get(INCOMING_WEBSITE))
        {
            //Wait for page to load
            yield return webRequest.SendWebRequest();

            //Checks to see if data has been recieved
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                //Assign incoming page data to string
                inComing = webRequest.downloadHandler.text;

                if (inComing == "0 results")
                {
                    Debug.LogWarning("Warning: No results found on database!");
                    yield break;
                }

                //Split string into individual words
                inComingArray = inComing.Split(' ');

                //Used in the next loop
                //Accounts for increase of loop iterator while maintaining steady count
                //Used for interactables index
                int indexOffset = 0;

                //Assign each count value to propper action interactable
                for(int i = 1; i < 6; i = i + 2)
                {
                    timesClickedBad[indexOffset] = int.Parse(inComingArray[i]);

                    ++indexOffset;
                }

                //Used for interactables index
                indexOffset = 0;

                for (int i = 7; i < 12; i = i + 2)
                {
                    timesClickedGood[indexOffset] = int.Parse(inComingArray[i]);

                    ++indexOffset;
                }

                //Reusing int for effiency
                //Simple normal index for the audienceNumbers Array
                indexOffset = 0;

                for (int i = 13; i < 16; i = i + 2)
                {
                    int audienceFinal = int.Parse(inComingArray[i]) - 1;

                    if (audienceFinal < 1) //Make sure theres at least one user
                        audienceFinal = 1;

                    audienceNumbers[indexOffset] = audienceFinal;

                    ++indexOffset;
                }

                ActionFinishedClickRequest(timesClickedGood, timesClickedBad, audienceNumbers);
            }
        }
    }

    public IEnumerator GetItemPrice(string item, Vector3 location)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ITEM_PRICE_WEBSITE))
        {
            //Wait for page to load
            yield return webRequest.SendWebRequest();

            //Checks to see if data has been recieved
            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                //Assign incoming page data to string
                inComing = webRequest.downloadHandler.text;

                if (inComing == "0 results")
                {
                    Debug.LogWarning("Warning: No results found on database!");
                    yield break;
                }

                //Split string into individual words
                inComingArray = inComing.Split(' ');

                //Find the correct item, then get its price
                int finalPrice = 0;
                int originalPrice = 0;
                int itemIndex = 0;

                for (int i = 0; i < inComingArray.Length; i = i + 2)
                {
                    if (inComingArray[i] == item)
                    {
                        originalPrice = int.Parse(itemPrices[itemIndex, 1]);

                        int nextIndex = i + 1;
                        finalPrice = int.Parse(inComingArray[nextIndex]);

                        break;
                    }

                    ++itemIndex;
                }

                //Compare website price to the base price
                int upOrDown;

                if (finalPrice > originalPrice)
                {
                    upOrDown = 0; //Price is higher than normal
                }
                else if (finalPrice < originalPrice)
                {
                    upOrDown = 1; //Price is less than normal
                }
                else
                {
                    upOrDown = 0; //Price is the normal
                }

                //PARKER PLACE YOUR FUNCTION HERE CALL HERE
                ActionGotPrice(item, finalPrice, upOrDown, location);
            }
        }
    }

    public IEnumerator LockFaction(int team, int locked, string winningTag)
    {
        if (connect)
        {
            WWWForm lockFaction = new WWWForm();

            // Handling both teams
            if (team == 2)
            {
                lockFaction.AddField("Faction", "Both");
                lockFaction.AddField("VLock", locked); // 0 Unlocked; 1 Locked
                lockFaction.AddField("Winner", winningTag);
            }
            else
            {
                string faction = team == 0 ? "BlackHats" : "WhiteHats";

                lockFaction.AddField("Faction", faction);
                lockFaction.AddField("VLock", locked); // 0 Unlocked; 1 Locked
                lockFaction.AddField("Winner", winningTag);
            }

            using (UnityWebRequest www = UnityWebRequest.Post(LOCK_FACTION_WEBSITE, lockFaction))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning("FAILURE TO SEND DATA TO DATABASE! ERROR: " + www.error);
                }
            }
        }
    }

    public IEnumerator UpdateInteractables(string[,] outgoingInteractables)
    {
        if (connect)
        {
            WWWForm interactableList = new WWWForm();

            //Good Interactions
            //The columns have been switched because the website needs to blackhat options first
            interactableList.AddField("WhiteHat1", outgoingInteractables[0, 0]);
            interactableList.AddField("WhiteHat2", outgoingInteractables[0, 1]);
            interactableList.AddField("WhiteHat3", outgoingInteractables[0, 2]);

            //Bad Interactions
            interactableList.AddField("BlackHat1", outgoingInteractables[1, 0]);
            interactableList.AddField("BlackHat2", outgoingInteractables[1, 1]);
            interactableList.AddField("BlackHat3", outgoingInteractables[1, 2]);

            using (UnityWebRequest www = UnityWebRequest.Post(OUTGOING_WEBSITE, interactableList))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning("FAILURE TO SEND DATA TO DATABASE! ERROR: " + www.error);
                }
            }
        }
    }
}
