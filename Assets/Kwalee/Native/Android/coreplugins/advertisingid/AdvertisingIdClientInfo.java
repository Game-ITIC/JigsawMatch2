package com.coreplugins.advertisingid;
import android.app.Activity;

import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import com.unity3d.player.UnityPlayer;

public class AdvertisingIdClientInfo 
{
    private final String CLASS_NAME="DeviceSettingsManager";
    private final String FUNCTION_NAME="_OnAdvertisingIDReceived";

    public static AdvertisingIdClientInfo instance=new AdvertisingIdClientInfo();
    public static AdvertisingIdClientInfo getInstance()
    {
        return instance;
    }
    public void methodName(final Activity context) {
        new Thread(new Runnable() {
            public void run() {
                try {
                    AdvertisingIdClient.Info info=AdvertisingIdClient.getAdvertisingIdInfo(context);
                    String infoString= info.getId()+"~"+info.isLimitAdTrackingEnabled();
                    UnityPlayer.UnitySendMessage(CLASS_NAME ,FUNCTION_NAME ,infoString);
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }).start();
    }
}

