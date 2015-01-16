// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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