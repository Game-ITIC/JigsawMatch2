package com.coreplugins.apploudspeaker;

import android.content.ComponentCallbacks;
import android.content.res.Configuration;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.HashMap;
import java.util.Map;

public class Broadcast
{
    private static boolean s_registered = false;

    private static Map<String, String> s_queuedMessages = new HashMap<String, String>();

    static public void Start()
    {
        if( !s_registered)
        {
            s_registered = true;

            Log.d("Unity", "AppLoudspeaker Register callbacks");

            UnityPlayer.currentActivity.getApplicationContext().registerComponentCallbacks(
                    new ComponentCallbacks()
                    {
                        // @Override
                        public void onConfigurationChanged(Configuration configuration)
                        {
                        }

                        // @Override
                        public void onLowMemory()
                        {
                            String jsonRet = "{}";

                            Log.d("Unity", "onLowMemory");

                            UnityPlayer.UnitySendMessage("AppDelegateLoudspeaker", "_BroadcastUIApplicationDidReceiveMemoryWarningNotification", jsonRet);
                        }
                    });

            if( !s_queuedMessages.isEmpty() )
            {
                for ( String key : s_queuedMessages.keySet() )
                {
                    String retData = s_queuedMessages.get(key);
                    UnityPlayer.UnitySendMessage("AppDelegateLoudspeaker", key, retData );
                }

                s_queuedMessages.clear();
            }
        }
    }

    static public void SendMessage(String messageID, String messageData )
    {
        if( !s_registered)
        {
            s_queuedMessages.put(messageID, messageData);
        }
        else
        {
            UnityPlayer.UnitySendMessage("AppDelegateLoudspeaker", messageID, messageData );
        }
    }
}
