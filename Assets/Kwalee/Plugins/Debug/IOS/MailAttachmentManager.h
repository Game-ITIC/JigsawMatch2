#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface MailAttachmentManager : NSObject
+ (void)openMailAppWithAttachment:(NSString *)subject body:(NSString *)body filePath:(NSString *)filePath;
@end

NS_ASSUME_NONNULL_END
