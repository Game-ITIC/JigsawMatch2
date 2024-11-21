package com.coreplugins.attachmentmail;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.util.Log;
import androidx.core.content.FileProvider;
import java.io.File;

public class MailAttachmentManager {

    public void openMailAppWithAttachment(Context context, String authority, String subject, String body, String logFilePath) {
    
        File file = new File(logFilePath);
        Uri uri = Uri.parse("mailto:");
        Intent intent = new Intent(Intent.ACTION_SEND,uri);
        intent.setType("application/octet-stream");
        intent.putExtra(Intent.EXTRA_SUBJECT, subject);
        intent.putExtra(Intent.EXTRA_TEXT, body);
        Uri fileUri = FileProvider.getUriForFile(context, authority, file);
        intent.putExtra(Intent.EXTRA_STREAM, fileUri);
        Intent chooserIntent = Intent.createChooser(intent, "Send Mail");
        chooserIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (chooserIntent.resolveActivity(context.getPackageManager()) != null) {
            context.startActivity(chooserIntent);
        } else {
            Log.d("MainActivity", "There are no email apps available!");
        }
    }
}
