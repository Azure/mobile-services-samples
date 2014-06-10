//
//  QSUIAlertViewWithBlock.m
//  blog20140611
//
//  Created by Carlos Figueira on 6/9/14.
//  Copyright (c) 2014 MobileServices. All rights reserved.
//

#import "QSUIAlertViewWithBlock.h"
#import <objc/runtime.h>

@interface QSUIAlertViewWithBlock()

@property (nonatomic, copy) QSUIAlertViewBlock callback;

@end

@implementation QSUIAlertViewWithBlock

static const char *key;

@synthesize callback = _callback;

- (id) initWithCallback:(QSUIAlertViewBlock)callback
{
    self = [super init];
    if (self) {
        _callback = [callback copy];
    }
    return self;
}

- (void) showAlertWithTitle:(NSString *)title message:(NSString *)message cancelButtonTitle:(NSString *)cancelButtonTitle otherButtonTitles:(NSArray *)otherButtonTitles {
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:title
                                                    message:message
                                                   delegate:self
                                          cancelButtonTitle:cancelButtonTitle
                                          otherButtonTitles:nil];
    
    if (otherButtonTitles) {
        for (NSString *buttonTitle in otherButtonTitles) {
            [alert addButtonWithTitle:buttonTitle];
        }
    }
    
    [alert show];
    
    objc_setAssociatedObject(alert, &key, self, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (void) alertView:(UIAlertView *)alertView didDismissWithButtonIndex:(NSInteger)buttonIndex
{
    if (self.callback) {
        self.callback(buttonIndex);
    }
}

@end
