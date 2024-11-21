#import "MailAttachmentManager.h"
#import <MessageUI/MessageUI.h>

@implementation MailAttachmentManager

+ (void)openMailAppWithAttachment:(NSString *)subject body:(NSString *)body filePath:(NSString *)filePath  {
    
    if ([MFMailComposeViewController canSendMail]) {
        
        MFMailComposeViewController *mailViewController = [[MFMailComposeViewController alloc] init];
        mailViewController.mailComposeDelegate = self;
        [mailViewController setSubject:subject];
        [mailViewController setMessageBody:body isHTML:NO];        
        NSData *fileData = [NSData dataWithContentsOfFile:filePath];
        NSString *fileName = [filePath lastPathComponent];
        [mailViewController addAttachmentData:fileData mimeType:@"application/octet-stream" fileName:fileName];
        [[UIApplication sharedApplication].keyWindow.rootViewController presentViewController:mailViewController animated:YES completion:nil];
    }
    else {
            NSLog(@"Device not configured to send email.");
        }
}

#pragma mark - MFMailComposeViewControllerDelegate

+ (void)mailComposeController:(MFMailComposeViewController *)controller didFinishWithResult:(MFMailComposeResult)result error:(NSError *)error {
    [controller dismissViewControllerAnimated:YES completion:nil];
}

@end
