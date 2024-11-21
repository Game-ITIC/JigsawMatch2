#import "UnityBridge.h"

void _OpenMailAppWithAttachment(const char* subject,const char* body,const char* filePath) {
    
      NSString *subjectStr = [NSString stringWithUTF8String:subject];
      NSString *bodyStr = [NSString stringWithUTF8String:body];
      NSString *filePathStr = [NSString stringWithUTF8String:filePath];
          
    [MailAttachmentManager openMailAppWithAttachment:subjectStr body:bodyStr filePath:filePathStr];
}
