using BepInEx;
using UnityEngine;
using System.Reflection;
using System.Linq;

// We use the SteamClient alias from the game's existing references
using SteamAPI = Steamworks.SteamClient;

[BepInPlugin("com.kingcox.sbg.ezlobbylink", "SBG EZ Lobby Link", "1.0.2")]
public class LobbyCopierMod : BaseUnityPlugin
{
    private const string AppId = "4069520";
    private string _statusMessage = "";
    private float _messageTimer = 0f;

    void OnGUI()
    {
       
        if (PauseMenu.IsPaused)
        {
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.black;
            buttonStyle.hover.textColor = Color.black; // Keep it black when hovering
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;

           
            Rect buttonRect = new Rect(20, Screen.height - 100, 240, 40);
            
            
            if (GUI.Button(buttonRect, "Copy Steam Invite Link", buttonStyle))
            {
                ExecuteCopy();
            }

            // Status message display
            if (_messageTimer > Time.time)
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { 
                    fontSize = 16, 
                    fontStyle = FontStyle.Bold 
                };
                labelStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(25, Screen.height - 150, 400, 30), _statusMessage, labelStyle);
            }
        }
    }

    private void ExecuteCopy()
    {
        try 
        {
            string mySteamId = SteamAPI.SteamId.ToString();
            ulong lobbyId = 0;

            
            var lobbyField = typeof(BNetworkManager).GetField("steamLobby", BindingFlags.Static | BindingFlags.NonPublic);
            
            if (lobbyField != null)
            {
                var rawValue = lobbyField.GetValue(null);
                if (rawValue != null)
                {
                    
                    var valueProp = rawValue.GetType().GetProperty("Value");
                    object actualLobby = (valueProp != null) ? valueProp.GetValue(rawValue) : rawValue;

                    if (actualLobby != null)
                    {
                        
                        var idField = actualLobby.GetType().GetProperty("Id");
                        if (idField != null)
                        {
                            var steamIdObj = idField.GetValue(actualLobby);
                            
                            
                            var finalValueProp = steamIdObj.GetType().GetProperty("Value");
                            if (finalValueProp != null)
                            {
                                lobbyId = (ulong)finalValueProp.GetValue(steamIdObj);
                            }
                            else
                            {
                                
                                lobbyId = ulong.Parse(steamIdObj.ToString());
                            }
                        }
                    }
                }
            }

            if (lobbyId != 0)
            {
                string inviteLink = $"steam://joinlobby/{AppId}/{lobbyId}/{mySteamId}";
                GUIUtility.systemCopyBuffer = inviteLink;
                _statusMessage = "Link Copied! ID: " + lobbyId;
                _messageTimer = Time.time + 4f;
                Debug.Log($"[LobbyCopier] Success! Copied: {inviteLink}");
            }
            else
            {
                _statusMessage = "No Active Lobby Found (ID 0)";
                _messageTimer = Time.time + 4f;
            }
        }
        catch (System.Exception e)
        {
            _statusMessage = "Reflection Error - Check Log";
            
            Debug.LogError($"[LobbyCopier] Error: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
        }
    }
}