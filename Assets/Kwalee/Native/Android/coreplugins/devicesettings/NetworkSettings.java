package com.coreplugins.networksettings;

import android.content.Context;
import android.os.Build;
import android.provider.Settings;

public class NetworkSettings
{
    public static String isAirplaneModeOn(Context context) {

        boolean isAirplaneMode = false;

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.JELLY_BEAN_MR1)
        {
            isAirplaneMode = Settings.System.getInt(context.getContentResolver(), Settings.System.AIRPLANE_MODE_ON, 0) != 0;
        }
        else
        {
            isAirplaneMode = Settings.Global.getInt(context.getContentResolver(), Settings.Global.AIRPLANE_MODE_ON, 0) != 0;
        }

        String airPlaneModeTrue = "true";
        String airPlaneModeFalse = "false";

        if(isAirplaneMode)
        {
            return airPlaneModeTrue;
        }
        else
        {
            return airPlaneModeFalse;
        }
    }
}