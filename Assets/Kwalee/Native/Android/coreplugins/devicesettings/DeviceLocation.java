package com.coreplugins.devicesettings;

import android.Manifest;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.pm.PackageManager;
import android.location.Address;
import android.location.Geocoder;
import android.location.Location;
import android.location.LocationManager;
import android.os.Build;
import android.os.LocaleList;
import androidx.core.app.ActivityCompat;
import androidx.appcompat.view.ContextThemeWrapper;
import androidx.appcompat.R.style;
import android.telephony.TelephonyManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.Arrays;
import java.util.List;
import java.util.Locale;

public class DeviceLocation
{
    private static final String[] s_fallbackLanguages = {"en-GB", "es-ES", "fr-FR", "de-DE"};

    public static String GetCountry()
    {
        String country = "";
        try
        {
            LocationManager locationManager = (LocationManager) UnityPlayer.currentActivity.getSystemService(Context.LOCATION_SERVICE);
            if (locationManager != null)
            {
                Location location = locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
                if (location == null)
                {
                    location = locationManager.getLastKnownLocation(LocationManager.NETWORK_PROVIDER);
                }
                if (location != null)
                {
                    Geocoder gcd = new Geocoder(UnityPlayer.currentActivity, Locale.getDefault());
                    List<Address> addresses;
                    addresses = gcd.getFromLocation(location.getLatitude(), location.getLongitude(), 1);

                    if (addresses != null && !addresses.isEmpty())
                    {
                        country = addresses.get(0).getCountryCode();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.i("Unity", "Error locating country: " + e.getMessage());
        }

        if (country == "")
        {
            country = getCountryBasedOnSimCardOrNetwork(UnityPlayer.currentActivity);
        }

        return country;
    }

    private static String getCountryBasedOnSimCardOrNetwork(Context context)
    {
        try
        {
            final TelephonyManager tm = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);
            final String simCountry = tm.getSimCountryIso();
            if (simCountry != null && simCountry.length() == 2)
            {
                // SIM country code is available
                return simCountry.toLowerCase();
            }
            else if (tm.getPhoneType() != TelephonyManager.PHONE_TYPE_CDMA)
            {
                // device is not 3G (would be unreliable)
                String networkCountry = tm.getNetworkCountryIso();

                if (networkCountry != null && networkCountry.length() == 2)
                {
                    return networkCountry.toLowerCase();
                }
            }
        }
        catch (Exception e)
        {
        }
        return null;
    }

    public static String GetLanguageCode()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N)
        {
            LocaleList localeList = UnityPlayer.currentActivity.getResources().getConfiguration().getLocales();

            Locale loc = localeList.get(0);

            return loc.toString();
        }
        else
        {
            return UnityPlayer.currentActivity.getResources().getConfiguration().locale.toString();
        }
    }

    public static String GetLanguageName(String language, String country, String variant)
    {
        Locale loc = null;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N)
        {
            LocaleList localeList = UnityPlayer.currentActivity.getResources().getConfiguration().getLocales();
            loc = localeList.get(0);
        }
        else
        {
            loc = UnityPlayer.currentActivity.getResources().getConfiguration().locale;
        }

        Locale testLocale = new Locale(language, country, variant);

        String displayLang = testLocale.getDisplayLanguage(loc);

        return displayLang;

    }

    public static String GetKeyboardLanguage(int index)
    {
        Log.i("Unity", "API: " + Build.VERSION.SDK_INT);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N)
        {
            LocaleList localeList = UnityPlayer.currentActivity.getResources().getConfiguration().getLocales();

            if (index >= 0 && index < localeList.size())
            {
                Locale loc = localeList.get(index);
                return loc.toString();
            }
            else
            {
                return "";
            }
        }
        else
        {
            String defaultLang = UnityPlayer.currentActivity.getResources().getConfiguration().locale.toString();
            boolean containsDefault = Arrays.asList(s_fallbackLanguages).contains(defaultLang);
            int arrayLength = containsDefault ? s_fallbackLanguages.length : s_fallbackLanguages.length + 1;

            if (index >= 0 && index < arrayLength)
            {
                if (containsDefault)
                {
                    return s_fallbackLanguages[index];
                }
                else
                {
                    return index == 0 ? defaultLang : s_fallbackLanguages[index - 1];
                }
            }
            else
            {
                return "";
            }
        }
    }

    public static int GetNumKeyboards()
    {
        int ret = 0;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N)
        {
            LocaleList localeList = UnityPlayer.currentActivity.getResources().getConfiguration().getLocales();

            return localeList.size();
        }
        else
        {
            int numLangs = s_fallbackLanguages.length;
            String defaultLang = UnityPlayer.currentActivity.getResources().getConfiguration().locale.toString();

            if (!Arrays.asList(s_fallbackLanguages).contains(defaultLang))
            {
                numLangs++;
            }

            return numLangs;
        }

    }

    public static boolean CheckWriteExternalPermission()
    {
        int res = UnityPlayer.currentActivity.checkCallingOrSelfPermission(Manifest.permission.WRITE_EXTERNAL_STORAGE);

        return (res == PackageManager.PERMISSION_GRANTED);
    }

    public static void RequestWriteExternalStoragePermission(String title, String body, String okay) {

        if (ActivityCompat.shouldShowRequestPermissionRationale(UnityPlayer.currentActivity, Manifest.permission.WRITE_EXTERNAL_STORAGE)) {
            new AlertDialog.Builder( new ContextThemeWrapper(UnityPlayer.currentActivity, android.R.style.Theme_DeviceDefault_Light_Dialog) )
                    .setTitle(title)
                    .setMessage(body)
                    .setPositiveButton(okay, new DialogInterface.OnClickListener() {
                        @Override
                        public void onClick(DialogInterface dialog, int which) {
                            ActivityCompat.requestPermissions(UnityPlayer.currentActivity, new String[]{Manifest.permission.WRITE_EXTERNAL_STORAGE}, 101);
                        }
                    })
                    .show();
        }
    }
}
